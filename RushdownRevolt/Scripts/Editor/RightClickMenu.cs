using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RushdownRevolt.ModTool.Editor
{
    public class RightClickMenu 
    {
        [MenuItem("Assets/CombatLabs/Create/Skin Definition", priority = 2)]
        private static void CreateSkinDefinition(MenuCommand command)
        {
            string fileType = ".asset";
            string fileName = "New" + typeof(SkinDefinition);
            string path = AssetDatabase.GetAssetPath(Selection.activeObject) + "/";

            string fullPath = path + fileName + fileType;

            SkinDefinition item = ObjectFactory.CreateInstance<SkinDefinition>();
            AssetDatabase.CreateAsset(item, fullPath);
        }

        [MenuItem("Assets/CombatLabs/Create/Skin Data", priority = 1)]
        private static void CreateSkinData(MenuCommand command)
        {
            string fileType = ".asset";
            string fileName = "New" + typeof(SkinData);
            string path = AssetDatabase.GetAssetPath(Selection.activeObject) + "/";

            string fullPath = path + fileName + fileType;

            SkinData item = ObjectFactory.CreateInstance<SkinData>();
            AssetDatabase.CreateAsset(item, fullPath);
        }
    }

}