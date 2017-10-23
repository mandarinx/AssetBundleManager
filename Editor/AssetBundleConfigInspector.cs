using UnityEngine;
using HyperGames.AssetBundles;
using Tests.AssetBundles;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(AssetBundleConfiguration))]
public class AssetBundleConfigInspector : Editor {

    private bool showScreenResHelp = false;
    private ReorderableList screenResVariants;

    private GUIStyle btnMini;

    private void OnEnable() {
        screenResVariants = new ReorderableList(
            serializedObject:    serializedObject,
            elements:            serializedObject.FindProperty("resolutionVariants"),
            draggable:           false,
            displayHeader:       true,
            displayAddButton:    true,
            displayRemoveButton: true);

        screenResVariants.drawElementCallback = (rect, index, active, focused) => {
            SerializedProperty elm = screenResVariants.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;
            float halfWidth = rect.width / 2;
            EditorGUI.LabelField(
                new Rect(rect.x, rect.y, halfWidth / 2, EditorGUIUtility.singleLineHeight), 
                "Variant");
            EditorGUI.PropertyField(
                new Rect(rect.x + (halfWidth / 2), rect.y, halfWidth / 2, EditorGUIUtility.singleLineHeight),
                elm.FindPropertyRelative("name"), 
                GUIContent.none);
            EditorGUI.LabelField(
                new Rect(rect.x + rect.width - (halfWidth - 5), rect.y, halfWidth / 2, EditorGUIUtility.singleLineHeight), 
                "Max DP");
            EditorGUI.PropertyField(
                new Rect(rect.x + rect.width - (halfWidth / 2), rect.y, halfWidth / 2, EditorGUIUtility.singleLineHeight),
                elm.FindPropertyRelative("maxDP"), 
                GUIContent.none);
        };

        screenResVariants.drawHeaderCallback = rect => {
            EditorGUI.LabelField(rect, "Resolution Variants");
        };
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        btnMini = EditorStyles.miniButton;
        btnMini.fixedWidth = 60;
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("bundlesFolder"));
        EditorGUILayout.HelpBox(
            "Bundles Folder is relative to the project folder, meaning the parent of the Assets folder.", 
            MessageType.None);
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("remoteURL"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("bundleTarget"));

        SerializedProperty numBundleLoaders = serializedObject.FindProperty("numBundleLoaders");
        int numLoaders = (int)EditorGUILayout.Slider("Num Bundle Loaders", numBundleLoaders.intValue, 1, 16);
        numBundleLoaders.intValue = numLoaders;
        
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("Screen Resolution Variants", EditorStyles.boldLabel);
            if (GUILayout.Button("Help", btnMini)) {
                showScreenResHelp = !showScreenResHelp;
            }
        }
        EditorGUILayout.EndHorizontal();

        if (showScreenResHelp) {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "DP - Density Indenpendent Pixels"+
                "\n\n"+
                "DP is an abstract unit based on the physical density of the screen. "+
                "DP is relative to a screen of "+AssetBundleConfiguration.defaultBaseDPI+" DPI. "+
                "One DP is one physical pixel on a "+AssetBundleConfiguration.defaultBaseDPI+" "+
                "DPI screen. Using DP helps to normalize the relationship between screen "+
                "DPI, size and resolution."+
                "\n\n"+
                "You can map Asset Bundle Variants to DP values using the table below. "+
                "Max DP indicated the highest DP value a variant will be used for. For "+
                "instance, variant 1x with a Max DP of 1 means that when the device's DP "+
                "value is 1 or less, VariantsRemapper will look for the asset bundle with "+
                "variant 1x."+
                "\n\n"+
                "If you haven't built a 1x variant for that specific asset bundle, "+
                "AssetBundleManager won't find the bundle and fail to load it.", 
                MessageType.None);
            EditorGUILayout.Space();
        }
        
        EditorGUILayout.BeginHorizontal();
        {
            SerializedProperty baseDPI = serializedObject.FindProperty("baseDPI");
            EditorGUILayout.PropertyField(baseDPI);

            if (GUILayout.Button("Reset", btnMini)) {
                baseDPI.intValue = AssetBundleConfiguration.defaultBaseDPI;
            }
        }
        EditorGUILayout.EndHorizontal();

        if (screenResVariants != null) {
            screenResVariants.DoLayoutList();

            if (screenResVariants.count == 0) {
                EditorGUILayout.HelpBox(
                    "There are no variants defined, and that's allright. "+
                    "The handling of screen resolution variants is now disabled.", 
                    MessageType.Info);
            }
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("editorResolutionVariant"));
        EditorGUILayout.HelpBox(
            "When in editor, override the screen resolution variants with this variant.", 
            MessageType.None);

        if (GUI.changed) {
            serializedObject.ApplyModifiedProperties();
        }
    }
}
