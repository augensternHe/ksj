import torch.nn as nn
import torch.nn.functional as F


#
class InConv(nn.Module):
    def __init__(self, dim, unflattened_size):
        super().__init__()
        self.in_block = nn.Sequential(
            nn.Unflatten(dim=dim, unflattened_size=unflattened_size)
        )

    def forward(self, x):
        return self.in_block(x)

class DownConv(nn.Module):
    def __init__(self, in_channels, out_channels, kernel_size):
        super().__init__()
        self.conv_block = nn.Sequential(
            nn.Conv3d(in_channels, out_channels, kernel_size=kernel_size, padding='same', padding_mode='reflect', bias=False),
            nn.ELU(),
            nn.BatchNorm3d(out_channels),
            nn.Conv3d(out_channels, out_channels, kernel_size=kernel_size, padding='same', padding_mode='reflect', bias=False),
            nn.ELU(),
            nn.BatchNorm3d(out_channels),
            nn.MaxPool3d(kernel_size=(1, 2, 2))
        )

    def forward(self, x):
        return self.conv_block(x)

class UpConv(nn.Module):
    def __init__(self, in_channels, out_channels, kernel_size):
        super().__init__()
        self.conv_block = nn.Sequential(
            nn.Conv3d(in_channels, out_channels, kernel_size=kernel_size, padding='same', padding_mode='reflect', bias=False),
            nn.ELU(),
            nn.BatchNorm3d(out_channels),
            nn.Upsample(scale_factor=(1, 2, 2))
        )

    def forward(self, x):
        return self.conv_block(x)

class OutConv(nn.Module):
    def __init__(self, in_channels, out_channels):
        super().__init__()
        self.conv_block = nn.Sequential(
            nn.Conv3d(in_channels, out_channels, kernel_size=1, padding='same', padding_mode='reflect', bias=False),
            nn.Flatten(start_dim=-4, end_dim=-3)
        )

    def forward(self, x, f):
        return self.conv_block(x) * 0.95 + f * 0.05

