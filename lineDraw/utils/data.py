import os
import glob
from datetime import datetime
import random
import time

import torch
import torch.utils
from torch.utils.data import Dataset
from torchvision import transforms

import numpy as np
from numpy.random import randint, uniform, randn
import matplotlib.pyplot as plt

from algorithm.lineDraw.utils.args import parse_args
from algorithm.lineDraw.utils.utils import SpectrumSolve, check_directory


#
class SpectrumGenerator():
    '''Generate ground truth data and sensor signal.
    Shape of ground truth vector: (num_wavelengths, )
    Shape of sensor signal vector: (num_measures, )
    Randomness is added into the generated data to avoid overfitting.'''

    def __init__(self, solver, args):
        super().__init__()
        self.num_measures = solver.num_FP_channels
        self.wavelengths = solver.wavelengths
        self.wl_min = solver.wl_min
        self.wl_max = solver.wl_max
        self.sys_T = solver.sys_T
        self.det_qe = solver.det_qe
        self.b_sqrt = solver.b_sqrt
        self.wl_center = (self.wl_min + self.wl_max) / 2 # unit: nm
        self.dwl = self.wl_max - self.wl_min

    def __iter__(self):
        return self
    
    def __next__(self):
        _n_pks = randint(low=1, high=4)
        _f_tmp = 0.0
        # add broad background
        for _ in range(_n_pks):
            _pks_wl = uniform() * self.dwl * 1.3 + self.wl_min * 0.7
            _a = uniform() * 0.7 + 0.3
            _sig = (uniform() * 0.7 + 0.1) * self.wl_center
            _f_tmp += _a * np.exp(-(self.wavelengths - _pks_wl)**2 / 2 / _sig**2)
        # add fine features
        _n_pks = randint(low=3, high=10)
        for _ in range(_n_pks):
            _pks_wl = uniform() * self.dwl * 0.9 + self.wl_min * 1.1
            _a = uniform() * 0.4 + 0.05
            _sig = uniform() * 20 + 2.5
            _f_tmp += _a * np.exp(-(self.wavelengths - _pks_wl)**2 / 2 / _sig**2)
        _f_tmp /= np.amax(_f_tmp)
        _f_tmp *= uniform() * 0.8 + 0.2
        
        gt = _f_tmp
        ssr = np.matmul(self.sys_T, self.det_qe * _f_tmp)
        _avg = np.mean(ssr)
        ssr += _avg * self.b_sqrt * uniform() * randn(self.num_measures)
        
        return np.squeeze(gt), np.squeeze(ssr)

#
class SpectrumDataset(Dataset):
    def __init__(self, solver, args):
        self.solver = solver
        self.args = args
        
        spectrumGenerator = SpectrumGenerator(solver, args)
        self.spectrumGenerator = iter(spectrumGenerator)
        self.len_ds = args.batch_size
        self.transform = True
    
    def __len__(self):
        return self.len_ds

    def __getitem__(self, idx):
        gt, ssr = next(self.spectrumGenerator)
        if self.transform:
            gt = self.__interp(gt)
        if isinstance(gt, np.ndarray):
            gt, ssr = map(torch.from_numpy, (gt, ssr))
        return gt.float(), ssr.float()

    def __interp(self, f):
        return np.interp(x=self.solver.new_wavelengths, xp=self.solver.wavelengths, fp=f)
    
    def inspect_data(self, f, extra_name: str = ""):
        # TODO: incomplete
        pass
        check_directory(self.args.savefig_dir)
        fn = "Example_channels_"+ self.args.run_code + extra_name + ".png"
        plt.savefig(os.path.join(self.args.savefig_dir, fn), dpi=600)



#
class SpectrumDatasetFromStorage(SpectrumDataset):
    def __init__(self, solver, args):
        super().__init__(solver, args)
        _ds_start = time.time()
        #
        data_dir = args.data_dir
        print(f"Loading datasets in \"{data_dir}\"...", end='\r')
        ds_files = glob.glob(os.path.join(data_dir, "dataset*.pth"))
        ds_files.sort(reverse=True)
        assert os.path.isfile(ds_files[0]), f"No checkpoint file found in {data_dir}."
        self.data = []
        for _idx in range(len(ds_files)):
            self.data += torch.load(ds_files[_idx])
        print(f"Datasets in \"{data_dir}\" have been loaded. Time: {time.time()-_ds_start:.2f}s", flush=True)

    def __len__(self):
        return len(self.data)
    
    def __getitem__(self, idx):
        gt, ssr = self.data[idx]
        if self.transform:
            gt = self.__interp(gt)
        if isinstance(gt, np.ndarray):
            gt, ssr = map(torch.from_numpy, (gt, ssr))
        
        return gt.float(), ssr.float()


# TODO: load test data
class SpectrumDatasetFromExperiments(SpectrumDataset):
    def __init__(self, solver, args):
        super().__init__(solver, args)
    def __len__(self):
        pass
    def __getitem__(self, idx):
        pass


#
def store_dataset(solver, args):
    '''Generate simulated spectrum dataset and save as .pth file'''

    spectrumGenerator = iter(SpectrumGenerator(solver, args))
    check_directory(args.data_dir)
    data = []
    for i in range(args.total_spectra):
        print(f"Item: {i+1}/{args.total_spectra}{' ':20}", end='\r')
        gt, ssr = next(spectrumGenerator)
        data.append((gt, ssr))
    fn = "dataset_dt" + args.run_code + "_len" + str(len(data)) + ".pth"
    torch.save(data, os.path.join(args.data_dir, fn))



####
def main():
    args = parse_args()
    spectrumSolve = SpectrumSolve(args)
    store_dataset(spectrumSolve, args)
    

if __name__ == "__main__":
    main()