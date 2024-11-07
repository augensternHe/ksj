# Argument parameters file
import argparse
import numpy as np
from datetime import datetime

def parse_args():
    def str2bool(v):
        assert v == "True" or v == "False"
        return v.lower() in ("true")

    def none_or_str(value):
        if value.lower() == "none":
            return None
        return value

    current_dt = datetime.now().strftime("%Y%m%d%H%M")

    parser = argparse.ArgumentParser(
        description="Parameter settings for neural-network super-resolution compound optics."
    )

    parser.add_argument("--inference_only", type=str2bool, default=False, help="For inference only")

    # Saving and logging arguments
    parser.add_argument("--save_dir", type=str, default="./training/ckpt/", help="Directory for saving checkpoints and TensorBoard sessions")
    parser.add_argument("--save_freq", type=int, default=1000, help="Step interval in model saving")
    parser.add_argument("--log_dir", type=str, default="./tb_log/", help="Directory for saving checkpoints and TensorBoard sessions")
    parser.add_argument("--ckpt_dir", type=none_or_str, default="None", help="Directory for checkpoints to load")
    parser.add_argument("--savefig_dir", type=none_or_str, default="./results/", help="Directory for inference results")
    parser.add_argument("--run_code", type=str, default=current_dt, help="Code for current running checkpoint files. Default: current date and time")

    # Physics configuration
    parser.add_argument("--fpa_dir", type=str, default="./data/fpa9ch_zhangChi_20240607/Ag20nmFPA.csv", help="Directory for FPA transmittance data")
    parser.add_argument("--wl_min", type=float, default=400, help="Minimum wavelength. Unit: nm. Default: 400")
    parser.add_argument("--wl_max", type=float, default=720, help="Maximum wavelength. Unit: nm. Default: 780")
    parser.add_argument("--wl_num", type=int, default=256, help="Number of wavelength points. Default: 256")
    parser.add_argument('--a_poisson', type=float, default=0, help='Poisson noise component')
    parser.add_argument('--b_sqrt', type=float, default=0, help='Gaussian noise standard deviation')
    parser.add_argument("--vignette_fields", type=str, default="0.0, 1.0", help="List of fields (0.0-1.0) for vignetting")
    parser.add_argument("--vignette_values", type=str, default="1.0, 0.1", help="List of vignetting values (0.0-1.0) for the corresponding fields")

    # Training
    parser.add_argument("--total_steps", type=int, default=5000, help="Total number of optimization cycles")
    parser.add_argument("--batch_size", type=int, default=4, help="Training batch size. Default: 10")
    parser.add_argument("--G_lr", type=float, default=1e-4, help="Learning rate")
    parser.add_argument("--G_beta1", type=float,default=0.9,help="Beta1 term for Adam optimizer")
    parser.add_argument("--print_summary", type=str2bool, default=True, help="Print network summary. Default: true")
    parser.add_argument("--lr_decay_rate", type=float, default=0.96, help="Learning rate reduction multiplier at every learning rate decay. Default: 0.96")
    parser.add_argument("--lr_decay_patience", type=int, default=5, help="Steps waited before learning rate reduces. Default: 5")
    parser.add_argument("--min_lr", type=float, default=3e-5, help="Minimum learning rate. Default: 3e-5")
    parser.add_argument("--reset_opt", type=str2bool, default=True, help="Always reset optimizer")
    parser.add_argument("--mse_weight", type=float, default=0.5, help="Mean-square-error loss weight. Default: 0.5")
    parser.add_argument("--tv_weight", type=float, default=0.0, help="Total-variation loss weight. Default: 0.0")
    parser.add_argument("--ssim_weight", type=float, default=0.5, help="Total-variation loss weight. Default: 0.0")
    parser.add_argument("--ps_weight", type=float, default=1e6, help="Point-spread loss weight. Default: 1e6")

    # Samples
    parser.add_argument("--fluoro_db", type=str, default="./data/lib_fluorophores/fluoro_db.mat", help="Directory for fluorophore database")
    parser.add_argument("--fluoros_list", type=none_or_str, default="DAPI_em,FITC_em,TRITC_em,mRFP1_em,alexa_fluor_647_em", help="List of fluorophores")

    # Dataset
    parser.add_argument("--total_images", type=int, default=200, help="Total number of (gt, sensor)-pair generated")
    parser.add_argument("--data_dir", type=none_or_str, default=None, help="Directory of source images for dataset creation.")
    parser.add_argument("--dataset", type=none_or_str, default=None, help="Path of the dataset to be extended. If None, a new dataset will be created.")
    parser.add_argument("--ds_ratio", type=float, default=1.0, help="Portion of dataset used for faster training. Default: 1.0 (all)")

    
    #
    args = parser.parse_args()
    
    assert args.total_steps > 0, "Training steps must be greater than 0."
    assert args.lr_decay_rate > 0 and args.lr_decay_rate < 1, "Learning rate decay steps must be greater than 0.0 and less than 1.0."
    assert args.lr_decay_patience > 0, "Learning rate decay patience must be greater than 0."

    args.vignette_fields = np.array([float(w) for w in args.vignette_fields.split(',')])
    args.vignette_values = np.array([float(w) for w in args.vignette_values.split(',')])

    args.fluoros_list = args.fluoros_list.split(',')
    
    assert args.total_images > 0, "Total number of generated images must be greater than 0."
    assert args.ds_ratio > 0.0 and args.ds_ratio <= 1.0, "Portion of dataset must within (0.0, 1.0]."

    if args.print_summary:
        for arg in vars(args):
            print(f"{arg}: {getattr(args, arg)}")
    
    return args