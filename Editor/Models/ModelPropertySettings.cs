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
        //实例化ModelPropertySettingsData类，用来在当前类下调用ModelPropertySettingsData类中的公开的变量与方法
        private ModelPropertySettingsData settingsData = new ModelPropertySettingsData();
        
        //声明一个列表，用来存储选择的模型数据
        private List<GameObject> models = new List<GameObject>();
        private List<String> modelStrs = new List<String>();
        private List<ModelImporter> modelImporters = new List<ModelImporter>();
        
        private Vector2 _scrollPos;

        [MenuItem("ArtTools/Model/模型导入设置", false, 101)]
        public static void showWindow()
        {
            EditorWindow window = GetWindow<ModelPropertySettings>();
            window.titleContent = new GUIContent("模型属性设置");

            Vector2 size = new Vector2(400.0f, 600.0f);
            window.minSize = size;
            window.maxSize = size;
        }

        public void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            //-----------------------------------------------------Model-----------------------------------------------------------------------//
            settingsData._ModelPropertyfoldout = EditorGUILayout.Foldout(settingsData._ModelPropertyfoldout, "ModelProperty");
            if (settingsData._ModelPropertyfoldout)
            {
                ModelPropertySettingsGUI.BeginGroup("Scene");
                settingsData.scaleValue = ModelPropertySettingsGUI.LabeledFloatField( settingsData.scaleValue, "Scale Factor");
                settingsData.isBakeAxisConversion = ModelPropertySettingsGUI.LabeledToggle(settingsData.isBakeAxisConversion, "Bake Axis Conversion");
                settingsData.isImportBlendShapes = ModelPropertySettingsGUI.LabeledToggle(settingsData.isImportBlendShapes, "Import Blend Shapes");
                settingsData.isImportCameras = ModelPropertySettingsGUI.LabeledToggle(settingsData.isImportCameras, "Import Cameras");
                settingsData.isImportLight = ModelPropertySettingsGUI.LabeledToggle(settingsData.isImportLight, "Import Lights");
                ModelPropertySettingsGUI.EndGroup();
               
                ModelPropertySettingsGUI.BeginGroup("Mesh");
                settingsData.meshCompression = ModelPropertySettingsGUI.EnumPopup(settingsData.meshCompression , "Mesh Compression");
                settingsData.isReadWrite = ModelPropertySettingsGUI.LabeledToggle(settingsData.isReadWrite, "Read/Write");
                settingsData.isOptimizeMesh = ModelPropertySettingsGUI.LabeledToggle(settingsData.isOptimizeMesh, "Optimize Mesh");
                settingsData.isGenerateColliders = ModelPropertySettingsGUI.LabeledToggle(settingsData.isGenerateColliders, "Generate Colliders");
                ModelPropertySettingsGUI.EndGroup();

                ModelPropertySettingsGUI.BeginGroup("Geometry");
                settingsData.isKeepQuads = ModelPropertySettingsGUI.LabeledToggle(settingsData.isKeepQuads, "Keep Quads");
                settingsData.isWeldVertices = ModelPropertySettingsGUI.LabeledToggle(settingsData.isWeldVertices, "Weld Vertices");
                settingsData.setIndexFormat = ModelPropertySettingsGUI.EnumPopup(settingsData.setIndexFormat , "Index Format");
                settingsData.isLegacyBlendShapeNormals = ModelPropertySettingsGUI.LabeledToggle(settingsData.isLegacyBlendShapeNormals , "Legacy Blend Shape Normals");
                settingsData.setNormals = ModelPropertySettingsGUI.EnumPopup(settingsData.setNormals , "Normals");
                if (settingsData.isImportBlendShapes)
                {
                    settingsData.setBlendShapeNormals = ModelPropertySettingsGUI.EnumPopup(settingsData.setBlendShapeNormals , "Blend Shape Normals");
                }
                if (settingsData.setNormals != ModelPropertySettingsData.SetNormal.None)
                {
                    settingsData.setNormalMode = ModelPropertySettingsGUI.EnumPopup(settingsData.setNormalMode, "Normal Mode");
                    if (!settingsData.isLegacyBlendShapeNormals)
                    {
                        settingsData.setSmoothnessSource = ModelPropertySettingsGUI.EnumPopup(settingsData.setSmoothnessSource , "Smoothness Source");

                        if (settingsData.setSmoothnessSource != ModelPropertySettingsData.SetSmoothnessSource.None && settingsData.setSmoothnessSource != ModelPropertySettingsData.SetSmoothnessSource.FromSmoothingGroups)
                        {
                            settingsData.sliderSmoothingAngle = ModelPropertySettingsGUI.LabeledSlider(settingsData.sliderSmoothingAngle , "Smoothing Angle" , 0.0f,180.0f);
                        }
                    }
                    settingsData.setTangents = ModelPropertySettingsGUI.EnumPopup(settingsData.setTangents , "Tangents");
                }
                settingsData.isSwapUVs = ModelPropertySettingsGUI.LabeledToggle(settingsData.isSwapUVs , "Swap UVs");
                settingsData.isGenerateLightmapUVs = ModelPropertySettingsGUI.LabeledToggle(settingsData.isGenerateLightmapUVs , "Generate Lightmap UVs");
                settingsData.isStrictVertexDataChecks = ModelPropertySettingsGUI.LabeledToggle(settingsData.isStrictVertexDataChecks , "StrictVertex Data Checks");
                ModelPropertySettingsGUI.EndGroup();
            }
            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("执行模型属性设置"))
            {
                AddModelData();
                SettingModelProperty();
            }
            GUILayout.Space(20);

            //-----------------------------------------------------Rig-----------------------------------------------------------------------//
            settingsData._RigPropertyFoldout = EditorGUILayout.Foldout(settingsData._RigPropertyFoldout, "RigProperty");
            if (settingsData._RigPropertyFoldout)
            {
                settingsData.setAnimationType = ModelPropertySettingsGUI.EnumPopup(settingsData.setAnimationType , "Animation Type");
                EditorGUILayout.Space(10);
                if (settingsData.setAnimationType != ModelPropertySettingsData.SetAnimationType.None)
                {
                    settingsData.setAvatarDefinition = ModelPropertySettingsGUI.EnumPopup(settingsData.setAvatarDefinition , "Avatar Definition");
                    
                    if (settingsData.setAvatarDefinition == ModelPropertySettingsData.SetAvatarDefinition.CreateFromThisModel)
                    {
                       settingsData.setRootnode = ModelPropertySettingsGUI.EnumPopup(settingsData.setRootnode , "Root Node"); 
                    }
                    
                    settingsData.setSkinWeight = ModelPropertySettingsGUI.EnumPopup(settingsData.setSkinWeight , "Skin Weight");
                }
            }
            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("执行Rig属性设置"))
            {
                AddModelData();
                SettingRigProperty();
            }
            GUILayout.Space(20);

            //-----------------------------------------------------Animation-----------------------------------------------------------------------//
            settingsData._AnimationPropertyFoldout = EditorGUILayout.Foldout(settingsData._AnimationPropertyFoldout, "AnimationProperty");
            if (settingsData._AnimationPropertyFoldout)
            {
                settingsData.isImportConstraints = ModelPropertySettingsGUI.LabeledToggle(settingsData.isImportConstraints , "Import Constraints");
                settingsData.isImportAnimation = ModelPropertySettingsGUI.LabeledToggle(settingsData.isImportAnimation, "Import Animation");
            }
            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("执行Animation属性设置"))
            {
                AddModelData();
                SettingAnimationProperty();
            }
            GUILayout.Space(20);

            //-----------------------------------------------------Material-----------------------------------------------------------------------//
            settingsData._MaterialPropertyFoldout = EditorGUILayout.Foldout(settingsData._MaterialPropertyFoldout, "Material Property");
            if (settingsData._MaterialPropertyFoldout)
            {
                settingsData.isMaterial = ModelPropertySettingsGUI.LabeledToggle(settingsData.isMaterial, "Material Creation Mode");
            }
            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("执行Material属性设置"))
            {
                AddModelData();
                SettingMaterialProperty();
            }
            GUILayout.Space(20);

            //-----------------------------------------------------Total-----------------------------------------------------------------------//
            GUI.backgroundColor = Color.yellow;
            if (GUILayout.Button("执行所有属性设置"))
            {
                AddModelData();
                SettingModelProperty();
                SettingRigProperty();
                SettingAnimationProperty();
                SettingMaterialProperty();
            }
            EditorGUILayout.Space(30);
            EditorGUILayout.EndScrollView();
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
                
                //Scene
                modelImporters[i].globalScale= settingsData.scaleValue;
                modelImporters[i].bakeAxisConversion = settingsData.isBakeAxisConversion;
                modelImporters[i].importBlendShapes = settingsData.isImportBlendShapes;
                if (settingsData.isImportBlendShapes == true)
                {
                    modelImporters[i].importBlendShapes = true;
                    if (settingsData.setBlendShapeNormals == ModelPropertySettingsData.SetBlendShapeNormals.Import)
                    {
                        modelImporters[i].importBlendShapeNormals = ModelImporterNormals.Import;
                    }
                    else if (settingsData.setBlendShapeNormals == ModelPropertySettingsData.SetBlendShapeNormals.Calculate)
                    {
                        modelImporters[i].importBlendShapeNormals = ModelImporterNormals.Calculate;
                    }
                    else
                    {
                        modelImporters[i].importBlendShapeNormals = ModelImporterNormals.None;
                    }
                }
                modelImporters[i].importBlendShapeDeformPercent = false;
                modelImporters[i].importVisibility = false;
                modelImporters[i].importCameras = settingsData.isImportCameras;
                modelImporters[i].importLights = settingsData.isImportLight;
                modelImporters[i].preserveHierarchy = false;
                modelImporters[i].sortHierarchyByName = false;

                //Meshes
                if (settingsData.meshCompression == ModelPropertySettingsData.MeshCompression.Off)
                {
                    modelImporters[i].meshCompression = ModelImporterMeshCompression.Off;
                }
                else if(settingsData.meshCompression == ModelPropertySettingsData.MeshCompression.Low)
                {
                    modelImporters[i].meshCompression = ModelImporterMeshCompression.Low;
                }
                else if(settingsData.meshCompression == ModelPropertySettingsData.MeshCompression.Medium)
                {
                    modelImporters[i].meshCompression = ModelImporterMeshCompression.Medium;
                }
                else
                {
                    modelImporters[i].meshCompression = ModelImporterMeshCompression.High;
                }
                
                modelImporters[i].isReadable = settingsData.isReadWrite;
               
                if (settingsData.isOptimizeMesh == true)
                {
                    modelImporters[i].optimizeMeshPolygons = true;
                    modelImporters[i].optimizeMeshVertices = true;
                }
                else
                {
                    modelImporters[i].optimizeMeshPolygons = false;
                    modelImporters[i].optimizeMeshVertices = false;
                }
                modelImporters[i].addCollider = settingsData.isGenerateColliders;
                
                //Geometry
                modelImporters[i].keepQuads = settingsData.isKeepQuads;
                modelImporters[i].weldVertices = settingsData.isWeldVertices;
                if (settingsData.setIndexFormat == ModelPropertySettingsData.SetIndexFormat.Atuo)
                {
                    modelImporters[i].indexFormat = ModelImporterIndexFormat.Auto;
                }
                else if (settingsData.setIndexFormat == ModelPropertySettingsData.SetIndexFormat.UInt16)
                {
                    modelImporters[i].indexFormat = ModelImporterIndexFormat.UInt16;
                }
                else
                {
                    modelImporters[i].indexFormat = ModelImporterIndexFormat.UInt32;
                }

                //TODO
                if (settingsData.isLegacyBlendShapeNormals)
                {
                    //modelImporters[i].importBlendShapeNormals = ModelImporterNormals.None;
                }
                else
                {
                    if (settingsData.setSmoothnessSource == ModelPropertySettingsData.SetSmoothnessSource.PreferSmoothingGroups)
                    {
                        modelImporters[i].normalSmoothingSource = ModelImporterNormalSmoothingSource.PreferSmoothingGroups;
                    }
                    else if (settingsData.setSmoothnessSource == ModelPropertySettingsData.SetSmoothnessSource.FromSmoothingGroups)
                    {
                        modelImporters[i].normalSmoothingSource = ModelImporterNormalSmoothingSource.FromSmoothingGroups;
                    }
                    else if (settingsData.setSmoothnessSource == ModelPropertySettingsData.SetSmoothnessSource.FromAngle)
                    {
                        modelImporters[i].normalSmoothingSource = ModelImporterNormalSmoothingSource.FromAngle;
                    }
                    else
                    {
                        modelImporters[i].normalSmoothingSource = ModelImporterNormalSmoothingSource.None;
                    }
                }

                if (settingsData.setNormals == ModelPropertySettingsData.SetNormal.Import)
                {
                    modelImporters[i].importNormals = ModelImporterNormals.Import;
                }
                else if (settingsData.setNormals == ModelPropertySettingsData.SetNormal.Calculate)
                {
                    modelImporters[i].importNormals = ModelImporterNormals.Calculate;
                }
                else
                {
                    modelImporters[i].importNormals = ModelImporterNormals.None;
                }

                if (settingsData.setNormalMode == ModelPropertySettingsData.SetNormalsMode.Unweighted)
                {
                    modelImporters[i].normalCalculationMode = ModelImporterNormalCalculationMode.Unweighted;
                }
                else if(settingsData.setNormalMode == ModelPropertySettingsData.SetNormalsMode.AreaWeighted)
                {
                    modelImporters[i].normalCalculationMode = ModelImporterNormalCalculationMode.AngleWeighted;
                }
                else if (settingsData.setNormalMode == ModelPropertySettingsData.SetNormalsMode.AngleWeighted)
                {
                    modelImporters[i].normalCalculationMode = ModelImporterNormalCalculationMode.AreaWeighted;
                }
                else if(settingsData.setNormalMode == ModelPropertySettingsData.SetNormalsMode.AreaAndAngleWeighted)
                {
                    modelImporters[i].normalCalculationMode = ModelImporterNormalCalculationMode.AreaAndAngleWeighted;
                }
                else
                {
                    modelImporters[i].normalCalculationMode = ModelImporterNormalCalculationMode.Unweighted;
                }
                
                modelImporters[i].normalSmoothingAngle = settingsData.sliderSmoothingAngle;

                // //设置切线的导入方式
                if (settingsData.setTangents == ModelPropertySettingsData.SetTangent.Import)
                {
                    modelImporters[i].importTangents = ModelImporterTangents.Import;
                }

                if (settingsData.setTangents == ModelPropertySettingsData.SetTangent.CalculateLegacy)
                {
                    modelImporters[i].importTangents = ModelImporterTangents.CalculateLegacy;
                }

                if (settingsData.setTangents == ModelPropertySettingsData.SetTangent.CalculateLegacyWithSplitTangents)
                {
                    modelImporters[i].importTangents = ModelImporterTangents.CalculateLegacyWithSplitTangents;
                }

                if (settingsData.setTangents == ModelPropertySettingsData.SetTangent.CalculateMikktspace)
                {
                    modelImporters[i].importTangents = ModelImporterTangents.CalculateMikk;
                }

                if (settingsData.setTangents == ModelPropertySettingsData.SetTangent.None)
                {
                    modelImporters[i].importTangents = ModelImporterTangents.None;
                }
                
                modelImporters[i].swapUVChannels = settingsData.isSwapUVs;
                modelImporters[i].generateSecondaryUV = settingsData.isGenerateLightmapUVs;
                modelImporters[i].strictVertexDataChecks = settingsData.isStrictVertexDataChecks;

                EditorUtility.SetDirty(importer);
                AssetDatabase.ImportAsset(modelStrs[i]);
            }
            AssetDatabase.Refresh();
        }

        //Rig属性设置
        private void SettingRigProperty()
        {
            for (int i = 0; i < modelImporters.Count; i++)
            {
                //ModelImporter importer = ModelImporter.GetAtPath(modelStrs[i]) as ModelImporter;
                
                //判断需要哪一种骨架,不同的骨架下的属性不完全相同
                if (settingsData.setAnimationType == ModelPropertySettingsData.SetAnimationType.None)
                {
                    modelImporters[i].animationType = ModelImporterAnimationType.None;
                }
                else
                {
                    if (settingsData.setAnimationType == ModelPropertySettingsData.SetAnimationType.Legacy)
                    {
                        modelImporters[i].animationType = ModelImporterAnimationType.Legacy;
                        //TODO
                    }
                    else if (settingsData.setAnimationType == ModelPropertySettingsData.SetAnimationType.Generic)
                    {
                        modelImporters[i].animationType = ModelImporterAnimationType.Generic;
                    
                        if (settingsData.setAvatarDefinition == ModelPropertySettingsData.SetAvatarDefinition.NoAvatar)
                        {
                            modelImporters[i].avatarSetup = ModelImporterAvatarSetup.NoAvatar;
                        }
                        else if (settingsData.setAvatarDefinition == ModelPropertySettingsData.SetAvatarDefinition.CopyFromOtherAvatar)
                        {
                            modelImporters[i].avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
                            //TODO
                        }
                        else
                        {
                            modelImporters[i].avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                            //TODO
                        }
                    }
                    else
                    {
                        modelImporters[i].animationType = ModelImporterAnimationType.Human;
                        //TODO
                    }
                    
                    modelImporters[i].skinWeights = ModelImporterSkinWeights.Standard;
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
                modelImporters[i].importAnimation = settingsData.isImportConstraints;
                modelImporters[i].importAnimation = settingsData.isImportAnimation;
               
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
                if (settingsData.isMaterial)
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
