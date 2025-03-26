using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;

public class ModelPropertySettingsGUI 
{
    public const float DEFAULT_LABEL_WIDTH = 180.0f;
    public const float DEFAULT_TOGGLE_INDENT = 15f; //缩进
    
    /// <summary>
    /// 绘制带缩进的Toggle
    /// </summary>
    public static bool Toggle(bool value, string label, float indent = DEFAULT_TOGGLE_INDENT, string suffix = "")
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(indent);
        value = EditorGUILayout.Toggle(label + suffix, value);
        EditorGUILayout.EndHorizontal();
        return value;
    }
    
    /// <summary>
    /// 绘制带固定宽度标签的右对齐Toggle
    /// </summary>
    public static bool LabeledToggle(bool value, string label, float labelWidth = DEFAULT_LABEL_WIDTH)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.Width(labelWidth));
        GUILayout.FlexibleSpace();
        value = EditorGUILayout.Toggle(value);
        EditorGUILayout.EndHorizontal();
        return value;
    }
    
    /// <summary>
    /// 绘制枚举下拉框
    /// </summary>
    public static T EnumPopup<T>(T value, string label, float labelWidth = DEFAULT_LABEL_WIDTH) where T : Enum
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.Width(labelWidth));
        GUILayout.FlexibleSpace();
        value = (T)EditorGUILayout.EnumPopup(value);
        EditorGUILayout.EndHorizontal();
        return value;
    }
    
    /// <summary>
    /// 绘制带标题的分组区域
    /// </summary>
    public static void BeginGroup(string header)
    {
        EditorGUILayout.LabelField(header, EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
    }

    public static void EndGroup()
    {
        EditorGUI.indentLevel--;
        GUILayout.Space(10);
    }
}
