from algorithm.enhance.networks.networks import *

def select_model(model, device=None):

    if model.lower() == 'srcnn':
        model = SrCNN(device=device)
    elif model.lower() == 'srunet':
        model = SrUNet(device=device)
    else:
        model = None
        assert False, "Network structure not supported."

    return model
