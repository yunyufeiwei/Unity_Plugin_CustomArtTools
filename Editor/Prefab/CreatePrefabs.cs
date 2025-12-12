/*
 * 该方法是直接使用模型创建预制体，该预制体是的组件直接就在父类上显示
 */

using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace yuxuetian
{
    public class CreatePrefabs : EditorWindow
    {
        //声明用来保存创建Prefab的路径
        public static string localPath = "Assets/Art/";
        private bool isFbx;

        [MenuItem("ArtTools/Prefab/创建预制体(无父类)", false, 120)]
        public static void ShowWindow()
        {
            EditorWindow window = GetWindow<CreatePrefabs>();
            window.titleContent = new GUIContent("创建prefab");
        }

        public void OnGUI()
        {
            //使用水平分布，将其中的GUI按水平分布的方式排列
            GUILayout.BeginHorizontal();
            GUILayout.Label("指定路径:", EditorStyles.boldLabel);
            localPath = EditorGUILayout.TextField(localPath);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("是否有选择Prefab"))
            {
                GetSelectAssetExtensions();
            }

            if (GUILayout.Button("创建Prefabs"))
            {
                CreatePrefabsAssets();
            }

            GUILayout.EndHorizontal();
        }

        //选择资产的类型
        public bool GetSelectAssetExtensions()
        {
            Object[] selectObjects = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);

            foreach (Object obj in selectObjects)
            {
                string assetPath = AssetDatabase.GetAssetPath(obj);

                if (!string.IsNullOrEmpty(assetPath) && Path.GetExtension(assetPath).ToLower() == ".fbx" || Path.GetExtension(assetPath).ToLower() == ".obj")
                {
                    isFbx = true;
                    //打印所有选择的文件名以及文件的后缀名
                    Debug.Log("当前选择的文件是：" + obj.name + assetPath.ToLower());
                }
                else
                {
                    Debug.LogWarning("当前存在prefab文件：" + assetPath);
                }
            }

            return isFbx;
        }

        public void CreatePrefabsAssets()
        {
            GetSelectAssetExtensions();

            if (isFbx == true)
            {
                if (!Directory.Exists(localPath))
                {
                    Directory.CreateDirectory(localPath);
                }

                GameObject[] objectArray = Selection.gameObjects;

                foreach (GameObject obj in objectArray)
                {
                    if (obj != null)
                    {
                        string prefabPath = Path.Combine(localPath, obj.name + ".prefab");
                        prefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabPath);

                        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, prefabPath);
                        if (prefab != null)
                        {
                            Debug.Log("Prefab created at path: " + prefabPath);
                        }
                        else
                        {
                            Debug.LogError("Failed to create Prefab for " + obj.name);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("未选择任何模型，请选中后再创建Prefab！");
                    }
                }
            }
            else
            {
                return;
            }
        }
    }
}
