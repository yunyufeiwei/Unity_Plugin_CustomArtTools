using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;

namespace yuxuetian
{
    public class ModelPropertySettingsGUI 
    {
        public const float DEFAULT_LABEL_WIDTH = 200.0f;
        
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
        
        /// <summary>
        /// 绘制枚举下拉框
        /// </summary>
        public static T EnumPopup<T>(T value, string label, float labelWidth = DEFAULT_LABEL_WIDTH) where T : Enum
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(labelWidth));
            value = (T)EditorGUILayout.EnumPopup(value);
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
            value = EditorGUILayout.Toggle(value);
            EditorGUILayout.EndHorizontal();
            return value;
        }
        
        /// <summary>
        /// 带标签的滑动条（float版本）
        /// </summary>
        public static float LabeledSlider(float value, string label, float leftValue, float rightValue, float labelWidth = DEFAULT_LABEL_WIDTH)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(labelWidth));
            value = EditorGUILayout.Slider(value, leftValue, rightValue);
            EditorGUILayout.EndHorizontal();
            return value;
        }

        /// <summary>
        /// 绘制带固定宽度标签的右对齐float
        /// </summary>
        public static float LabeledFloatField(float value, string label, float labelWidth = DEFAULT_LABEL_WIDTH)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(labelWidth));
            value = EditorGUILayout.FloatField(value);
            EditorGUILayout.EndHorizontal();
            return value;
        }
    }
}

