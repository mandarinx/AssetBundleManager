using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine.Networking;

namespace HyperGames.AssetBundles {

    public interface ITransporter {
        IEnumerator Load(BundleLoadOperation op, int streamIndex, string path);
    }

    public class TransportFromWeb : ITransporter {
    
        public IEnumerator Load(BundleLoadOperation op, int streamIndex, string path) {
            int bundleIndex = op.nextBundle;
            string bundleName = op.GetBundleName(bundleIndex);

            // Due to how fast some bundles load, make the transporter wait a frame
            // before starting the loading. This is important for the editor, where
            // loading is instantaneous.
            // Important to get the bundle first 
            yield return null;
            
            UnityWebRequest request = UnityWebRequest.GetAssetBundle(path + bundleName);
            request.Send();
            
            while (!request.isDone) {
                op.SetCurrentBundleProgress(request.downloadProgress);

                if (request.isHttpError || request.isNetworkError) {
                    op.BundleFailed(bundleIndex, streamIndex, request.error);
                    yield break;
                }

                yield return null;
            }
            
            op.BundleLoaded(bundleIndex, streamIndex, DownloadHandlerAssetBundle.GetContent(request));
        }
    }
    
    public class TransportFromFile : ITransporter {

        public IEnumerator Load(BundleLoadOperation op, int streamIndex, string path) {
            int bundleIndex = op.nextBundle;
            string bundleName = op.GetBundleName(bundleIndex);
            yield return null;

            if (!File.Exists(Path.Combine(path, bundleName))) {
                op.BundleFailed(bundleIndex, streamIndex, GetErrorMsg(bundleName, path));
                yield break;
            }

            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(path + bundleName);

            while (!request.isDone) {
                op.SetCurrentBundleProgress(op.progress);

                if (request.assetBundle == null) {
                    op.BundleFailed(bundleIndex, streamIndex, GetErrorMsg(bundleName, path));
                    yield break;
                }

                yield return null;
            }
            
            op.BundleLoaded(bundleIndex, streamIndex, request.assetBundle);
        }

        private static string GetErrorMsg(string bundleName, string path) {
            return string.Format("File not found. Asset Bundle {0} at {1}", bundleName, path);
        }
    }

    // Proxy for handling bundle loading coroutines for AssetBundleManager
    public class BundleLoader : MonoBehaviour {}

    public interface ILoadOpHandler {
        void OnBundleFailed(int streamIndex, int retries);
        void OnBundleLoaded(string bundleName, int streamIndex, AssetBundle bundle);
        void OnLoadOpComplete(BundleLoadOperation op);
        void OnLoadOpFailed(BundleLoadOperation op);
    }
    
    public class BundleLoadOperation : IPoolable {
        
        private readonly ILoadOpHandler loadHandler;
        private string[]                bundleNames;
        private int[]                   retries;
        private bool[]                  loading;
        private int                     bundlesLoaded;
        private int                     bundlesFailed;
        private float                   curBundleProgress;
        private readonly StringBuilder  errorMessages;
        
        public bool                     error    { get; private set; }

        public string errorMsg {
            get { return errorMessages.ToString(); }
        }

        public int nextBundle {
            get {
                int next = 0;
                for (int i = 0; i < loading.Length; ++i) {
                    if (loading[i]) {
                        continue;
                    }
                    if (retries[i] >= 3) {
                        continue;
                    }
                    next = i;
                    loading[i] = true;
                    break;
                }
                return next;
            }
        }

        public bool canLoadBundle {
            get {
                bool canLoad = false;
                for (int i = 0; i < loading.Length; ++i) {
                    if (loading[i]) {
                        continue;
                    }
                    if (retries[i] >= 3) {
                        continue;
                    }
                    canLoad = true;
                    break;
                }
                return canLoad;
            }
        }

        public float progress {
            get { return (bundlesLoaded + curBundleProgress) / bundleNames.Length; }
        }

        public bool isComplete {
            get { return retryCount >= bundleNames.Length * 3; }
        }

        private int retryCount {
            get {
                int count = 0;
                for (int i = 0; i < retries.Length; ++i) {
                    count += retries[i];
                }
                return count;
            }
        }

        public BundleLoadOperation(ILoadOpHandler loadHandler) {
            this.loadHandler = loadHandler;
            errorMessages = new StringBuilder();
        }
        
        public void Init(List<string> p_bundleNames) {
            bundleNames = p_bundleNames.ToArray();
            retries = new int[bundleNames.Length];
            loading = new bool[bundleNames.Length];
        }

        public void Init(string bundleName) {
            bundleNames = new string[1] { bundleName };
            retries = new int[bundleNames.Length];
            loading = new bool[bundleNames.Length];
        }

        public void BundleFailed(int bundleIndex, int streamIndex, string errorMessage) {
            retries[bundleIndex] += 1;
            loading[bundleIndex] = false;
            
            if (retries[bundleIndex] >= 3) {
                ++bundlesFailed;
                errorMessages.AppendLine(errorMessage);
            }
            
            // Pass retries to prevent manager from starting a new load
            loadHandler.OnBundleFailed(streamIndex, retries[bundleIndex]);

            if (!isComplete) {
                return;
            }

            curBundleProgress = 0f;
            error = true;
            loadHandler.OnLoadOpFailed(this);
        }
        
        public void BundleLoaded(int bundleIndex, int streamIndex, AssetBundle bundle) {
            retries[bundleIndex] += 3;
            loading[bundleIndex] = false;
            ++bundlesLoaded;

            loadHandler.OnBundleLoaded(bundleNames[bundleIndex], streamIndex, bundle);

            if (!isComplete) {
                return;
            }
            
            curBundleProgress = 0f;

            if (bundlesFailed > 0) {
                loadHandler.OnLoadOpFailed(this);
            } else {
                loadHandler.OnLoadOpComplete(this);
            }
        }

        public void SetCurrentBundleProgress(float bundleProgress) {
            curBundleProgress = bundleProgress;
        }

        public string GetBundleName(int index) {
            return bundleNames[index];
        }

        public void OnEnable() {
            bundlesLoaded = 0;
            bundlesFailed = 0;
            error = false;
            errorMessages.Remove(0, errorMessages.Length);
        }

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
