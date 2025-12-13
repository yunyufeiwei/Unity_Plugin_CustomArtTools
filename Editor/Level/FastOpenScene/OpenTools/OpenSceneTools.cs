/*
 * 用于在播放按钮旁边添加一个自定义的按钮显示
 */
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine.UIElements;


namespace yuxuetian
{
    // 使用[InitializeOnLoad]属性确保类在Unity编辑器启动时立即加载
    [InitializeOnLoad]
    public class OpenSceneTools
    {
        // 静态构造函数在类被加载时自动调用
        static OpenSceneTools()
        {
            // 在Unity编辑器的左侧工具栏上添加一个自定义的GUI元素
            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
        }
        
        // 自定义的GUI函数，用于绘制工具栏上的按钮
        static void OnToolbarGUI()
        {
            //用于填充空白区域，当前希望按钮在右侧，因此在左边的部分就使用空格填充，如果不填充该按钮就会在引擎默认播放按钮左侧的最左边，
            GUILayout.FlexibleSpace(); 
            
            
            //在按钮上显示文字内容的名称，这里直接获取的是场景的名称
            var currentScene = EditorSceneManager.GetActiveScene().name;
            //如果希望按钮上显示的内容是文字说明，而不是地图名称，使用下面的方式
            //var currentScene = "美术地图列表";
            
            //设置GUI.Button的按钮宽度
            float width = 30 + currentScene.Length * 8;
            width = Mathf.Clamp(width, 100, 300);
            
            //设置GUI的样式
            var style = new GUIStyle(EditorStyles.toolbarButton);
            //设置GUI中文字的对齐方式
            style.alignment = TextAnchor.MiddleCenter;
            
            //声明一个Icon图标，将其作为按钮的背景图显示，这里使用了Unity的图标样式（估计是默认的）
            var sceneIcon = EditorGUIUtility.IconContent("SceneAsset Icon").image;
        
            //这里才是实际上创建GUI的地方
            if (GUILayout.Button(new GUIContent(currentScene, sceneIcon), style, GUILayout.Width(width)))
            {
                // 调用OpenSceneList类中的ShowWindow方法，以显示项目中的所有地图
                OpenSceneList.ShowWindow();
            }
            //在按钮的后面添加空行
            //GUILayout.Space(20);
        }
    }
}

