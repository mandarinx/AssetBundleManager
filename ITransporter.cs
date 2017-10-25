using System.Collections;

namespace HyperGames.AssetBundles {

    public interface ITransporter {
        IEnumerator Load(BundleLoadOperation op, int streamIndex, string path);
    }
}
