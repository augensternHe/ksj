from algorithm.lineDraw.networks.networks import SpUNet


def select_model(args, solver):
    if args.G_network.lower() == 'spcnn':
        pass
    elif args.G_network.lower() == 'spunet':
        model = SpUNet(solver)
    else:
        model = None
        assert False, "Network type not supported."
    
    if args.print_summary: print(model)

    return model