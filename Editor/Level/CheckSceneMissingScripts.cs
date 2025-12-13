using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

public class CheckSceneMissingScripts : EditorWindow
{
    private List<GameObject> missingScriptObjects = new List<GameObject>();
    private Vector2 scrollPosition;
    private bool includeInactive = true;
    private bool includePrefabAssets = true;
    private int totalChecked = 0;
    private int totalMissing = 0;
    
    [MenuItem("ArtTools/Level/检查场景物体脚本丢失", false, 302)]
    public static void ShowWindow()
    {
        CheckSceneMissingScripts window = GetWindow<CheckSceneMissingScripts>();
        window.titleContent = new GUIContent("检查丢失脚本");
        window.minSize = new Vector2(400, 300);
        window.Show();
    }
    
    private void OnGUI()
    {
        DrawToolbar();
        DrawResults();
    }
    
    private void DrawToolbar()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.BeginHorizontal();
        
        // 检查当前场景按钮
        if (GUILayout.Button("检查当前场景", GUILayout.Height(30)))
        {
            CheckCurrentScene();
        }
        
        // 检查选中物体按钮
        if (GUILayout.Button("检查选中物体", GUILayout.Height(30)))
        {
            CheckSelectedObjects();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        // 选项设置
        EditorGUILayout.BeginHorizontal();
        includeInactive = EditorGUILayout.ToggleLeft("包含非激活物体", includeInactive, GUILayout.Width(150));
        includePrefabAssets = EditorGUILayout.ToggleLeft("包含Prefab资源", includePrefabAssets, GUILayout.Width(150));
        EditorGUILayout.EndHorizontal();
        
        // 显示统计信息
        if (totalChecked > 0)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField($"检查结果: 共检查 {totalChecked} 个物体，发现 {totalMissing} 个丢失脚本", EditorStyles.boldLabel);
            
            if (missingScriptObjects.Count > 0)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("修复选中物体的脚本", GUILayout.Height(25)))
                {
                    FixSelectedMissingScripts();
                }
                
                if (GUILayout.Button("全部修复", GUILayout.Height(25)))
                {
                    FixAllMissingScripts();
                }
                
                if (GUILayout.Button("清除列表", GUILayout.Height(25)))
                {
                    missingScriptObjects.Clear();
                    totalChecked = 0;
                    totalMissing = 0;
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawResults()
    {
        if (missingScriptObjects.Count == 0)
        {
            EditorGUILayout.HelpBox("没有发现丢失脚本的物体", MessageType.Info);
            return;
        }
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("丢失脚本的物体列表:", EditorStyles.boldLabel);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        for (int i = 0; i < missingScriptObjects.Count; i++)
        {
            if (missingScriptObjects[i] == null) continue;
            
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            
            // 显示物体信息
            EditorGUILayout.BeginVertical(GUILayout.Width(300));
            EditorGUILayout.LabelField($"物体: {missingScriptObjects[i].name}", EditorStyles.boldLabel);
            
            // 显示完整路径
            string path = GetGameObjectPath(missingScriptObjects[i]);
            EditorGUILayout.LabelField($"路径: {path}", EditorStyles.miniLabel);
            
            // 显示场景信息（如果是场景物体）
            if (missingScriptObjects[i].scene.IsValid())
            {
                EditorGUILayout.LabelField($"场景: {missingScriptObjects[i].scene.name}", EditorStyles.miniLabel);
            }
            
            EditorGUILayout.EndVertical();
            
            GUILayout.FlexibleSpace();
            
            // 操作按钮
            if (GUILayout.Button("定位", GUILayout.Width(60)))
            {
                Selection.activeObject = missingScriptObjects[i];
                EditorGUIUtility.PingObject(missingScriptObjects[i]);
                SceneView.lastActiveSceneView?.FrameSelected();
            }
            
            if (GUILayout.Button("修复", GUILayout.Width(60)))
            {
                FixMissingScriptsOnObject(missingScriptObjects[i]);
                missingScriptObjects.RemoveAt(i);
                totalMissing--;
                EditorUtility.SetDirty(missingScriptObjects[i]);
                break; // 退出循环，因为列表已更改
            }
            
            if (GUILayout.Button("删除", GUILayout.Width(60)))
            {
                if (EditorUtility.DisplayDialog("确认删除", $"确定要删除物体 {missingScriptObjects[i].name} 吗？", "确定", "取消"))
                {
                    GameObject.DestroyImmediate(missingScriptObjects[i], true);
                    missingScriptObjects.RemoveAt(i);
                    totalMissing--;
                    break; // 退出循环，因为列表已更改
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    private void CheckCurrentScene()
    {
        missingScriptObjects.Clear();
        totalChecked = 0;
        totalMissing = 0;
        
        GameObject[] allObjects;
        if (includeInactive)
        {
            // 包含非激活物体
            allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        }
        else
        {
            // 只包含激活物体
            allObjects = GameObject.FindObjectsOfType<GameObject>();
        }
        
        foreach (GameObject obj in allObjects)
        {
            // 过滤掉Prefab资源（除非用户选择包含）
            if (!includePrefabAssets && PrefabUtility.GetPrefabAssetType(obj) != PrefabAssetType.NotAPrefab)
                continue;
            
            // 只检查当前场景的物体
            if (!obj.scene.IsValid() || obj.scene.name == null)
                continue;
            
            totalChecked++;
            if (HasMissingScripts(obj))
            {
                missingScriptObjects.Add(obj);
                totalMissing++;
            }
        }
        
        Debug.Log($"检查完成！共检查 {totalChecked} 个物体，发现 {totalMissing} 个丢失脚本");
        EditorUtility.DisplayDialog("检查完成", 
            $"检查完成！\n共检查 {totalChecked} 个物体\n发现 {totalMissing} 个丢失脚本", 
            "确定");
    }
    
    private void CheckSelectedObjects()
    {
        missingScriptObjects.Clear();
        totalChecked = 0;
        totalMissing = 0;
        
        GameObject[] selectedObjects = Selection.gameObjects;
        
        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("提示", "请先在场景中选择要检查的物体", "确定");
            return;
        }
        
        foreach (GameObject obj in selectedObjects)
        {
            CheckGameObjectRecursive(obj);
        }
        
        Debug.Log($"检查完成！共检查 {totalChecked} 个物体，发现 {totalMissing} 个丢失脚本");
        EditorUtility.DisplayDialog("检查完成", 
            $"检查完成！\n共检查 {totalChecked} 个物体\n发现 {totalMissing} 个丢失脚本", 
            "确定");
    }
    
    private void CheckGameObjectRecursive(GameObject obj)
    {
        if (obj == null) return;
        
        totalChecked++;
        if (HasMissingScripts(obj))
        {
            missingScriptObjects.Add(obj);
            totalMissing++;
        }
        
        // 递归检查子物体
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            CheckGameObjectRecursive(obj.transform.GetChild(i).gameObject);
        }
    }
    
    private bool HasMissingScripts(GameObject obj)
    {
        Component[] components = obj.GetComponents<Component>();
        
        foreach (Component component in components)
        {
            if (component == null)
            {
                return true;
            }
        }
        
        return false;
    }
    
    private string GetGameObjectPath(GameObject obj)
    {
        string path = "/" + obj.name;
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = "/" + obj.name + path;
        }
        return path;
    }
    
    private void FixSelectedMissingScripts()
    {
        int fixedCount = 0;
        foreach (GameObject obj in Selection.gameObjects)
        {
            if (obj != null && HasMissingScripts(obj))
            {
                if (FixMissingScriptsOnObject(obj))
                {
                    fixedCount++;
                    missingScriptObjects.Remove(obj);
                }
            }
        }
        
        if (fixedCount > 0)
        {
            Debug.Log($"已修复 {fixedCount} 个物体的丢失脚本");
            totalMissing -= fixedCount;
            Repaint();
        }
    }
    
    private void FixAllMissingScripts()
    {
        if (missingScriptObjects.Count == 0) return;
        
        if (!EditorUtility.DisplayDialog("确认修复", 
            $"确定要修复所有 {missingScriptObjects.Count} 个物体的丢失脚本吗？", "确定", "取消"))
        {
            return;
        }
        
        int fixedCount = 0;
        List<GameObject> objectsToRemove = new List<GameObject>();
        
        foreach (GameObject obj in missingScriptObjects)
        {
            if (obj != null)
            {
                if (FixMissingScriptsOnObject(obj))
                {
                    fixedCount++;
                    objectsToRemove.Add(obj);
                }
            }
        }
        
        // 移除已修复的物体
        foreach (GameObject obj in objectsToRemove)
        {
            missingScriptObjects.Remove(obj);
        }
        
        totalMissing -= fixedCount;
        Debug.Log($"已修复 {fixedCount} 个物体的丢失脚本");
        Repaint();
    }
    
    private bool FixMissingScriptsOnObject(GameObject obj)
    {
        if (obj == null) return false;
        
        #if UNITY_2018_3_OR_NEWER
        // Unity 2018.3+ 使用新的API
        int removedCount = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
        if (removedCount > 0)
        {
            Debug.Log($"已从物体 {obj.name} 中移除 {removedCount} 个丢失的脚本");
            EditorUtility.SetDirty(obj);
            return true;
        }
        #else
        // 旧版本Unity使用序列化处理
        SerializedObject serializedObject = new SerializedObject(obj);
        SerializedProperty componentsProp = serializedObject.FindProperty("m_Component");
        
        int removedCount = 0;
        for (int i = componentsProp.arraySize - 1; i >= 0; i--)
        {
            SerializedProperty elementProp = componentsProp.GetArrayElementAtIndex(i);
            if (elementProp.objectReferenceValue == null)
            {
                componentsProp.DeleteArrayElementAtIndex(i);
                removedCount++;
            }
        }
        
        if (removedCount > 0)
        {
            serializedObject.ApplyModifiedProperties();
            Debug.Log($"已从物体 {obj.name} 中移除 {removedCount} 个丢失的脚本");
            EditorUtility.SetDirty(obj);
            return true;
        }
        #endif
        
        return false;
    }
}