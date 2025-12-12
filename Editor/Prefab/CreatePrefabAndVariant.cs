/*
 * 基于选择的模型fbx创建预制体，并同时创建预制体变体
 * 创建的预制体父类是一个空的GameObject，其上没有添加任何组件，尽可能的保证了父类的结构干净
 */

using UnityEditor;  
using UnityEngine;  
using System.IO;
using Unity.Mathematics;

namespace yuxuetian
{
    //批量创建Prefab及其变体，使用嵌套的方式
    public class CreatePrefabsAndVariants : EditorWindow
    {
        //预制体以及预制体变体的路径
        public string CharacterPrefabPath = "Assets/Art/Character/";
        public string CharacterVariantPath = "Assets/Resources/Prefabs/Character/Player_Variant/";
        public string WeaponPrefabPath = "Assets/Art/Weapon/Prefab/";
        public string WeaponVariantPath = "Assets/Resources/Prefabs/Weapon/Weapon_Variant/";
        public string VehiclePrefabPath = "Assets/Art/Vehicle/Prefab/";
        public string VehicleVariantPath = "Assets/Resources/Prefabs/Vehicle/Vehicle_Variant/";
        public string ScenePrefabPath = "Assets/Art/Scene/Prefab/";
        public string SceneVariantPath = "Assets/Resources/Prefabs/Scene/Scene_Variant/";

        public bool isNeedCreateCharacterVariant = true;
        public bool isNeedCreateWeaponVariant = true;
        public bool isNeedCreateVehicleVariant = true;
        public bool isNeedCreateSceneVariant = false;

        public bool isShowCharacterPathInput = false;
        public bool isShowWeaponPathInput = false;
        public bool isShowVehiclePathInput = false;
        public bool isShowScenePathInput = false;

        private Vector2 scroPos;

        [MenuItem("ArtTools/Prefab/创建预制体以及变体(同时)",false,121)]
        private static void ShowWindow()
        {
            GetWindow<CreatePrefabsAndVariants>("Create Prefabs and Variants");
        }

        private void OnGUI()
        {
            GUIStyle lableStyle = new GUIStyle();
            lableStyle.fontSize = 14;
            lableStyle.normal.textColor = Color.white;

            #region 角色模块

            //角色GUI绘制，并将其使用一个box的背景框住
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            isNeedCreateCharacterVariant = EditorGUILayout.Toggle("是否需要创建角色变体", isNeedCreateCharacterVariant);
            isShowCharacterPathInput = EditorGUILayout.Foldout(isShowCharacterPathInput, "角色路径");
            if (isShowCharacterPathInput)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("指定预制体路径:", EditorStyles.label, GUILayout.MaxWidth(150));
                CharacterPrefabPath = EditorGUILayout.TextField(CharacterPrefabPath);
                EditorGUILayout.EndHorizontal();

                //如果不需要创建变体，则在面板上不绘制变体路径
                if (isNeedCreateCharacterVariant)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("指定变体路径:", EditorStyles.label, GUILayout.MaxWidth(150));
                    CharacterVariantPath = EditorGUILayout.TextField(CharacterVariantPath);
                    EditorGUILayout.EndHorizontal();
                }
            }

            //如果不需要创建遍历，则在面板上不需要绘制变体的设置路径
            if (isNeedCreateCharacterVariant)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Player_Variant路径"))
                {
                    CharacterVariantPath = "Assets/Resources/Prefabs/Character/Player_Variant/";
                }
                else if (GUILayout.Button("Enemy_Variant路径"))
                {
                    CharacterVariantPath = "Assets/Resources/Prefabs/Character/Enemy_Variant/";
                }

                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("创建角色预制体及其变体"))
            {
                CheckPath(CharacterPrefabPath);
                if (isNeedCreateCharacterVariant)
                {
                    CheckPath(CharacterVariantPath);
                }

                CreatCharacterPrefab(CharacterPrefabPath, CharacterVariantPath, isNeedCreateCharacterVariant);
            }

            EditorGUILayout.EndVertical();

            #endregion

            #region 武器模块

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            isNeedCreateWeaponVariant = EditorGUILayout.Toggle("是否需要创建武器变体", isNeedCreateWeaponVariant);
            isShowWeaponPathInput = EditorGUILayout.Foldout(isShowWeaponPathInput, "武器路径");
            if (isShowWeaponPathInput)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("指定预制体路径:", EditorStyles.label, GUILayout.MaxWidth(150));
                WeaponPrefabPath = EditorGUILayout.TextField(WeaponPrefabPath);
                EditorGUILayout.EndHorizontal();

                if (isNeedCreateWeaponVariant)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("指定变体路径:", EditorStyles.label, GUILayout.MaxWidth(150));
                    WeaponVariantPath = EditorGUILayout.TextField(WeaponVariantPath);
                    EditorGUILayout.EndHorizontal();
                }

            }

            if (GUILayout.Button("创建武器预制体及其变体"))
            {
                CheckPath(WeaponPrefabPath);
                if (isNeedCreateWeaponVariant)
                {
                    CheckPath(WeaponVariantPath);
                }

                CreateWeaponPrefab(WeaponPrefabPath, WeaponVariantPath, isNeedCreateWeaponVariant);
            }

            EditorGUILayout.EndVertical();


            #endregion

            #region 载具模块

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            isNeedCreateVehicleVariant = EditorGUILayout.Toggle("是否需要创建载具变体", isNeedCreateVehicleVariant);
            isShowVehiclePathInput = EditorGUILayout.Foldout(isShowVehiclePathInput, "载具路径");
            if (isShowVehiclePathInput)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("指定预制体路径:", EditorStyles.label, GUILayout.MaxWidth(150));
                VehiclePrefabPath = EditorGUILayout.TextField(VehiclePrefabPath);
                EditorGUILayout.EndHorizontal();

                if (isNeedCreateVehicleVariant)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("指定变体路径:", EditorStyles.label, GUILayout.MaxWidth(150));
                    VehicleVariantPath = EditorGUILayout.TextField(VehicleVariantPath);
                    EditorGUILayout.EndHorizontal();
                }
            }

            if (GUILayout.Button("创建载具预制体及其变体"))
            {
                CheckPath(VehiclePrefabPath);
                if (isNeedCreateVehicleVariant)
                {
                    CheckPath(VehicleVariantPath);
                }

                CreateVehicalePrefab(VehiclePrefabPath, VehicleVariantPath, isNeedCreateVehicleVariant);
            }

            EditorGUILayout.EndVertical();

            #endregion


            #region 场景模块

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            isNeedCreateSceneVariant = EditorGUILayout.Toggle("是否需要创建场景变体", isNeedCreateSceneVariant);
            isShowScenePathInput = EditorGUILayout.Foldout(isShowScenePathInput, "场景路径");
            if (isShowScenePathInput)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("指定预制体路径:", EditorStyles.label, GUILayout.MaxWidth(150));
                ScenePrefabPath = EditorGUILayout.TextField(ScenePrefabPath);
                EditorGUILayout.EndHorizontal();

                if (isNeedCreateSceneVariant)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("指定变体路径:", EditorStyles.label, GUILayout.MaxWidth(150));
                    SceneVariantPath = EditorGUILayout.TextField(SceneVariantPath);
                    EditorGUILayout.EndHorizontal();
                }

            }

            if (GUILayout.Button("创建场景预制体及其变体"))
            {
                CheckPath(ScenePrefabPath);
                if (isNeedCreateSceneVariant)
                {
                    CheckPath(SceneVariantPath);
                }

                CreateScenePrefab(ScenePrefabPath, SceneVariantPath, isNeedCreateSceneVariant);
            }

            EditorGUILayout.EndHorizontal();

            #endregion

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            //滑动条GUI
            scroPos = EditorGUILayout.BeginScrollView(scroPos);
            GUILayout.Label("说明：\n" +
                            "1.第一个路径表示了使用该模型创建预制体的路径，该路径下的预制体是提供给美术使用.\n" +
                            "2.第二个路径表示了该预制体变体的路径，该变体提供给程序使用开发功能，其路径根据类型可能需要调整......", lableStyle);
            GUILayout.Label("可能需要调整的路径：\n" +
                            "1.角色预制体路径，根据需要修改不同的路径\n" +
                            "2.角色预制体变体的路径，根据需要修改不同的路径，当前提供了3种路径，点击即可直接修改预设路径。\n" +
                            "3.场景的预制体路径，需要根据其分类修改路径.\n", lableStyle);
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();
        }

        //检查路径是否存在，如果不存在就直接创建路径
        public void CheckPath(string str)
        {
            if (!Directory.Exists(str))
            {
                Directory.CreateDirectory(str);
            }
        }

        public void CreatePrefabAndVariants(string OriginalprefabPath, string VariantPrefabPath, bool isNeedVariant)
        {
            //申明一个数组来存储所选择的物体
            GameObject[] selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length > 0)
            {
                foreach (GameObject selectedObject in selectedObjects)
                {
                    //定义一个字符创，用来获得每遍历的每一个物体的文件名和拓展名
                    string selectObjectAssetPath = AssetDatabase.GetAssetPath(selectedObject);
                    //判断每一个物体的拓展名是否为.fbx,如果是在会执行创建预制体和预制体变体内容，否则将直接给出警告提示
                    if (!string.IsNullOrEmpty(selectObjectAssetPath) &&
                        Path.GetExtension(selectObjectAssetPath).ToLower() == ".fbx")
                    {
                        // 创建原始Prefab（空的GameObject）  
                        GameObject selectedObjectFBX = selectedObject as GameObject;
                        GameObject emptyPrefabGo = new GameObject(selectedObject.name); //prefab的根名称

                        // 实例化FBX模型作为原始Prefab的子对象实例化在空的prefab中 
                        GameObject fbxInstance = Instantiate(selectedObjectFBX, emptyPrefabGo.transform);

                        //实例化的模型和原始的FBX没有关联，使用prefab中的reconnect来重新和FBX关联
                        // //PrefabUtility.ConnectGameObjectToPrefab(fbxInstance , selectedObjectPrefab);  //旧方法
                        ConvertToPrefabInstanceSettings convertToPrefabInstanceSettings =
                            new ConvertToPrefabInstanceSettings();
                        InteractionMode interactionMode = InteractionMode.AutomatedAction;
                        PrefabUtility.ConvertToPrefabInstance(fbxInstance, selectedObjectFBX,
                            convertToPrefabInstanceSettings, interactionMode);

                        string OriginalPrefabName = emptyPrefabGo.name + ".prefab";
                        string OriginalPrefabFullPath =
                            Path.Combine(OriginalprefabPath, OriginalPrefabName); //全路径名，文件名+拓展名(类型) 
                        // 保存空的Prefab  
                        if (!File.Exists(OriginalPrefabFullPath))
                        {
                            PrefabUtility.SaveAsPrefabAsset(emptyPrefabGo, OriginalPrefabFullPath);
                            DestroyImmediate(emptyPrefabGo); // 销毁临时创建的GameObject  
                        }
                        else
                        {
                            DestroyImmediate(emptyPrefabGo); // 销毁临时创建的GameObject  
                            Debug.LogWarning("该Prefab已经存在，无法再次创建！" + OriginalPrefabName);
                        }

                        // 加载刚创建的Prefab  
                        GameObject originalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(OriginalPrefabFullPath);
                        if (isNeedVariant)
                        {
                            //创建变体Prefab实例，同样使用选择的模型创建prefab变体
                            GameObject emptyPrefabInstanceGo = new GameObject(selectedObject.name); //prefab的跟名称
                            GameObject variantPrefabInstance =
                                Instantiate(originalPrefab, Vector3.zero, quaternion.identity);
                            //与创建Prefab不同的是，变体里面加入的不是fbx，而是上一步中创建的原始prefab
                            PrefabUtility.ConvertToPrefabInstance(variantPrefabInstance, originalPrefab,
                                convertToPrefabInstanceSettings, interactionMode);

                            string VariantPrefabName = variantPrefabInstance.name;
                            int parentesisIndex = VariantPrefabName.IndexOf('(');
                            if (parentesisIndex != -1)
                            {
                                string newVariantPrefabName = VariantPrefabName.Substring(0, parentesisIndex);
                                VariantPrefabName = newVariantPrefabName + "_Variant" + ".prefab";
                            }

                            string VariantPrefabFullPath = Path.Combine(VariantPrefabPath, VariantPrefabName);

                            // 清理：销毁变体Prefab的实例,如果不销毁，场景会存在实例化的GameObject
                            if (!File.Exists(VariantPrefabFullPath))
                            {
                                PrefabUtility.SaveAsPrefabAsset(variantPrefabInstance, VariantPrefabFullPath);
                                DestroyImmediate(emptyPrefabInstanceGo);
                                DestroyImmediate(variantPrefabInstance);
                            }
                            else
                            {
                                DestroyImmediate(emptyPrefabInstanceGo);
                                DestroyImmediate(variantPrefabInstance);
                            }
                            // Debug.Log($"预制体变体创建成功: {variantPrefabFullPath}");  
                        }

                        // 刷新AssetDatabase以显示新创建的Prefab  
                        AssetDatabase.Refresh();

                        // Debug.Log($"预制体创建成功: {originalPrefab}");  
                    }
                    else
                    {
                        Debug.LogWarning("选择的物体存在不是.fbx格式的文件！");
                    }
                }
            }
            else
            {
                Debug.LogError("未选中任何物体!");
            }
        }

        public void CreatCharacterPrefab(string ori, string variant, bool isNeedVariant)
        {
            CreatePrefabAndVariants(ori, variant, isNeedVariant);
        }

        public void CreateWeaponPrefab(string ori, string variant, bool isNeedVariant)
        {
            CreatePrefabAndVariants(ori, variant, isNeedVariant);
        }

        public void CreateVehicalePrefab(string ori, string variant, bool isNeedVariant)
        {
            CreatePrefabAndVariants(ori, variant, isNeedVariant);
        }

        public void CreateScenePrefab(string ori, string variant, bool isNeedVariant)
        {
            CreatePrefabAndVariants(ori, variant, isNeedVariant);
        }
    }
}