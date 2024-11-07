import glob
import os
import sys
from torch.utils.data import DataLoader
from algorithm.lineDraw.networks.network_select import select_model
from algorithm.lineDraw.utils.utils import *
from algorithm.lineDraw.utils.data import SpectrumDataset
from algorithm.lineDraw.utils.args import parse_args
from PIL import Image
import io  # 添加这一行来导入 io 模块
import tifffile

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
# Configuration variables from config.sh
SAVE_DIR = os.path.abspath(os.path.join(SCRIPT_DIR, 'training', 'ckpt'))
SAVE_FREQ = 200
LOG_DIR = os.path.abspath(os.path.join(SCRIPT_DIR, 'tb_log'))
CKPT_DIR = os.path.abspath(os.path.join(SCRIPT_DIR, 'training', 'saved_ckpt'))
SAVEFIG_DIR = os.path.abspath(os.path.join(SCRIPT_DIR, 'results'))
G_NETWORK = 'SPUNET'
PRINT_SUMMARY = False
G_LR = 1e-3
TOTAL_STEPS = 1
LR_DECAY_RATE = 0.99
LR_DECAY_PATIENCE = 5
MIN_LR = 3e-5
TV_WEIGHT = 0.2
MSE_WEIGHT = 0.8
SOLVER_CONFIG = os.path.abspath(os.path.join(SCRIPT_DIR, 'data', 'configurations.mat'))
WL_MIN = 400
WL_MAX = 780
WL_NUM = 512
WL_RES = 5
A_POISSON = 0
B_SQRT = 0.01

# Configuration variables from run.sh
INFERENCE_ONLY = True
BATCH_SIZE = 1600

# GPU configuration
prefetch_factor = 16
num_workers = 6
if torch.cuda.is_available():
    gpu_dev = torch.device("cuda")
else:
    gpu_dev = torch.device("cpu")
    num_workers = 1
# Constants
nm = 1e-9
um = 1e-6
mm = 1e-3
m = 1
nsec = 1e-9
usec = 1e-6
msec = 1e-3
sec = 1

def mutilTiffSave(outputimg):
    outPutData = np.ascontiguousarray((outputimg / outputimg.max() * 65535).astype(np.uint16))

    # 保存为标准多页TIFF
    tiff_filename = os.path.join(SAVEFIG_DIR, "mutilTiff.tiff")
    with tifffile.TiffWriter(tiff_filename) as tiff_writer:
        for k in range(outPutData.shape[2]):
            # 单独提取每一页
            tiff_writer.write(outPutData[:, :, k])
    return tiff_filename

def inference(args,tensor_image,G):
    savefig_dir = args.savefig_dir
    check_directory(savefig_dir)
    file_affix = "_run" + args.run_code + ".png"
    #模型加载

    #模型加载结束

    b_size = args.batch_size
    sz = int(np.sqrt(b_size))
    # _ds = SpectrumDataset(solver=spectrumSolve, args=args)
    # dataloader = DataLoader(dataset=_ds, batch_size=b_size, num_workers=0, pin_memory=True,
    #                         prefetch_factor=None)
    # 生成一个随机的单通道Tensor图片，大小为(1, 256, 256)
    # ssr = torch.rand(1600, 200)
    ssr = tensor_image
    # _, ssr = next(iter(dataloader))
    # print(f"输出结果为：{ssr.shape}")
    ssr = to_device(ssr, device=gpu_dev)
    G.eval()
    with torch.no_grad():
        output = G(ssr)
    spectrumSolve = SpectrumSolve(args)
    n_out = spectrumSolve.nn_output
    output = batch_to_cube(output, sizes=(sz, sz, n_out))
    output = to_ndarray(output)
    processed_data = np.round(output[:, :, :76] * 255).astype(int)
    resStr = mutilTiffSave(output[:, :, :76])
    formatted_data = []
    for k in range(processed_data.shape[2]):  # 遍历每个数组 (第3维度)
        inner_array = []
        for i in range(processed_data.shape[0]):  # 遍历每行
            row_data = ','.join(map(str, processed_data[i, :, k]))  # 用逗号分隔列
            inner_array.append(row_data)
        formatted_data.append(';'.join(inner_array))  # 用分号分隔行

    # Step 3: 将所有数组数据连接成一个最终字符串，并用竖线分隔不同的数组
    final_string = '|'.join(formatted_data)
    return final_string,resStr

def load_model():
    args = parse_args()
    args.inference_only = INFERENCE_ONLY
    args.save_dir = SAVE_DIR
    args.log_dir = LOG_DIR
    args.save_freq = SAVE_FREQ
    args.ckpt_dir = CKPT_DIR
    args.savefig_dir = SAVEFIG_DIR
    args.batch_size = BATCH_SIZE
    args.G_network = G_NETWORK
    args.print_summary = PRINT_SUMMARY
    args.a_poisson = A_POISSON
    args.b_sqrt = B_SQRT
    args.solver_config = SOLVER_CONFIG

    spectrumSolve = SpectrumSolve(args)
    G = select_model(args, spectrumSolve)
    ckpt_dir = args.ckpt_dir
    ckpt_files = glob.glob(os.path.join(ckpt_dir, "ckpt_*.pth"))
    ckpt_files.sort(reverse=True)
    assert os.path.isfile(ckpt_files[0]), f"No checkpoint file found in {ckpt_dir}."

    checkpoint = torch.load(ckpt_files[0], map_location=gpu_dev)
    G.load_state_dict(checkpoint['model_state_dict'], strict=False)
    G = to_device(G, device=gpu_dev)
    return G

def chanDeal(image_data,G):
    # 将字节数据转换为PIL图像
    image = Image.open(io.BytesIO(image_data)).convert('L')  # 转为灰度图像

    # 调整图片大小为 (1600, 200)
    resized_image = image.resize((200, 1600))

    # 将图像转换为Tensor
    tensor_image = torch.tensor(list(resized_image.getdata()), dtype=torch.float32).reshape(1600, 200)
    args = parse_args()
    args.inference_only = INFERENCE_ONLY
    args.save_dir = SAVE_DIR
    args.log_dir = LOG_DIR
    args.save_freq = SAVE_FREQ
    args.ckpt_dir = CKPT_DIR
    args.savefig_dir = SAVEFIG_DIR
    args.batch_size = BATCH_SIZE
    args.G_network = G_NETWORK
    args.print_summary = PRINT_SUMMARY
    args.a_poisson = A_POISSON
    args.b_sqrt = B_SQRT
    args.solver_config = SOLVER_CONFIG
    resCSStr = inference(args,tensor_image,G)

    return resCSStr

if __name__ == "__main__":
    Gres = load_model()
    with open(r'E:\csCode\cs\001\csAcq4\csAcq4\bin\Debug\algorithm\lineDraw\1.png', 'rb') as f:
        image_file = f.read()
    res = chanDeal(image_file,Gres)
    print(f"最终结果{res}")