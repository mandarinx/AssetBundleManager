using System;
using System.Collections.Generic;
using UnityEngine;

namespace HyperGames.AssetBundles {
    
    [CreateAssetMenu(menuName = "Asset Bundle Manager Config", fileName = "AssetBundleManagerConfig.asset")]
    [Serializable]
    public class AssetBundleManagerConfig : ScriptableObject {

        public const string ONE_X     = "1x";
        public const string TWO_X     = "2x";
        public const string FOUR_X    = "4x";
        public const string EIGHT_X   = "8x";

        public static readonly string[] VARIANTS = {
            ONE_X, TWO_X, FOUR_X, EIGHT_X
        };

        public const int defaultBaseDPI = 160;
        
        public string bundlesFolder;
        // "https://s3-eu-west-1.amazonaws.com/mndrassetbundles/"
        public string remoteURL;
        public AssetBundleTarget bundleTarget;
        public int numBundleLoaders;

        public int baseDPI = defaultBaseDPI;
        
        public List<ResolutionVariant> resolutionVariants = new List<ResolutionVariant> {
            new ResolutionVariant { name = ONE_X,  maxDP = 1 },
            new ResolutionVariant { name = TWO_X,  maxDP = 2 },
            new ResolutionVariant { name = FOUR_X, maxDP = 4 },
        };

        public string editorResolutionVariant;
        
        // Used during integration testing
        public bool isTestingIntegration = false;
    }
}
