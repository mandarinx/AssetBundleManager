using UnityEngine;

namespace HyperGames.AssetBundles {

    public interface ILoadOpHandler {
        void OnBundleFailed(int streamIndex, int retries);
        void OnBundleLoaded(string bundleName, int streamIndex, AssetBundle bundle);
        void OnLoadOpComplete(BundleLoadOperation op);
        void OnLoadOpFailed(BundleLoadOperation op);
    }
}
