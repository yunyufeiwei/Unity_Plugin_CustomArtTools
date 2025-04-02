using UnityEngine;
using UnityEditor;

namespace yuxuetian
{
    public static class TextureGUIStyleHelper
    {
        private static GUIStyle _boxStyle;
        private static Texture2D _boxBackground;
    
        // 创建纯色纹理的辅助方法
        private static Texture2D CreateSolidTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;
        
            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
    
        // 初始化样式
        private static void InitializeStyles()
        {
            if (_boxStyle == null)
            {
                _boxStyle = new GUIStyle(GUI.skin.box);
                _boxBackground = CreateSolidTexture(2, 2, new Color(0.15f, 0.15f, 0.15f, 0.5f));
                _boxStyle.normal.background = _boxBackground;
                _boxStyle.padding = new RectOffset(10, 10, 10, 10);
                _boxStyle.margin = new RectOffset(5, 5, 5, 5);
            }
        }
    
        // 开始带标题的自定义框
        public static void BeginCustomBox(string title)
        {
            InitializeStyles();
        
            EditorGUILayout.BeginVertical(_boxStyle);
            if (!string.IsNullOrEmpty(title))
            {
                EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            }
        }
    
        // 结束自定义框
        public static void EndCustomBox()
        {
            EditorGUILayout.EndVertical();
        }
    
        // 清理资源
        public static void Cleanup()
        {
            if (_boxBackground != null)
            {
                Object.DestroyImmediate(_boxBackground);
                _boxBackground = null;
            }
            _boxStyle = null;
        }
    }
}