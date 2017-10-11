using System;
using UnityEngine;
using System.Collections;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace HyperGames.AssetBundles {
    
//    public class BundleLoadingHandler : MonoBehaviour {
//        
//        public bool isLoading { get; private set; }
//
//        #if UNITY_EDITOR
//        [SerializeField]
//        private string loaderName;
//
//        public void SetName(string newName) {
//            loaderName = newName;
//        }
//        #endif
//
//        private string assetBundleName;
//        private Action<string, AssetBundle> callback = (n, b) => { };
//        
//        public void AddLoadingCallback(Action<string, AssetBundle> cb) {
//            callback = cb;
//        }
//
//        public void Load(AssetBundleConfig cfg, string bundleName) {
//            if (isLoading) {
//                #if UNITY_EDITOR
//                LogWarning("BundleLoadingHandler "+loaderName+" is currently loading a bundle. "
//                    +"Cannot start a new load operation.");
//                #endif
//                return;
//            }
//            isLoading = true;
//            StartCoroutine(LoadBundle(cfg, bundleName));
//        }
//
//        private IEnumerator LoadBundle(AssetBundleConfig cfg, string bundleName) {
//            assetBundleName = bundleName;
//            string path = BundlesHelper.GetPath(cfg, BundlesHelper.GetPlatformName()) + bundleName;
//
//            IBundleLoader loader = BundlesHelper.GetLoader(cfg);
//
//            if (loader == null) {
//                LogError(
//                    "Could not get AssetBundleJob operation using AssetBundleTarget option. "+
//                    "AssetBundleConfig.bundleTarget value is "
//                    +Enum.GetName(typeof(AssetBundleTarget), cfg.bundleTarget));
//                yield break;
//            }
//
//            yield return loader.Load(path);
//
//            if (loader.error) {
//                LogError(loader.errorMessage);
//                yield break;
//            }
//
//            isLoading = false;
//            callback(assetBundleName, loader.bundle);
//        }
//
//        [Conditional("UNITY_EDITOR")]
//        private static void LogError(string msg) {
//            Debug.LogError(msg);
//        }
//
//        [Conditional("UNITY_EDITOR")]
//        private static void LogWarning(string msg) {
//            Debug.LogWarning(msg);
//        }
//    }

}
