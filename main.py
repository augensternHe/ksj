from lineDraw import channelDeal
from enhance import picDefini
import sys
from PIL import Image

def picDefiniLoad():
    ga1, ga2 = picDefini.load_model()
    return ga1,ga2

def picDefinInference(ga1, ga2, imageList):
    # 调用 inference 函数
    picDefini.inference(ga1, ga2, imageList)




if __name__ == "__main__":
    ga1, ga2 = picDefiniLoad()
    print(f"python {sys.version} on {sys.platform}")
    # 假设 imageList 包含九张已加载的 PIL 图像
    imageList = [Image.open(f"pic9/image_{i}.png") for i in range(1, 10)]

    picDefinInference(ga1, ga2, imageList)