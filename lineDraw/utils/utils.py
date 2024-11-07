import os

import torch
import numpy as np
from scipy.io import loadmat
import matplotlib.pyplot as plt

class SpectrumSolve():

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

        _sol_conf = loadmat(file_name=args.solver_config,
                                variable_names=["wavelengthnm", "spectra400to800", "QE"])
        # WARNING: all wavelength quantities are in NANOMETER.
        # Other physical quantiites are in SI units.
        # Conversion must be implemented when used.
        self.wl_min = args.wl_min
        self.wl_max = args.wl_max
        self.wl_res = args.wl_res
        self.num_wavelengths: int = args.wl_num
        
        raw_wl = _sol_conf['wavelengthnm'].astype(np.float32)
        tfa = np.transpose(_sol_conf['spectra400to800'].astype(np.float32)) # shape: (n_cavity, n_wave)
        new_wl = np.linspace(self.wl_min, self.wl_max, self.num_wavelengths)
        n_ch: int = np.shape(tfa)[0]
        new_tfa = np.zeros((n_ch, self.num_wavelengths))
        for _idx in range(n_ch):
            _tf = np.interp(new_wl.flatten(), raw_wl.flatten(), tfa[_idx, :])
            new_tfa[_idx, :] = _tf / np.sqrt(np.sum(_tf**2))
        
        self.wavelengths = new_wl
        self.num_FP_channels = n_ch
        self.sys_T = new_tfa
        self.sys_T_inv = np.linalg.pinv(new_tfa)
        
        # sensor
        self.det_qe_raw = _sol_conf['QE'].astype(np.float32)
        self.det_qe = np.interp(self.wavelengths, self.det_qe_raw[:,0], self.det_qe_raw[:,1])
        self.b_sqrt = args.b_sqrt

        # network
        _n_new_wl = np.int16((self.wl_max - self.wl_min) / self.wl_res) # 76 @ 5nm resolution
        self.new_wavelengths = np.linspace(self.wl_min, self.wl_max, _n_new_wl)
        self.nn_input: int = n_ch # 200
        self.nn_output: int = _n_new_wl
        
        
    def summary(self):
        '''Print the specifications of the SpectraSolve object.'''

        print("Instance attributes:")
        for name, value in vars(self).items():
            print(f"{name}: {value}")

        print("\nClass attributes:")
        for name in dir(self.__class__):
            if not name.startswith('__') and not callable(getattr(self.__class__, name)):
                value = getattr(self.__class__, name)
                print(f"{name}: {value}")


    def inspect_FPA(self, projection: str = '2d'):
        plt.figure()
        if projection.lower() == '2d':
            for _idx in range(self._n_ch):
                plt.plot(self.wavelengths, self.sys_T[_idx, :])
            plt.xlim((np.amin(self.wavelengths), np.amax(self.wavelengths)))
            plt.ylim((0, 1.05))
            plt.xlabel("Wavelength (nm)")
            plt.ylabel("T")
        else:
            ax = plt.axes(projection='3d')
            for _idx in range(self._n_ch):
                ax.plot3D(self.wavelengths, _idx*10+0*self.wavelengths, self.sys_T[_idx, :])
            plt.xlim((np.amin(self.wavelengths), np.amax(self.wavelengths)))
            plt.ylim((0, 10*self._n_ch))
            ax.set_zlim((0, 1.05))
            plt.xlabel("Wavelength (nm)")
            plt.ylabel("Filter index")
            ax.set_zlabel("Transmittance")
        
        savefig_dir = self.args.savefig_dir
        check_directory(savefig_dir)
        plt.savefig(os.path.join(savefig_dir, "FPA.png"), dpi=600)



# make folders if not exist
def check_directory(path: str, erase=False):
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
def batch_to_cube(*args: torch.Tensor, sizes: list):
    '''Wrap the batch dimension into the spatial dimensions.'''
    
    if isinstance(sizes, tuple):
        sizes = [sizes]
    
    p_list = zip(args, sizes)
    proc_tensors = []
    for x, sz in p_list:
        proc_tensors.append(x.reshape(sz))
    if len(proc_tensors) == 1:
        proc_tensors = proc_tensors[0]
    
    return proc_tensors

#
def cube_to_batch(*args: torch.Tensor):
    '''Serialize the spatial dimensions into the batch dimension.'''

    proc_tensors = [x.flatten(start_dim=0, end_dim=1) for x in args]
    if len(proc_tensors) == 1:
        proc_tensors = proc_tensors[0]
    
    return proc_tensors


