using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace yuxuetian
{
    public class ReplaceUnpackScenePrefab : EditorWindow
    {
        private List<GameObject> prefabsToReplace = new List<GameObject>();
        private Vector2 scrollPosition;
        private bool matchExactName = false;
        private bool includeInactive = true;
        private bool preserveNumbersInName = true;
        private bool debugMode = false;
        
        // Unity复制物体时常见的后缀模式
        private readonly string[] unitySuffixPatterns = new string[]
        {
            @"\s+\(\d+\)$",          // 空格+(数字)
            @"\s+\d+$",              // 空格+数字
            @"_\d+$",                // 下划线+数字
            @"\(\d+\)$",             // (数字) 没有空格
            @"-\d+$",                // 短横线+数字
            @"\s+Copy$",            // 空格+Copy
            @"\s+Copy \d+$",        // 空格+Copy 数字
            @"\s+copy$",            // 空格+copy (小写)
            @"\s+copy \d+$",        // 空格+copy 数字 (小写)
            @"\s+Duplicate$",       // 空格+Duplicate
            @"\s+Duplicate \d+$",   // 空格+Duplicate 数字
            @"\s+Instance$",        // 空格+Instance
            @"\s+Instance \d+$",    // 空格+Instance 数字
        };
        
        [MenuItem("ArtTools/Prefab/批量替换场景中的Prefab", false, 141)]
        public static void ShowWindow()
        {
            EditorWindow window = GetWindow<ReplaceUnpackScenePrefab>();
            window.titleContent = new GUIContent("批量替换Prefab");
            window.minSize = new Vector2(500, 700);
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("批量替换Prefab配置", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            // 设置选项
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            matchExactName = EditorGUILayout.Toggle("精确匹配名称", matchExactName);
            preserveNumbersInName = EditorGUILayout.Toggle("保留名称中的数字", preserveNumbersInName);
            includeInactive = EditorGUILayout.Toggle("包含非激活物体", includeInactive);
            debugMode = EditorGUILayout.Toggle("调试模式", debugMode);
            
            if (!matchExactName)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("自动处理的名称后缀:", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField("• 空格+(1), 空格+(2)", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("• 空格+1, 空格+2", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("• (1), (2)", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("• 空格+Copy, 空格+Duplicate", EditorStyles.miniLabel);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10);
            
            // Prefab列表
            EditorGUILayout.LabelField("要替换的Prefab列表:", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("添加需要替换的Prefab，系统会自动查找场景中同名的展开物体进行替换", MessageType.Info);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            // 添加Prefab按钮
            if (GUILayout.Button("添加Prefab", GUILayout.Height(30)))
            {
                prefabsToReplace.Add(null);
            }
            EditorGUILayout.Space(5);
            
            // 显示Prefab列表
            for (int i = 0; i < prefabsToReplace.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                
                prefabsToReplace[i] = (GameObject)EditorGUILayout.ObjectField($"Prefab {i + 1}", prefabsToReplace[i], typeof(GameObject), false);
                
                // 显示干净名称预览
                if (prefabsToReplace[i] != null)
                {
                    string cleanName = GetCleanObjectName(prefabsToReplace[i].name);
                    EditorGUILayout.LabelField($"→ {cleanName}", GUILayout.Width(150));
                }
                // 删除按钮
                if (GUILayout.Button("×", GUILayout.Width(25)))
                {
                    prefabsToReplace.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            
            // 批量添加按钮
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("从选中物体批量添加", GUILayout.Height(30)))
            {
                AddSelectedPrefabs();
            }
            if (GUILayout.Button("从选中文件夹添加", GUILayout.Height(30)))
            {
                AddPrefabsFromFolder();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(3);
            
            // 清空按钮
            if (GUILayout.Button("清空列表", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("确认", "确定要清空Prefab列表吗？", "确定", "取消"))
                {
                    prefabsToReplace.Clear();
                }
            }
            EditorGUILayout.Space(10);
            
            // 操作按钮
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("扫描场景", GUILayout.Height(40)))
            {
                ScanSceneForUnpackedPrefabs();
            }
            
            if (GUILayout.Button("手动替换选中物体", GUILayout.Height(40)))
            {
                ManualReplaceSelectedObjects();
            }
            
            //GUI.backgroundColor = Color.green;
            if (GUILayout.Button("自动替换全部", GUILayout.Height(40)))
            {
                AutoReplaceAll();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();
        }
        
        private void AddSelectedPrefabs()
        {
            foreach (var obj in Selection.gameObjects)
            {
                // 检查是否是Prefab
                if (PrefabUtility.GetPrefabAssetType(obj) != PrefabAssetType.NotAPrefab && 
                    PrefabUtility.IsPartOfPrefabAsset(obj))
                {
                    if (!prefabsToReplace.Contains(obj))
                    {
                        prefabsToReplace.Add(obj);
                    }
                }
            }
        }
        
        private void AddPrefabsFromFolder()
        {
            string folderPath = EditorUtility.OpenFolderPanel("选择Prefab文件夹", "Assets", "");
            if (!string.IsNullOrEmpty(folderPath) && folderPath.Contains("Assets"))
            {
                string relativePath = "Assets" + folderPath.Substring(Application.dataPath.Length);
                string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { relativePath });
                
                int addedCount = 0;
                foreach (string guid in prefabGuids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab != null && !prefabsToReplace.Contains(prefab))
                    {
                        prefabsToReplace.Add(prefab);
                        addedCount++;
                    }
                }
                Debug.Log($"从文件夹添加了 {addedCount} 个Prefab");
            }
        }
        
        private void ScanSceneForUnpackedPrefabs()
        {
            Dictionary<string, int> unpackedPrefabs = new Dictionary<string, int>();
            Dictionary<string, List<string>> detailedNames = new Dictionary<string, List<string>>();
            
            GameObject[] allObjects = GetAllSceneObjects();
            
            int count = 0;
            foreach (var obj in allObjects)
            {
                if (obj == null || obj.hideFlags != HideFlags.None || 
                    PrefabUtility.GetPrefabAssetType(obj) != PrefabAssetType.NotAPrefab)
                    continue;
                
                string cleanName = GetCleanObjectName(obj.name);
                string originalName = obj.name;
                
                if (!unpackedPrefabs.ContainsKey(cleanName))
                {
                    unpackedPrefabs[cleanName] = 0;
                    detailedNames[cleanName] = new List<string>();
                }
                
                unpackedPrefabs[cleanName]++;
                if (!detailedNames[cleanName].Contains(originalName))
                {
                    detailedNames[cleanName].Add(originalName);
                }
                count++;
            }
            
            // 显示扫描结果
            Debug.Log($"扫描完成！找到 {count} 个已展开的物体，涉及 {unpackedPrefabs.Count} 种不同的名称。");
            
            // 生成报告
            string report = $"扫描结果 ({count} 个物体):\n";
            foreach (var kvp in unpackedPrefabs)
            {
                report += $"\n{kvp.Key}: {kvp.Value} 个";
                if (detailedNames.ContainsKey(kvp.Key) && detailedNames[kvp.Key].Count > 1)
                {
                    report += " [包含: ";
                    for (int i = 0; i < Mathf.Min(detailedNames[kvp.Key].Count, 3); i++)
                    {
                        if (i > 0) report += ", ";
                        report += detailedNames[kvp.Key][i];
                    }
                    if (detailedNames[kvp.Key].Count > 3) report += "...";
                    report += "]";
                }
            }
            
            EditorUtility.DisplayDialog("扫描结果", report, "确定");
        }
        
        private string GetCleanObjectName(string originalName)
        {
            if (matchExactName)
                return originalName;
            
            string cleanName = originalName;
            
            // 移除Unity常见的复制后缀
            foreach (string pattern in unitySuffixPatterns)
            {
                cleanName = Regex.Replace(cleanName, pattern, "");
            }
            
            // 如果开启了保留数字，且原始名称中有数字，但处理后数字被移除了
            // 这里可以添加逻辑来保留原始名称中的数字部分
            
            // 移除末尾的空格
            cleanName = cleanName.Trim();
            
            if (debugMode && originalName != cleanName)
            {
                Debug.Log($"名称清理: '{originalName}' -> '{cleanName}'");
            }
            
            return cleanName;
        }
        
        private bool ShouldReplaceObject(GameObject sceneObject, GameObject prefab)
        {
            if (sceneObject == null || prefab == null)
                return false;
            
            // 跳过已经是Prefab实例的物体
            if (PrefabUtility.GetPrefabAssetType(sceneObject) != PrefabAssetType.NotAPrefab)
                return false;
            
            string sceneObjectName = GetCleanObjectName(sceneObject.name);
            string prefabName = GetCleanObjectName(prefab.name);
            
            bool shouldReplace = sceneObjectName == prefabName;
            
            if (debugMode && shouldReplace)
            {
                Debug.Log($"匹配成功: 场景物体='{sceneObject.name}' (清理后:'{sceneObjectName}') " +
                         $"Prefab='{prefab.name}' (清理后:'{prefabName}')");
            }
            
            return shouldReplace;
        }
        
        private GameObject[] GetAllSceneObjects()
        {
            if (includeInactive)
            {
                return Resources.FindObjectsOfTypeAll<GameObject>();
            }
            else
            {
                FindObjectsSortMode sortMode = FindObjectsSortMode.None;
                return GameObject.FindObjectsByType<GameObject>(sortMode);
            }
        }
        
        private void ManualReplaceSelectedObjects()
        {
            if (prefabsToReplace.Count == 0)
            {
                EditorUtility.DisplayDialog("错误", "请先在列表中添加至少一个Prefab", "确定");
                return;
            }
            
            GameObject[] selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("提示", "请先在场景中选择要替换的物体", "确定");
                return;
            }
            
            int totalReplaced = 0;
            Undo.SetCurrentGroupName("批量替换Prefab");
            int undoGroup = Undo.GetCurrentGroup();
            
            try
            {
                EditorUtility.DisplayProgressBar("替换中...", "正在替换选中的物体", 0);
                
                for (int i = 0; i < selectedObjects.Length; i++)
                {
                    GameObject selectedObj = selectedObjects[i];
                    EditorUtility.DisplayProgressBar("替换中...", 
                        $"正在处理: {selectedObj.name}", 
                        (float)i / selectedObjects.Length);
                    
                    // 为每个选中的物体查找匹配的Prefab
                    foreach (var prefab in prefabsToReplace)
                    {
                        if (prefab == null) continue;
                        
                        if (ShouldReplaceObject(selectedObj, prefab))
                        {
                            if (ReplaceObjectWithPrefab(selectedObj, prefab))
                                totalReplaced++;
                            break; // 一个物体只替换一次
                        }
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                Undo.CollapseUndoOperations(undoGroup);
            }
            
            Debug.Log($"手动替换完成！共替换了 {totalReplaced} 个物体。");
            SceneView.RepaintAll();
        }
        
        private void AutoReplaceAll()
        {
            if (prefabsToReplace.Count == 0)
            {
                EditorUtility.DisplayDialog("错误", "请先在列表中添加至少一个Prefab", "确定");
                return;
            }
            
            // 先统计可能匹配的数量
            GameObject[] allSceneObjects = GetAllSceneObjects();
            Dictionary<string, int> matchCount = new Dictionary<string, int>();
            
            foreach (var prefab in prefabsToReplace)
            {
                if (prefab == null) continue;
                string prefabName = GetCleanObjectName(prefab.name);
                matchCount[prefabName] = 0;
            }
            
            foreach (var sceneObj in allSceneObjects)
            {
                if (sceneObj == null || sceneObj.hideFlags != HideFlags.None) continue;
                
                foreach (var prefab in prefabsToReplace)
                {
                    if (prefab == null) continue;
                    
                    if (ShouldReplaceObject(sceneObj, prefab))
                    {
                        string prefabName = GetCleanObjectName(prefab.name);
                        matchCount[prefabName]++;
                        break;
                    }
                }
            }
            
            // 显示确认对话框
            string confirmMessage = $"将自动替换场景中所有匹配的已展开Prefab:\n\n";
            int totalMatches = 0;
            foreach (var kvp in matchCount)
            {
                confirmMessage += $"{kvp.Key}: {kvp.Value} 个\n";
                totalMatches += kvp.Value;
            }
            confirmMessage += $"\n总计: {totalMatches} 个物体\n确定要继续吗？";
            
            if (totalMatches == 0)
            {
                EditorUtility.DisplayDialog("提示", "没有找到需要替换的物体", "确定");
                return;
            }
            
            if (!EditorUtility.DisplayDialog("确认替换", confirmMessage, "确定", "取消"))
            {
                return;
            }
            
            int totalReplaced = 0;
            int totalChecked = 0;
            Undo.SetCurrentGroupName("自动批量替换Prefab");
            int undoGroup = Undo.GetCurrentGroup();
            
            try
            {
                // 先收集所有需要替换的物体
                List<GameObject> objectsToReplace = new List<GameObject>();
                List<GameObject> prefabsForReplacement = new List<GameObject>();
                
                EditorUtility.DisplayProgressBar("扫描中...", "正在查找需要替换的物体", 0);
                
                for (int i = 0; i < allSceneObjects.Length; i++)
                {
                    totalChecked++;
                    
                    if (i % 100 == 0 && EditorUtility.DisplayCancelableProgressBar("扫描中...", 
                        $"已检查 {i}/{allSceneObjects.Length} 个物体", 
                        (float)i / allSceneObjects.Length))
                    {
                        Debug.Log("用户取消了操作");
                        return;
                    }
                    
                    GameObject sceneObj = allSceneObjects[i];
                    if (sceneObj == null || sceneObj.hideFlags != HideFlags.None)
                        continue;
                    
                    // 检查是否为已展开的Prefab
                    if (PrefabUtility.GetPrefabAssetType(sceneObj) != PrefabAssetType.NotAPrefab)
                        continue;
                    
                    // 查找匹配的Prefab
                    foreach (var prefab in prefabsToReplace)
                    {
                        if (prefab == null) continue;
                        
                        if (ShouldReplaceObject(sceneObj, prefab))
                        {
                            objectsToReplace.Add(sceneObj);
                            prefabsForReplacement.Add(prefab);
                            break;
                        }
                    }
                }
                
                // 执行替换
                for (int i = 0; i < objectsToReplace.Count; i++)
                {
                    if (EditorUtility.DisplayCancelableProgressBar("替换中...", 
                        $"正在替换: {objectsToReplace[i].name} ({i + 1}/{objectsToReplace.Count})", 
                        (float)i / objectsToReplace.Count))
                    {
                        Debug.Log("用户取消了操作");
                        break;
                    }
                    
                    if (ReplaceObjectWithPrefab(objectsToReplace[i], prefabsForReplacement[i]))
                        totalReplaced++;
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                Undo.CollapseUndoOperations(undoGroup);
            }
            
            Debug.Log($"自动替换完成！共检查了 {totalChecked} 个物体，替换了 {totalReplaced} 个物体。");
            SceneView.RepaintAll();
        }
        
        private bool ReplaceObjectWithPrefab(GameObject oldObject, GameObject prefab)
        {
            if (oldObject == null || prefab == null)
                return false;
            
            // 检查Prefab是否有效
            if (PrefabUtility.GetPrefabAssetType(prefab) == PrefabAssetType.NotAPrefab)
            {
                Debug.LogWarning($"{prefab.name} 不是有效的Prefab，跳过替换");
                return false;
            }
            
            if (debugMode)
            {
                Debug.Log($"正在替换: {oldObject.name} -> {prefab.name} (位于 {oldObject.transform.parent?.name ?? "根层级"})");
            }
            
            // 创建新Prefab实例
            GameObject newObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (newObject == null)
            {
                Debug.LogError($"无法实例化Prefab: {prefab.name}");
                return false;
            }
            
            // 记录所有操作到Undo系统
            Undo.RegisterCompleteObjectUndo(oldObject, "Replace Prefab");
            
            // 复制变换信息
            newObject.transform.position = oldObject.transform.position;
            newObject.transform.rotation = oldObject.transform.rotation;
            newObject.transform.localScale = oldObject.transform.localScale;
            
            // 设置父物体
            newObject.transform.SetParent(oldObject.transform.parent, false);
            
            // 保持兄弟索引以维持顺序
            newObject.transform.SetSiblingIndex(oldObject.transform.GetSiblingIndex());
            
            // 复制名称（保持原来的显示名称）
            newObject.name = oldObject.name;
            
            // 记录新对象的创建
            Undo.RegisterCreatedObjectUndo(newObject, "Create Prefab Instance");
            
            // 销毁旧物体
            Undo.DestroyObjectImmediate(oldObject);
            
            return true;
        }
    }
}