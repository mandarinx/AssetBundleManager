using UnityEngine;
using System.Collections.Generic;
using System.Text;

namespace HyperGames.AssetBundles {

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
}
