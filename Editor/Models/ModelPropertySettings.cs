using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

namespace yuxuetian
{
    public class ModelPropertySettings : EditorWindow
    {
        public enum SetIndexFormat
        {
            Atuo,
            [InspectorName("16 bits")] UInt16,
            [InspectorName("32 bits")] UInt32
        }

        public enum SetNormal
        {
            Import,
            Calculate,
            None
        }

        public enum SetTangent
        {
            Import,
            CalculateLegacy,
            CalculateLegacyWithSplitTangents,
            CalculateMikktspace,
            None
        }

        public enum SetAnimationType
        {
            None,
            Legacy,
            Generic,
            Humanoid
        }

        //声明一个列表，用来存储选择的模型数据
        private List<GameObject> models = new List<GameObject>();
        private List<String> modelStrs = new List<String>();
        private List<ModelImporter> modelImoprters = new List<ModelImporter>();

        #region Model属性变量声明

        //生命GUI变量名
        public bool isImportCameras = false;
        public bool isImportLight = false;
        public bool isReadWrite = false;
        public bool isOptimizeMesh = true;
        public bool isGenerateColliders = false;
        public SetIndexFormat setIndexFormat = SetIndexFormat.UInt16;
        public SetNormal setNormals = SetNormal.Import;
        public SetTangent setTangents = SetTangent.CalculateMikktspace;
        public bool isSwapUVs = false;
        public bool isGenerateLightmapUVs = false;

        //定义折叠开关变量
        public bool _ModelPropertyfoldout = false;
        // private bool isIndexFormat = true;

        #endregion

        #region Rig属性变量声明

        public bool isRig = false;
        public SetAnimationType setAnimationType = SetAnimationType.Generic;

        //定义骨架折叠开关变量
        private bool _RigPropertyFoldout = false;

        #endregion

        #region Aniamtion属性变量声明

        public bool hasAnimation = false;
        public bool importAnimation = false;

        //定义模型是否带有动画的折叠开关
        private bool _AnimationPropertyFoldout = false;

        #endregion

        #region Material属性变量声明

        public bool isMaterial = false;

        private bool _MaterialPropertyFoldout = false;

        #endregion


        [MenuItem("ArtTools/Model/模型导入设置", false, 101)]
        public static void showWindow()
        {
            //显示窗口，该行作用让自定义窗口打开
            EditorWindow window = GetWindow<ModelPropertySettings>();
            //取代了工具的titlecontent名称
            window.titleContent = new GUIContent("模型属性设置");

            //显示窗口的大小，但这里的max.size在当前版本（Unity2023.2。20）无法生效
            Vector2 size = new Vector2(400.0f, 600.0f);
            window.minSize = size;
            window.maxSize = size;
        }

        //定义点击函数，执行函数体里面的内容
        public void OnGUI()
        {
            //-----------------------------------------------------Model-----------------------------------------------------------------------//
            //绘制一个折叠窗口，该窗口里面包含了模型相关的重要属性，默认设置为false，表示了该折叠窗口默认为折叠状态,需要注意将_foldout的状态在保存会该变量
            _ModelPropertyfoldout = EditorGUILayout.Foldout(_ModelPropertyfoldout, "ModelProperty");
            if (_ModelPropertyfoldout)
            {
                //将GUI绘制在面板上
                isImportCameras = GUILayout.Toggle(isImportCameras, "ImportCamera:(是否导入相机)");
                isImportLight = GUILayout.Toggle(isImportLight, "ImportLights:(是否导入灯光)");
                isReadWrite = GUILayout.Toggle(isReadWrite, "Read/Write:(是否启用读写)");
                isOptimizeMesh = GUILayout.Toggle(isOptimizeMesh, "OptimizeMesh:(优化网格)");
                isGenerateColliders = GUILayout.Toggle(isGenerateColliders, "GenerateColliders:(是否生成碰撞)");

                //绘制网格索引缓冲区的位数，默认索引格式为 16 位，因为这种格式占用的内存和带宽较少。
                GUILayout.BeginHorizontal();
                GUILayout.Label("IndexFormat", EditorStyles.label);
                GUILayout.FlexibleSpace(); //填充左侧剩余空间，确保右边的内容能左对齐
                setIndexFormat = (SetIndexFormat)EditorGUILayout.EnumPopup(setIndexFormat);
                GUILayout.EndHorizontal();

                //绘制Normal的选择项
                GUILayout.BeginHorizontal();
                GUILayout.Label("Normals", EditorStyles.label);
                GUILayout.FlexibleSpace(); //填充左侧剩余空间，确保右边的内容能左对齐
                setNormals = (SetNormal)EditorGUILayout.EnumPopup(setNormals);
                GUILayout.EndHorizontal();

                //绘制Tangents的选择项
                GUILayout.BeginHorizontal();
                GUILayout.Label("Tangents", EditorStyles.label);
                GUILayout.FlexibleSpace(); //填充左侧剩余空间，确保右边的内容能左对齐
                setTangents = (SetTangent)EditorGUILayout.EnumPopup(setTangents);
                GUILayout.EndHorizontal();

                isSwapUVs = GUILayout.Toggle(isSwapUVs, "SwapUVs:(交换UV)");
                isGenerateLightmapUVs = GUILayout.Toggle(isGenerateLightmapUVs, "GenerateLightmapUVs:(生成光照题图)");

                GUILayout.Space(10);
            }

            //绘制GUI按钮，如果点击执行条件里面的内容
            if (GUILayout.Button("执行模型属性设置"))
            {
                AddModelData();
                SettingModelProperty();

                // Debug.Log("点击反馈！");
            }

            GUILayout.Space(20);

            //-----------------------------------------------------Rig-----------------------------------------------------------------------//
            _RigPropertyFoldout = EditorGUILayout.Foldout(_RigPropertyFoldout, "RigProperty");
            if (_RigPropertyFoldout)
            {
                isRig = GUILayout.Toggle(isRig, "Rig:(是否需要骨架)");
                if (isRig)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("AnimationType", EditorStyles.label);
                    GUILayout.FlexibleSpace(); //填充左侧剩余空间，确保右边的内容能左对齐
                    setAnimationType = (SetAnimationType)EditorGUILayout.EnumPopup(setAnimationType);
                    GUILayout.EndHorizontal();
                }
            }

            //绘制GUI按钮，如果点击执行条件里面的内容
            if (GUILayout.Button("执行Rig属性设置"))
            {
                AddModelData();
                SettingRigProperty();
            }

            GUILayout.Space(20);

            //-----------------------------------------------------Animation-----------------------------------------------------------------------//
            _AnimationPropertyFoldout = EditorGUILayout.Foldout(_AnimationPropertyFoldout, "AnimationProperty");
            if (_AnimationPropertyFoldout)
            {
                hasAnimation = GUILayout.Toggle(hasAnimation, "hasAnimation");
                if (hasAnimation)
                {
                    GUILayout.BeginHorizontal();
                    importAnimation = GUILayout.Toggle(importAnimation, "Import Animation");
                    GUILayout.EndHorizontal();
                }
            }

            //绘制GUI按钮，如果点击执行条件里面的内容
            if (GUILayout.Button("执行Rig属性设置"))
            {
                AddModelData();
                SettingAnimationProperty();
            }

            GUILayout.Space(20);

            //-----------------------------------------------------Material-----------------------------------------------------------------------//

            _MaterialPropertyFoldout = EditorGUILayout.Foldout(_MaterialPropertyFoldout, "MaterialProperty");
            if (_MaterialPropertyFoldout)
            {
                isMaterial = GUILayout.Toggle(isMaterial, "MaterialCreationMode");
            }


            //绘制GUI按钮，如果点击执行条件里面的内容
            if (GUILayout.Button("执行Material属性设置"))
            {
                AddModelData();
                SettingMaterialProperty();
            }

            GUILayout.Space(20);

            //-----------------------------------------------------Total-----------------------------------------------------------------------//
            if (GUILayout.Button("执行所有属性设置"))
            {
                AddModelData();
                SettingModelProperty();
                SettingRigProperty();
                SettingAnimationProperty();
                SettingMaterialProperty();
            }
        }

        //选择模型并返回
        private Object[] GetSelectedModelObjs()
        {
            //这里的type如果是Object类，则会连所选择的文件目录都包括，如果是GameObject则不会
            Object[] objs = Selection.GetFiltered(typeof(GameObject), SelectionMode.DeepAssets);
            for (int i = 0; i < objs.Length; i++)
            {
                //打印出来选择的所有的模型的名称
                Debug.Log("Name" + objs[i]);
            }

            return objs;
        }

        private void AddModelData()
        {
            //定义一个局部的数组，将选择的所有的模型添加到这个数组
            Object[] selsetModelData = GetSelectedModelObjs();

            //遍历数组中的所有数据，并使用mesh来存储
            foreach (GameObject mesh in selsetModelData)
            {
                //向列表中添加遍历到的mesh模型
                models.Add(mesh);
            }

            for (int i = 0; i < selsetModelData.Length; i++)
            {
                //声明变量mod，将models数组中的每一个元素作为一个GameObject来获取到
                GameObject mod = models[i] as GameObject;
                //从获取的GameObject中获取每个资产路径，如果不存在则为null
                string path = AssetDatabase.GetAssetPath(mod);
                //将每个GameObject的路径存到路径列表中
                modelStrs.Add(path);
                ModelImporter meshes = ModelImporter.GetAtPath(path) as ModelImporter;
                modelImoprters.Add(meshes);
            }
        }

        //模型属性设置内容
        private void SettingModelProperty()
        {
            //循环遍历存储在模型列表中的所有元素
            for (int i = 0; i < modelImoprters.Count; i++)
            {
                //设置模型是否导入相机
                if (isImportCameras == true)
                {
                    modelImoprters[i].importCameras = true;
                }
                else
                {
                    modelImoprters[i].importCameras = false;
                }

                //设置模型是否导入灯光
                if (isImportLight == true)
                {
                    modelImoprters[i].importLights = true;
                }
                else
                {
                    modelImoprters[i].importLights = false;
                }

                //设置模型的读写属性
                if (isReadWrite == true)
                {
                    modelImoprters[i].isReadable = true;
                }
                else
                {
                    modelImoprters[i].isReadable = false;
                }

                //设置模型的碰撞属性
                if (isGenerateColliders == true)
                {
                    modelImoprters[i].addCollider = true;
                }
                else
                {
                    modelImoprters[i].addCollider = false;
                }

                if (setIndexFormat == SetIndexFormat.Atuo)
                {
                    modelImoprters[i].indexFormat = ModelImporterIndexFormat.Auto;
                }
                else if (setIndexFormat == SetIndexFormat.UInt16)
                {
                    modelImoprters[i].indexFormat = ModelImporterIndexFormat.UInt16;
                }
                else
                {
                    modelImoprters[i].indexFormat = ModelImporterIndexFormat.UInt32;
                }

                //设置网格属性优化
                if (isOptimizeMesh == true)
                {
                    modelImoprters[i].optimizeMeshPolygons = true;
                    modelImoprters[i].optimizeMeshVertices = true;
                    modelImoprters[i].optimizeMeshVertices = true;
                }
                else
                {
                    modelImoprters[i].optimizeMeshPolygons = false;
                    modelImoprters[i].optimizeMeshVertices = false;
                    modelImoprters[i].optimizeMeshVertices = false;
                }

                //设置法线的导入方式
                if (setNormals == SetNormal.Import)
                {
                    modelImoprters[i].importNormals = ModelImporterNormals.Import;
                }

                if (setNormals == SetNormal.Calculate)
                {
                    modelImoprters[i].importNormals = ModelImporterNormals.Calculate;
                }

                if (setNormals == SetNormal.None)
                {
                    modelImoprters[i].importNormals = ModelImporterNormals.None;
                }

                // //设置切线的导入方式
                if (setTangents == SetTangent.Import)
                {
                    modelImoprters[i].importTangents = ModelImporterTangents.Import;
                }

                if (setTangents == SetTangent.CalculateLegacy)
                {
                    modelImoprters[i].importTangents = ModelImporterTangents.CalculateLegacy;
                }

                if (setTangents == SetTangent.CalculateLegacyWithSplitTangents)
                {
                    modelImoprters[i].importTangents = ModelImporterTangents.CalculateLegacyWithSplitTangents;
                }

                if (setTangents == SetTangent.CalculateMikktspace)
                {
                    modelImoprters[i].importTangents = ModelImporterTangents.CalculateMikk;
                }

                if (setTangents == SetTangent.None)
                {
                    modelImoprters[i].importTangents = ModelImporterTangents.None;
                }

                //设置第二套uv交换
                if (isSwapUVs == true)
                {
                    modelImoprters[i].swapUVChannels = true;
                }
                else
                {
                    modelImoprters[i].swapUVChannels = false;
                }

                if (isGenerateLightmapUVs == true)
                {
                    modelImoprters[i].generateSecondaryUV = true;
                }
                else
                {
                    modelImoprters[i].generateSecondaryUV = false;
                }



                AssetDatabase.ImportAsset(modelStrs[i]);
                AssetDatabase.Refresh();

                //执行完成后对model进行保存，通常使用SaveAndReimport来应用更改
                // modelImoprters[i].SaveAndReimport();
            }

        }

        //Rig属性设置
        private void SettingRigProperty()
        {
            for (int i = 0; i < modelImoprters.Count; i++)
            {
                //判断模型是否需要骨骼
                if (isRig == true)
                {
                    //判断需要哪一种骨架,不同的骨架下的属性不完全相同
                    if (setAnimationType == SetAnimationType.None)
                    {
                        modelImoprters[i].animationType = ModelImporterAnimationType.None;
                    }
                    else if (setAnimationType == SetAnimationType.Legacy)
                    {
                        modelImoprters[i].animationType = ModelImporterAnimationType.Legacy;
                        //TODO
                        //后续有需要再添加对应骨架下的属性设置
                    }
                    else if (setAnimationType == SetAnimationType.Generic)
                    {
                        modelImoprters[i].animationType = ModelImporterAnimationType.Generic;
                        //TODO
                    }
                    else
                    {
                        //这里的设置当前存在问题（待完善）
                        modelImoprters[i].animationType = ModelImporterAnimationType.Human;
                        //TODO
                    }

                }
                else
                {
                    modelImoprters[i].animationType = ModelImporterAnimationType.None;
                }

                AssetDatabase.ImportAsset(modelStrs[i]);
                AssetDatabase.Refresh();
            }
        }

        //设置Animation动画属性
        private void SettingAnimationProperty()
        {
            for (int i = 0; i < modelImoprters.Count; i++)
            {
                if (hasAnimation == true)
                {
                    if (importAnimation == true)
                    {
                        modelImoprters[i].importAnimation = true;
                    }
                    else
                    {
                        modelImoprters[i].importAnimation = false;
                    }
                }
                else
                {
                    modelImoprters[i].importAnimation = false;
                }

                AssetDatabase.ImportAsset(modelStrs[i]);
                AssetDatabase.Refresh();
            }

        }

        //设置Material材质属性
        private void SettingMaterialProperty()
        {
            for (int i = 0; i < modelImoprters.Count; i++)
            {
                //如果勾选了导入材质，直接使用引擎的stander.shader，否则就不要导入材质（这里完全不考虑使用Import via MaterialDescription，它会导入你所使用DCC软件的材质shader)
                if (isMaterial)
                {
                    modelImoprters[i].materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
                    modelImoprters[i].materialLocation = ModelImporterMaterialLocation.InPrefab;
                }
                else
                {
                    modelImoprters[i].materialImportMode = ModelImporterMaterialImportMode.None;
                }

                AssetDatabase.ImportAsset(modelStrs[i]);
                AssetDatabase.Refresh();
            }
        }
    }
}
