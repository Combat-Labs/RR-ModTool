using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace RushdownRevolt.ModTool.Editor
{
    [CustomEditor(typeof(IEquipmentData), true)]
[CanEditMultipleObjects]
public class EquipmentDataEditor : UnityEditor.Editor
{
     SerializedProperty m_Script;
     
     public string[] m_PropertyPathToExcludeForChildClassess;
     public bool workingOnMultipleObjects = false;
     public IEquipmentData[] data;

     public bool DrawChildClassPropertiesGUI = true;
     
    protected virtual void OnEnable()
    {
        m_Script = serializedObject.FindProperty("m_Script");
        
        workingOnMultipleObjects = targets.Length > 1;
        data = Array.ConvertAll(targets, item => (IEquipmentData)item);
        
        m_PropertyPathToExcludeForChildClassess = new[]
        {
            m_Script.propertyPath,
        };
    }
    

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EquipmentDataHeader();
        EditorGUILayoutExtentions.LineSeparator();
        EquipmentDataBody();
        EditorGUILayoutExtentions.LineSeparator();
        EquipmentDataFooter();
        
        serializedObject.ApplyModifiedProperties();
        
    }
    protected virtual void EquipmentDataHeader()
    {
       
    }

    protected virtual void EquipmentDataBody()
    {
        EditorGUILayoutExtentions.BeginColoredArea();
        GUIStyle centeredTextStyle = new GUIStyle("label");
        centeredTextStyle.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label("Default Item Data", centeredTextStyle);
        EditorGUILayoutExtentions.EndColoredArea();
        
        if (!workingOnMultipleObjects)
            ChildClassPropertiesGUI();
    }

    protected virtual void EquipmentDataFooter()
    {

    }
    
    private void ChildClassPropertiesGUI()
    {
        DrawPropertiesExcluding(serializedObject, m_PropertyPathToExcludeForChildClassess);
    }

}


public static class EditorGUILayoutExtentions
{
    public static void LineSeparator()
    {
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
    }

    public static void BeginColoredArea(Color? BGColor = null)
    {
        Color defaultColor=GUI.color;
        
        GUI.color= BGColor.GetValueOrDefault(Color.black);
        EditorGUILayout.BeginHorizontal("box");
        GUI.color=defaultColor;
    }

    public static void EndColoredArea()
    {
        EditorGUILayout.EndHorizontal();

    }
    
    public static GUIStyle GUIStyleColor(Color color)
    {
        GUIStyle style = new GUIStyle(EditorStyles.label);

        style.normal.textColor = color;
        
        return style;
    }
}


}