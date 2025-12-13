using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(ShaderTableData))]
public class ShaderTableEditor : Editor
{
    private ShaderTableData _data;
    private Vector2 _scrollPos;
    
    // 折叠状态
    private bool _sceneFoldout = true;
    private bool _characterFoldout = true;
    private bool _effectFoldout = true;
    private bool _uiFoldout = true;
    
    private void OnEnable()
    {
        _data = target as ShaderTableData;
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.LabelField("ShaderInfo", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);
        
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
        
        // 场景模块
        EditorGUILayout.BeginVertical("Box");
        DrawModuleHeader(_data.sceneItems, ref _sceneFoldout, "SceneShader");
        if (_sceneFoldout)
        {
            DrawModuleContent(_data.sceneItems, "Scene");
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // 角色模块
        EditorGUILayout.BeginVertical("Box");
        DrawModuleHeader(_data.characterItems, ref _characterFoldout, "CharacterShader");
        if (_characterFoldout)
        {
            DrawModuleContent(_data.characterItems, "Character");
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // 特效模块
        EditorGUILayout.BeginVertical("Box");
        DrawModuleHeader(_data.effectItems, ref _effectFoldout, "EffectShader");
        if (_effectFoldout)
        {
            DrawModuleContent(_data.effectItems, "Effect");
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // UI模块
        EditorGUILayout.BeginVertical("Box");
        DrawModuleHeader(_data.uiItems, ref _uiFoldout, "UIShader");
        if (_uiFoldout)
        {
            DrawModuleContent(_data.uiItems, "UI");
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.EndScrollView();
        serializedObject.ApplyModifiedProperties();
    }
    
    // 绘制模块标题行
    private void DrawModuleHeader(List<ResourceItem> items, ref bool foldoutState, string moduleName)
    {
        EditorGUILayout.BeginHorizontal();
        
        // 折叠箭头和标签一起作为一个可点击区域
        GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout);
        foldoutStyle.fontStyle = FontStyle.Bold;
        
        string foldoutLabel = $"{moduleName} ({items.Count})";
        foldoutState = EditorGUILayout.Foldout(foldoutState, foldoutLabel, true, foldoutStyle);
        
        EditorGUILayout.EndHorizontal();
        
        // 添加一个小的分割线
        EditorGUILayout.Space(1);
        if (!foldoutState && items.Count > 0)
        {
            // 折叠状态下显示一个简短的提示
            EditorGUILayout.LabelField($"已折叠 - 包含 {items.Count} 个配置项", EditorStyles.miniLabel);
        }
    }
    
    // 绘制模块内容
    private void DrawModuleContent(List<ResourceItem> items, string moduleName)
    {
        if (items.Count == 0)
        {
            EditorGUILayout.HelpBox($"暂无{moduleName}配置", MessageType.Info);
            
            // 空的模块仍然显示添加按钮
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+ 添加配置", GUILayout.Width(100)))
            {
                items.Add(new ResourceItem());
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            // 绘制配置项
            for (int i = 0; i < items.Count; i++)
            {
                EditorGUILayout.Space(5);
                
                // 每个小整体的背景框
                EditorGUILayout.BeginVertical("Box");
                
                // 标题栏 - 可编辑的标题
                EditorGUILayout.BeginHorizontal();
                
                // 可编辑的标题文本字段
                string titleLabel = string.IsNullOrEmpty(items[i].itemName) ? 
                    $"配置项 {i + 1}" : 
                    items[i].itemName;
                
                // 使用文本字段代替标签，让用户可以编辑
                EditorGUILayout.LabelField("类型:", GUILayout.Width(40));
                items[i].itemName = EditorGUILayout.TextField(items[i].itemName, 
                    GUILayout.MinWidth(30),GUILayout.MaxWidth(80));
                
                GUILayout.FlexibleSpace();
                
                // 上移按钮
                if (i > 0 && GUILayout.Button("↑", GUILayout.Width(25)))
                {
                    var temp = items[i];
                    items[i] = items[i - 1];
                    items[i - 1] = temp;
                }
                
                // 下移按钮
                if (i < items.Count - 1 && GUILayout.Button("↓", GUILayout.Width(25)))
                {
                    var temp = items[i];
                    items[i] = items[i + 1];
                    items[i + 1] = temp;
                }
                
                // 删除按钮
                if (GUILayout.Button("删除", GUILayout.Width(60)))
                {
                    if (EditorUtility.DisplayDialog("确认删除", $"确定要删除配置项 {titleLabel} 吗？", "确定", "取消"))
                    {
                        items.RemoveAt(i);
                        return; // 直接返回，避免索引错误
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                // 第一行：文件路径或文件名
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("路径/名称:", EditorStyles.miniBoldLabel);
                items[i].pathOrName = EditorGUILayout.TextField(items[i].pathOrName);
                
                // 第二行：使用说明
                EditorGUILayout.LabelField("使用说明:", EditorStyles.miniBoldLabel);
                items[i].description = EditorGUILayout.TextArea(items[i].description, 
                    GUILayout.MinHeight(60));
                EditorGUILayout.EndVertical();
                
                // 分割线（除了最后一个）
                if (i < items.Count - 1)
                {
                    EditorGUILayout.Space(2);
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                }
            }
            
            // 模块底部的添加按钮
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+ 添加配置", GUILayout.Width(100)))
            {
                items.Add(new ResourceItem());
            }
            EditorGUILayout.EndHorizontal();
        }
    }
    
    // 创建菜单项
    // [MenuItem("Assets/Create/ArtTools/资源配置表", false, 10)]
    // public static void CreateResourceTable()
    // {
    //     // 创建新的ScriptableObject
    //     ResourceTableData asset = ScriptableObject.CreateInstance<ResourceTableData>();
    //     
    //     // 设置默认值（可选）
    //     asset.sceneItems = new List<ResourceItem>();
    //     asset.characterItems = new List<ResourceItem>();
    //     asset.effectItems = new List<ResourceItem>();
    //     asset.uiItems = new List<ResourceItem>();
    //     
    //     // 保存文件
    //     string path = AssetDatabase.GetAssetPath(Selection.activeObject);
    //     if (path == "")
    //     {
    //         path = "Assets";
    //     }
    //     else if (System.IO.Path.GetExtension(path) != "")
    //     {
    //         path = path.Replace(System.IO.Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
    //     }
    //     
    //     string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New Resource Table.asset");
    //     AssetDatabase.CreateAsset(asset, assetPathAndName);
    //     AssetDatabase.SaveAssets();
    //     AssetDatabase.Refresh();
    //     EditorUtility.FocusProjectWindow();
    //     Selection.activeObject = asset;
    // }
}