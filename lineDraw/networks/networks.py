import torch
import torch.nn as nn

from algorithm.lineDraw.networks.spunet_parts import *


#
class SpUNet(nn.Module):
    '''Reference: D. Gedalin, Y. Oiknine, and A. Stern, "DeepCubeNet: reconstruction of spectrally compressive sensed hyperspectral images with deep neural networks," Opt. Express 27, 35811-35822 (2019)'''
    
    def __init__(self, solve):
        super().__init__()
        
        _A = torch.from_numpy(solve.sys_T_inv).float()
        self.register_buffer('A', _A)
        
        _nn_input = solve.nn_input
        _nn_output = solve.nn_output
        
        # keep the kerel size small to avoid losing fine features
        self.inc = Inc(self.A, _nn_input)
        self.down1 = DownConv(1, 8, 3)
        self.down2 = DownConv(8, 16, 3)
        self.down3 = DownConv(16, 32, 3)
        self.down4 = DownConv(32, 128, 3)
        self.up1 = UpConv(128, 32, 3)
        self.up2 = UpConv(64, 16, 3)
        self.up3 = UpConv(32, 8, 3)
        self.up4 = UpConv(16, 8, 3)
        self.outc = Outc(8, 1, _nn_output)

    def forward(self, x):                               # (B, nn_input=200)
        x = self.inc(x)                                 # (B, 1, 512)
        x1 = self.down1(x)                              # (B, 8, 256)
        x2 = self.down2(x1)                             # (B, 16, 128)
        x3 = self.down3(x2)                             # (B, 32, 64)
        x4 = self.down4(x3)                             # (B, 128, 32)
        x = self.up1(x4)                                # (B, 32, 64)
        x = self.up2(torch.cat([x, x3], dim=1))         # (B, 16, 128)
        x = self.up3(torch.cat([x, x2], dim=1))         # (B, 8, 256)
        x = self.up4(torch.cat([x, x1], dim=1))         # (B, 8, 512))
        logits = self.outc(x)                                # (B, nn_output=76)

        return logits


