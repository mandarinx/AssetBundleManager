using UnityEngine;
using System.Collections;
using System.IO;

namespace HyperGames.AssetBundles {

    public class TransportFromDisk : ITransporter {

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
                Debug.Log("[TransportFromFile] progress: "+op.progress);
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
}
