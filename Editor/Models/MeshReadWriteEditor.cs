using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
/// <summary>
/// 这个插件主要是基于在引擎中的mesh文件不是.fbx格式，仅仅具有mesh属性时，用来修改该mesh的读写属性
/// </summary>

public class MeshReadWriteEditor : EditorWindow
{
    [MenuItem("ArtTools/Model/Setting Mesh ReadWrite")]
    static void MeshReadWriteModfly()
    {
        // 获取所有选中的对象
        Object[] selectedObjects = Selection.objects;

        if (selectedObjects.Length == 0)
        {
            // 显示弹窗提示未选择任何物体
            EditorUtility.DisplayDialog("错误", "未选中任何物体！\n\n请在Project窗口中选中一个或多个.asset文件。", "确定");
            return;
        }

        List<string> processedAssets = new List<string>();
        int totalMeshesProcessed = 0;

        // 显示进度条
        EditorUtility.DisplayProgressBar("批量处理Mesh", "正在处理中...", 0f);

        try
        {
            for (int i = 0; i < selectedObjects.Length; i++)
            {
                Object selectedObject = selectedObjects[i];
                string assetPath = AssetDatabase.GetAssetPath(selectedObject);

                // 更新进度条
                float progress = (float)i / selectedObjects.Length;
                EditorUtility.DisplayProgressBar("批量处理Mesh", 
                    $"正在处理: {System.IO.Path.GetFileName(assetPath)} ({i + 1}/{selectedObjects.Length})", 
                    progress);

                // 跳过非.asset文件
                if (!assetPath.EndsWith(".asset", System.StringComparison.OrdinalIgnoreCase))
                {
                    Debug.LogWarning($"跳过非.asset文件: {assetPath}");
                    continue;
                }

                // 获取文件中的所有Mesh
                Mesh[] meshes = AssetDatabase.LoadAllAssetsAtPath(assetPath)
                    .OfType<Mesh>()
                    .ToArray();

                if (meshes.Length == 0)
                {
                    Debug.LogWarning($"在文件中未找到Mesh: {assetPath}");
                    continue;
                }

                // 处理当前文件中的所有Mesh
                foreach (Mesh mesh in meshes)
                {
                    SetMeshReadable(mesh, false);
                }

                processedAssets.Add(assetPath);
                totalMeshesProcessed += meshes.Length;
                Debug.Log($"已处理 {meshes.Length} 个Mesh在: {assetPath}");
            }
        }
        finally
        {
            // 确保进度条被关闭
            EditorUtility.ClearProgressBar();
        }

        // 显示结果对话框
        if (processedAssets.Count > 0)
        {
            AssetDatabase.SaveAssets();
            
            string resultMessage = $"成功处理完成！\n\n" +
                                 $"共处理: {totalMeshesProcessed} 个Mesh\n" +
                                 $"涉及文件: {processedAssets.Count} 个";
            
            // 显示详细结果对话框
            bool showDetails = EditorUtility.DisplayDialog("处理完成", 
                resultMessage, 
                "查看详情", "确定");

            if (showDetails)
            {
                string details = "处理详情:\n";
                for (int i = 0; i < processedAssets.Count; i++)
                {
                    details += $"{i + 1}. {processedAssets[i]}\n";
                }
                
                // 创建并显示详细信息的窗口
                ShowResultDetailsWindow("Mesh处理详情", details);
            }
        }
        else
        {
            EditorUtility.DisplayDialog("处理完成", 
                "未找到可处理的.asset文件。\n\n请确保选中的是包含Mesh的.asset文件。", 
                "确定");
        }
    }

    // 显示详细结果的窗口
    private static void ShowResultDetailsWindow(string title, string content)
    {
        // 创建一个简单的编辑器窗口来显示详细信息
        ResultDetailsWindow window = GetWindow<ResultDetailsWindow>(true, title, true);
        window.content = content;
        window.minSize = new Vector2(500, 400);
    }

    private static void SetMeshReadable(Mesh mesh, bool isReadable)
    {
        SerializedObject serializedMesh = new SerializedObject(mesh);
        SerializedProperty isReadableProp = serializedMesh.FindProperty("m_IsReadable");

        if (isReadableProp != null)
        {
            isReadableProp.boolValue = isReadable;
            serializedMesh.ApplyModifiedProperties();
            EditorUtility.SetDirty(mesh);
            mesh.UploadMeshData(!isReadable);
            
            // 动态标记处理（兼容不同Unity版本）
            if (isReadable)
            {
                #if UNITY_2019_3_OR_NEWER
                mesh.MarkDynamic();
                #else
                mesh.markDynamic = true;
                #endif
            }
        }
    }

    [MenuItem("ArtTools/Model/Batch Disable Mesh ReadWrite", true)]
    static bool ValidateSelection()
    {
        // 只在选中对象时启用菜单项
        return Selection.objects.Length > 0;
    }

    // 内部类：用于显示详细结果的窗口
    private class ResultDetailsWindow : EditorWindow
    {
        public string content = "";
        private Vector2 scrollPosition = Vector2.zero;

        void OnGUI()
        {
            GUILayout.Space(10);
            
            // 添加复制按钮
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("复制到剪贴板", GUILayout.Width(120)))
            {
                GUIUtility.systemCopyBuffer = content;
                EditorUtility.DisplayDialog("已复制", "详细结果已复制到剪贴板！", "确定");
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            
            // 滚动视图显示详细内容
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
            GUILayout.TextArea(content, GUILayout.ExpandHeight(true));
            GUILayout.EndScrollView();
            
            GUILayout.Space(10);
            
            // 关闭按钮
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("关闭", GUILayout.Width(80)))
            {
                this.Close();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
}

