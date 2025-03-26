using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
// using UnityEngine.Windows;
using Object = UnityEngine.Object;

namespace yuxuetian
{
    public class ModelPropertySettings : EditorWindow
    {
        public enum MeshCompression
        {
            Off,
            Low,
            Medium,
            High
        }
        
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

        public enum SetNormalsMode
        {
            Unweighted,
            AreaWeighted,
            AngleWeighted,
            AreaAndAngleWeighted
        }

        public enum SetSmoothnessSource
        {
            PreferSmoothingGroups,
            FromSmoothingGroups,
            FromAngle,
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

        private static readonly float labelWidth = 120.0f;
        private static readonly float labelHeight = 20.0f;
        //声明一个列表，用来存储选择的模型数据
        private List<GameObject> models = new List<GameObject>();
        private List<String> modelStrs = new List<String>();
        private List<ModelImporter> modelImporters = new List<ModelImporter>();

        #region Model属性变量声明
        //模型属性中,这部分的属性通常不需要,因此不开放给美术人员
        public bool isBakeAxisConversion = false;
        public bool isImportBlendShapes = false;
        public bool isImportDeformPercent = false;
        public bool isImportVisibility = false;
        public bool isImportCameras = false;
        public bool isImportLight = false;
        public bool isPreserveHierarchy = false;
        public bool isSortHierarchyByName = false;
        
        public MeshCompression meshCompression = MeshCompression.Off;
        public bool isReadWrite = false;
        public bool isOptimizeMesh = true;
        public bool isGenerateColliders = false;
        
        public bool isKeepQuads = false;
        public bool isWeldVertices = false;
        public SetIndexFormat setIndexFormat = SetIndexFormat.UInt16;
        public bool isLegacyBlendShapeNormals = false;
        public SetNormal setNormals = SetNormal.Import;
        public SetNormalsMode setNormalMode = SetNormalsMode.AreaAndAngleWeighted;
        public SetSmoothnessSource setSmoothnessSource = SetSmoothnessSource.PreferSmoothingGroups;
        public float sliderSmoothingAngle = 60.0f;
        public SetTangent setTangents = SetTangent.CalculateMikktspace;
        public bool isSwapUVs = false;
        public bool isGenerateLightmapUVs = false;
        public bool isStrictVertexDataChecks = false;

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
                ModelPropertySettingsGUI.BeginGroup("Scene");
                isBakeAxisConversion = ModelPropertySettingsGUI.LabeledToggle(isBakeAxisConversion, "Bake Axis Conversion");
                isImportBlendShapes = ModelPropertySettingsGUI.LabeledToggle(isImportBlendShapes, "Import Blend Shapes");
                isImportCameras = ModelPropertySettingsGUI.LabeledToggle(isImportCameras, "Import Cameras");
                isImportLight = ModelPropertySettingsGUI.LabeledToggle(isImportLight, "Import Lights");
                ModelPropertySettingsGUI.EndGroup();
               
                ModelPropertySettingsGUI.BeginGroup("Mesh");
                ModelPropertySettingsGUI.EnumPopup(meshCompression , "Mesh Compression");
                isReadWrite = ModelPropertySettingsGUI.LabeledToggle(isReadWrite, "Read/Write");
                isOptimizeMesh = ModelPropertySettingsGUI.LabeledToggle(isOptimizeMesh, "Optimize Mesh");
                isGenerateColliders = ModelPropertySettingsGUI.LabeledToggle(isGenerateColliders, "Generate Colliders");
                ModelPropertySettingsGUI.EndGroup();

                ModelPropertySettingsGUI.BeginGroup("Geometry");
                isKeepQuads = ModelPropertySettingsGUI.LabeledToggle(isKeepQuads, "Keep Quads");
                isWeldVertices = ModelPropertySettingsGUI.LabeledToggle(isWeldVertices, "Weld Vertices");
                setIndexFormat = ModelPropertySettingsGUI.EnumPopup(setIndexFormat , "Index Format");
                isLegacyBlendShapeNormals = ModelPropertySettingsGUI.LabeledToggle(isLegacyBlendShapeNormals , "Legacy Blend Shape Normals");
                setNormals = ModelPropertySettingsGUI.EnumPopup(setNormals , "Normals");
                setNormalMode = ModelPropertySettingsGUI.EnumPopup(setNormalMode, "Normal Mode");
                if (!isLegacyBlendShapeNormals)
                {
                    setSmoothnessSource = ModelPropertySettingsGUI.EnumPopup(setSmoothnessSource , "Smoothness Source");
                }
                //setBlendShapeNormals = ModelPropertySettingsGUI.EnumPopup(setBlendShapeNormals , "Blend Shape Normals");
                
                sliderSmoothingAngle = ModelPropertySettingsGUI.LabeledSlider(sliderSmoothingAngle , "Smoothing Angle" , 0.0f,180.0f);
                setTangents = ModelPropertySettingsGUI.EnumPopup(setTangents , "Tangents");
                isSwapUVs = ModelPropertySettingsGUI.LabeledToggle(isSwapUVs , "Swap UVs");
                isGenerateLightmapUVs = ModelPropertySettingsGUI.LabeledToggle(isGenerateLightmapUVs , "Generate Lightmap UVs");
                isStrictVertexDataChecks = ModelPropertySettingsGUI.LabeledToggle(isStrictVertexDataChecks , "StrictVertex Data Checks");
                ModelPropertySettingsGUI.EndGroup();
            }

            //绘制GUI按钮，如果点击执行条件里面的内容
            if (GUILayout.Button("执行模型属性设置"))
            {
                AddModelData();
                SettingModelProperty();
                // GetSelectedModelObjs();
            
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
            List<Object> allModels = new List<Object>();
            
            Object[] selectedObjects = Selection.objects;

            foreach (var obj in selectedObjects)
            {
                //如果是目录
                if (obj is DefaultAsset)
                {
                    string path = AssetDatabase.GetAssetPath(obj);
                    if (Directory.Exists(path))
                    {
                        // 递归获取目录下所有模型文件
                        string[] modelPaths = Directory.GetFiles(path, "*.fbx", SearchOption.AllDirectories).ToArray();
                
                        foreach (string modelPath in modelPaths)
                        {
                            Object model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
                            if (model != null)
                            {
                                allModels.Add(model);
                                Debug.Log("Found model in folder: " + modelPath);
                            }
                        }
                    }
                }
                else if (obj is GameObject) // 如果是模型文件
                {
                    allModels.Add(obj);
                    Debug.Log("Selected model: " + obj.name);
                }
            }
            return allModels.ToArray();
        }

        private void AddModelData()
        {
            models.Clear();
            modelStrs.Clear();
            modelImporters.Clear();
            
            //定义一个局部的数组，将选择的所有的模型添加到这个数组
            Object[] selectedModelData = GetSelectedModelObjs();

            //遍历数组中的所有数据，并使用mesh来存储
            foreach (Object obj in selectedModelData)
            {
                GameObject mesh = obj as GameObject;
                if (mesh == null) continue;
        
                // 向列表中添加遍历到的mesh模型
                models.Add(mesh);
        
                // 获取资产路径
                string path = AssetDatabase.GetAssetPath(mesh);
                if (!string.IsNullOrEmpty(path))
                {
                    modelStrs.Add(path);
                    ModelImporter importer = ModelImporter.GetAtPath(path) as ModelImporter;
                    if (importer != null)
                    {
                        modelImporters.Add(importer);
                    }
                }
            }
        }

        //模型属性设置内容
        private void SettingModelProperty()
        {
            //循环遍历存储在模型列表中的所有元素
            for (int i = 0; i < modelImporters.Count; i++)
            {
                ModelImporter importer = ModelImporter.GetAtPath(modelStrs[i]) as ModelImporter;
                #region Scene
                if (isBakeAxisConversion == true)
                {
                    modelImporters[i].bakeAxisConversion = true;
                }
                else
                {
                    modelImporters[i].bakeAxisConversion = false;
                }

                if (isImportBlendShapes == true)
                {
                    modelImporters[i].importBlendShapes = true;
                }
                else
                {
                    modelImporters[i].importBlendShapes = false;
                }
                
                modelImporters[i].importBlendShapeDeformPercent = false;
                modelImporters[i].importVisibility = false;
                //设置模型是否导入相机
                if (isImportCameras == true)
                {
                    modelImporters[i].importCameras = true;
                }
                else
                {
                    modelImporters[i].importCameras = false;
                }

                //设置模型是否导入灯光
                if (isImportLight == true)
                {
                    modelImporters[i].importLights = true;
                }
                else
                {
                    modelImporters[i].importLights = false;
                }
                
                modelImporters[i].preserveHierarchy = false;
                modelImporters[i].sortHierarchyByName = false;
                #endregion

                #region Meshes
                if (meshCompression == MeshCompression.Off)
                {
                    modelImporters[i].meshCompression = ModelImporterMeshCompression.Off;
                }
                else if(meshCompression == MeshCompression.Low)
                {
                    modelImporters[i].meshCompression = ModelImporterMeshCompression.Low;
                }
                else if(meshCompression == MeshCompression.Medium)
                {
                    modelImporters[i].meshCompression = ModelImporterMeshCompression.Medium;
                }
                else
                {
                    modelImporters[i].meshCompression = ModelImporterMeshCompression.High;
                }

                if (isReadWrite == true)
                {
                    modelImporters[i].isReadable = true;
                }
                else
                {
                    modelImporters[i].isReadable = false;
                }
                
                if (isOptimizeMesh == true)
                {
                    modelImporters[i].optimizeMeshPolygons = true;
                    modelImporters[i].optimizeMeshVertices = true;
                }
                else
                {
                    modelImporters[i].optimizeMeshPolygons = false;
                    modelImporters[i].optimizeMeshVertices = false;
                }

                if (isGenerateColliders == true)
                {
                    modelImporters[i].addCollider = true;
                }
                else
                {
                    modelImporters[i].addCollider = false;
                }
                #endregion

                #region Geometry
                if (isKeepQuads == true)
                {
                    modelImporters[i].keepQuads = true;
                }
                else
                {
                    modelImporters[i].keepQuads = false;
                }

                if (isWeldVertices == true)

                {
                    modelImporters[i].weldVertices = true;
                }
                else
                {
                    modelImporters[i].weldVertices = false;                    
                }
                
                if (setIndexFormat == SetIndexFormat.Atuo)
                {
                    modelImporters[i].indexFormat = ModelImporterIndexFormat.Auto;
                }
                else if (setIndexFormat == SetIndexFormat.UInt16)
                {
                    modelImporters[i].indexFormat = ModelImporterIndexFormat.UInt16;
                }
                else
                {
                    modelImporters[i].indexFormat = ModelImporterIndexFormat.UInt32;
                }

                //TODO
                if (isLegacyBlendShapeNormals)
                {
                    //modelImporters[i].legacyBlendShapeNormals = true;
                }
                else
                {
                    if (setSmoothnessSource == SetSmoothnessSource.PreferSmoothingGroups)
                    {
                        modelImporters[i].normalSmoothingSource = ModelImporterNormalSmoothingSource.PreferSmoothingGroups;
                    }
                    else if (setSmoothnessSource == SetSmoothnessSource.FromSmoothingGroups)
                    {
                        modelImporters[i].normalSmoothingSource = ModelImporterNormalSmoothingSource.FromSmoothingGroups;
                    }
                    else if (setSmoothnessSource == SetSmoothnessSource.FromAngle)
                    {
                        modelImporters[i].normalSmoothingSource = ModelImporterNormalSmoothingSource.FromAngle;
                    }
                    else
                    {
                        modelImporters[i].normalSmoothingSource = ModelImporterNormalSmoothingSource.None;
                    }
                }

                if (setNormals == SetNormal.Import)
                {
                    modelImporters[i].importNormals = ModelImporterNormals.Import;
                }
                else if (setNormals == SetNormal.Calculate)
                {
                    modelImporters[i].importNormals = ModelImporterNormals.Calculate;
                }
                else
                {
                    modelImporters[i].importNormals = ModelImporterNormals.None;
                }

                //TODO
                if (setNormalMode == SetNormalsMode.Unweighted)
                {
                }
                else if(setNormalMode == SetNormalsMode.AreaWeighted)
                {
                    
                }
                else if (setNormalMode == SetNormalsMode.AngleWeighted)
                {
                    
                }
                else if(setNormalMode == SetNormalsMode.AreaAndAngleWeighted)
                {
                    
                }
                else
                {
                    
                }

                // //设置切线的导入方式
                if (setTangents == SetTangent.Import)
                {
                    modelImporters[i].importTangents = ModelImporterTangents.Import;
                }

                if (setTangents == SetTangent.CalculateLegacy)
                {
                    modelImporters[i].importTangents = ModelImporterTangents.CalculateLegacy;
                }

                if (setTangents == SetTangent.CalculateLegacyWithSplitTangents)
                {
                    modelImporters[i].importTangents = ModelImporterTangents.CalculateLegacyWithSplitTangents;
                }

                if (setTangents == SetTangent.CalculateMikktspace)
                {
                    modelImporters[i].importTangents = ModelImporterTangents.CalculateMikk;
                }

                if (setTangents == SetTangent.None)
                {
                    modelImporters[i].importTangents = ModelImporterTangents.None;
                }

                //设置第二套uv交换
                if (isSwapUVs == true)
                {
                    modelImporters[i].swapUVChannels = true;
                }
                else
                {
                    modelImporters[i].swapUVChannels = false;
                }

                if (isGenerateLightmapUVs == true)
                {
                    modelImporters[i].generateSecondaryUV = true;
                }
                else
                {
                    modelImporters[i].generateSecondaryUV = false;
                }
                

                #endregion
                

                EditorUtility.SetDirty(importer);
                AssetDatabase.ImportAsset(modelStrs[i]);
                //执行完成后对model进行保存，通常使用SaveAndReimport来应用更改
                // modelImporters[i].SaveAndReimport();
            }
            AssetDatabase.Refresh();
        }

        //Rig属性设置
        private void SettingRigProperty()
        {
            for (int i = 0; i < modelImporters.Count; i++)
            {
                //判断模型是否需要骨骼
                if (isRig == true)
                {
                    //判断需要哪一种骨架,不同的骨架下的属性不完全相同
                    if (setAnimationType == SetAnimationType.None)
                    {
                        modelImporters[i].animationType = ModelImporterAnimationType.None;
                    }
                    else if (setAnimationType == SetAnimationType.Legacy)
                    {
                        modelImporters[i].animationType = ModelImporterAnimationType.Legacy;
                        //TODO
                        //后续有需要再添加对应骨架下的属性设置
                    }
                    else if (setAnimationType == SetAnimationType.Generic)
                    {
                        modelImporters[i].animationType = ModelImporterAnimationType.Generic;
                        //TODO
                    }
                    else
                    {
                        //这里的设置当前存在问题（待完善）
                        modelImporters[i].animationType = ModelImporterAnimationType.Human;
                        //TODO
                    }

                }
                else
                {
                    modelImporters[i].animationType = ModelImporterAnimationType.None;
                }

                AssetDatabase.ImportAsset(modelStrs[i]);
                AssetDatabase.Refresh();
            }
        }

        //设置Animation动画属性
        private void SettingAnimationProperty()
        {
            for (int i = 0; i < modelImporters.Count; i++)
            {
                if (hasAnimation == true)
                {
                    if (importAnimation == true)
                    {
                        modelImporters[i].importAnimation = true;
                    }
                    else
                    {
                        modelImporters[i].importAnimation = false;
                    }
                }
                else
                {
                    modelImporters[i].importAnimation = false;
                }

                AssetDatabase.ImportAsset(modelStrs[i]);
                AssetDatabase.Refresh();
            }

        }

        //设置Material材质属性
        private void SettingMaterialProperty()
        {
            for (int i = 0; i < modelImporters.Count; i++)
            {
                //如果勾选了导入材质，直接使用引擎的stander.shader，否则就不要导入材质（这里完全不考虑使用Import via MaterialDescription，它会导入你所使用DCC软件的材质shader)
                if (isMaterial)
                {
                    modelImporters[i].materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
                    modelImporters[i].materialLocation = ModelImporterMaterialLocation.InPrefab;
                }
                else
                {
                    modelImporters[i].materialImportMode = ModelImporterMaterialImportMode.None;
                }

                AssetDatabase.ImportAsset(modelStrs[i]);
                AssetDatabase.Refresh();
            }
        }
    }
}
