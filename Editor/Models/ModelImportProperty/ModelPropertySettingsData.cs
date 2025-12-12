using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace yuxuetian
{
    [System.Serializable]
    public class ModelPropertySettingsData
    {
        /// <summary>
        /// Redefine some enumeration variables
        /// </summary>
        public enum MeshCompression{ Off, Low, Medium, High }
        public enum SetIndexFormat{ Atuo, [InspectorName("16 bits")] UInt16, [InspectorName("32 bits")] UInt32 }
        public enum SetNormal{ Import, Calculate, None }
        public enum SetBlendShapeNormals{ Import, Calculate, None }
        public enum SetNormalsMode{ Unweighted, AreaWeighted, AngleWeighted, AreaAndAngleWeighted }
        public enum SetSmoothnessSource{ PreferSmoothingGroups, FromSmoothingGroups, FromAngle, None }
        public enum SetTangent{ Import, CalculateLegacy, CalculateLegacyWithSplitTangents, CalculateMikktspace, None }
        
        public enum SetAnimationType{ None, Legacy, Generic, Humanoid }
        public enum SetAvatarDefinition{ NoAvatar , CreateFromThisModel , CopyFromOtherAvatar }
        public enum SetSkinWeights{ Standard, Custom }
        public enum SetRootnode{none, Root}

        /// <summary>
        /// Define model attribute variables, which may not be commonly used, so many of them are not open to artists
        /// </summary>
        public float scaleValue = 1.0f;
        public bool isBakeAxisConversion = false;
        public bool isImportBlendShapes = true;
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
        public bool isLegacyBlendShapeNormals = false;  //未实现
        public SetNormal setNormals = SetNormal.Import;
        public SetBlendShapeNormals setBlendShapeNormals = SetBlendShapeNormals.Calculate;
        public SetNormalsMode setNormalMode = SetNormalsMode.AreaAndAngleWeighted;
        public SetSmoothnessSource setSmoothnessSource = SetSmoothnessSource.PreferSmoothingGroups;
        public float sliderSmoothingAngle = 60.0f;
        public SetTangent setTangents = SetTangent.CalculateMikktspace;
        public bool isSwapUVs = false;
        public bool isGenerateLightmapUVs = false;
        public bool isStrictVertexDataChecks = false;

        /// <summary>
        //Define Rig attribute variables
        /// </summary>
        public SetAnimationType setAnimationType = SetAnimationType.None;
        public SetAvatarDefinition setAvatarDefinition = SetAvatarDefinition.NoAvatar;
        public SetSkinWeights setSkinWeight = SetSkinWeights.Standard;
        public bool isStripBones = true;
        public SetRootnode setRootnode = SetRootnode.Root;
        public bool isOptimizeGameObjects = false;
        
        /// <summary>
        //Define Animation attribute variables
        /// </summary>
        public bool isImportConstraints = false;
        public bool isImportAnimation = false;
        
        /// <summary>
        //Define Material attribute variables
        /// </summary>
        public bool isMaterial = false;
        
        /// <summary>
        /// This is used to control the attribute folding switch, which will remain closed by default
        /// </summary>
        public bool _ModelPropertyfoldout = false;
        public bool _RigPropertyFoldout = false;
        public bool _AnimationPropertyFoldout = false;
        public bool _MaterialPropertyFoldout = false;
    }

}
