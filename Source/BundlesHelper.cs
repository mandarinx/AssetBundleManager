using System;
using HyperGames.AssetBundles.Config;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HyperGames.AssetBundles {
    
    public static class BundlesHelper {

        public static string GetPath(AssetBundleManagerConfig cfg, string platform) {
            switch (cfg.bundleTarget) {
                case AssetBundleTarget.ASSET_BUNDLE_FOLDER:
                    return Application.dataPath.Replace("Assets", "") + cfg.bundlesFolder + "/" + platform + "/";

                case AssetBundleTarget.STREAMING_ASSETS:
                    return Application.streamingAssetsPath + "/";

                case AssetBundleTarget.LOCAL_SERVER:
                    return "";

                case AssetBundleTarget.REMOTE_SERVER:
                    return cfg.remoteURL;

                case AssetBundleTarget.OBB:
                    return "";

                case AssetBundleTarget.ON_DEMAND_RESOURCES:
                    return "";

                case AssetBundleTarget.APP_SLICING:
                    return "";

                default:
                    return "";
            }
        }

        public static ITransporter GetTransporter(AssetBundleManagerConfig cfg) {
            ITransporter transporter = null;

            switch (cfg.bundleTarget) {
                case AssetBundleTarget.ASSET_BUNDLE_FOLDER:
                case AssetBundleTarget.STREAMING_ASSETS:
                    transporter = new TransportFromDisk();
                    break;

                case AssetBundleTarget.LOCAL_SERVER:
                case AssetBundleTarget.REMOTE_SERVER:
                case AssetBundleTarget.OBB:
                case AssetBundleTarget.ON_DEMAND_RESOURCES:
                case AssetBundleTarget.APP_SLICING:
                    transporter = new TransportFromWeb();
                    break;
            }

            if (transporter == null) {
                Debug.LogError(
                    "Could not get Transporter using AssetBundleTarget option. " +
                    "AssetBundleConfig.bundleTarget value is " +
                    Enum.GetName(typeof(AssetBundleTarget), cfg.bundleTarget));
            }

            return transporter;
        }

        public static string GetPlatformName() {
            #if UNITY_EDITOR
            switch (EditorUserBuildSettings.activeBuildTarget) {
                case BuildTarget.Android:
                    return "Android";
                
                case BuildTarget.iOS:
                    return "iOS";
                
                case BuildTarget.StandaloneOSXIntel:
                case BuildTarget.StandaloneOSXIntel64:
                case BuildTarget.StandaloneOSXUniversal:
                    return "macOS";
                
                case BuildTarget.tvOS:
                    return "tvOS";
                
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "Windows";
                
                case BuildTarget.WSAPlayer:
                    return "WindowsStore";
                
                case BuildTarget.XboxOne:
                    return "XboxOne";
                
                case BuildTarget.WebGL:
                    return "WebGL";
                
                case BuildTarget.N3DS:
                    return "Nintendo3DS";
                
                case BuildTarget.Switch:
                    return "NintendoSwitch";
                
                case BuildTarget.WiiU:
                    return "NintendoWiiU";
                    
                case BuildTarget.PS4:
                    return "PS4";
                
                case BuildTarget.PSM:
                    return "PSM";
                
                case BuildTarget.PSP2:
                    return "PSP2";
                
                case BuildTarget.SamsungTV:
                    return "SamsungTV";
                
                case BuildTarget.StandaloneLinux:
                case BuildTarget.StandaloneLinux64:
                case BuildTarget.StandaloneLinuxUniversal:
                    return "Linux";
                
                case BuildTarget.Tizen:
                    return "Tizen";
                
                default:
                    return "N/A";
            }
            #else
            switch (Application.platform) {
                case RuntimePlatform.Android:
                    return "Android";
                
                case RuntimePlatform.IPhonePlayer:
                    return "iOS";
                
                case RuntimePlatform.OSXPlayer:
                    return "macOS";
                
                case RuntimePlatform.tvOS:
                    return "tvOS";
                
                case RuntimePlatform.WindowsPlayer:
                    return "Windows";
                
                case RuntimePlatform.WSAPlayerARM:
                case RuntimePlatform.WSAPlayerX64:
                case RuntimePlatform.WSAPlayerX86:
                    return "WindowsStore";
                
                case RuntimePlatform.XboxOne:
                    return "XboxOne";
                
                case RuntimePlatform.WebGLPlayer:
                    return "WebGL";
                
                case RuntimePlatform.Switch:
                    return "NintendoSwitch";
                
                case RuntimePlatform.WiiU:
                    return "NintendoWiiU";
                    
                case RuntimePlatform.PS4:
                    return "PS4";
                
                case RuntimePlatform.PSM:
                    return "PSM";
                
                case RuntimePlatform.PSP2:
                    return "PSP2";
                
                case RuntimePlatform.SamsungTVPlayer:
                    return "SamsungTV";
                
                case RuntimePlatform.LinuxPlayer:
                    return "Linux";
                
                case RuntimePlatform.TizenPlayer:
                    return "Tizen";
                
                default:
                    return "N/A";
            }
            #endif
        }
    }

}
