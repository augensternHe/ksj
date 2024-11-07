import os
import glob
import sys
import random
import torch
import torch.utils
from torch.utils.data import DataLoader
import matplotlib.pyplot as plt
from algorithm.enhance.networks.network_select import select_model
from algorithm.enhance.utils.utils import *
from algorithm.enhance.utils.data import ImageDatasetFromStorage
from algorithm.enhance.utils.args import parse_args
import matplotlib
import cv2
matplotlib.use('Agg')  # 设置为非交互式后端，适用于无图形界面的环境
import matplotlib.pyplot as plt
import torchvision.transforms as transforms
from PIL import Image
import io
#
num_workers = 4
prefetch_factor = 8
if torch.cuda.is_available():
    gpu_dev = torch.device("cuda")
else:
    gpu_dev = torch.device("cpu")
    num_workers = 1
print(f"{gpu_dev}".upper() + " is now used for computation.")

#
nm = 1e-9
um = 1e-6
mm = 1e-3
nsec = 1e-9
usec = 1e-6
msec = 1e-3
sec = 1
current_dir = os.path.dirname(os.path.abspath(__file__))
args = parse_args()
args.inference_only = True
args.ckpt_dir = os.path.join(current_dir, "training", "saved_ckpt")
args.savefig_dir = os.path.join(current_dir, "results")
args.print_summary = True
args.fpa_dir = os.path.join(current_dir, "data", "fpa9ch_zhangChi_20240607", "Ag20nmFPA.csv")
args.vignette_fields = [0.0, 0.5, 1.0, 1.25]
args.vignette_values = [1.0, 1.0, 1.0, 1.0]
args.fluoro_db = os.path.join(current_dir, "data", "lib_fluorophores", "fluoro_db.mat")
args.fluoros_list = ["DAPI_em", "FITC_em", "TRITC_em", "mRFP1_em", "alexa_fluor_647_em"]
args.b_sqrt = 0.01
args.dataset = os.path.join(current_dir, "training", "datasets", "test_ds", "dataset_test_len50.h5")
args.batch_size = 8

savefig_dir = args.savefig_dir
#
def worker_init_fn(id):
    worker_seed = torch.initial_seed() + id
    if worker_seed > 2 ** 30:
        worker_seed = worker_seed % 2 ** 30
    np.random.seed(worker_seed)
    random.seed(worker_seed)
    torch.manual_seed(worker_seed)


#
def G_fwd(input, gt, G, args):
    snr = torch.tensor(1 / args.b_sqrt, device=gpu_dev, requires_grad=False)
    G1, G2 = G
    psfs = G1()  # (1, 9, 100, 100)
    deconv = wiener_deconv(input, psfs, snr=snr)  # (B, 9, 500, 500)
    deconv = center_crop(deconv, sz=480)  # (B, 9, 480, 480)

    output = G2(deconv)  # (B, 9, 480, 480)
    output, gt = map(center_crop, (output, gt))  # (B, 9, 400, 400)

    return output, gt, psfs, deconv

def process_image_list(image_list, target_size=(500, 500)):
    images = []
    transform = transforms.Compose([
        transforms.Resize(target_size),
        transforms.ToTensor()  # 转为 PyTorch Tensor
    ])
    for img in image_list:
        # 如果 img 是 bytes 类型，转换为 PIL Image 对象
        if isinstance(img, bytes):
            img = Image.open(io.BytesIO(img))
        img = img.convert('L')  # 转为单通道灰度图
        img = transform(img)  # 应用 resize 和 tensor 转换
        images.append(img)
    return torch.cat(images, dim=0)  # 将9张图片沿通道维度拼接 (9, H, W)

def inference(G1, G2, image_list):

    check_directory(savefig_dir)
    file_affix = "_run" + args.run_code + ".png"
    # 检查 image_list 是否包含9张图片
    assert len(image_list) == 9, "需要提供9张图片进行推理"

    # 处理输入图片 (9, 500, 500)
    ssr = process_image_list(image_list, target_size=(500, 500))
    ssr = ssr.unsqueeze(0)  # 增加 batch 维度 (1, 9, 500, 500)

    # 设置模型为评估模式
    G1.eval()
    G2.eval()
    with torch.no_grad():
        # 传递输入到模型进行推理
        output, gt, _, _ = G_fwd(input=ssr, gt=ssr, G=(G1, G2), args=args)

    # 导出结果
    # for idx in range(1, len(image_list) + 1):
    #     _gt, _output = map(make_collage, (gt, output))
    #     pkg = {"ground_truth": _gt, "recovered": _output}
    #     print(f"Exporting {idx}/{len(image_list)}")
    #     for _key, _img in pkg.items():
    #         plt.figure(figsize=(6, 4))
    #         plt.imshow(_img, cmap='gray')  # 灰度图
    #         plt.savefig(os.path.join(savefig_dir, f"b{str(idx)}_{_key}_run" + file_affix), dpi=600)

    gtRes = make_collage(gt)
    imgres = gtRes[0]
    if not os.path.exists(savefig_dir):
        os.makedirs(savefig_dir)
    savePath = os.path.join(savefig_dir, 'image.png')
    plt.figure(figsize=(imgres.shape[1] / 100, imgres.shape[0] / 100))  # Set figure size based on image dimensions

    plt.subplots_adjust(left=0, right=1, top=1, bottom=0)
    plt.imshow(imgres, cmap='gray')  # Assuming grayscale image
    plt.axis('off')
    plt.savefig(os.path.join(savePath), dpi=600, bbox_inches='tight', pad_inches=0)
    plt.close()

    return savePath

def load_model():

    neuralCam = NeuralCam(args)
    print(neuralCam)
    # model and training
    G1, G2 = select_model('srcnn', neuralCam), select_model('srunet', neuralCam)

    ckpt_dir = args.ckpt_dir
    ckpt_files = glob.glob(os.path.join(ckpt_dir, "ckpt*.pth"))
    ckpt_files.sort(reverse=True)
    assert os.path.isfile(ckpt_files[0]), f"No checkpoint file found in {ckpt_dir}."

    checkpoint = torch.load(ckpt_files[0], weights_only=True, map_location=gpu_dev)
    G1.load_state_dict(checkpoint['model_1_state_dict'], strict=False)
    G2.load_state_dict(checkpoint['model_2_state_dict'], strict=False)
    print(f"Checkpoint file \"{ckpt_files[0]}\" has been loaded.", flush=True)
    G1, G2 = to_device(G1, G2, device=gpu_dev)
    return G1,G2


if __name__ == "__main__":
    print(f"python {sys.version} on {sys.platform}")
    ga1,ga2 = load_model()
    # 假设 imageList 包含九张已加载的 PIL 图像
    imageList = [Image.open(f"pic9/image_{i}.png") for i in range(1, 10)]

    # 调用 inference 函数
    inference(ga1, ga2, imageList)
