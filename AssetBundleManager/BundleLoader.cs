using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace HyperGames.AssetBundles {

    public interface ITransporter {
        bool            error          { get; }
        string          errorMessage   { get; }
        float           progress       { get; }
        IEnumerator     Load(string path);
        AssetBundle     GetBundle();
    }

    public class TransportFromWeb : ITransporter {
        
        private UnityWebRequest req;

        public bool         error          { get; private set; }
        public string       errorMessage   { get; private set; }

        public float progress {
            get { return req != null ? req.downloadProgress : 0f; }
        }
    
        public IEnumerator Load(string path) {
            req = UnityWebRequest.GetAssetBundle(path);
            yield return req.Send();
     
            if (req.isHttpError || req.isNetworkError) {
                error = true;
                errorMessage = req.error;
            }
        }

        public AssetBundle GetBundle() {
            return DownloadHandlerAssetBundle.GetContent(req);
        }
    }
    
    public class TransportFromFile : ITransporter {

        private AssetBundleCreateRequest req;

        public bool        error          { get; private set; }
        public string      errorMessage   { get; private set; }

        public float progress {
            get { return req != null ? req.progress : 0f; }
        }

        public IEnumerator Load(string path) {
            req = AssetBundle.LoadFromFileAsync(path);
            yield return req;
    
            if (req.assetBundle == null) {
                error = true;
                errorMessage = "Could not load asset bundle from "+path;
            }
        }

        public AssetBundle GetBundle() {
            return req.assetBundle;
        }
    }

    public interface IBundleLoaderDone {
        void OnBundleLoaderDone(string bundleName, AssetBundle bundle);
    }
    
    public class BundleLoader : MonoBehaviour {
        public bool                 isLoading { get; private set; }
        public float                progress  { get; private set; }

        private ITransporter        transporter;
        private string              basePath;
        private BundleLoadOperation operation;
        private IBundleLoaderDone   onLoaderDone;

        public void Init(AssetBundleConfig config) {
            basePath = BundlesHelper.GetPath(config, BundlesHelper.GetPlatformName());
            transporter = BundlesHelper.GetTransporter(config);
        }
        
        public void Load(string bundleName, IBundleLoaderDone onDone) {
            isLoading = true;
            onLoaderDone = onDone;
            progress = 0f;
            StartCoroutine(StartLoader(basePath, bundleName));
        }
        
        public IEnumerator Load(string bundleName) {
            isLoading = true;
            progress = 0f;
            onLoaderDone = null;
            yield return StartLoader(basePath, bundleName);
        }

        private void Update() {
            if (!isLoading) {
                return;
            }

            progress = transporter.progress;
        }

        private IEnumerator StartLoader(string path, string bundleName) {
            Debug.Log("[Loader] Start loading using "+transporter.GetType()+" full path: "+path+bundleName);
            yield return transporter.Load(path + bundleName);
            Debug.Log("[Loader] Done loading using "+transporter.GetType());
            
            isLoading = false;
            if (onLoaderDone != null) {
                onLoaderDone.OnBundleLoaderDone(bundleName, transporter.GetBundle());
            }
        }

        public AssetBundle GetBundle() {
            return transporter.GetBundle();
        }
    }

    public class BundleLoadOperation : IBundleLoaderDone, IPoolable {

        public struct LoadedBundle {
            public string      name;
            public AssetBundle bundle;
        }
        
        private string[]                    bundleNames;
        private int                         loadedBundles;
        private Action<BundleLoadOperation> onOperationDone;
        private Action<LoadedBundle>        onBundleLoaded;
        
        public int                          curBundle { get; private set; }
        public LoadedBundle                 loadedBundle    { get; private set; }

        public int numBundles {
            get { return bundleNames.Length; }
        }

        public bool done {
            get { return loadedBundles >= bundleNames.Length; }
        }

        public bool active {
            get { return bundleNames.Length > 0 && curBundle < bundleNames.Length; }
        }

        public void Init(List<string> p_bundleNames) {
            curBundle = 0;
            loadedBundles = 0;
            bundleNames = new string[p_bundleNames.Count];
            
            for (int i = 0; i < bundleNames.Length; ++i) {
                bundleNames[i] = p_bundleNames[i];
            }
        }

        public void Init(string bundleName) {
            curBundle = 0;
            loadedBundles = 0;
            bundleNames = new string[1] { bundleName };
        }
        
        public void Init(Action<BundleLoadOperation> callback, Action<LoadedBundle> bundleCallback, List<string> p_bundleNames) {
            onOperationDone = callback;
            onBundleLoaded = bundleCallback;
            curBundle = 0;
            loadedBundles = 0;
            bundleNames = new string[p_bundleNames.Count];
            
            for (int i = 0; i < bundleNames.Length; ++i) {
                bundleNames[i] = p_bundleNames[i];
            }
        }

        public void Init(Action<BundleLoadOperation> doneCallback, Action<LoadedBundle> bundleCallback, string bundleName) {
            onOperationDone = doneCallback;
            onBundleLoaded = bundleCallback;
            curBundle = 0;
            loadedBundles = 0;
            bundleNames = new string[1] { bundleName };
        }

        public IEnumerator Load(BundleLoader loader) {
            Debug.Log("[LoadOp] Start loading routine");
            int cur = curBundle;
            ++curBundle;
            yield return loader.Load(bundleNames[cur]);

            loadedBundle = new LoadedBundle {
                name = bundleNames[cur],
                bundle = loader.GetBundle()
            };
            Debug.Log("[LoadOp] Got bundle? "+loadedBundle.name);
        }

        public void LoadAsync(BundleLoader loader) {
            Debug.Log("[LoadOp] Send "+bundleNames[curBundle]+" to BundleLoader");
            loader.Load(bundleNames[curBundle], this);
            ++curBundle;
        }

        public void OnBundleLoaderDone(string bundleName, AssetBundle bundle) {
            Debug.Log("[LoadOp] Bundle "+bundleName+" loaded");
            loadedBundle = new LoadedBundle {
                name = bundleName,
                bundle = bundle
            };
            
            onBundleLoaded(loadedBundle);
            
            ++loadedBundles;
            if (!done) {
                Debug.Log("[LoadOp] Load operation is NOT done");
                return;
            }
            Debug.Log("[LoadOp] Load operation is done");
            onOperationDone(this);
        }

        public void OnEnable() {}

        public void OnDisable() {}

        public void OnDestroy() {}
    }
}
