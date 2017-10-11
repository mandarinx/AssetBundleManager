using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace HyperGames.AssetBundles {
    
//    public class AssetBundleLoadFromFile : IBundleLoader {
//        public AssetBundle bundle { get; private set; }
//        public bool error { get; private set; }
//        public string errorMessage { get; private set; }
//    
//        public IEnumerator Load(string path) {
//            AssetBundleCreateRequest req = AssetBundle.LoadFromFileAsync(path);
//            yield return req;
//    
//            if (req.assetBundle == null) {
//                error = true;
//                errorMessage = "Could not load asset bundle from "+path;
//                yield break;
//            }
//            
//            bundle = req.assetBundle;
//        }
//    }
//    
//    public class AssetBundleLoadFromWeb : IBundleLoader {
//        public AssetBundle bundle { get; private set; }
//        public bool error { get; private set; }
//        public string errorMessage { get; private set; }
//    
//        public IEnumerator Load(string path) {
//            UnityWebRequest www = UnityWebRequest.GetAssetBundle(path);
//            yield return www.Send();
//     
//            if (www.isHttpError || www.isNetworkError) {
//                error = true;
//                errorMessage = www.error;
//                yield break;
//            }
//    
//            bundle = DownloadHandlerAssetBundle.GetContent(www);
//        }
//    }

}
