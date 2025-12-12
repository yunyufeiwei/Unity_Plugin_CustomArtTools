using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class SettingPrefabsTagsAndLayer : EditorWindow
{
    public static string PrefabPaths = "Assets/Prefabs";
    public bool settingTagsProperty = false;
    public bool settingLayersProperty = false;
    public bool onlyModifyParent = true; // 新增：是否仅修改父物体
    
    // 从项目设置中获取的变量
    private string selectedTag = "Untagged";
    private int selectedLayer = 0; // Default layer
    
    // 缓存Tags和Layers列表
    private string[] availableTags;
    private string[] availableLayers;
    private int tagIndex = 0;
    private int layerIndex = 0;
    
    [MenuItem("ArtTools/Model/修改标签和层级", false, 112)]
    private static void ShowWindow()
    {
        EditorWindow window = GetWindow<SettingPrefabsTagsAndLayer>();
        window.titleContent = new GUIContent("修改Tags And Layer");
        window.minSize = new Vector2(300, 400);
    }

    /// <summary>
    /// 使用OnEnable()方法，在EditorWindow第一次打开时调用里面的方法，执行时机比OnGUI()更早
    /// </summary>
    private void OnEnable()
    {
        // 初始化时加载Tags和Layers
        LoadTagsAndLayers();
    }

    private void LoadTagsAndLayers()
    {
        // 获取所有Tags
        availableTags = GetAllTags();
        
        // 获取所有Layers
        availableLayers = GetAllLayers();
        
        // 查找selectedTag当前选择的tag在列表availableTags中的索引
        tagIndex = Array.IndexOf(availableTags, selectedTag);
        if (tagIndex < 0) tagIndex = 0;
        
        // 查找当前选择的layer在列表中的索引
        string layerName = LayerMask.LayerToName(selectedLayer);
        layerIndex = Array.IndexOf(availableLayers, layerName);
        if (layerIndex < 0) layerIndex = 0;
    }

    private string[] GetAllTags()
    {
        // 使用Unity内置方法获取所有Tags
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");
        
        string[] tags = new string[tagsProp.arraySize + 1];
        tags[0] = "Untagged"; // 确保包含Untagged
        
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty tag = tagsProp.GetArrayElementAtIndex(i);
            string tagName = tag.stringValue;
            if (!string.IsNullOrEmpty(tagName))
            {
                tags[i + 1] = tagName;
            }
        }
        
        return tags;
    }

    private string[] GetAllLayers()
    {
        // 获取所有可用的Layers
        string[] layers = new string[32]; // Unity有32个Layer
        
        for (int i = 0; i < 32; i++)
        {
            string layerName = LayerMask.LayerToName(i);
            if (!string.IsNullOrEmpty(layerName))
            {
                layers[i] = layerName;
            }
            else
            {
                // 将未使用的Layer位置设置为"Layer " + i
                layers[i] = $"Layer {i}";
            }
        }
        
        return layers;
    }

    public void OnGUI()
    {
        float _width = 80;
        
        GUILayout.Space(20.0f);
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("指定路径：", EditorStyles.boldLabel);
        PrefabPaths = GUILayout.TextField(PrefabPaths, GUILayout.MinWidth(_width), GUILayout.MaxWidth(_width * 6));
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        
        // 新增：仅修改父物体开关
        GUILayout.BeginHorizontal();
        onlyModifyParent = GUILayout.Toggle(onlyModifyParent, "仅修改父物体", GUILayout.Width(120));
        GUILayout.FlexibleSpace();
        
        if (onlyModifyParent)
        {
            GUILayout.Label("√ 仅修改预制体根节点", EditorStyles.helpBox);
        }
        else
        {
            GUILayout.Label("√ 修改预制体所有子物体", EditorStyles.helpBox);
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        
        // Tags设置区域
        GUILayout.BeginHorizontal();
        settingTagsProperty = GUILayout.Toggle(settingTagsProperty, "Tag", GUILayout.Width(80));
        GUILayout.FlexibleSpace();
        
        if (settingTagsProperty)
        {
            // 使用Popup选择Tag
            int newTagIndex = EditorGUILayout.Popup("", tagIndex, availableTags, GUILayout.MinWidth(80),GUILayout.MaxWidth(120));
            if (newTagIndex != tagIndex)
            {
                tagIndex = newTagIndex;
                selectedTag = availableTags[tagIndex];
            }
        }
        else
        {
            // 禁用状态显示当前选择的Tag
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.Popup("", tagIndex, availableTags, GUILayout.MinWidth(40), GUILayout.MaxWidth(120));
            EditorGUI.EndDisabledGroup();
        }
        GUILayout.EndHorizontal();
        
        // Layers设置区域
        GUILayout.BeginHorizontal();
        settingLayersProperty = GUILayout.Toggle(settingLayersProperty, "Layer", GUILayout.Width(80));
        GUILayout.FlexibleSpace();
        
        if (settingLayersProperty)
        {
            // 使用Popup选择Layer
            int newLayerIndex = EditorGUILayout.Popup("", layerIndex, availableLayers, GUILayout.MinWidth(40),GUILayout.MaxWidth(120));
            if (newLayerIndex != layerIndex)
            {
                layerIndex = newLayerIndex;
                selectedLayer = layerIndex; // Layer索引直接对应Layer编号
            }
        }
        else
        {
            // 禁用状态显示当前选择的Layer
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.Popup("", layerIndex, availableLayers, GUILayout.MinWidth(40),GUILayout.MaxWidth(120));
            EditorGUI.EndDisabledGroup();
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(20);
        
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("修改预制体设置", GUILayout.Height(25)))
        {
            FindPrefabsAndSetTags();
        }
        GUILayout.Space(5);
        
        // 添加刷新按钮
        if (GUILayout.Button("刷新Tags和Layers列表", GUILayout.Height(25)))
        {
            LoadTagsAndLayers();
            Repaint();
        }
        GUILayout.Space(20);
    }

    private void FindPrefabsAndSetTags()
    {
        if (!Directory.Exists(PrefabPaths))
        {
            //DisplayDialog为显示对话框，当指定路径不存在时，就弹窗打印错误提示。
            EditorUtility.DisplayDialog("错误", "指定的路径不存在！", "确定");
            return;
        }

        string[] prefabPaths = Directory.GetFiles(PrefabPaths, "*.prefab", SearchOption.AllDirectories);
        
        if (prefabPaths.Length == 0)
        {
            EditorUtility.DisplayDialog("提示", "在指定路径下未找到预制体文件！", "确定");
            return;
        }

        int modifiedPrefabsCount = 0;
        int modifiedObjectsCount = 0;
        
        // 开始进度条显示
        EditorUtility.DisplayProgressBar("修改预制体", "正在处理预制体...", 0);
        
        try
        {
            for (int i = 0; i < prefabPaths.Length; i++)
            {
                string path = prefabPaths[i];
                EditorUtility.DisplayProgressBar("修改预制体", $"正在处理: {Path.GetFileName(path)}", (float)i / prefabPaths.Length);
                
                GameObject prefabObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefabObject == null)
                    continue;

                bool prefabModified = false;
                
                if (onlyModifyParent)
                {
                    // 仅修改父物体
                    if (settingTagsProperty)
                    {
                        try
                        {
                            prefabObject.tag = selectedTag;
                            prefabModified = true;
                            modifiedObjectsCount++;
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"设置Tag失败 ({Path.GetFileName(path)}): {e.Message}");
                        }
                    }

                    if (settingLayersProperty)
                    {
                        prefabObject.layer = selectedLayer;
                        prefabModified = true;
                        modifiedObjectsCount++;
                    }
                }
                else
                {
                    // 修改所有子物体
                    Transform[] allTransforms = prefabObject.GetComponentsInChildren<Transform>(true);
                    foreach (Transform transform in allTransforms)
                    {
                        if (settingTagsProperty)
                        {
                            try
                            {
                                transform.gameObject.tag = selectedTag;
                                prefabModified = true;
                                modifiedObjectsCount++;
                            }
                            catch (Exception e)
                            {
                                Debug.LogWarning($"设置Tag失败 ({Path.GetFileName(path)} - {transform.name}): {e.Message}");
                            }
                        }

                        if (settingLayersProperty)
                        {
                            transform.gameObject.layer = selectedLayer;
                            prefabModified = true;
                            modifiedObjectsCount++;
                        }
                    }
                }

                if (prefabModified)
                {
                    EditorUtility.SetDirty(prefabObject);
                    modifiedPrefabsCount++;
                }
            }
        }
        finally
        {
            // 确保进度条被关闭
            EditorUtility.ClearProgressBar();
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        string message = $"成功修改了 {modifiedPrefabsCount} 个预制体";
        if (!onlyModifyParent)
        {
            message += $"，共 {modifiedObjectsCount} 个游戏对象";
        }
        
        EditorUtility.DisplayDialog("完成", message, "确定");
    }
}