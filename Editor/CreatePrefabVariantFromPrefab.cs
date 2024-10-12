/*
 * 基于选择的预制体创建新的变体
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

public class CreatePrefabVariantFromPrefab : EditorWindow
{
    public string PrefabPath = "Assets/Temp";
    public bool isPrefab = false;

    //添加预制体
    public bool isNeedCustomComponent = false;
    public string EditorPrefabVariantPath = "Assets/Resources/Designer";

    private MonoScript customScript;

    [MenuItem("ArtTools/Model/创建预制体变体" , false , 122)]  
    private static void ShowWindow()  
    {  
        GetWindow<CreatePrefabVariantFromPrefab>("Create Prefab Variant From Prefab");  
    }  
    
    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("指定路径：");
        PrefabPath = EditorGUILayout.TextField(PrefabPath);
        EditorGUILayout.EndHorizontal();

        isPrefab = EditorGUILayout.Toggle("是否生成原始预制体", isPrefab);
        
        if (GUILayout.Button("开始执行"))
        {
            //
            CreateVariant();
        }
    }
    
    //检查路径是否存在，如果不存在就直接创建路径
    public void CheckPath(string str)
    {
        if (!Directory.Exists(str))
        {
            Directory.CreateDirectory(str);
        }
    }

    public void CreateVariant()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length > 0)
        {
            foreach (GameObject selectObject in selectedObjects)
            {
                string selectObjectAssetPath = AssetDatabase.GetAssetPath(selectObject);
                
                if (!string.IsNullOrEmpty(selectObjectAssetPath) && Path.GetExtension(selectObjectAssetPath).ToLower() == ".prefab")
                {
                    if (isPrefab)
                    {
                        //创建预制体形式
                        GameObject emptyGameObject = new GameObject(selectObject.name);
                        GameObject selectPrefab = selectObject as GameObject;
                        
                        GameObject prefabInstance = Instantiate(selectPrefab, emptyGameObject.transform);
                        
                        ConvertToPrefabInstanceSettings convertToPrefabInstanceSettings = new ConvertToPrefabInstanceSettings();
                        InteractionMode interactionMode = InteractionMode.AutomatedAction;
                        PrefabUtility.ConvertToPrefabInstance(prefabInstance , selectPrefab , convertToPrefabInstanceSettings,interactionMode);
                        
                        string originalPrefabName = emptyGameObject.name + ".prefab";
                        string originalPrefabFullPath = Path.Combine(PrefabPath, originalPrefabName);
                        
                        if (!File.Exists(originalPrefabFullPath))
                        {
                            // 保存为新的预制体  
                            PrefabUtility.SaveAsPrefabAsset(emptyGameObject, originalPrefabFullPath);
                            DestroyImmediate(prefabInstance); // 销毁临时创建的GameObject  
                        }
                        else
                        {
                            DestroyImmediate(prefabInstance); // 销毁临时创建的GameObject  
                            Debug.LogWarning("该Prefab已经存在，无法再次创建！" + originalPrefabName);
                        }
                    
                        DestroyImmediate(emptyGameObject);
                    }
                    else
                    {
                        //创建预制体变体形式
                        GameObject prefabInstance = Instantiate(selectObject, Vector3.zero, Quaternion.identity) as GameObject;  
                        ConvertToPrefabInstanceSettings convertToPrefabInstanceSettings = new ConvertToPrefabInstanceSettings();
                        InteractionMode interactionMode = InteractionMode.AutomatedAction;
                        PrefabUtility.ConvertToPrefabInstance(prefabInstance,selectObject,convertToPrefabInstanceSettings,interactionMode);
  
                        // 设置新预制体的名称和路径  
                        string originalPrefabName = prefabInstance.name + ".prefab";  
                        string originalPrefabFullPath = Path.Combine(PrefabPath, originalPrefabName);

                        if (!File.Exists(originalPrefabFullPath))
                        {
                            // 保存为新的预制体  
                            PrefabUtility.SaveAsPrefabAsset(prefabInstance, originalPrefabFullPath);
                            DestroyImmediate(prefabInstance); // 销毁临时创建的GameObject  
                        }
                        else
                        {
                            DestroyImmediate(prefabInstance); // 销毁临时创建的GameObject  
                            Debug.LogWarning("该Prefab已经存在，无法再次创建！" + originalPrefabName);
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("选择的文件不符合要求！");
                }
                AssetDatabase.Refresh();  
            }
        }
    }
}
