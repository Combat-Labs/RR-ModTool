using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RushdownRevolt.ModTool.Editor
{
    [CustomEditor(typeof(SkinData))]
    [CanEditMultipleObjects]
    public class SkinDataEditor : UnityEditor.Editor
    {
        private SkinData _skinData;

        private SerializedProperty _prefab;
        private SerializedProperty _combinedPrefab;
        private SerializedProperty _partnerPrefab;
        private SerializedProperty _combinedPartnerPrefab;

        private SerializedProperty _skinDefinition;

        private SerializedProperty _weaponTrails;
        private SerializedProperty _particles;

        private void OnEnable()
        {
            _skinData = target as SkinData;

            _prefab = serializedObject.FindProperty("characterPrefabFile");
            _combinedPrefab = serializedObject.FindProperty("combinedPrefabFile");
            _partnerPrefab = serializedObject.FindProperty("partnerPrefabFile");
            _combinedPartnerPrefab = serializedObject.FindProperty("combinedPartnerPrefabFile");

            _skinDefinition = serializedObject.FindProperty("skinDefinitionFile");

            _weaponTrails = serializedObject.FindProperty("weaponTrails");
            _particles = serializedObject.FindProperty("uniquePrefabs");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(_prefab, new GUIContent("Character Prefab"));
            EditorGUILayout.PropertyField(_combinedPrefab, new GUIContent("Combined Prefab"));
            EditorGUILayout.PropertyField(_partnerPrefab, new GUIContent("Partner Prefab"));
            EditorGUILayout.PropertyField(_combinedPartnerPrefab, new GUIContent("Combined Partner Prefab"));

            EditorGUILayout.PropertyField(_skinDefinition, new GUIContent("Skin Definition"));

            EditorGUILayout.PropertyField(_weaponTrails, new GUIContent("Weapon Trails"));
            EditorGUILayout.PropertyField(_particles, new GUIContent("Custom Particles"));

            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Save"))
            {
                EditorUtility.SetDirty(_skinData);
                AssetDatabase.SaveAssetIfDirty(_skinData);
            }
        }
    }
}