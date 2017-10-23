using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using Tests.AssetBundles;
using UnityEditor;
using UnityEditor.Sprites;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

public static class VariantsBuilder {
    
    [MenuItem("Assets/Clear AssetBundle info")]
    public static void AssignVariant() {
        string path = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
        AssetImporter.GetAtPath(path).SetAssetBundleNameAndVariant(null, null);
    }

    [MenuItem("Dev/Clear All AssetBundles")]
    public static void ClearAllBundles() {
        // Clear directories
        string[] directories = Directory.GetDirectories(Application.dataPath, "*", SearchOption.AllDirectories);
        
        List<string> assets = new List<string>();
        string[] bundles = AssetDatabase.GetAllAssetBundleNames();
        foreach (string bundleFullName in bundles) {
            assets.AddRange(AssetDatabase.GetAssetPathsFromAssetBundle(bundleFullName.Split('.')[0]));
        }
        
        int counter = 0;
        float max = directories.Length + assets.Count;
        
        foreach (string directory in directories) {
            string dir = "Assets" + directory.Substring(Application.dataPath.Length);
            AssetImporter importer = AssetImporter.GetAtPath(dir);
            if (importer == null) {
                continue;
            }

            ++counter;
            EditorUtility.DisplayProgressBar(
                "Clearing AssetBundle info", 
                "Removing AssetBundle names and variants from assets and directories",
                (counter / max));

            importer.SetAssetBundleNameAndVariant(null, null);
            importer.SaveAndReimport();
        }
        
        // Clear assets
        foreach (string asset in assets) {
            AssetImporter importer = AssetImporter.GetAtPath(asset);
            if (importer == null) {
                continue;
            }

            ++counter;
            EditorUtility.DisplayProgressBar(
                "Clearing AssetBundle info", 
                "Removing AssetBundle names and variants from assets and directories",
                (counter / max));

            importer.SetAssetBundleNameAndVariant(null, null);
            importer.SaveAndReimport();
        }

        EditorUtility.ClearProgressBar();
    }

    // Plugin system could have a pre method for doing preparations, and a GotBundles method for 
    // doing whatever with the bundles, and lastly a post method for finalizing stuff.
    // BuildVariants needs to initialize a plugin manager, load all plugins, execute
    // them one by one.
    // ! Pass AssetBundleConfig to pre-method?
    // !! Plugins shouldn't move or delete assets around. If they do, then BuildVariants
    // needs to scan for new bundles after each plugin run.

    [MenuItem("Dev/Build Variants")]
    public static void BuildVariants() {

        const string configPath = "Assets/AssetBundleConfig.asset";
        string bundlesRoot = Application.dataPath + "/__BUNDLES__";

        AssetDatabase.StartAssetEditing();

        // Load the AssetBundleConfig
        Debug.Log("[BV] Does "+
            Application.dataPath+"/AssetBundleConfig.asset"+
            " exist? "+
            (File.Exists(Application.dataPath+"/AssetBundleConfig.asset") ? "Yes" : "No"));

        string[] configs = AssetDatabase.FindAssets("t:AssetBundleConfig");
        Debug.Log("[BV] Found "+configs.Length+" AssetBundleConfig files");
        foreach (string c in configs) {
            string cp = AssetDatabase.GUIDToAssetPath(c);
            Debug.Log("[BV] config: "+cp);

            AssetBundleConfiguration cfg = AssetDatabase.LoadAssetAtPath<AssetBundleConfiguration>(cp);
            Debug.Log("[BV] "+(cfg != null ? "Could" : "Couldn't")+" LoadAssetAtPath<AssetBundleConfig> from " + cp);
            AssetBundleConfiguration cfg2 = (AssetBundleConfiguration)AssetDatabase.LoadAssetAtPath(cp, typeof(AssetBundleConfiguration));
            Debug.Log("[BV] "+(cfg2 != null ? "Could" : "Couldn't")+" LoadAssetAtPath typeof(AssetBundleConfig) from " + cp);
        }

        AssetBundleConfiguration configuration = AssetDatabase.LoadAssetAtPath<AssetBundleConfiguration>(configPath);
//        if (config == null) {
//            Debug.Log("[BV] Cannot load<AssetBundleConfig> from " + configPath);
////            return;
//        }
//
//        AssetBundleConfig config2 = (AssetBundleConfig)AssetDatabase.LoadAssetAtPath(configPath, typeof(AssetBundleConfig));
//        if (config2 == null) {
//            Debug.Log("[BV] Cannot load typeof(AssetBundleConfig) from " + configPath);
////            return;
//        }
        return;

        // Sort the resolution variants.
        // It doesn't matter if they are sorted in the asset, they should be anyway.
        configuration.resolutionVariants.Sort(new ResolutionVariantComparer());

        // Validate defined resolutionVariants with asset bundles
        Debug.Log("[BV] Validate resolutionVariants");
        string[] allBundles = AssetDatabase.GetAllAssetBundleNames();
        foreach (string bundle in allBundles) {
            string ext = bundle.Substring(bundle.Length - 3);
            Debug.Log("[BV] Bundle: "+bundle+" ext: "+ext);
            if (ext[0] != '.') {
                continue;
            }
            string variant = ext.Substring(1);
            Debug.Log("[BV] Variant: "+variant);
            
            bool found = false;
            foreach (ResolutionVariant rv in configuration.resolutionVariants) {
                found |= rv.name == variant;
            }
            Debug.Log("[BV] Found variant? "+found);
            if (found) {
                continue;
            }
            Debug.LogWarning("[BV] Variant "+variant+" cannot be found in AssetBundleConfig's resolutionVariants");
            return;
        }

        Debug.Log("[BV] Clear "+bundlesRoot+" directory");
        // Create directory for storing screen res variant bundles
        if (!Directory.Exists(bundlesRoot)) {
            Debug.Log("[BV] Create directory: " + bundlesRoot);
            Directory.CreateDirectory(bundlesRoot);
        } else {
            Debug.Log("[BV] Empty directory: " + bundlesRoot);
            DirectoryInfo di = new DirectoryInfo(bundlesRoot);

            foreach (FileInfo file in di.GetFiles("*", SearchOption.AllDirectories)) {
                Debug.Log("[BV] Delete file: "+file.Name);
                file.Delete(); 
            }
            foreach (DirectoryInfo dir in di.GetDirectories("*", SearchOption.AllDirectories)) {
                if (!Directory.Exists(dir.FullName)) {
                    continue;
                }
                Debug.Log("[BV] Delete dir: "+dir.Name);
                dir.Delete(true); 
            }
        }
        AssetDatabase.Refresh();
        
        // Get all directories with a screen res variant
        Debug.Log("[BV] Get all directories with a screen res variant");
        List<string> variantPathIndex = new List<string>();
        string[] directories = Directory.GetDirectories(Application.dataPath, "*", SearchOption.AllDirectories);
        
        foreach (string directory in directories) {
            string dir = "Assets" + directory.Substring(Application.dataPath.Length);
            AssetImporter importer = AssetImporter.GetAtPath(dir);
            if (importer == null) {
                continue;
            }
            
            if (Array.IndexOf(AssetBundleConfiguration.VARIANTS, importer.assetBundleVariant) < 0) {
                // variant is not a res variant
                continue;
            }

            Debug.Log("[BV] Add variant dir to variantPathIndex. "+dir);
            variantPathIndex.Add(dir);
        }

        // Get a list of all bundles
        string[] bundleNames = AssetDatabase.GetAllAssetBundleNames();
        for (int i = 0; i < bundleNames.Length; ++i) {
            
            string[] nameParts = bundleNames[i].Split('.');
            if (nameParts.Length < 2) {
                // Ignore AssetBundles without a variant
                continue;
            }

            string bundleVariant = nameParts[nameParts.Length - 1];
            if (Array.IndexOf(AssetBundleConfiguration.VARIANTS, bundleVariant) < 0) {
                // variant is not a res variant
                // TODO: Use plugin system for handling other variants
                continue;
            }

            // This bundle has a screen res variant. This variant is the
            // one that all the others will be created from.
            string bundleName = nameParts[0];
            string bundlePath = bundlesRoot + "/" + bundleName;
            string variantPath = bundlePath + "/" + bundleVariant;
            string variantDir = "Assets" + variantPath.Substring(Application.dataPath.Length);

            Debug.Log("[BV] Bundle: "+bundleNames[i]+" has variant: "+bundleVariant);

            // Create a subdir for bundles with screen res variants
            // Will this work for bundles with a slash in the name?
            if (!Directory.Exists(bundlePath)) {
                Debug.Log("[BV] Create directory for bundle at: "+bundlePath);
                Directory.CreateDirectory(bundlePath);
                AssetDatabase.Refresh();
            }
            
            // Create subdir for variant
            if (!Directory.Exists(variantPath)) {
                Debug.Log("[BV] Create directory for variant at: "+variantPath);
                Directory.CreateDirectory(variantPath);
                AssetDatabase.Refresh();
                
                // Set assetbundle info on variant subdir
                AssetImporter importer = AssetImporter.GetAtPath(variantDir);
                importer.SetAssetBundleNameAndVariant(bundleName, bundleVariant);
                Debug.Log("[BV] Set AssetBundle name: "+bundleName+" and variant: "+bundleVariant+" on folder: "+variantDir);
            }

            string[] assets = AssetDatabase.GetAssetPathsFromAssetBundle(bundleNames[i]);
            Debug.Log("[BV] Got "+assets.Length+" assets for bundle: "+bundleNames[i]);
            string[] newAssetPaths = new string[assets.Length];
            
            for (int a=0; a<assets.Length; ++a) {
                string asset = assets[a];
                // move into variant subfolder
                string[] assetParts = asset.Split('/');
                string targetDir = variantDir + "/" + assetParts[assetParts.Length - 1];
                newAssetPaths[a] = targetDir;
                Debug.Log("[BV] Move: "+asset+" to: "+targetDir);
//                AssetDatabase.CopyAsset(asset, targetDir);
                AssetDatabase.MoveAsset(asset, targetDir);
                AssetDatabase.Refresh();
                
                // remove assetbundle data. Let parent folder handle asset bundle info
                AssetImporter importer = AssetImporter.GetAtPath(targetDir);
                if (importer == null) {
                    continue;
                }
                Debug.Log("[BV] Clear AssetBundle info from "+targetDir);
                importer.SetAssetBundleNameAndVariant(null, null);
            }
            
            // Scale variants

            List<string> resVariants = new List<string>();
            for (int r = 0; r < configuration.resolutionVariants.Count; ++r) {
                resVariants.Add(configuration.resolutionVariants[r].name);
            }
            
            List<int> variantScales = new List<int>();
            for (int s = 0; s < resVariants.Count; ++s) {
                int si = Array.IndexOf(AssetBundleConfiguration.VARIANTS, resVariants[s]);
                variantScales.Add((int)Mathf.Pow(2, si));
            }

            int variantIndex = Array.IndexOf(AssetBundleConfiguration.VARIANTS, bundleVariant);
            float masterScale = variantScales[variantIndex];
            Debug.Log("[BV] Remove variant "+bundleVariant+" ("+variantIndex+") from variants lists");
            variantScales.RemoveAt(variantIndex);
            resVariants.RemoveAt(variantIndex);

            for (int v = 0; v < resVariants.Count; ++v) {
                string scaledVariant = resVariants[v];
                Debug.Log("[BV] Scale from "+bundleVariant+" to "+scaledVariant);
                // create res folder
                string scaledVariantPath = bundlePath + "/" + scaledVariant;
                
                if (!Directory.Exists(scaledVariantPath)) {
                    Debug.Log("[BV] Create directory for variant "+scaledVariant+" at: " + scaledVariantPath);
                    Directory.CreateDirectory(scaledVariantPath);
                    AssetDatabase.Refresh();
                }
                
                // add assetbundle info
                string scaledVariantDir = "Assets" + scaledVariantPath.Substring(Application.dataPath.Length);
                Debug.Log("[BV] Set "+scaledVariantDir+" AssetBundle to: "+bundleName+" and variant to: "+scaledVariant);
                SetAssetBundleNameAndVariant(scaledVariantDir, bundleName, scaledVariant);

                float scaleFactor = variantScales[v] / masterScale;
                Debug.Log("[BV] Variant: "+scaledVariant+" Scale Factor: "+scaleFactor);
                
                // copy assets
                foreach (string assetPath in newAssetPaths) {
                    string[] assetParts = assetPath.Split('/');
                    string assetName = assetParts[assetParts.Length - 1];
                    Debug.Log("[BV] Copy: "+assetName+" to: "+scaledVariantDir + "/" + assetName);
                    AssetDatabase.CopyAsset(assetPath, scaledVariantDir + "/" + assetName);

                    if (assetName.Length > 12 &&
                        assetName.Substring(assetName.Length - 12) == ".spriteatlas") {
                        
                        Debug.Log("[BV] SPRITEATLAS");
                        SpriteAtlas masterAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(assetPath);
                        SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(scaledVariantDir + "/" + assetName);
                        Debug.Log("[BV] Atlas name: "+atlas.name);

                        Assembly editorAssembly = Assembly.GetAssembly(typeof(Editor));
                        Type spriteAtlasExt = editorAssembly.GetType("UnityEditor.U2D.SpriteAtlasExtensions");

                        MethodInfo SetIsVariant = spriteAtlasExt.GetMethod("SetIsVariant");
                        SetIsVariant.Invoke(atlas, new object[] { atlas, true });

                        MethodInfo SetMasterAtlas = spriteAtlasExt.GetMethod("SetMasterAtlas");
                        SetMasterAtlas.Invoke(atlas, new object[] { atlas, masterAtlas });

                        MethodInfo SetVariantMultiplier = spriteAtlasExt.GetMethod("SetVariantMultiplier");
                        SetVariantMultiplier.Invoke(atlas, new object[] { atlas, scaleFactor });

                        // Pack the atlas!
                        Type spriteAtlasUtil = editorAssembly.GetType("UnityEditor.U2D.SpriteAtlasUtility");
                        MethodInfo PackAtlases = spriteAtlasUtil.GetMethod("PackAtlases", BindingFlags.Static | BindingFlags.NonPublic);
                        PackAtlases.Invoke(null, new object[] {
                            new SpriteAtlas[] { atlas },
                            EditorUserBuildSettings.activeBuildTarget
                        });
                        continue;
                    }
                    
                    AssetImporter importer = AssetImporter.GetAtPath(scaledVariantDir + "/" + assetName);
                    TextureImporter timporter = importer as TextureImporter;
                    if (timporter != null) {
                        Debug.Log("[BV] TEXTURE");
                        Debug.Log("[BV] maxsize: "+timporter.maxTextureSize+" >> "+(int)(timporter.maxTextureSize * scaleFactor));
                        timporter.maxTextureSize = (int)(timporter.maxTextureSize * scaleFactor);
                    }
                }
            }
        }

        foreach (string variantPath in variantPathIndex) {
            Debug.Log("[BV] Remove Asset Bundle info from: "+variantPath);
            AssetImporter importer = AssetImporter.GetAtPath(variantPath);
            importer.SetAssetBundleNameAndVariant(null, null);
        }
        
        AssetDatabase.StopAssetEditing();
    }

    private static void SetAssetBundleNameAndVariant(string path, string name, string variant) {
        AssetImporter importer = AssetImporter.GetAtPath(path);
        if (importer == null) {
            Debug.LogWarning("[BV] Cannot set AssetBundle name and variant for "+path+". Cannot get AssetImporter.");
            return;
        }
        importer.SetAssetBundleNameAndVariant(name, variant);
    }
}
