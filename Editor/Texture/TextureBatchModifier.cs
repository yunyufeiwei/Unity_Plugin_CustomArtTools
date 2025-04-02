using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;

// using PlasticGui;

namespace yuxuetian
{
    public class TextureBatchModifier : EditorWindow
    {
        private Vector2 scrollPosition;
        

        // Basic Settings
        private TextureImporterType customtextureType = TextureImporterType.Default;
        private TextureImporterShape customTextureShape = TextureImporterShape.Texture2D;
        private bool sRGB = true;
        private TextureImporterAlphaSource customAlphaSource = TextureImporterAlphaSource.FromInput;
        private bool customAlphaIsTransparency = false; 

        // Advanced Settings
        private bool isAdvanced = false;
        private bool customrReadWrite = false;
        private bool customGenerateMipmaps = false;
        private bool customMipStreaming = false;
        private int  customPriority = 0;
        private TextureImporterMipFilter customMipmapFiltering = TextureImporterMipFilter.BoxFilter;  
        private bool customPreserveCoverage = false;
        private bool customReplicateBorder = false;
        private bool customFadeoutToGray = false;
        
        private TextureWrapMode wrapMode = TextureWrapMode.Repeat;
        private FilterMode filterMode = FilterMode.Bilinear;
        private int anisoLevel = 1;

        private int maxSize = 1024;
        private int[] powerOfTwoSizes = { 32, 64, 128, 256, 512, 1024, 2048, 4096 };
        private int selectedIndex = 1;
        private TextureResizeAlgorithm resizeAlgorithm = TextureResizeAlgorithm.Mitchell;
        private TextureImporterFormat format = TextureImporterFormat.Automatic;
        private TextureImporterCompression compression = TextureImporterCompression.Compressed;

        [MenuItem("Tools/Texture Batch Modifier")]
        public static void ShowWindow()
        {
            GetWindow<TextureBatchModifier>("Texture Batch Modifier");
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            //纹理基础部分设置
            customtextureType = (TextureImporterType)EditorGUILayout.EnumPopup("Texture Type", customtextureType);
            customTextureShape = (TextureImporterShape)EditorGUILayout.EnumPopup("Texture Shape", customTextureShape);
            sRGB = EditorGUILayout.Toggle("SRGB", sRGB);
            customAlphaSource = (TextureImporterAlphaSource)EditorGUILayout.EnumPopup("Alpha Source" , customAlphaSource);
            // 使用DisableScope控制Alpha Is Transparency的可编辑状态
            using (new EditorGUI.DisabledScope(customAlphaSource == TextureImporterAlphaSource.None))
            {
                // 当Alpha来源为None时，此选项会变灰且不可编辑
                customAlphaIsTransparency = EditorGUILayout.Toggle("Alpha作为透明度", customAlphaIsTransparency);
            }
            //纹理高级部分设置
            isAdvanced = EditorGUILayout.Foldout(isAdvanced, "Advanced");
            if (isAdvanced)
            {
                EditorGUI.indentLevel++;
                customrReadWrite = EditorGUILayout.Toggle("Read/Write", customrReadWrite);
                customGenerateMipmaps = EditorGUILayout.Toggle("Generate Mipmaps", customGenerateMipmaps);
                if (customGenerateMipmaps)
                {
                    EditorGUI.indentLevel++;
                    customMipStreaming = EditorGUILayout.Toggle("Mip Streaming", customMipStreaming);
                    if (customMipStreaming)
                    {
                        EditorGUI.indentLevel++;
                        customPriority = EditorGUILayout.IntField("Priority", customPriority);
                        EditorGUI.indentLevel--;
                    }
                    customMipmapFiltering = (TextureImporterMipFilter)EditorGUILayout.EnumPopup("Mip Filtering", customMipmapFiltering);
                    customPreserveCoverage = EditorGUILayout.Toggle("Preserve Coverage", customPreserveCoverage);
                    customReplicateBorder = EditorGUILayout.Toggle("Replicate Border", customReplicateBorder);
                    customFadeoutToGray = EditorGUILayout.Toggle("Fadeout to Gray", customFadeoutToGray);
                    EditorGUI.indentLevel--;
                }
                wrapMode = (TextureWrapMode)EditorGUILayout.EnumPopup("Wrap Mode", wrapMode);
                filterMode = (FilterMode)EditorGUILayout.EnumPopup("Filter", filterMode);
                anisoLevel = EditorGUILayout.IntSlider("Aniso Level" , anisoLevel, 1, 16);
                EditorGUI.indentLevel--;
            }
            
            //平台部分的设置
            TextureGUIStyleHelper.BeginCustomBox("");
            selectedIndex = EditorGUILayout.Popup("Max Size", selectedIndex, Array.ConvertAll(powerOfTwoSizes, x => x.ToString()));
            maxSize = powerOfTwoSizes[selectedIndex];
            resizeAlgorithm = (TextureResizeAlgorithm)EditorGUILayout.EnumPopup("Resize Algorithm", resizeAlgorithm);
            format = (TextureImporterFormat)EditorGUILayout.EnumPopup("Texture Format", format);
            compression = (TextureImporterCompression)EditorGUILayout.EnumPopup("Texture Compression", compression);
            TextureGUIStyleHelper.EndCustomBox();
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Apply Platform Settings"))
            {
                GetSelectedTexturePaths();
                // ApplyPlatformSettings();
            }

            if (GUILayout.Button("Apply Other Settings"))
            {
                ApplyOtherSettings();
            }
        }
        
        private void ApplyPlatformSettings()
        {
            string[] paths = GetSelectedTexturePaths();
            if (paths == null || paths.Length == 0) return;

            foreach (string path in paths)
            {
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

                // Apply platform settings
                TextureImporterPlatformSettings platformSettings = importer.GetDefaultPlatformTextureSettings();
                platformSettings.overridden = true;
                platformSettings.maxTextureSize = maxSize;
                platformSettings.resizeAlgorithm = resizeAlgorithm;
                platformSettings.format = format;
                platformSettings.textureCompression = compression;
            
                importer.SetPlatformTextureSettings(platformSettings);
                importer.SaveAndReimport();
            }

            AssetDatabase.Refresh();
            Debug.Log($"Applied platform settings to {paths.Length} textures.");
        }
        
        private void ApplyOtherSettings()
        {
            string[] paths = GetSelectedTexturePaths();
            if (paths == null || paths.Length == 0) return;

            foreach (string path in paths)
            {
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

                string fileName = Path.GetFileNameWithoutExtension(path);

                // Apply texture type rules first
                TextureRuleProcessor.ProcessTextureRules(importer, fileName, customtextureType, customTextureShape, sRGB);

                // Apply other settings
                importer.alphaSource = customAlphaSource;
                importer.alphaIsTransparency = customAlphaIsTransparency;
                importer.isReadable = customrReadWrite;
                importer.mipmapEnabled = customGenerateMipmaps;
            
                if (customGenerateMipmaps)
                {
                    importer.streamingMipmaps = customMipStreaming;
                    importer.streamingMipmapsPriority = customPriority;
                    importer.mipmapFilter = customMipmapFiltering;
                    importer.mipMapsPreserveCoverage = customPreserveCoverage;
                    importer.borderMipmap = customReplicateBorder;
                    importer.fadeout = customFadeoutToGray;
                }

                importer.wrapMode = wrapMode;
                importer.filterMode = filterMode;
                importer.anisoLevel = anisoLevel;

                importer.SaveAndReimport();
            }

            AssetDatabase.Refresh();
            Debug.Log($"Applied other settings to {paths.Length} textures.");
        }
        
        private string[] GetSelectedTexturePaths()
        {
            // Get selected objects (could be files or folders)
            Object[] selectedObjects = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
            if (selectedObjects.Length == 0)
            {
                Debug.LogWarning("No textures or folders selected.");
                return null;
            }

            // Collect all texture paths
            var texturePaths = new System.Collections.Generic.List<string>();

            foreach (Object obj in selectedObjects)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(path)) continue;

                // If it's a directory, find all textures recursively
                if (Directory.Exists(path))
                {
                    string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                        .Where(file => file.EndsWith(".png") || file.EndsWith(".jpg") || file.EndsWith(".jpeg") || file.EndsWith(".tga") || file.EndsWith(".bmp"))
                        .ToArray();
                    texturePaths.AddRange(files);
                    Debug.Log($"Selected texture path: {path}" + obj.name);
                }
                // If it's a texture file, add it directly
                else if (IsTextureFile(path))
                {
                    texturePaths.Add(path);
                }
            }

            if (texturePaths.Count == 0)
            {
                Debug.LogWarning("No valid texture files found in selection.");
                return null;
            }

            return texturePaths.ToArray();
        }
        
        private bool IsTextureFile(string path)
        {
            string ext = Path.GetExtension(path).ToLower();
            return ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".tga" || ext == ".bmp";
        }
    }
}