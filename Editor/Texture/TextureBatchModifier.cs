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
        
        private TextureImporterAlphaSource _AlphaSource = TextureImporterAlphaSource.FromInput;
        private bool _AlphaIsTransparency = false; 

        // Advanced Settings
        private bool _isAdvanced = false;
        private bool _readWrite = false;
        private bool _virtualTextureOnly = false;
        private bool _generateMipmaps = false;
        private bool _mipStreaming = false;
        private int  _priority = 0;
        private TextureImporterMipFilter _mipmapFiltering = TextureImporterMipFilter.BoxFilter;  
        private bool _preserveCoverage = false;
        private bool _replicateBorder = false;
        private bool _fadeoutToGray = false;
        private bool _ignorePNGGamma = false;
        
        private TextureWrapMode _wrapMode = TextureWrapMode.Repeat;
        private FilterMode _filterMode = FilterMode.Bilinear;
        private int _anisoLevel = 1;

        private int _maxSize = 1024;
        private int[] powerOfTwoSizes = { 32, 64, 128, 256, 512, 1024, 2048, 4096 };
        private int selectedIndex = 1;
        private TextureResizeAlgorithm _resizeAlgorithm = TextureResizeAlgorithm.Mitchell;
        private TextureImporterFormat _format = TextureImporterFormat.Automatic;
        private TextureImporterCompression _compression = TextureImporterCompression.Compressed;

        [MenuItem("ArtTools/Texture/Texture Batch Modifier" , false, 201)]
        public static void ShowWindow()
        {
            GetWindow<TextureBatchModifier>("Texture Batch Modifier");
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
           
            _AlphaSource = (TextureImporterAlphaSource)EditorGUILayout.EnumPopup("Alpha Source" , _AlphaSource);
            // 使用DisableScope控制Alpha Is Transparency的可编辑状态，当Alpha来源为None时，此选项会变灰且不可编辑
            using (new EditorGUI.DisabledScope(_AlphaSource == TextureImporterAlphaSource.None))
            {
                _AlphaIsTransparency = EditorGUILayout.Toggle("Alpha is Transparency", _AlphaIsTransparency);
            }
            GUILayout.Space(10);
            
            _isAdvanced = EditorGUILayout.Foldout(_isAdvanced, "Advanced");
            if (_isAdvanced)
            {
                EditorGUI.indentLevel++;
                _readWrite = EditorGUILayout.Toggle("Read/Write", _readWrite);
                _virtualTextureOnly = EditorGUILayout.Toggle("Virtual Texture Only", _virtualTextureOnly);
                _generateMipmaps = EditorGUILayout.Toggle("Generate Mipmaps", _generateMipmaps);
                if (_generateMipmaps)
                {
                    EditorGUI.indentLevel++;
                    _mipStreaming = EditorGUILayout.Toggle("Mip Streaming", _mipStreaming);
                    if (_mipStreaming)
                    {
                        EditorGUI.indentLevel++;
                        _priority = EditorGUILayout.IntField("Priority", _priority);
                        EditorGUI.indentLevel--;
                    }
                    _mipmapFiltering = (TextureImporterMipFilter)EditorGUILayout.EnumPopup("Mip Filtering", _mipmapFiltering);
                    _preserveCoverage = EditorGUILayout.Toggle("Preserve Coverage", _preserveCoverage);
                    _replicateBorder = EditorGUILayout.Toggle("Replicate Border", _replicateBorder);
                    _fadeoutToGray = EditorGUILayout.Toggle("Fadeout to Gray", _fadeoutToGray);
                    EditorGUI.indentLevel--;
                }
                _ignorePNGGamma = EditorGUILayout.Toggle("Ignore PNG Gamma", _ignorePNGGamma);
                EditorGUI.indentLevel--;
                GUILayout.Space(10);
                
                _wrapMode = (TextureWrapMode)EditorGUILayout.EnumPopup("Wrap Mode", _wrapMode);
                _filterMode = (FilterMode)EditorGUILayout.EnumPopup("Filter", _filterMode);
                using (new EditorGUI.DisabledScope(_generateMipmaps == false))
                {
                    _anisoLevel = EditorGUILayout.IntSlider("Aniso Level" , _anisoLevel, 1, 16);
                }
            }
            
            //平台设置的GUI
            TextureGUIStyleHelper.BeginCustomBox("");
            selectedIndex = EditorGUILayout.Popup("Max Size", selectedIndex, Array.ConvertAll(powerOfTwoSizes, x => x.ToString()));
            _maxSize = powerOfTwoSizes[selectedIndex];
            _resizeAlgorithm = (TextureResizeAlgorithm)EditorGUILayout.EnumPopup("Resize Algorithm", _resizeAlgorithm);
            _format = (TextureImporterFormat)EditorGUILayout.EnumPopup("Format", _format);
            _compression = (TextureImporterCompression)EditorGUILayout.EnumPopup("Compression", _compression);
            TextureGUIStyleHelper.EndCustomBox();
            
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Apply Settings"))
            {
                ApplyDefaultSettings();
            }
            GUILayout.Space(10);
        }
        
        private void ApplyDefaultSettings()
        {
            string[] paths = GetSelectedTexturePaths();
            if (paths == null || paths.Length == 0) return;
            
            TextureRuleProcessor.InitializeBasicSettings(_AlphaSource , _AlphaIsTransparency , _readWrite , _virtualTextureOnly,_generateMipmaps,_ignorePNGGamma , _wrapMode, _filterMode, _anisoLevel);
            TextureRuleProcessor.InitializePlatformSettings(_maxSize , _resizeAlgorithm ,_format , _compression);
            
            foreach (string path in paths)
            {
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                TextureImporterPlatformSettings platformSettings = importer.GetDefaultPlatformTextureSettings();
                if (importer == null) continue;

                string fileName = Path.GetFileNameWithoutExtension(path);
                
                TextureRuleProcessor.SetCharacterTextureSettings(importer, fileName , platformSettings);
                TextureRuleProcessor.SetSceneTextureSettings(importer, fileName , platformSettings);
                TextureRuleProcessor.SetEffectTextureSettings(importer, fileName , platformSettings);
                TextureRuleProcessor.SetUITextureSettings(importer, fileName , platformSettings);

                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
            }
            AssetDatabase.Refresh();
        }
        
        private string[] GetSelectedTexturePaths()
        {
            Object[] selectedObjects = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
            if (selectedObjects.Length == 0) return null;

            var texturePaths = new List<string>();

            foreach (Object obj in selectedObjects)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(path)) continue;

                if (Directory.Exists(path))
                {
                    texturePaths.AddRange(Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).Where(IsTextureFile));
                }
                else if (IsTextureFile(path))
                {
                    texturePaths.Add(path);
                }
            }

            //如果选择的路径下没有贴图文件，打印提示。
            if (texturePaths.Count == 0)
            {
                Debug.LogWarning("No valid texture files found in selection.");
                return null;
            }

            return texturePaths.ToArray();
        }
        
        /// <summary>
        /// 检查给定路径的文件是否为支持的纹理格式（.png 或 .tga）
        /// 该方法不区分扩展名大小写，会同等处理.PNG/.png/.Png等所有大小写变体
        /// </summary>
        private bool IsTextureFile(string path)
        {
            // 获取文件扩展名（包含点符号，如".png"）
            string ext = Path.GetExtension(path);
            // 检查扩展名是否为.png或.tga（不区分大小写）
            return ext.Equals(".png", StringComparison.OrdinalIgnoreCase) || 
                   ext.Equals(".tga", StringComparison.OrdinalIgnoreCase);
        }
    }
}