using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using PlasticGui.WorkspaceWindow.Items;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Overlays;
using UnityEngine;

namespace RushdownRevolt.ModTool.Editor
{
    public class ModToolsEditor : EditorWindow
    {
        private static string CurrentBuildTarget
        {
            get
            {
                switch (EditorUserBuildSettings.activeBuildTarget)
                {
                    default:
                    case BuildTarget.StandaloneWindows:
                    case BuildTarget.StandaloneWindows64:
                        return "Windows";
                    case BuildTarget.StandaloneLinux64:
                    case BuildTarget.EmbeddedLinux:
                        return "Linux";
                }
            }
        }

        private static readonly string FIRST_TIME_LOAD_KEY = "RR.MOD_TOOL.FIRST_LOAD_COMPLETE";
        private static readonly string LAYER_NAME = "Foreground_Lighting";

        private static string ExportPath
        {
            get { return Directory.GetCurrentDirectory() + "\\Exported"; }
        }

        private static string CatalogPath
        {
            get
            {
                return Directory.GetCurrentDirectory() + "\\Library\\com.unity.addressables\\aa\\" +
                       CurrentBuildTarget + "\\catalog.json";
            }
        }

        private static string ModSavePath
        {
            get { return Directory.GetCurrentDirectory() + "\\Mods"; }
        }


        private static SerializedObject _tagManager;
        private static SerializedProperty _layer16;
        private static string _modName;

        private static SerializedObject _addressableSettings;
        private static SerializedProperty _shaderPrefix;

        [MenuItem("Mod Tools/Open Editor")]
        public static void OpenEditor()
        {
            GetWindow<ModToolsEditor>(false, "RR Mod Tools", true);
        }

        public static void AutoOpenTool()
        {
            if (!EditorPrefs.GetBool(FIRST_TIME_LOAD_KEY, false))
            {
                EditorPrefs.SetBool(FIRST_TIME_LOAD_KEY, true);
                CleanMods();
                OpenEditor();
            }
        }

        private void OnEnable()
        {
            if (!Directory.Exists(ModSavePath))
            {
                Directory.CreateDirectory(ModSavePath);
            }

            _tagManager = new SerializedObject(AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));
            _layer16 = _tagManager.FindProperty("layers.Array.data[16]");

            _addressableSettings =
                new SerializedObject(
                    AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>(
                        "Assets/AddressableAssetsData/AddressableAssetSettings.asset"));
            _shaderPrefix = _addressableSettings.FindProperty("m_ShaderBundleCustomNaming");
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            if (_layer16.stringValue != LAYER_NAME)
            {
                EditorGUILayout.LabelField("Layer 16: ");
                EditorGUILayoutExtentions.BeginColoredArea();
                EditorGUILayout.LabelField(_layer16.stringValue.IsNullOrEmpty() ? "EMPTY" : _layer16.stringValue,
                    EditorGUILayoutExtentions.GUIStyleColor(Color.red));
                EditorGUILayoutExtentions.EndColoredArea();
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button("Set layer"))
                {
                    _layer16.stringValue = LAYER_NAME;
                    _tagManager.ApplyModifiedProperties();
                }
            }
            else
            {
                EditorGUILayout.LabelField("Layer 16: ");
                EditorGUILayoutExtentions.BeginColoredArea();
                EditorGUILayout.LabelField(_layer16.stringValue, EditorGUILayoutExtentions.GUIStyleColor(Color.green));
                EditorGUILayoutExtentions.EndColoredArea();
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Clean Addressables"))
            {
                CleanMods();
            }

            if (GUILayout.Button("Set Local Paths"))
            {
                SetPaths();
            }

            EditorGUILayoutExtentions.LineSeparator();

            EditorGUILayoutExtentions.BeginColoredArea();
            GUIStyle centeredTextStyle = new GUIStyle("label");
            centeredTextStyle.alignment = TextAnchor.MiddleCenter;
            GUILayout.Label("Build Addressables", centeredTextStyle);
            EditorGUILayoutExtentions.EndColoredArea();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Mod Name:");
            _modName = EditorGUILayout.TextField(_modName);
            EditorGUILayout.EndHorizontal();

            if (!_modName.IsNullOrEmpty())
            {
                if (GUILayout.Button("Build Windows"))
                {
                    Directory.CreateDirectory(ExportPath);
                    
                    foreach (string file in Directory.GetFiles(ExportPath))
                    {
                        File.Delete(file);
                    }

                    SwitchToBuildTarget(BuildTarget.StandaloneWindows64);

                    _shaderPrefix.stringValue = _modName;
                    _addressableSettings.ApplyModifiedProperties();
                    
                    if (BuildMods())
                    {
                        AfterBuildTasks();
                    }
                }

                if (GUILayout.Button("Build Linux"))
                {
                    Directory.CreateDirectory(ExportPath);
                    
                    foreach (string file in Directory.GetFiles(ExportPath))
                    {
                        File.Delete(file);
                    }

                    SwitchToBuildTarget(BuildTarget.StandaloneLinux64);

                    _shaderPrefix.stringValue = _modName;
                    _addressableSettings.ApplyModifiedProperties();
                    AssetDatabase.SaveAssets();

                    if (BuildMods())
                    {
                        AfterBuildTasks();
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField("Please enter a name for your mod",
                    EditorGUILayoutExtentions.GUIStyleColor(Color.red));
            }

            EditorGUILayoutExtentions.LineSeparator();
        }

        private static void SwitchToBuildTarget(BuildTarget target)
        {
            Debug.Log("Attempting to switch build target to " + target);

            if (EditorUserBuildSettings.activeBuildTarget == target)
            {
                Debug.Log("TargetPlatform already set to " + target);
                return;
            }

            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, target);
        }

        private static bool BuildMods()
        {
            AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
            bool success = string.IsNullOrEmpty(result.Error);

            if (!success)
            {
                Debug.LogError("Build error: " + result.Error);
            }

            return success;
        }

        private static void FixGroupTemplate()
        {
            AddressableAssetSettings settings =
                AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>(
                    "Assets/AddressableAssetsData/AddressableAssetSettings.asset");

            settings.GroupTemplateObjects.Clear();

            settings.GroupTemplateObjects.Add(
                (AddressableAssetGroupTemplate)AssetDatabase.LoadMainAssetAtPath(
                    "Packages/com.rushdownrevolt.modtool/RushdownRevolt/Assets/ModTemplate.asset"));

            AssetDatabase.SaveAssets();

            AssetDatabase.Refresh();
        }

        private static void AfterBuildTasks()
        {
            Dictionary<string, string> filesToRename = new();

            if (Directory.Exists(ModSavePath + "\\" + _modName))
            {
                Directory.Delete(ModSavePath + "\\" + _modName, true);
            }

            Directory.CreateDirectory(ModSavePath + "\\" + _modName + "\\" + _modName + "_data");

            string buildPath = EditorUtility.SaveFolderPanel("Save Mods", ModSavePath + "\\" + _modName, "");

            File.Copy(CatalogPath, buildPath + "\\" + _modName + ".json");

            foreach (string file in Directory.GetFiles(ExportPath))
            {
                string filename = Path.GetFileName(file);
                File.Copy(file, buildPath + "\\" + _modName + "_data" + "\\" + filename);
            }

            filesToRename.Add("{MODNAME}", _modName + "_data");

            string catalog = File.ReadAllText(buildPath + "\\" + _modName + ".json");
            catalog = catalog.Replace("{MODNAME}", _modName + "_data");

            File.WriteAllText(buildPath + "\\" + _modName + ".json", catalog);
        }

        private static void CleanMods()
        {
            if (Directory.GetFiles(Directory.GetCurrentDirectory() +
                                   "\\Assets\\AddressableAssetsData\\AssetGroups").Length > 0)
            {
                Directory.Delete(Directory.GetCurrentDirectory() + "\\Assets\\AddressableAssetsData\\AssetGroups",
                    true);
                AssetDatabase.Refresh();
            }
        }
        
        private static void SetPaths()
        {
            AddressableAssetSettings settings =
                AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>(
                    "Assets/AddressableAssetsData/AddressableAssetSettings.asset");

            var id = settings.profileSettings.GetProfileId("Default");
            settings.profileSettings.SetValue(id, "Local.BuildPath", "Exported");
            settings.profileSettings.SetValue(id, "Local.LoadPath", "Mods/{MODNAME}/");
        }
    }
    
    [InitializeOnLoad]
    class ModToolsLoad
    {
        static ModToolsLoad()
        {
            ModToolsEditor.AutoOpenTool();
        }
    }

    public static class RRExtentions
    {
        public static bool IsNullOrEmpty(this string str)
        {
            return String.IsNullOrEmpty(str);
        }
    }
}