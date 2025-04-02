using System;
using UnityEngine;
using UnityEditor;

namespace yuxuetian
{
    public static class TextureRuleProcessor
    {
        // 默认使用全局设置
        private static TextureImporterType currentType;
        private static TextureImporterShape currentShape;
        private static  bool currentSRGB;
        
        public static void ProcessTextureRules(TextureImporter importer, string fileName, 
            TextureImporterType defaultType, TextureImporterShape defaultShape, bool defaultSRGB)
        {
            ResetToDefaults(defaultType, defaultShape, defaultSRGB);
            ProcessPrefixRules(importer, fileName);
        }
        
        private static void ResetToDefaults(TextureImporterType type, TextureImporterShape shape, bool sRGB)
        {
            currentType = type;
            currentShape = shape;
            currentSRGB = sRGB;
        }
        
        //判断前缀
        private static void ProcessPrefixRules(TextureImporter importer, string fileName)
        {
            if (fileName.StartsWith("T_C_", StringComparison.OrdinalIgnoreCase))
            {
                if (fileName.EndsWith("_B", StringComparison.OrdinalIgnoreCase))
                {
                    currentType = TextureImporterType.Default;
                    currentShape = TextureImporterShape.Texture2D;
                    currentSRGB = true;
                    importer.isReadable = false;
                    importer.mipmapEnabled = true;
                }
                else if (fileName.EndsWith("_N", StringComparison.OrdinalIgnoreCase))
                {
                    currentType = TextureImporterType.NormalMap;
                    currentShape = TextureImporterShape.Texture2D;
                    currentSRGB = false;
                    importer.isReadable = false;
                    importer.mipmapEnabled = true;
                }
                else if (fileName.EndsWith("_M", StringComparison.OrdinalIgnoreCase) ||
                         fileName.EndsWith("_Mask", StringComparison.OrdinalIgnoreCase))
                {
                    currentType = TextureImporterType.Default;
                    currentShape = TextureImporterShape.Texture2D;
                    currentSRGB = false;
                    importer.isReadable = false;
                    importer.mipmapEnabled = true;
                }
                else if (fileName.EndsWith("_Cube", StringComparison.OrdinalIgnoreCase))
                {
                    currentType = TextureImporterType.Default;
                    currentShape = TextureImporterShape.TextureCube;
                    currentSRGB = true;
                    importer.isReadable = false;
                    importer.mipmapEnabled = true;
                }
            }
            else if (fileName.StartsWith("T_UI_", StringComparison.OrdinalIgnoreCase))
            {
                currentType = TextureImporterType.Sprite;
                currentShape = TextureImporterShape.Texture2D;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Bilinear;
            }
        }
    }
}