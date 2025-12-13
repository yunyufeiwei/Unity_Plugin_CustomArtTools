using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace yuxuetian
{
    // 使用[InitializeOnLoad]属性确保类在Unity编辑器启动时立即加载
    [InitializeOnLoad]
    public class OpenSkyboxTools
    {
        // 静态构造函数在类被加载时自动调用
        static OpenSkyboxTools()
        {
            // 在Unity编辑器的右侧工具栏上添加一个自定义的GUI元素
            ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);
            // ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);    //在播放按钮左侧添加按钮
        }
        
        // 自定义的GUI函数，用于绘制工具栏上的按钮
        static void OnToolbarGUI()
        {
            //用于填充空白区域，当前希望按钮在左侧，因此在左边的部分就不使用空格填充
            // GUILayout.FlexibleSpace();

            var currentSkyName = RenderSettings.skybox ? RenderSettings.skybox.name : "No Sky";
            
            //设置GUI.Button的按钮宽度
            float width = 30 + currentSkyName.Length * 8;
            width = Mathf.Clamp(width, 100, 300);
            
            //设置GUI的样式
            var style = new GUIStyle(EditorStyles.toolbarButton);
            //设置GUI中文字的对齐方式
            style.alignment = TextAnchor.MiddleCenter;
            
            //声明一个Icon图标，将其作为按钮的背景图显示
            var skyboxIcon = EditorGUIUtility.IconContent("Skybox Icon").image;
        
            if (GUILayout.Button(new GUIContent(currentSkyName, skyboxIcon), style, GUILayout.Width(width)))
            {
                // 调用OpenSceneList类中的ShowWindow方法，以显示项目中的所有地图
                OpenSkyboxList.ShowWindow();
            }
        }
    }
}
