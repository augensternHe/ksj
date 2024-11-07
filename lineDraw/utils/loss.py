import torch
from torch import square, sqrt, sum, abs, amax, from_numpy
from torch import nn
import piq

import numpy as np

#
class TotalVariationLoss(nn.Module):
    def __init__(self):
        super(TotalVariationLoss, self).__init__()

        _tv_loss = piq.TVLoss()
        _transform = lambda x: x / torch.amax(x, dim=-1, keepdim=True)
        self.TV_LOSS = lambda x, y: abs(_tv_loss( _transform(x[:,None,None,:]))
                                        -_tv_loss( _transform(y[:,None,None,:]))
                                        )

    def forward(self, x: torch.Tensor, y: torch.Tensor):
        return self.TV_LOSS(x, y) * 10


#
class MSELoss(nn.Module):
    def __init__(self, solve, taper_edge: bool = True):
        super(MSELoss, self).__init__()
        
        _mask = 1.0
        if taper_edge:
            _xi = np.linspace(-1.0, 1.0, solve.nn_output) #TODO: chagne the 400 to a variable
            _mask = np.exp(-np.power(_xi, 20) / 2 / 0.5**2)
        _mask = torch.from_numpy(_mask).float()
        self.register_buffer('mask', _mask) # move along when .to(device) is called

        _mse_loss = nn.MSELoss()
        _transform = lambda x: x / torch.amax(x, dim=-1, keepdim=True) * self.mask
        self.MSE_LOSS = lambda x, y: _mse_loss( _transform(x), _transform(y))
        
    def forward(self, x: torch.Tensor, y: torch.Tensor):
        return self.MSE_LOSS(x, y) * 100


#
class MixedLoss(nn.Module):
    def __init__(self, solve, args):
        super(MixedLoss, self).__init__()
        self.MSE_LOSS = MSELoss(solve)
        self.TV_LOSS = TotalVariationLoss()

        self.mse_weight = args.mse_weight
        self.tv_weight = args.tv_weight
    
    def forward(self, x: torch.Tensor, y: torch.Tensor):
        loss_comp = dict()
        mse_loss = 0.0
        tv_loss = 0.0
        total_loss = 0.0

        if self.mse_weight > 0:
            mse_loss = self.mse_weight * self.MSE_LOSS(x, y)
            total_loss = total_loss + mse_loss
            loss_comp["mse_loss"] = mse_loss
        if self.tv_weight > 0:
            tv_loss = self.tv_weight * self.TV_LOSS(x, y)
            total_loss = total_loss + tv_loss
            loss_comp["tv_loss"] = tv_loss
       
        return total_loss, loss_comp
