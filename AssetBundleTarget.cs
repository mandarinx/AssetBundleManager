using System;

namespace HyperGames.AssetBundles {
    
    [Serializable]
    public enum AssetBundleTarget {
        ASSET_BUNDLE_FOLDER = 1,
        STREAMING_ASSETS = 2,
        LOCAL_SERVER = 3,
        REMOTE_SERVER = 4,
        OBB = 5,
        ON_DEMAND_RESOURCES = 6,
        APP_SLICING = 7,
    }
}
