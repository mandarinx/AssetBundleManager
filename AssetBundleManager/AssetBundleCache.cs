using UnityEngine;
using System.Collections.Generic;

namespace HyperGames.AssetBundles {
    
    public class AssetBundleCache {
    
        private readonly Dictionary<string, AssetBundle> cache;
    
        public AssetBundleCache() {
            cache = new Dictionary<string, AssetBundle>();
        }
    
        public void Add(string name, AssetBundle bundle) {
            if (cache.ContainsKey(name)) {
                Debug.LogError("Bundle cache already contains a key name "+name);
                return;
            }
            cache.Add(name, bundle);
        }

        public bool TryGetBundle(string name, out AssetBundle bundle) {
            return cache.TryGetValue(name, out bundle);
        }
    }

}
