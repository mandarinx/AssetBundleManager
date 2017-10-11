using System.Collections;
using UnityEngine;

namespace HyperGames.AssetBundles {
    
    public interface IBundleLoader {
        AssetBundle bundle { get; }
        bool error { get; }
        string errorMessage { get; }
        IEnumerator Load(string path);
    }

}
