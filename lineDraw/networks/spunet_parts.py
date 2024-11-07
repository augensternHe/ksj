import torch
import torch.nn as nn
from torch.nn import functional


#
class Inc(nn.Module):
    def __init__(self, sys_T_inv, len_feat):
        super().__init__()
        self.register_buffer('A', sys_T_inv)
        self.n_features = len_feat
        
        _unflatten = lambda x: torch.unflatten(x, dim=-1, sizes=[self.n_features, 1])
        _pinv = lambda x: torch.matmul(self.A, x)
        _swap_axis = lambda x: torch.permute(x, dims=(0, 2, 1))
        self.inc_block = lambda x: _swap_axis( _pinv( _unflatten(x)))
    
    def forward(self, x):
        return self.inc_block(x)



#
class DownConv(nn.Module):
    def __init__(self, in_channels, out_channels, kernel_size):
        super().__init__()
        self.conv_block = nn.Sequential(
            nn.Conv1d(in_channels, out_channels, kernel_size=kernel_size, padding='same', padding_mode='reflect', bias=False),
            nn.PReLU(),
            nn.BatchNorm1d(out_channels),
            nn.Conv1d(out_channels, out_channels, kernel_size=kernel_size, padding='same', padding_mode='reflect', bias=False),
            nn.PReLU(),
            nn.BatchNorm1d(out_channels),
            nn.MaxPool1d(kernel_size=2)
        )

    def forward(self, x):
        return self.conv_block(x)
    
#
class UpConv(nn.Module):
    def __init__(self, in_channels, out_channels, kernel_size):
        super().__init__()
        self.conv_block = nn.Sequential(
            nn.Conv1d(in_channels, out_channels, kernel_size=kernel_size, padding='same', padding_mode='reflect', bias=False),
            nn.PReLU(),
            nn.BatchNorm1d(out_channels),
            nn.Upsample(scale_factor=2, mode='nearest')
        )

    def forward(self, x):
        return self.conv_block(x)

#
class Outc(nn.Module):
    def __init__(self, in_channels, out_channels, out_length):
        super().__init__()
        
        self.conv_block = nn.Sequential(
            nn.Conv1d(in_channels, out_channels, kernel_size=3, padding='same', padding_mode='reflect', bias=False),
            nn.PReLU(),
        )
        _resample = lambda x: functional.interpolate(x, size=out_length, mode='linear')
        _flatten = lambda x: torch.flatten(x, start_dim=-2, end_dim=-1)
        self.out_block = lambda x: _flatten( _resample(x))
        
    def forward(self, x):
        x = self.conv_block(x)
        x = self.out_block(x)
        
        return torch.clamp(x, min=1e-20, max=1.0)
