using System.Collections.Generic;
using UnityEngine;

namespace HyperGames.AssetBundles {

    public class MultiBundleLoad : CustomYieldInstruction {

        private readonly AssetBundleLoadStatus[] loadStatuses;
        
        public override bool keepWaiting {
            get { return progress < 1f; }
        }

        public float progress {
            get {
                float p = 0f;
                for (int i = 0; i < loadStatuses.Length; ++i) {
                    p += loadStatuses[i].progress;
                }
                return p;
            }
        }

        public MultiBundleLoad(AssetBundleManager manager, string[] bundles) {
            List<string> uniqueBundles = new List<string>();

            for (int i = 0; i < bundles.Length; ++i) {
                string[] nameParts = bundles[i].Split('.');
                if (!uniqueBundles.Contains(nameParts[0])) {
                    uniqueBundles.Add(nameParts[0]);
                }
            }
            
            loadStatuses = new AssetBundleLoadStatus[uniqueBundles.Count];

            Debug.Log("[[[MultiBundleLoad]]]");
            
            for (int i = 0; i < uniqueBundles.Count; ++i) {
                Debug.Log(uniqueBundles[i]);
            }
            
            for (int i = 0; i < uniqueBundles.Count; ++i) {
                loadStatuses[i] = manager.LoadBundle(uniqueBundles[i]);
            }
        }
    }
}
