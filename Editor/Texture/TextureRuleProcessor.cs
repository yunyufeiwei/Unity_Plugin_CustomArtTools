using System;
using UnityEngine;
using UnityEditor;

namespace yuxuetian
{
    public class TextureRuleProcessor
    {
        // Common settings that will be applied to all textures
        private static TextureImporterAlphaSource  customAlphaSource;
        private static bool customAlphaIsTransparency;
        private static bool customIsReadWrite = false;
        private static bool customVirtualTextureOnly = false;
        private static bool customGenerateMipmaps = false;
        private static bool customMipStreaming = false;
        private static int customProiority = 0;
        private static TextureImporterMipFilter customMipFilter;
        private static bool customPreserveCoverage = false;
        private static float customAlphaCutoff = 0.5f;
        private static bool customReplicateBorder = false;
        private static bool customFadeoutToGray = false;
        private static bool customIgnorePNGGamma = false;
        
        private static TextureWrapMode customWrapMode = TextureWrapMode.Repeat;
        private static FilterMode customFilterMode = FilterMode.Bilinear;
        private static int customAnisoLevel = 1;
        
        private static int customMaxSize = 1024;
        private static TextureResizeAlgorithm customResizeAlgorithm = TextureResizeAlgorithm.Mitchell;
        private static TextureImporterFormat customFormat = TextureImporterFormat.Automatic;
        private static TextureImporterCompression customCompression = TextureImporterCompression.Compressed;

        //初始化基础属性设置,将GUI上设置的属性传入过来
        public static void InitializeBasicSettings( TextureImporterAlphaSource alphaSource, bool alphaIsTransparency, bool isReadable, bool virtualTextureOnly , 
                                                    bool generateMipmaps, bool mipStreaming, int proiority , TextureImporterMipFilter mipFilter, bool preserveCoverage , bool replicateBorder , bool fadeoutToGray,
                                                    float alphaCutoff, bool ignorePNGGamma, TextureWrapMode wrapMode , FilterMode  filterMode, int anisoLevel)
        {
            customAlphaSource = alphaSource;
            customAlphaIsTransparency = alphaIsTransparency;
            
            customIsReadWrite = isReadable;
            customVirtualTextureOnly = virtualTextureOnly;
            customGenerateMipmaps = generateMipmaps;
            customMipStreaming = mipStreaming;
            customProiority = proiority;
            customMipFilter = mipFilter;
            customPreserveCoverage = preserveCoverage;
            customAlphaCutoff = alphaCutoff;
            customReplicateBorder = replicateBorder;
            customFadeoutToGray = fadeoutToGray;
            customIgnorePNGGamma = ignorePNGGamma;
            
            customWrapMode = wrapMode;
            customFilterMode = filterMode;
            customAnisoLevel = anisoLevel;
        }
        
        //初始化平台属性设置
        public static void InitializePlatformSettings(int maxSize , TextureResizeAlgorithm resizeAlgorithm , TextureImporterFormat format , TextureImporterCompression compression)
        {
            
            customMaxSize = maxSize;
            customResizeAlgorithm = resizeAlgorithm;
            customFormat = format;
            customCompression = compression;
        }

        //公用参数设置执行部分
        private static void ApplyCommonSettings(TextureImporter importer)
        {
            importer.alphaSource = customAlphaSource;
            importer.alphaIsTransparency = customAlphaIsTransparency;
            
            importer.isReadable = customIsReadWrite;
            importer.mipmapEnabled = customGenerateMipmaps;
            if (importer.mipmapEnabled == true)
            {
                if (importer.textureShape != TextureImporterShape.TextureCube)
                {
                    importer.vtOnly = customVirtualTextureOnly;
                }
                importer.streamingMipmaps = customMipStreaming;
                importer.streamingMipmapsPriority = customProiority;
                importer.mipmapFilter = customMipFilter;
                importer.mipMapsPreserveCoverage = customPreserveCoverage;
                if (importer.mipMapsPreserveCoverage == true)
                {
                    importer.alphaTestReferenceValue = customAlphaCutoff;
                }
                importer.borderMipmap = customReplicateBorder;
                importer.fadeout  = customFadeoutToGray;
                importer.anisoLevel = customAnisoLevel;
            }
            else
            {
                importer.vtOnly = false;
                importer.anisoLevel = 1;
            }
            importer.ignorePngGamma = customIgnorePNGGamma;
            
            importer.wrapMode = customWrapMode;
            importer.filterMode = customFilterMode;
        }

        //平台参数设置部分
        private static void ApplyPlatformSettings(TextureImporter importer ,string path)
        {
            TextureImporterPlatformSettings platformSettings = importer.GetDefaultPlatformTextureSettings();
            
            if (platformSettings.maxTextureSize > customMaxSize)
            {
                platformSettings.maxTextureSize = customMaxSize;
            }
            platformSettings.resizeAlgorithm = customResizeAlgorithm;
            platformSettings.format = customFormat;
            platformSettings.textureCompression = customCompression;
            
            importer.SetPlatformTextureSettings(platformSettings);
            // 可选：应用到其他特定平台（如Android、iOS）
            // ApplyToSpecificPlatforms(importer);
        }
        
        private static void ApplyToSpecificPlatforms(TextureImporter importer)
        {
            // 示例：应用到Android平台（如果该平台设置被覆盖）
            TextureImporterPlatformSettings androidSettings = importer.GetPlatformTextureSettings("Android");
            if (androidSettings.overridden)
            {
                androidSettings.maxTextureSize = customMaxSize;
                androidSettings.resizeAlgorithm = customResizeAlgorithm;
                androidSettings.format = customFormat;
                androidSettings.textureCompression = customCompression;
                importer.SetPlatformTextureSettings(androidSettings);
            }
        }
      
        //根据前缀和后缀进行纹理的类型与sRGB设置
        public static void SetCharacterTextureSettings(TextureImporter importer, string fileName , TextureImporterPlatformSettings platformSettings)
        {
            if (fileName.StartsWith("T_C_", StringComparison.OrdinalIgnoreCase))
            {
                if (fileName.EndsWith("_B", StringComparison.OrdinalIgnoreCase))
                {
                    //这三个属性仅根据纹理贴图的命名规则直接定义，基于部分人员对纹理类型不太了解，因此不允许通过外部设置来修改
                    importer.textureType = TextureImporterType.Default;
                    importer.textureShape = TextureImporterShape.Texture2D;
                    importer.sRGBTexture = true;
                }
                else if (fileName.EndsWith("_N", StringComparison.OrdinalIgnoreCase))
                {
                    importer.textureType = TextureImporterType.NormalMap;
                    importer.textureShape = TextureImporterShape.Texture2D;
                    importer.sRGBTexture = false;
                }
                else if (fileName.EndsWith("_M", StringComparison.OrdinalIgnoreCase) ||
                         fileName.EndsWith("_Mask", StringComparison.OrdinalIgnoreCase) ||
                         fileName.EndsWith("_MRA", StringComparison.OrdinalIgnoreCase))
                {
                    importer.textureType = TextureImporterType.Default;
                    importer.textureShape = TextureImporterShape.Texture2D;
                    importer.sRGBTexture = false;
                }
                else if (fileName.EndsWith("_Cube", StringComparison.OrdinalIgnoreCase))
                {
                    importer.textureType = TextureImporterType.Default;
                    importer.textureShape = TextureImporterShape.TextureCube;
                    importer.sRGBTexture = true;
                }
                ApplyCommonSettings(importer);
                ApplyPlatformSettings(importer , fileName);
            }   
        }

        public static void SetSceneTextureSettings(TextureImporter importer, string fileName , TextureImporterPlatformSettings platformSettings)
        {
            if (fileName.StartsWith("T_L_", StringComparison.OrdinalIgnoreCase))
            {
                if (fileName.EndsWith("_B", StringComparison.OrdinalIgnoreCase))
                {
                    //这三个属性仅根据纹理贴图的命名规则直接定义，基于部分人员对纹理类型不太了解，因此不允许通过外部设置来修改
                    importer.textureType = TextureImporterType.Default;
                    importer.textureShape = TextureImporterShape.Texture2D;
                    importer.sRGBTexture = true;
                }
                else if (fileName.EndsWith("_N", StringComparison.OrdinalIgnoreCase))
                {
                    importer.textureType = TextureImporterType.NormalMap;
                    importer.textureShape = TextureImporterShape.Texture2D;
                    importer.sRGBTexture = false;
                }
                else if (fileName.EndsWith("_M", StringComparison.OrdinalIgnoreCase) ||
                         fileName.EndsWith("_Mask", StringComparison.OrdinalIgnoreCase) ||
                         fileName.EndsWith("_MRA", StringComparison.OrdinalIgnoreCase))
                {
                    importer.textureType = TextureImporterType.Default;
                    importer.textureShape = TextureImporterShape.Texture2D;
                    importer.sRGBTexture = false;
                }
                else if (fileName.EndsWith("_Cube", StringComparison.OrdinalIgnoreCase))
                {
                    importer.textureType = TextureImporterType.Default;
                    importer.textureShape = TextureImporterShape.TextureCube;
                    importer.sRGBTexture = true;
                }
                ApplyCommonSettings(importer);
                ApplyPlatformSettings(importer , fileName);
            }
        }
        
        public static void SetEffectTextureSettings(TextureImporter importer, string fileName , TextureImporterPlatformSettings platformSettings)
        {
            if (fileName.StartsWith("T_E_", StringComparison.OrdinalIgnoreCase))
            {
                if (fileName.EndsWith("_B", StringComparison.OrdinalIgnoreCase))
                {
                    //这三个属性仅根据纹理贴图的命名规则直接定义，基于部分人员对纹理类型不太了解，因此不允许通过外部设置来修改
                    importer.textureType = TextureImporterType.Default;
                    importer.textureShape = TextureImporterShape.Texture2D;
                    importer.sRGBTexture = true;
                }
                else if (fileName.EndsWith("_N", StringComparison.OrdinalIgnoreCase))
                {
                    importer.textureType = TextureImporterType.NormalMap;
                    importer.textureShape = TextureImporterShape.Texture2D;
                    importer.sRGBTexture = false;
                }
                else if (fileName.EndsWith("_M", StringComparison.OrdinalIgnoreCase) ||
                         fileName.EndsWith("_Mask", StringComparison.OrdinalIgnoreCase) ||
                         fileName.EndsWith("_MRA", StringComparison.OrdinalIgnoreCase))
                {
                    importer.textureType = TextureImporterType.Default;
                    importer.textureShape = TextureImporterShape.Texture2D;
                    importer.sRGBTexture = false;
                }
                else if (fileName.EndsWith("_Cube", StringComparison.OrdinalIgnoreCase))
                {
                    importer.textureType = TextureImporterType.Default;
                    importer.textureShape = TextureImporterShape.TextureCube;
                    importer.sRGBTexture = true;
                }
                ApplyCommonSettings(importer);
                ApplyPlatformSettings(importer , fileName);
            }
        }

        public static void SetUITextureSettings(TextureImporter importer, string fileName , TextureImporterPlatformSettings platformSettings)
        {
            if (fileName.StartsWith("T_UI_", StringComparison.OrdinalIgnoreCase))
            {
                if (fileName.EndsWith("_B", StringComparison.OrdinalIgnoreCase))
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.textureShape = TextureImporterShape.Texture2D;
                    importer.sRGBTexture = true;
                }
                else if (fileName.EndsWith("_N", StringComparison.OrdinalIgnoreCase))
                {
                    importer.textureType = TextureImporterType.NormalMap;
                    importer.textureShape = TextureImporterShape.Texture2D;
                    importer.sRGBTexture = true;
                }
                else if (fileName.EndsWith("_M", StringComparison.OrdinalIgnoreCase) ||
                         fileName.EndsWith("_Mask", StringComparison.OrdinalIgnoreCase) ||
                         fileName.EndsWith("_MRA", StringComparison.OrdinalIgnoreCase))
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.textureShape = TextureImporterShape.Texture2D;
                    importer.sRGBTexture = false;
                }
                importer.spriteImportMode = SpriteImportMode.Single;
            }
            ApplyCommonSettings(importer);
            ApplyPlatformSettings(importer , fileName);
            
            //对于UI纹理，先按照规则执行一遍上述设置，然后单独将前缀表示是UI的纹理额alpha进行额外的设置
            importer.alphaIsTransparency = true;

        }
    }
}