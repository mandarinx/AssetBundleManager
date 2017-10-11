using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace HyperGames.AssetBundles {

    public class AssetBundleManager {

        private readonly BundleLoader[]                      loaders;
        private readonly AssetBundleCache                    cache;
        private readonly ObjectPool<BundleLoadOperation>     loadOps;
        private readonly BundleManagerUpdate                 updater;
        private AssetBundleManifest                          manifest;
        private Action<string[]>                             onManifestLoaded = str => { };
        private Action<List<string>>                         onLoadBundles = str => { };
        
        public AssetBundleManager(AssetBundleConfig cfg, GameObject owner) {
            cache = new AssetBundleCache();
            loaders = new BundleLoader[cfg.numBundleLoaders];
            
            updater = owner.AddComponent<BundleManagerUpdate>();
            updater.Init(this);
            
            loadOps = new ObjectPool<BundleLoadOperation>(16, true);
            loadOps.Fill();

            for (int i = 0; i < loaders.Length; ++i) {
                loaders[i] = owner.AddComponent<BundleLoader>();
                loaders[i].Init(cfg);
            }

            // check cfg.bundleTarget and start local server if necessary.
        }

        public void AddVariantsResolver(VariantsResolver resolver) {
            onManifestLoaded = resolver.RegisterBundles;
            onLoadBundles = resolver.RemapVariants;
        }

        public void Update() {
            if (loadOps.numSpawned == 0) {
                updater.Deactivate();
                return;
            }
            StartNextLoadOperation();
        }

        public IEnumerator LoadMasterManifest() {
            BundleLoadOperation loadOp;
            loadOps.Spawn(out loadOp);

            loadOp.Init(BundlesHelper.GetPlatformName());
            
            BundleLoader loader;
            if (!TryGetBundleLoader(out loader)) {
                yield break;
            }
            yield return loadOp.Load(loader);
            
            cache.Add(loadOp.loadedBundle.name, loadOp.loadedBundle.bundle);
            OnManifestLoaded(loadOp);
        }

        public void LoadMasterManifest(Action callback) {
            if (manifest != null) {
                Log("Manifest is already loaded");
                return;
            }
            
            Action<BundleLoadOperation> opCallback = op => {
                OnManifestLoaded(op);
                callback();
            };
            
            Log("Load Manifest for "+BundlesHelper.GetPlatformName());
            BundleLoadOperation loadOp;
            loadOps.Spawn(out loadOp);
            Log("Got LoadOp "+(loadOps.numSpawned - 1));
            loadOp.Init(opCallback, OnBundleLoaded, BundlesHelper.GetPlatformName());
            
            updater.Activate();
        }

        public IEnumerator LoadBundle(string bundleName) {
            if (manifest == null) {
                LogError(
                    "Cannot load bundle "+bundleName+". "+
                    "Master manifest is not loaded. "+
                    "Call AssetBundleManager.Init() to resolve the issue.");
                yield break;
            }

            // check bundle cache

            List<string> bundleNames = new List<string>();
            bundleNames.AddRange(manifest.GetAllDependencies(bundleName));
            bundleNames.Add(bundleName);

            onLoadBundles(bundleNames);

            BundleLoadOperation loadOp;
            loadOps.Spawn(out loadOp);
            loadOp.Init(bundleNames);
            
            while (loadOp.curBundle < loadOp.numBundles) {
                BundleLoader loader;
                if (!TryGetBundleLoader(out loader)) {
                    yield break;
                }
                yield return loadOp.Load(loader);
                BundleLoadOperation.LoadedBundle loaded = loadOp.loadedBundle;
                cache.Add(loaded.name, loaded.bundle);
            }
            loadOps.Despawn(loadOp);
        }

        public void LoadBundle(string bundleName, Action<string, AssetBundleManager> callback) {
            if (manifest == null) {
                LogError(
                    "Cannot load bundle "+bundleName+". "+
                    "Master manifest is not loaded. "+
                    "Call AssetBundleManager.Init() to resolve the issue.");
                return;
            }

            // check bundle cache
            
            List<string> bundleNames = new List<string>();
            bundleNames.AddRange(manifest.GetAllDependencies(bundleName));
            bundleNames.Add(bundleName);
            
            onLoadBundles(bundleNames);
            
            Action<BundleLoadOperation> opCallback = op => {
                OnLoadOperationDone(op);
                callback(bundleName, this);
            };
            
            BundleLoadOperation loadOp;
            loadOps.Spawn(out loadOp);
            loadOp.Init(opCallback, OnBundleLoaded, bundleNames);
            
            updater.Activate();
        }

        public T GetAsset<T>(string bundleName, string assetName) where T : UnityEngine.Object {
            AssetBundle bundle;
            return cache.TryGetBundle(bundleName, out bundle)
                ? bundle.LoadAsset<T>(assetName)
                : default(T);
        }
            
        private void StartNextLoadOperation() {
            Log("Start next loadop");
            for (int i=0; i<loaders.Length; ++i) {
                BundleLoader loader = loaders[i];
                if (loader.isLoading) {
                    continue;
                }
                
                Log("Got loader "+i);
                BundleLoadOperation op = null;
                if (TryGetLoadOperation(out op)) {
                    op.LoadAsync(loader);
                }
            }
        }

        private bool TryGetBundleLoader(out BundleLoader loader) {
            for (int i=0; i<loaders.Length; ++i) {
                if (loaders[i].isLoading) {
                    continue;
                }
                loader = loaders[i];
                return true;
            }
            loader = null;
            return false;
        }
            
        private bool TryGetLoadOperation(out BundleLoadOperation op) {
            for (int i=0; i<loadOps.numSpawned; ++i) {
                op = loadOps.GetInstance(i);
                Log("loadop "+i+" done: "+op.done+" active: "+op.active);
                if (op.active) {
                    return true;
                }
            }
            op = null;
            return false;
        }

        private void OnBundleLoaded(BundleLoadOperation.LoadedBundle loaded) {
            Log("OnBundleLoaded : Add "+loaded.name+" to cache");
            cache.Add(loaded.name, loaded.bundle);
        }
            
        private void OnLoadOperationDone(BundleLoadOperation op) {
            Log("OnLoadOperationDone : Despawn LoadOp");
            loadOps.Despawn(op);
        }

        private void OnManifestLoaded(BundleLoadOperation op) {
            Debug.Log("OnManifestLoaded");
            loadOps.Despawn(op);
            AssetBundle bundle;
            cache.TryGetBundle(op.loadedBundle.name, out bundle);
            manifest = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            Debug.Log("Manifest: "+manifest);

            onManifestLoaded(manifest.GetAllAssetBundlesWithVariant());
        }

        [Conditional("UNITY_EDITOR")]
        private static void Log(string msg) {
            Debug.Log(msg);
        }

        [Conditional("UNITY_EDITOR")]
        private static void LogError(string msg) {
            Debug.LogError(msg);
        }

        [Conditional("UNITY_EDITOR")]
        private static void LogWarning(string msg) {
            Debug.LogWarning(msg);
        }
    }

}
