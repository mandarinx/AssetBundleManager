using System;
using System.Collections.Generic;
using System.Text;
using Tests.AssetBundles;
using UnityEngine;

namespace HyperGames.AssetBundles {
    
    // Instead of using partial, could I use the plugin system instead?
    
    // Made partial so that you can append game specific custom code to it without
    // having to add yet another class to the namespace
    
    public partial class VariantsResolver {

        //private readonly StringBuilder sbResolution = new StringBuilder();
        private readonly Dictionary<string, Func<string>>    variantRemappers;
        private readonly Dictionary<string, Func<string>>    variantMap;

        private readonly AssetBundleConfig config;
        
        public VariantsResolver(AssetBundleConfig cfg) {
            config = cfg;
            cfg.resolutionVariants.Sort(new ResolutionVariantComparer());
            variantRemappers = new Dictionary<string, Func<string>>();
            variantMap = new Dictionary<string, Func<string>>();
        }

        // Helper method for registering all resolution variants
        public void RegisterResolutionVariants() {
            for (int i = 0; i < config.resolutionVariants.Count; ++i) {
                RegisterVariant(GetResolution, config.resolutionVariants[i].name);
            }
        }
        
        // Register multiple variants for the same resolver
        public void RegisterVariants(Func<string> resolver, params string[] variantNames) {
            for (int i = 0; i < variantNames.Length; ++i) {
                variantRemappers.Add(variantNames[i], resolver);
            }
        }

        // Register one variant for a resolver
        public void RegisterVariant(Func<string> resolver, string variantName) {
            variantRemappers.Add(variantName, resolver);
        }

        public string RemapVariant(string bundleName) {
            string[] nameParts = bundleName.Split('.');
            if (nameParts.Length == 1) {
                // bundle name has no variant
                return bundleName;
            }
                
            #if UNITY_EDITOR
            return nameParts[0] + "." + config.editorResolutionVariant;
            #else 
            // Use the bundle name without the variant to look up the
            // remapping function.
            Func<string> Remapper;
            return variantMap.TryGetValue(nameParts[0], out Remapper)
                ? nameParts[0] + "." + Remapper()
                : nameParts[0];
            #endif
        }

        // Used as callback for AssetBundleManager's internal onLoadBundle callback.
        // Gets a list of the asset bundles to load, and remaps the variants according
        // to the config.
        public void RemapVariants(List<string> bundleNames) {
//            Debug.Log("Before remapping:");
//            for (int i = 0; i < bundleNames.Count; ++i) {
//                Debug.Log(bundleNames[i]);
//            }
            
            for (int i = 0; i < bundleNames.Count; ++i) {
                bundleNames[i] = RemapVariant(bundleNames[i]);
            }

//            Debug.Log("After remapping:");
//            for (int i = 0; i < bundleNames.Count; ++i) {
//                Debug.Log(bundleNames[i]);
//            }
        }

        // Registers bundles in a variantMap for quick lookup during remapping.
        // Used by AssetBundleManager's internal onManifestLoaded callback;
        // The bundle list passed is a list of all the bundles in the master
        // manifest that has one or more variants associated.
        public void RegisterBundles(string[] bundles) {
            for (int i = 0; i < bundles.Length; ++i) {
                string[] nameParts = bundles[i].Split('.');

                if (variantMap.ContainsKey(nameParts[0])) {
                    // Bundles with variant are registered with multiple entries in the manifest,
                    // one for each variant. If the map contains a key with the asset name, it is
                    // because it has variant and we've already processed one of them.
                    continue;
                }
                
                Func<string> remapper;
                if (!variantRemappers.TryGetValue(nameParts[1], out remapper)) {
                    Debug.LogWarning("[AssetBundleManager] "+
                                     "No remap handler registered for variant "+
                                     nameParts[1]+ " on asset "+bundles[i]+". AssetBundleManager will not be able to "+
                                     "load the bundle without knowing which variant to choose. Register a variant remapper "+
                                     "using AssetBundleManager.RegisterVariants()");
                    continue;
                }

                Debug.Log("Register remapper for "+nameParts[0]+ " variant "+nameParts[1]);
                // Bundle name registered with a direct pointer to the remapper function
                variantMap.Add(nameParts[0], remapper);
            }
        }

        private string GetResolution() {
            // DP - Density Indenpendent Pixels.
            // DP is an abstract unit based on the physical density of the screen.
            // By recommendation from the Android dev docs, DP is relative to a 
            // screen of 160 DPI. One DP is one physical pixel on a 160 DPI screen.
            // Using DP helps to normalize the relationship between screen DPI, size
            // and resolution.
            float dp = 1f / (config.baseDPI / Screen.dpi);
            
            // Resolution variants are ordered by the max DP threshold. Given the
            // thresholds [1, 3, 5], threshold 0.5 is smaller than 1, so it falls
            // within the first group. Threshold 1 is not smaller than 1, so it
            // falls within the second group. Any threshold greater than the 
            // max DP of the last group will use the last group.
            
            int v = 0;
            while (v < config.resolutionVariants.Count) {
                ResolutionVariant rv = config.resolutionVariants[v];
                if (dp < rv.maxDP) {
                    break;
                }
                ++v;
            }

            // If DP doesn't fall within any of the groups, use the last one
            if (v == config.resolutionVariants.Count) {
                v = config.resolutionVariants.Count - 1;
            }

            return config.resolutionVariants[v].name;
        }
    }
}
