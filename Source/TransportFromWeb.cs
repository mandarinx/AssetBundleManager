using System.Collections;
using UnityEngine.Networking;

namespace HyperGames.AssetBundles {

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
            // Cloud Build will throw an error for this line.
            // warning CS0618: `UnityEngine.Networking.UnityWebRequest.Send()' is obsolete: 
            // `Use SendWebRequest. It returns a UnityWebRequestAsyncOperation which contains 
            // a reference to the WebRequest object.'
            // For some reason, Rider won't accept SendWebRequest as a valid method. 
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
}
