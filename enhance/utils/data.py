import os
import glob
import time
import random
import multiprocessing

import torch
import torch.utils
from torch.utils.data import Dataset
from torchvision import transforms

import numpy as np
from numpy.random import randint, choice, rand
from numpy.fft import fft2, ifft2, fftshift, ifftshift
from scipy.io import loadmat
from scipy.interpolate import interp1d
import cv2
import matplotlib.pyplot as plt
import h5py

from algorithm.enhance.utils.args import parse_args
from algorithm.enhance.utils.utils import NeuralCam, check_directory

#
nm = 1e-9
um = 1e-6
mm = 1e-3
nsec = 1e-9
usec = 1e-6
msec = 1e-3
sec = 1
uint16_max = 2**16-1

#
class AddNoise():
    def __init__(self, mean=0.0, sig=1.0, clip=True):
        self.mean = mean
        self.sig = sig
        self.clip = clip

    def __call__(self, x):
        x = x + torch.normal(self.mean, self.sig*rand(), size=x.shape)
        return torch.clamp(x, min=1e-20, max=1.0)

#
class FluoroDatabase():
    '''Import fluorophore data and make database'''
    def __init__(self, args, wavelengths=None):
        super().__init__()
        self.fluoro_db = loadmat(args.fluoro_db)
        self.fluoro_db = np.squeeze(self.fluoro_db['fluoro_db'])

        _tmp = dict()
        for i in range(np.size(self.fluoro_db)):
            _key = np.array2string(self.fluoro_db[i][0])
            _key = _key.replace("['", '')
            _key = _key.replace("']", '')
            # add zeros to the both ends to avoid interpolation error
            _wavelengths_raw = np.concatenate((np.array([100, 200]), self.fluoro_db[i][1][:, 0], np.array([850, 1000])), axis=0)
            _intensity_raw = np.concatenate((np.array([0, 0]), self.fluoro_db[i][1][:, 1], np.array([0, 0])), axis=0)
            if wavelengths is not None:
                f = interp1d(_wavelengths_raw, _intensity_raw, fill_value='extrapolate')
                _intensity = f(wavelengths)
                _intensity[np.isnan(_intensity)] = 0
            else:
                wavelengths, _intensity = _wavelengths_raw, _intensity_raw
            _tmp[_key] = {'wavelength_nm': wavelengths, 'intensity': _intensity}
        self.fluoro_db_all = _tmp
        # pick out the selected fluorophores
        if args.fluoros_list is None:
            self.fluoro_db = self.fluoro_db_all
        else:
            self.fluoro_db = self.__select_fluoros(args.fluoros_list)
        #
        if False:
            self.__inspect_fluoros(all_fluoros=True, save_dir=args.savefig_dir, extra_name="_all")
            self.__inspect_fluoros(all_fluoros=False, save_dir=args.savefig_dir, extra_name="_selected")

    def __select_fluoros(self, fluoro_list):
        sel_fluoro = dict()
        for name in fluoro_list:
            if name in self.fluoro_db_all:
                sel_fluoro[name] = self.fluoro_db_all[name]
            else:
                assert False, f"Fluorophore \"{name}\" does not exist in the database."
        return sel_fluoro

    def __inspect_fluoros(self, all_fluoros=True, save_dir: str = None, extra_name: str = ""):
        '''Export functions of fluorophores'''

        fdb = self.fluoro_db_all if all_fluoros else self.fluoro_db
        plt.figure(figsize=(13, 5))
        for key in fdb:
            wavelengths, intensity = fdb[key]['wavelength_nm'], fdb[key]['intensity']
            plt.plot(wavelengths, intensity, label=key)
            plt.fill_between(wavelengths, intensity, alpha=0.4)
        plt.xlim((np.amin(wavelengths)-90, np.amax(wavelengths)))
        plt.ylim((-0.03, 1.03))
        plt.xlabel("Wavelength (nm)")
        plt.ylabel("Emission intensity")
        plt.title("Fluorophore Emission Spectra")
        plt.legend(loc='upper left')

        if save_dir:
            check_directory(save_dir)
            _fn = "fluorophores" + extra_name + ".png"
            plt.savefig(os.path.join(save_dir, _fn), dpi=600)

#
class LabelGenerator():
    def __init__(self, device, args):
        super().__init__()
        self.device = device
        self.args = args
        self.cvas_sz :int = device.canvas_size
        self.n_labels: int = len(args.fluoros_list)
        
        self.data_dir: str = args.data_dir
        if self.data_dir is not None:
            _entries = os.listdir(self.data_dir)
            self.dirs = [entry for entry in _entries if os.path.isdir(os.path.join(self.data_dir, entry))]
            self.length: int = len(self.dirs)
        
    def __iter__(self):
        return self
    
    def __len__(self):
        return self.length
    
    def __next__(self):
        _fd = self.dirs[randint(self.length)]
        _imgs = glob.glob(os.path.join(self.data_dir, _fd, "*.png"))
        _nImgs: int = len(_imgs)
        _nLabels: int  = self.n_labels
        # check if the total images are more than the numbers of fluorophores
        if _nImgs > _nLabels:
            _im_idx: int = choice(np.arange(_nImgs, dtype=np.uint8), _nLabels, replace=False)
            _fl_idx: int = choice(np.arange(_nLabels, dtype=np.uint8), _nLabels, replace=False)
        else:
            _im_idx: int = choice(np.arange(_nImgs, dtype=np.uint8), _nImgs, replace=False)
            _fl_idx: int = choice(np.arange(_nLabels, dtype=np.uint8), _nImgs, replace=False)

        # ROI random select, crop, and rotate
        # assume all images in this folder have the same size
        # many PNGs have transparent margins
        _sz = self.cvas_sz
        _tmp = cv2.imread(_imgs[0], cv2.IMREAD_UNCHANGED)
        assert _tmp is not None, "Error: Could not load image."
        if len(_tmp.shape) > 2:
            _, _, _, _tmp = cv2.split(_tmp)
            _nz_alpha = np.where(_tmp != 0)
            _r_lb, _r_ub = np.min(_nz_alpha[0]), np.max(_nz_alpha[0])
            _c_lb, _c_ub = np.min(_nz_alpha[1]), np.max(_nz_alpha[1])
            _tmp = _tmp[_r_lb: _r_ub, _c_lb: _c_ub] # remove transparent margins
            _nr, _nc = _tmp.shape
        else:
            _nr, _nc = _tmp.shape
            _r_lb, _r_ub = 0, _nr
            _c_lb, _c_ub = 0, _nc
        assert _nr >= _sz and _nc >= _sz, "Image size should be larger."

        _r_ini, _c_ini = randint(low=0, high=_nr-_sz), randint(low=0, high=_nc-_sz)
        _rot: int = randint(low=0, high=4)

        # create a map of probability mass functions for all pixels
        _pmf = np.zeros((_nLabels, _sz, _sz))
        for _ii in range(min(_nImgs, _nLabels)):
            _tmp = cv2.imread(_imgs[_im_idx[_ii]], cv2.IMREAD_GRAYSCALE).astype(np.float32)
            _tmp = _tmp[_r_lb: _r_ub, _c_lb: _c_ub] # remove margins
            _pmf[_fl_idx[_ii], :, :] = _tmp[_r_ini: _r_ini+_sz, _c_ini: _c_ini+_sz]
        _pmf = np.rot90(_pmf, k=_rot, axes=(-2, -1))
        _psum = np.sum(_pmf, axis=0, keepdims=False)
        _psum_max = np.amax(_psum, keepdims=True) if np.amax(_psum, keepdims=True) > 0 else 1 # to avoid no label at all
        _p0 = _psum_max - _psum
        _pmf = np.vstack((_p0[None, :, :], _pmf)) # add no-label at the top of the stack
        _pmf /= np.sum(_pmf, axis=0, keepdims=True)
        
        # create a map of labels, including 0 for no fluorophore
        fmap = np.zeros((_sz, _sz), dtype=np.uint8)
        for _ii in range(_sz):
            for _jj in range(_sz):
                _p = _pmf[:, _ii, _jj]
                fmap[_ii, _jj] = choice(a=np.arange(0, _nLabels+1, dtype=np.uint8),
                                        p=_p)
        return fmap

#
class ImageGenerator():
    '''Generate an image the same size of the intermediate image of the microscope.
    - The total image is around 22mm x 22mm. Microlens pitch is 2.2mm x 2.2mm.
    - 3x3 microlenses capture all 9 channels of a field of 2.2mm x 2.2mm.
    - Another 1.1mm is added as the convolution margin so the total field is 3.3mm x 3.3mm.
    - Microscope image NA is ~0.012 (diffraction-limit PSF size of 20um @ 400nm).
    - Minimum field size is 330 x 330 pixels.
    - We oversample the field at 3000 x 3000 pixels to simulate sub-pixel shifting.
    - At the end, the sensor image stacked will be undersampled by sensor to 65 x 65 pixels.
    - The ground truth image stacked will be resized to 400 x 400 pixels.
    - Returns (gt, sensor)'''

    def __init__(self, device, args):
        super().__init__()
        self.device = device
        self.args = args
        self.wavelengths = device.wavelengths # unit: nm
        self.fpa_tf = device.FPA # filter array transfer functions
        self.vign_maps = device.vignette_maps
        self.num_channels = device.num_FP_channels
        self.image_na = device.image_na
        self.det_qe = device.detector_qe
        self.b_sqrt = args.b_sqrt
        # simulated PSFs
        self.psf_weights: dict = dict()
        for _wl in range(400, 800, 50): self.psf_weights[float(_wl)] = 1.0
        
        _canvas_ratio = device.canvas_ratio
        _output_ratio = device.output_ratio
        self.im_width = _canvas_ratio * device.lens_diameter
        self.mag = device.total_mag
        self.px_sz = device.sensor_pixel_size
        self.fraction_shift = device.pixel_fraction_shift

        # Imaging parameters
        # Assuming only one fluorophore at a point and all points are distanced beyond diff. limit:
        # 1) Create a small image _tmp_sub with pitch slightly greater than diff-limited PSF size; 2) expand _tmp_sub into _tmp_f by inserting 0 between elements
        self.f_sz: int = int(88 * _canvas_ratio) # lens_diameter / 25um = 88 px
        self.sc_ratio: int = 25
        self.f_sz_ext: int = int(self.f_sz * self.sc_ratio) # 3300 px
        self.dx = self.im_width / self.f_sz_ext # element size, 1um
        
        # Downsampling and cropping. Here
        # Field is referred to a sub-diff-limit array.
        # Image is referred to a downsampled array
        # Cropping results in a size = valid size + 2 * margin size
        # GT: field (3300) --> downsampled full image (600) --> cropped image (500)
        # SSR: field size (3300) --> cropped field (2750) --> downsampled sensor image (93)
        # The order of SSR is important. Downsample first will result in noticeable rounding errors and image offsets.
        self.canvas_sz: int = int(device.canvas_size)
        self.gt_sz: int = int(device.gt_size)
        self.gt_margin: int = int((self.canvas_sz - self.gt_sz) // 2)
        self.ssr_margin: int = int(self.f_sz_ext // 12)
        self.ssr_out: int = int((self.f_sz_ext) * 1.25 / 1.5)
        self.ssr_px = int(self.im_width * self.mag / self.px_sz * _output_ratio / _canvas_ratio) # final sensor output size
        #
        _v_maps = np.transpose(self.vign_maps, axes=[2,1,0])
        _v_maps = cv2.resize(_v_maps, dsize=[self.ssr_px, self.ssr_px], interpolation=cv2.INTER_LINEAR)
        self.vign_maps = np.transpose(_v_maps, axes=[2,1,0])

        # prepare fluorophore data
        fluoroDatabase = FluoroDatabase(args=self.args, wavelengths=self.wavelengths)
        self.fl_db = fluoroDatabase.fluoro_db
        self.num_fl, self.den_fl = len(args.fluoros_list), 1e-2
        #
        self.labelGenerator = iter(LabelGenerator(device=self.device, args=self.args))

    def __iter__(self):
        return self

    def __next__(self):
        _fmap = self.fetch_labels() 
        gt = np.zeros((self.num_channels, self.gt_sz, self.gt_sz))
        ssr = np.zeros((self.num_channels, self.ssr_px, self.ssr_px))
        _tmp = np.zeros((self.canvas_sz, self.canvas_sz), dtype=np.float32)
        _fstack = np.zeros((self.f_sz_ext, self.f_sz_ext), dtype=np.float32)  
        for _idx in range(self.num_channels):
            _tmp, _fstack = _tmp*0,  _fstack*0
            _transfer_fn = self.fpa_tf[:, _idx] * self.det_qe # pixel value = integral(Em_fn * Filt_tf * Det_qe)
            for _id, _key in enumerate(self.fl_db.keys()):
                _tmp[_fmap==_id+1] = np.sum( self.fl_db[_key]['intensity'] * _transfer_fn)
            # interleave lo-res to hi-res
            _fstack = cv2.resize(_tmp, dsize=(self.f_sz_ext, self.f_sz_ext), interpolation=cv2.INTER_NEAREST_EXACT)
            # calculate wavelength contribution
            for _key in self.psf_weights:
                self.psf_weights[_key] = np.interp(_key, self.wavelengths, _transfer_fn)
            # GT
            _tmp_g = self.__optical_blur(_fstack, dx=self.dx, na=self.image_na, weights=self.psf_weights)
            _tmp_g = cv2.resize(_tmp_g, dsize=(self.canvas_sz, self.canvas_sz), interpolation=cv2.INTER_AREA)
            gt[_idx, :, :] = _tmp_g[self.gt_margin: self.gt_margin+self.gt_sz, self.gt_margin: self.gt_margin+self.gt_sz]  
            # SSR: SUB-PIXEL SHIFT FIRST. Otherwise noticeable rounding errors and image offsets occur.
            _tmp_s = self.__optical_blur(_fstack, dx=self.dx*self.mag, na=self.image_na, weights=self.psf_weights)
            _tmp_s = self.__subpixel_shift(_tmp_s, fraction_shift=self.fraction_shift[_idx], pixel_ratio=round(self.px_sz/self.dx))
            _tmp_s = _tmp_s[self.ssr_margin: self.ssr_margin+self.ssr_out, self.ssr_margin: self.ssr_margin+self.ssr_out]
            ssr[_idx, :, :] = cv2.resize(_tmp_s, dsize=(self.ssr_px, self.ssr_px), interpolation=cv2.INTER_AREA)  
        # add vignetting
        ssr *= self.vign_maps

        gt = np.nan_to_num(gt/np.amax(gt), nan=1e-20) * uint16_max
        ssr = np.nan_to_num(ssr/np.amax(ssr), nan=1e-20) * uint16_max

        return gt.astype(np.uint16), ssr.astype(np.uint16)
    
    #
    def fetch_labels(self):
        '''Fetch fluorophore label map. Can be overriden to create other patterns'''
        return next(self.labelGenerator)
    
    #
    def __optical_blur(self, img, dx, na, weights: dict):
        '''Add realistic optical blurring to images'''
        size = img.shape[0]
        xi = dx * np.linspace(-np.floor(size/2), np.floor(size/2), size)
        xv, yv = np.meshgrid(xi, xi, indexing='xy')
        psf = xv * 0
        for _wl, _value in weights.items():
            diff_limit = 0.61 * _wl * nm / na
            psf += np.exp(-(xv**2+yv**2) / 2 / diff_limit**2) * _value
        psf = psf / np.sum(psf)

        OTF, F = fft2(fftshift(psf)), fft2(fftshift(img))
        blur_img = ifftshift(ifft2(OTF*F))
        
        return np.abs(blur_img)
     
    #
    def __subpixel_shift(self, img, fraction_shift: tuple, pixel_ratio):
        '''px_shift and py_shift: number of pixels shifted in the x direction and y direction, respectively.
        The image shifts toward the top left corner if the inputs are positive and toward the bottom right corner if the inputs are negative.'''
        assert pixel_ratio >= 1, "The ratio of field image size to sensor image size should be equal to or greater than 1."
        h, w = np.shape(img)
        px, py = fraction_shift
        px, py = round(pixel_ratio * px), round(pixel_ratio * py)
        pad_r = (0, 2*py) if py > 0 else (-2*py, 0)
        pad_c = (0, 2*px) if px > 0 else (-2*px, 0)
        img = np.pad(img, pad_width=(pad_r, pad_c), mode='constant', constant_values=0)

        return img[abs(py): abs(py)+h, abs(px): abs(px)+w]

#
class ImageDataset(Dataset):
    def __init__(self, device, args, debug=False):
        self.device = device
        self.args = args
        self.imageGenerator = iter(ImageGenerator(device=device, args=args))
        self.len_ds = args.batch_size
        self.transform = transforms.Compose([
            transforms.Resize(device.gt_size),
            AddNoise(mean=0.0, sig=args.b_sqrt, clip=True)
        ])
    
    def __len__(self):
        return self.len_ds
    #
    def __getitem__(self, idx=None):
        gt, ssr = next(self.imageGenerator)
        if isinstance(gt, np.ndarray):
            gt = gt.astype(np.float32) / uint16_max
            ssr = ssr.astype(np.float32) / uint16_max
            gt, ssr = map(torch.from_numpy, (gt, ssr))
        if self.transform:
            ssr = self.transform(ssr)
        return gt, ssr
    
    def __inspect_data(self, f, extra_name: str = ""):
        _hw = self.device.lens_diameter * self.device.output_ratio * self.device.total_mag / um / 2
        fig, axes = plt.subplots(nrows=3, ncols=3, sharex=True, sharey=True)
        k = 0
        for i in range(3):
            for j in range(3):
                axes[i,j].imshow(f[k, :, :],
                                 extent=[-_hw, _hw, -_hw, _hw],
                                 vmin=0.0, vmax=1.0, cmap='gray', origin='lower')
                axes[i,j].set_xlabel("x (um)")
                axes[i,j].set_ylabel("y (um)")
                k += 1
        fig.tight_layout()

        check_directory(self.args.savefig_dir)
        fn = "Example_channels_run"+ self.args.run_code + extra_name + ".png"
        plt.savefig(os.path.join(self.args.savefig_dir, fn), dpi=600)

#
class ImageDatasetFromStorage(ImageDataset):
    def __init__(self, device, args):
        super().__init__(device, args)

        self.hdf = None
        self.ds_path = args.dataset
        assert self.ds_path is not None, "Please specify dataset path."
        assert os.path.isfile(self.ds_path), "Indicated dataset not found."
        with h5py.File(self.ds_path, 'r') as f:
            self.len_ds: int = int(len(f['gt']) * args.ds_ratio)
        print(f"Dataset \"{self.ds_path}\" is loaded.", flush=True)

    def __len__(self):
        return self.len_ds
    
    def __getitem__(self, idx):
         # must do it here as hdf object does not allow pickling
        if self.hdf is None:
            self.hdf = h5py.File(self.ds_path, 'r')
        #
        gt, ssr = self.hdf['gt'][idx], self.hdf['ssr'][idx]
        if isinstance(gt, np.ndarray):
            gt = gt.astype(np.float32) / uint16_max
            ssr = ssr.astype(np.float32) / uint16_max
            gt, ssr = map(torch.from_numpy, (gt, ssr))
        if self.transform:
            ssr = self.transform(ssr)
        return gt, ssr

    def __getstate__(self):
        state = self.__dict__.copy()
        state['hdf'] = None # remove the h5 file handle before pickling
        return state
    
    def __setstate__(self, state):
        self.__dict__.update(state)
        self.file = None # reinitialize the h5 file handle to None


# TODO: load experimental test data
class ImageDatasetFromExperiments(ImageDataset):
    def __init__(self, device, args, debug=False):
        pass 
    def __len__(self):
        return self.len_ds
    def __getitem__(self, idx):
        pass

    
























########################################
def store_dataset(device, args):
    '''Generate simulated fluorescence microscopy dataset and save as H5 file'''

    #
    def append_to_hdf5(lock, file_path: str, new_data: tuple, labels: tuple):
        with lock:
            with h5py.File(file_path, 'a') as _f:
                for _i in range(len(new_data)):
                    _ds = _f[labels[_i]]
                    _ds.resize(_ds.shape[0]+1, axis=0)
                    _ds[-1, :, :, :] = new_data[_i]

    #  
    def worker(lock, file_path, start, end):
        for _ in range(start, end):
            gt, ssr = next(imageGenerator)
            append_to_hdf5(lock, file_path=file_path, new_data=(gt, ssr), labels=('gt', 'ssr'))

    #
    def disp_progress(items: dict):
        for _key, _val in items.keys():
            assert _val >= 0.0 and _val <= 1.0, "Invalid progress reading."
            _tot: int = int(20)
            _done: int = int(_tot * _val)
            _undone: int = _tot - _done
            _fb, _eb = u"\u2588", u"\u2581"
            print(f"[Process {_key}: {_fb*_done}{_eb*_undone}]")


    #
    data_dir = args.data_dir
    check_directory(data_dir)
    imageGenerator = iter(ImageGenerator(device=device, args=args))
    _gt, _ssr = next(imageGenerator)
    _ch, _sz_gt, _sz_ssr = _gt.shape[0], _gt.shape[1], _ssr.shape[1]

    # create dataset if not exist
    h5_path = args.dataset
    if h5_path is None:
        h5_path = os.path.join(data_dir, "dataset_"+args.run_code+".h5")
        with h5py.File(h5_path, 'w') as _f:
            _f.create_dataset('gt',
                              shape=(0, _ch, _sz_gt, _sz_gt),
                              maxshape=(None, _ch, _sz_gt, _sz_gt),
                              dtype=np.uint16,
                              chunks=(20, _ch, _sz_gt, _sz_gt))
            _f.create_dataset('ssr',
                              shape=(0, _ch, _sz_ssr, _sz_ssr),
                              maxshape=(None, _ch, _sz_ssr, _sz_ssr),
                              dtype=np.uint16,
                              chunks=(20, _ch, _sz_ssr, _sz_ssr))
    
    # add data
    _lck = multiprocessing.Lock()
    _tot_iter = args.total_images
    _n_proc = 5
    _iter_per_proc = _tot_iter // _n_proc
    
    processes = []
    for _i in range(_n_proc):
        _stt = _i * _iter_per_proc
        _end = _stt + _iter_per_proc
        _p = multiprocessing.Process(target=worker, args=(_lck, h5_path, _stt, _end))
        processes.append(_p)
        _p.start()

    for _p in processes:
        _p.join()
    print("All processes finished writing data to the file.")

    # shuffle data
    print("Shuffling data...")
    with h5py.File(h5_path, 'r') as _f:
        _len = len(_f['gt'])
        idx = np.arange(_len)
        np.random.shuffle(idx)

        new_h5_path = os.path.join(data_dir, "chunked_compressed.h5")
        with h5py.File(new_h5_path, 'w') as _nf:
            gt_dset = _nf.create_dataset('gt',
                                        shape=(_len, _ch, _sz_gt, _sz_gt),
                                        maxshape=(None, _ch, _sz_gt, _sz_gt),
                                        dtype=np.uint16, chunks=(20, _ch, _sz_gt, _sz_gt))    
            ssr_dset = _nf.create_dataset('ssr',
                                         shape=(_len, _ch, _sz_ssr, _sz_ssr),
                                         maxshape=(None, _ch, _sz_ssr, _sz_ssr),
                                         dtype=np.uint16, chunks=(20, _ch, _sz_ssr, _sz_ssr))
            print(f"Shuffled indices: {idx}")
            for _i, _idx in enumerate(idx):
                gt_dset[_i] = _f['gt'][_idx]
                ssr_dset[_i] = _f['ssr'][_idx]
    
    # rename file
    _fn = os.path.join(data_dir, "dataset_"+args.run_code+"_len"+str(_len)+".h5")
    os.rename(new_h5_path, _fn)
    os.remove(h5_path)
    print("Data has been successfully generated with chunking and shuffling.")



#
def main():
    args = parse_args()
    neuralCam = NeuralCam(args)
    store_dataset(neuralCam, args)
    
if __name__ == "__main__":
    main()




































































































































######################### archive #########################
# def store_dataset_special(device, args):
#     '''Generate simulated natural dataset and save as .pth file''' 
#     class SpecialImageGenerator(ImageGenerator):
#         def __init__(self, device, args, dir):
#             super().__init__(device=device, args=args)
#             _data_dir = dir
#             _entries = os.listdir(_data_dir)
#             _dirs = [entry for entry in _entries if os.path.isdir(os.path.join(_data_dir, entry))]
#             self.dirs = iter(_dirs)
#             self.length = len(_dirs) if args.special == "natural" else args.total_images
#             self._f_spacing = 1
        
#         def __len__(self):
#             return self.length
        
#         # overwrite this method to get different types of labels
#         def fetch_labels(self):
#             if self.args.special.lower() == "natural":
#                 print("Creating natural dataset...")
#                 fd = next(self.dirs)
#                 fmap = glob.glob(os.path.join(self.dir, fd, "*.png"))
#             elif self.args.special.lower() == "randomDots":
#                 print("Creating random dot grids...")
#                 assert False, "Not implemented yet."
#             elif self.args.special.lower() == "uniformDots":
#                 print("Creating uniform dot grids...")
#                 assert False, "Not implemented yet."
#             else:
#                 assert False, "Type of patterns not supported."
#             return fmap    
#     #
#     _dir = args.data_dir
#     _specialImageGenerator = SpecialImageGenerator(device=device, args=args, dir=_dir)
#     total_images = len(_specialImageGenerator)
#     speicalImageGenerator = iter(_specialImageGenerator)
#     #
#     _dir = "./training/datasets/"
#     check_directory(_dir)
#     data = []
#     for i in range(total_images):
#         print(f"Item: {i+1}/{total_images}{' ':20}", end='\r')
#         gt, ssr = next(speicalImageGenerator)
#         data.append((gt, ssr))
#     fn = f"dataset_dt{args.run_code}_len{len(data)}_{args.special.lower()}.pth"
#     torch.save(data, os.path.join(_dir, fn))
