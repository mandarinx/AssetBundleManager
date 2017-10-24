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

        public MultiBundleLoad(AssetBundleManager manager, VariantsResolver variants, string[] bundles) {
            
            Debug.Log("[MBL] Load multiple bundles");
//            for (int i = 0; i < bundles.Length; ++i) {
//                Debug.Log("[MBL] Bundle: "+bundles[i]);
//            }
            
            variants.RemapVariants(bundles);
            
            List<string> uniqueBundles = new List<string>();
            
            for (int i = 0; i < bundles.Length; ++i) {
                if (!uniqueBundles.Contains(bundles[i])) {
                    uniqueBundles.Add(bundles[i]);
                }
            }

            for (int i = 0; i < uniqueBundles.Count; ++i) {
                Debug.Log("[MBL] Bundle: "+uniqueBundles[i]);
            }

            loadStatuses = new AssetBundleLoadStatus[uniqueBundles.Count];
            
            for (int i = 0; i < uniqueBundles.Count; ++i) {
                loadStatuses[i] = manager.LoadBundle(uniqueBundles[i]);
            }

            //--

//
//            
//            loadStatuses = new AssetBundleLoadStatus[uniqueBundles.Count];
//
//            Debug.Log("[[[MultiBundleLoad]]]");
//            
//            for (int i = 0; i < uniqueBundles.Count; ++i) {
//                Debug.Log(uniqueBundles[i]);
//            }
//            
//            for (int i = 0; i < uniqueBundles.Count; ++i) {
//                loadStatuses[i] = manager.LoadBundle(uniqueBundles[i]);
//            }
        }
    }
}
