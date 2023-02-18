using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEditor.AddressableAssets;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using Object = UnityEngine.Object;

[CustomEditor(typeof(SkinDefinition))]
[CanEditMultipleObjects]
public class SkinDefinitionEditor : EquipmentDataEditor
{
    private SkinDefinition skin;

    public string[] m_PropertyPathToExcludeForChildClassessChild;

    SerializedProperty m_CharacterID;
    SerializedProperty m_skinName;
    SerializedProperty m_skinGroupLocalizationKey;
    SerializedProperty m_priority;
    SerializedProperty m_skinColor;

    protected override void OnEnable()
    {
        base.OnEnable();
        skin = target as SkinDefinition;

        Addressables.InitializeAsync().WaitForCompletion();

        m_CharacterID = serializedObject.FindProperty("CharacterID");
        m_skinName = serializedObject.FindProperty("skinName");
        m_skinGroupLocalizationKey = serializedObject.FindProperty("skinGroupLocalizationKey");
        m_priority = serializedObject.FindProperty("priority");
        m_skinColor = serializedObject.FindProperty("skinColor");

        m_PropertyPathToExcludeForChildClassessChild = new[]
        {
            m_CharacterID.propertyPath,
            m_skinName.propertyPath,
            m_skinGroupLocalizationKey.propertyPath,
            m_priority.propertyPath,
            m_skinColor.propertyPath,
        };

        m_PropertyPathToExcludeForChildClassess.Concat(m_PropertyPathToExcludeForChildClassessChild);
    }

    protected override void EquipmentDataBody()
    {
        string tempCharName = RemoveSpecialCharacters(skin.CharacterID.ToString()).Replace(" ", "");
        string tempKey = tempCharName + "_" + RemoveSpecialCharacters(skin.skinName).Replace(" ", "");

        string addressablesGroup = tempCharName + "Skins";

        EditorGUILayoutExtentions.BeginColoredArea();
        GUIStyle centeredTextStyle = new GUIStyle("label");
        centeredTextStyle.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label("Skin Definition Data", centeredTextStyle);
        EditorGUILayoutExtentions.EndColoredArea();

        EditorGUILayout.PropertyField(m_CharacterID, new GUIContent("Character"));
        EditorGUILayout.PropertyField(m_skinName, new GUIContent("SkinName"));
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Unique Key");
        EditorGUILayoutExtentions.BeginColoredArea();
        EditorGUILayout.LabelField(tempKey, ValidAddressableColor(tempKey));
        if (tempKey != skin.uniqueKey)
        {
            EditorUtility.SetDirty(skin);
            skin.uniqueKey = tempKey;
            AssetDatabase.SaveAssetIfDirty(skin);
        }

        EditorGUILayoutExtentions.EndColoredArea();
        EditorGUILayout.EndHorizontal();

        if (!AddressableResourceExists(tempKey))
        {
            SkinData thisSkinData =
                AssetDatabase.LoadAssetAtPath(
                    "Assets/RushdownRevolt/SkinData/" + tempCharName + "/" + tempKey + ".asset",
                    typeof(SkinData)) as SkinData;
            if (thisSkinData != null)
            {
                if (GUILayout.Button("Add To " + addressablesGroup))
                {
                    AddAssetToGroup(AssetDatabase.GetAssetPath(thisSkinData), addressablesGroup);
                }
            }
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Skin Group");
        SkinGroupDropDown();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(m_priority, new GUIContent("Priority"));
        EditorGUILayout.PropertyField(m_skinColor, new GUIContent("Skin Color"));
    }

    private void SkinGroupDropDown()
    {
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();


        if (GUILayout.Button("Save"))
        {
            EditorUtility.SetDirty(skin);
            AssetDatabase.SaveAssetIfDirty(skin);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private static string RemoveSpecialCharacters(string str)
    {
        if (str == null)
            return "";

        return Regex.Replace(str, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);
    }

    private static bool AddressableResourceExists(object key)
    {
        foreach (var l in Addressables.ResourceLocators)
        {
            IList<IResourceLocation> locs;
            if (l.Locate(key, typeof(SkinData), out locs))
                return true;
        }

        return false;
    }

    public GUIStyle ValidAddressableColor(string key)
    {
        GUIStyle color = new GUIStyle(EditorStyles.label);
        color.normal.textColor = Color.white;
        if (skin != null)
        {
            if (AddressableResourceExists(key))
                color.normal.textColor = Color.green;
            else
                color.normal.textColor = Color.red;
        }

        return color;
    }

    public static void AddAssetToGroup(string path, string groupName)
    {
        var group = AddressableAssetSettingsDefaultObject.Settings.FindGroup(groupName);
        if (!group)
        {
            throw new Exception($"Addressable : can't find group {groupName}");
        }

        var entry = AddressableAssetSettingsDefaultObject.Settings.CreateOrMoveEntry(
            AssetDatabase.AssetPathToGUID(path), group,
            false,
            true);

        entry.SetAddress(Path.GetFileNameWithoutExtension(entry.address));

        if (entry == null)
        {
            throw new Exception($"Addressable : can't add {path} to group {groupName}");
        }
    }
}