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
        description="Parameter settings for end-to-end optimization of very-dispersive neural meta-optics"
    )

    parser.add_argument("--inference_only", type=str2bool, default=False, help="Inference mode")

    # Saving and logging arguments
    parser.add_argument("--save_dir", type=str, default="./training/ckpt/", help="Directory for saving checkpoints")
    parser.add_argument("--save_freq", type=int, default=1000, help="Interval to save model")
    parser.add_argument("--log_dir", type=str, default="./tb_log/", help="Directory for TensorBoard file")
    parser.add_argument("--ckpt_dir", type=none_or_str, default="None", help="Restoring from a checkpoint")
    parser.add_argument("--savefig_dir", type=none_or_str, default="./results/", help="Directory for inference results")
    parser.add_argument("--run_code", type=str, default=current_dt, help="Code for current running checkpoint files. Default: current date and time")

    # Physics
    parser.add_argument("--solver_config", type=str, default="./data/configurations.mat", help="Configuration file for the solver")
    parser.add_argument("--wl_min", type=float, default=400, help="Minimum wavelength. Unit: nm. Default: 400")
    parser.add_argument("--wl_max", type=float, default=780, help="Maximum wavelength. Unit: nm. Default: 780")
    parser.add_argument("--wl_num", type=int, default=512, help="Number of wavelength points. 2^n (n>8) is recommended. Default: 512")
    parser.add_argument("--wl_res", type=float, default=5.0, help="Reconstructed spectral resolution. Default: 5 nm")
    parser.add_argument('--a_poisson', type=float, default=0, help='Poisson noise component')
    parser.add_argument('--b_sqrt', type=float, default=0, help='Gaussian noise standard deviation')

    # Training
    parser.add_argument("--total_steps", type=int, default=5000, help="Total number of optimization epochs")
    parser.add_argument("--batch_size", type=int, default=10, help="Training batch size. Default: 20")
    parser.add_argument("--G_lr", type=float, default=1e-4, help="Learning rate for training")
    parser.add_argument("--G_beta1", type=float,default=0.9,help="Beta1 term for Adam optimizer")
    parser.add_argument("--G_network", type=str, default="spunet", help="Select reconstruction network")
    parser.add_argument("--print_summary", type=str2bool, default=False, help="Print summary of the neural network")
    parser.add_argument("--lr_decay_rate", type=float, default=0.96, help="Learning rate reduction multiplier at every learning rate decay")
    parser.add_argument("--lr_decay_patience", type=int, default=20, help="Steps waited before learning rate reduces. Default: 20")
    parser.add_argument("--min_lr", type=float, default=3e-5, help="Minimum learning rate. Default: 3e-5")
    parser.add_argument("--reset_opt", type=str2bool, default=True, help="Always reset optimizer")
    parser.add_argument("--tv_weight", type=float, default=0.5, help="Total-variation loss weight. Default: 0.5")
    parser.add_argument("--mse_weight", type=float, default=0.5, help="Mean-square-error loss weight. Default: 0.5")

    # Dataset
    parser.add_argument("--total_spectra", type=int, default=200, help="Total number of (gt, sensor)-pair generated")
    parser.add_argument("--data_dir", type=none_or_str, default="./training/datasets/", help="Directory of datasets.")

    args = parser.parse_args()

    # condition check
    assert args.total_steps > 0, "Training steps must be greater than 0."
    assert args.lr_decay_rate > 0 and args.lr_decay_rate < 1, "Learning rate decay steps must be greater than 0.0 and less than 1.0."
    assert args.lr_decay_patience > 0, "Learning rate decay patience must be greater than 0."

    if args.print_summary:
        print(args) 
    
    return args