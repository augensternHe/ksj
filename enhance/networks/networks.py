import torch
import torch.nn as nn

import numpy as np

from algorithm.enhance.networks.srunet_parts import *

class SrCNN(nn.Module):
    def __init__(self, device):
        super().__init__()
        
        _n_ch: int = device.num_FP_channels
        _sz: int = device.psf_max_size # 50. See utils/utils.py
        _xi = np.linspace(-1.0, 1.0, _sz)
        _xv, _yv = np.meshgrid(_xi, _xi)
        _psf_init = np.exp(-(_xv**2 + _yv**2)/2/0.05**2)
        _psf_init = np.tile(_psf_init[None, :, :], reps=(_n_ch, 1, 1))
        _psf_init = torch.from_numpy(_psf_init).float()
        self.register_buffer('psf_init', _psf_init)

        _n_in: int = _sz
        _tmp = np.random.randn(1, _n_ch, _n_in)
        self.seed = nn.Parameter(torch.from_numpy(_tmp).float(), requires_grad=True)
        
        self.model = nn.Sequential(
            nn.Linear(in_features=_n_in, out_features=75),
            nn.ReLU(),
            nn.Linear(in_features=75, out_features=150),
            nn.ReLU(),
            nn.Linear(in_features=150, out_features=300),
            nn.ReLU(),
            nn.Linear(in_features=300, out_features=_sz**2//4),
            nn.ReLU(),
            nn.Unflatten(dim=-1, unflattened_size=(_sz//2, _sz//2)),
            nn.Upsample(scale_factor=2, mode='bilinear')
        )
        
    def forward(self):
        x = self.model(self.seed) * 0.5 + self.psf_init * 0.5
        x = x / torch.sum(x, dim=(-2, -1), keepdim=True)

        return x


##
class SrUNet(nn.Module):
    def __init__(self, device):
        super(SrUNet, self).__init__()
        _n_ch: int = device.num_FP_channels

        self.inc = InConv(dim=-3, unflattened_size=(1, _n_ch))
        self.down1 = DownConv(1, 8, (5, 5, 5))
        self.down2 = DownConv(8, 16, (3, 3, 3))
        self.down3 = DownConv(16, 32, (3, 3, 3))
        self.down4 = DownConv(32, 128, (3, 3, 3))
        self.up1 = UpConv(128, 32, (3, 3, 3))
        self.up2 = UpConv(64, 16, (3, 3, 3))
        self.up3 = UpConv(32, 8, (5, 5, 5))
        self.up4 = UpConv(16, 8, (3, 3, 3))
        self.outc = OutConv(8, 1)

    def forward(self, f):                       # (B, 9, 480, 480)
        x = self.inc(f)                         # (B, 1, 9, 480, 480)
        x1 = self.down1(x)                      # (B, 8, 9, 240, 240)
        x2 = self.down2(x1)                     # (B, 16, 9, 120, 120)
        x3 = self.down3(x2)                     # (B, 32, 9, 60, 60)
        x4 = self.down4(x3)                     # (B, 128, 9, 30, 30)
        x = self.up1(x4)                        # (B, 1, 32, 60, 60)
        x = self.up2(torch.cat([x, x3], dim=1)) # (B, 16, 9, 120, 120)
        x = self.up3(torch.cat([x, x2], dim=1)) # (B, 8, 9, 240, 240)
        x = self.up4(torch.cat([x, x1], dim=1)) # (B, 8, 9, 480, 480)
        x = self.outc(x, f)                        # (B, 9, 480, 480)

        return torch.clamp(x, min=1e-20, max=1.0)

