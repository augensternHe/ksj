import os

import torch
from torch import nn
from torch.nn.functional import pad
from torch.fft import fft2, ifft2, fftshift, ifftshift
import torch.utils
from torchvision.utils import make_grid
from torchvision.transforms import functional

import numpy as np
from scipy.io import savemat, loadmat
from scipy.interpolate import interp1d
import matplotlib.pyplot as plt
from matplotlib import colormaps

from algorithm.enhance.utils.args import parse_args

class NeuralCam():
    def __init__(self, args):
        #
        nm = 1e-9
        um = 1e-6
        mm = 1e-3
        nsec = 1e-9
        usec = 1e-6
        msec = 1e-3
        sec = 1

        self.args = args
        # WARNING: all wavelength quantities are in NANOMETER.
        # Other physical quantiites are in SI units.
        # Conversion must be implemented when used.        
        self.min_wavelength = args.wl_min
        self.max_wavelength = args.wl_max
        self.num_wavelength: int = args.wl_num
        self.wavelengths = np.linspace(self.min_wavelength, self.max_wavelength, self.num_wavelength, dtype=np.float32)

        # load FPA transmittances
        self.num_FP_channels: int = 9
        _fpa_raw = np.genfromtxt(args.fpa_dir, delimiter=',', skip_header=True)
        _fpa = np.zeros((self.num_wavelength, self.num_FP_channels), dtype=np.float32)
        for _idx in range(0, 17, 2):
            _wl_raw = _fpa_raw[:, _idx]
            _f_raw = _fpa_raw[:, _idx+1]
            _wl = _wl_raw[~np.isnan(_wl_raw) & ~np.isnan(_f_raw)]
            _f = _f_raw[~np.isnan(_wl_raw) & ~np.isnan(_f_raw)]
            _fpa[:, np.int8(_idx/2)] = np.interp(self.wavelengths, _wl, _f)
        self.FPA = _fpa / np.amax(_fpa)
        
        self.lens_diameter = 2.2 * mm
        self.image_na = 0.015
        self.mla_mag = 0.25
        self.relay_mag = 0.75
        self.total_mag = self.mla_mag * self.relay_mag
        
        self.sensor_pixel_size = 6.5 * um
        # sub-pixel shift of each field (from top left to bottom right). Range: (-0.5, 0.5] due to periodicity
        self.pixel_fraction_shift = [(0.5, 0.5), (0.1, -0.1), (-0.4, 0.5),
                                     (0.3, -0.1), (-0.2, 0.1), (-0.2, 0.1),
                                     (-0.3, 0.2), (0.2, -0.3), (0.1, -0.4)]
        self.detector_qe = 1 #TODO: load sensor qe
        self.a_poisson = args.a_poisson
        self.b_sqrt = args.b_sqrt

        # Data array size settings
        # Full field (very hi-res) will be downsampled to a canvas.
        # Canvas will be edge trimmed to the output size = valid size + 2*margin
        # Valid means the size equating the meaningful FOV.
        self.canvas_ratio = 1.5
        self.output_ratio = 1.25 # this ratio leaves an image margin of 50px, which is half the PSF size
        self.psf_ratio = 0.125
        self.valid_size: int = 400
        self.canvas_size: int = int(self.canvas_ratio * self.valid_size)
        self.gt_size: int = int(self.output_ratio * self.valid_size)
        self.psf_max_size: int = int(self.psf_ratio * self.valid_size)
        
        # vignetting
        _vign_fcn = interp1d(
            x=args.vignette_fields, y=args.vignette_values, fill_value='extrapolate'
            )
        offsets = [(-1, 1), (0, 1), (1, 1),
                (-1, 0), (0, 0), (1, 0),
                (-1, -1), (0, -1), (1, -1)]
        _vign_sub = []
        _hw = self.output_ratio / 2
        for _idx in range(self.num_FP_channels):
            _ox, _oy = offsets[_idx]
            _xi = np.linspace(-_hw+_ox, _hw+_ox, 31) * self.lens_diameter
            _yi = np.linspace(-_hw+_oy, _hw+_oy, 31) * self.lens_diameter
            _xv, _yv = np.meshgrid(_xi, _yi, indexing='xy')
            _vign_sub.append(
                _vign_fcn(np.sqrt(_xv**2+_yv**2)/2/self.lens_diameter)
            )
        self.vignette_maps = np.array(_vign_sub)

        #
        if False:
            self.__summary
            self.__inspect_FPA(save_dir=args.savefig_dir)
            self.__inspect_vignette_maps(save_dir=args.savefig_dir)

    def __summary(self):
        '''Print the specifications of the neuralCam object.'''

        print("Instance attributes:")
        for name, value in vars(self).items():
            print(f"{name}: {value}")

        print("\nClass attributes:")
        for name in dir(self.__class__):
            if not name.startswith('__') and not callable(getattr(self.__class__, name)):
                value = getattr(self.__class__, name)
                print(f"{name}: {value}")
    #
    def __inspect_FPA(self, projection: str = '2d', save_dir: str = None):
        plt.figure()
        if projection.lower() == '2d':
            for i in range(self.num_FP_channels):
                plt.plot(self.wavelengths, self.FPA[:, i])
            plt.xlim((np.amin(self.wavelengths), np.amax(self.wavelengths)))
            plt.ylim((0, 1.05))
            plt.xlabel("Wavelength (nm)")
            plt.ylabel("T")
        else:
            ax = plt.axes(projection='3d')
            for i in range(self.num_FP_channels):
                ax.plot3D(self.wavelengths, i+1+0*self.wavelengths, self.FPA[:, i])
            plt.xlim((np.amin(self.wavelengths), np.amax(self.wavelengths)))
            plt.ylim((1, self.num_FP_channels))
            ax.set_zlim((0, 1.05))
            plt.xlabel("Wavelength (nm)")
            plt.ylabel("Filter index")
            ax.set_zlabel("Transmittance")
        if save_dir:
            check_directory(save_dir)
            plt.savefig(os.path.join(save_dir, "fpa_transmittances.png"), dpi=600)
        else:
            plt.show()
    #    
    def __inspect_vignette_maps(self, save_dir: str = None):
        fig, axes = plt.subplots(nrows=3, ncols=3, sharex=True, sharey=True) 
        _kk = 0
        for _ii in range(3):
            for _jj in range(3):
                axes[_ii,_jj].imshow(self.vignette_maps[_kk, :, :],
                                     cmap='gray', vmin=0.0, vmax=1.0, origin='lower')
                _CS = axes[_ii, _jj].contour(self.vignette_maps[_kk, :, :],
                                       levels=np.arange(0.0, 1.0, step=0.08),
                                       cmap='YlGnBu', alpha=0.95,
                                       vmin=0, vmax=1.0,
                                       linewidths=0.5, linestyles='dashed')
                axes[_ii, _jj].clabel(_CS, _CS.levels, inline=True, fontsize=4)
                _kk += 1
        fig.tight_layout()
        if save_dir:
            check_directory(save_dir)
            plt.savefig(os.path.join(save_dir, "mla_vignette_maps.png"), dpi=600)
        else:
            plt.show()











##
def wiener_deconv(f, psf, snr=1e3):
    _sf, _sp = f.shape[-1], psf.shape[-1]
    _pp = (_sf - _sp) // 2
    psf = pad(psf, pad=(_pp, _pp, _pp, _pp), mode='constant', value=0.0)

    F, OTF = map(fft2, (f, psf))
    H = torch.conj(OTF) / (torch.abs(OTF)**2 + 1/torch.abs(snr))
    g = torch.abs(ifftshift(ifft2(F * H), dim=(-2, -1)))

    return g


#
def center_crop(f, sz=400):
    return functional.center_crop(f, output_size=[sz, sz])


# make folders if not exist
def check_directory(path, erase=False):
    if not os.path.isdir(path):
            os.mkdir(path)
    if erase:
        for file_name in os.listdir(path):
            os.remove(os.path.join(path, file_name))

#
def to_ndarray(*args: torch.Tensor):
    '''Transfer torch.Tensor to cpu and make it a numpy array.'''
    
    proc_tensors = [x.cpu().detach().numpy() for x in args]
    if len(proc_tensors) == 1:
        proc_tensors = proc_tensors[0]
    
    return proc_tensors

#
def to_device(*args: torch.Tensor, device):
    '''Transfer torch.Tensor to the designated computing device.'''

    proc_tensors = [x.to(device) for x in args]
    if len(proc_tensors) == 1:
        proc_tensors = proc_tensors[0]

    return proc_tensors

#
# def make_collage(f, order: str = 'bchw', flipup=True, scale_each=False):
#     '''Assume input shape:
#         - 'bchw': (B, C, H, W)
#         - 'bhwc': (B, H, W, C)
#         - 'bdchw': (B, Depth, C, H, W)
#     Output shape: (H, W, C)'''
#
#     order = order.lower()
#     if order == 'bchw':
#         f = f[0, :, :, :]
#     elif order == 'bhwc':
#         f = f[0, :, :, :].permute(2, 1, 0)
#     elif order == 'bdchw':
#         f = f[0, 0, :, :, :].permute(2, 1, 0)
#     else:
#         assert False, "Unsupported input shape."
#
#     # this is useful when the array is created with xy ordering with origin at the lower left
#     if flipup:
#         f = torch.flip(f, dims=[1])
#     f = torch.unsqueeze(f, dim=1)
#     f = make_grid(f, nrow=3, padding=1, pad_value=1.0, normalize=True, scale_each=scale_each)
#     # to (H, W, C) for matplotlib
#     return f.permute(1, 2, 0)


def make_collage(f, order: str = 'bchw', flipup=True, scale_each=False):
    '''Assume input shape:
        - 'bchw': (B, C, H, W)
        - 'bhwc': (B, H, W, C)
        - 'bdchw': (B, Depth, C, H, W)
    Output: list of processed images'''

    order = order.lower()
    image_list = []

    for i in range(f.shape[0]):
        if order == 'bchw':
            img = f[i, :, :, :]
        elif order == 'bhwc':
            img = f[i, :, :, :].permute(2, 1, 0)
        elif order == 'bdchw':
            img = f[i, 0, :, :, :].permute(2, 1, 0)
        else:
            assert False, "Unsupported input shape."

        # this is useful when the array is created with xy ordering with origin at the lower left
        if flipup:
            img = torch.flip(img, dims=[1])
        img = torch.unsqueeze(img, dim=1)
        img = make_grid(img, nrow=3, padding=1, pad_value=1.0, normalize=True, scale_each=scale_each)
        # to (H, W, C) for matplotlib
        img = img.permute(1, 2, 0)

        image_list.append(img)

    return image_list







#####
def main():
    '''Save an example of the FPA and vigetting maps of the camera.
    Usage
        From parent directory, enter the following command:
            >> python -m utils.utils --fpa_dir <FPA_DIR> --vignette_fields <VIGN_FIELDS>
    '''

    args = parse_args()
    neuralCam = NeuralCam(args)
    neuralCam.inspect_FPA()
    neuralCam.inspect_vignette_maps()

if __name__ == "__main__":
    main()
