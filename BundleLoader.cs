using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace HyperGames.AssetBundles {

    public interface ITransporter {
        IEnumerator Load(BundleLoadOperation op, int streamIndex, string path);
    }

    public class TransportFromWeb : ITransporter {
    
        public IEnumerator Load(BundleLoadOperation op, int streamIndex, string path) {
            string bundleName = op.nextBundle;
            Debug.Log("[Transporter] Load "+bundleName);
            UnityWebRequest request = UnityWebRequest.GetAssetBundle(path + bundleName);
            request.Send();
            
            while (!request.isDone) {
                op.SetCurrentBundleProgress(request.downloadProgress);

                if (request.isHttpError || request.isNetworkError) {
                    op.BundleFailed(request.error, streamIndex);
                    yield break;
                }

                yield return null;
            }
            
            op.BundleLoaded(bundleName, DownloadHandlerAssetBundle.GetContent(request), streamIndex);
        }
    }
    
    public class TransportFromFile : ITransporter {

        public IEnumerator Load(BundleLoadOperation op, int streamIndex, string path) {
            string bundleName = op.nextBundle;
            Debug.Log("[Transporter] Load "+bundleName);
            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(path + bundleName);

            while (!request.isDone) {
                op.SetCurrentBundleProgress(op.progress);

                if (request.assetBundle == null) {
                    op.BundleFailed("Could not load Asset Bundle "+op.nextBundle, streamIndex);
                    yield break;
                }

                yield return null;
            }
            
            op.BundleLoaded(bundleName, request.assetBundle, streamIndex);
        }
    }

    // Proxy for handling bundle loading coroutines for AssetBundleManager
    public class BundleLoader : MonoBehaviour {}

    public interface ILoadOpHandler {
        void OnBundleFailed(int streamIndex);
        void OnBundleLoaded(string bundleName, AssetBundle bundle, int streamIndex);
        void OnLoadOpComplete(BundleLoadOperation op);
    }
    
    public class BundleLoadOperation : IPoolable {
        
        private readonly ILoadOpHandler loadHandler;
        private string[]                bundleNames;
        private int                     curBundle;
        private int                     loadedBundles;
        private float                   curBundleProgress;
        
        public bool   error    { get; private set; }
        public string errorMsg { get; private set; }

        public string nextBundle {
            get {
                int b = curBundle;
                ++curBundle;
                Debug.Log("[LoadOp] curBundle: "+b+" next bundle: "+curBundle);
                return bundleNames[b];
            }
        }

        public int bundlesLeftToLoad {
            get { return bundleNames.Length - curBundle; }
        }

        public float progress {
            get { return (curBundle + curBundleProgress) / bundleNames.Length; }
        }

        public BundleLoadOperation(ILoadOpHandler loadHandler) {
            this.loadHandler = loadHandler;
        }
        
        public void Init(List<string> p_bundleNames) {
            curBundle = 0;
            loadedBundles = 0;
            bundleNames = p_bundleNames.ToArray();
        }

        public void Init(string bundleName) {
            curBundle = 0;
            loadedBundles = 0;
            bundleNames = new string[1] { bundleName };
        }

        public void BundleFailed(string errorMessage, int streamIndex) {
            error = true;
            errorMsg = errorMessage;
            loadHandler.OnBundleFailed(streamIndex);
        }
        
        public void BundleLoaded(string bundleName, AssetBundle bundle, int streamIndex) {
            loadHandler.OnBundleLoaded(bundleName, bundle, streamIndex);
            ++loadedBundles;
            if (loadedBundles < bundleNames.Length) {
                return;
            }
            curBundleProgress = 0f;
            loadHandler.OnLoadOpComplete(this);
        }

        public void SetCurrentBundleProgress(float bundleProgress) {
            curBundleProgress = bundleProgress;
        }

        public void OnEnable() {}

        public void OnDisable() {}

        public void OnDestroy() {}
    }
    
    public class AssetBundleLoadStatus : CustomYieldInstruction {
    
        private readonly BundleLoadOperation op;

        public override bool keepWaiting {
            get { return op.progress < 1f; }
        }

        public float progress {
            get { return op.progress; }
        }

        public bool error {
            get { return op.error; }
        }

        public string errorMessage {
            get { return op.errorMsg; }
        }

        public AssetBundleLoadStatus(BundleLoadOperation loadOp) {
            op = loadOp;
        }
    }

}
