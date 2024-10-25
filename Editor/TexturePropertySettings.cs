using System;
using System.Collections.Generic;
using System.Drawing.Text;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace yuxuetian
{
    public class TexturePropertySettings : EditorWindow
    {
        public enum TextureMaxSize
        {
            [InspectorName("128")] MaxSize128,
            [InspectorName("256")] MaxSize256,
            [InspectorName("512")] MaxSize512,
            [InspectorName("1024")] MaxSize1024,
            [InspectorName("2048")] MaxSize2048,
        }

        public enum TextureWarpMode
        {
            Repeat,
            Clamp
        }

        public enum TextureFilterMode
        {
            Point,
            Billinear,
            Trilinear
        }

        // 声明一个列表，用来存储选择的贴图数据
        private List<TextureImporter> _textureImporters = new List<TextureImporter>();
        private List<string> texturePaths = new List<string>();
        private List<Texture2D> textures2D = new List<Texture2D>();
        private Vector2 scrllViewValue = new Vector2(2.0f, 20.0f);

        #region 贴图公共属性

        public bool Property = true;
        public bool isReadWrite = false;
        public bool isGenerateMipmaps = true;
        public TextureWarpMode texWarpMode = TextureWarpMode.Repeat;
        public TextureFilterMode texFilterMode = TextureFilterMode.Billinear;
        public TextureMaxSize sceneTextureMaxSize = TextureMaxSize.MaxSize512;
        public bool checkAlphaChannel = true;

        #endregion

        #region 场景贴图规则选项

        public bool sceneTextureProperty = false;
        public string sceneTexturePrefix = "T_L_";
        public string sceneTextureSuffixColor = "_B";
        public string sceneTextureSuffixNormal = "_N";
        public string sceneTextureSuffixMix = "_MRA";
        public string sceneTextureSuffixMask = "_Mask";
        public string sceneTextureSuffixGray = "_Gray";
        public string sceneTextureSuffixCube = "_Cube";

        #endregion

        #region 角色贴图规则选项

        public bool characterTextureProperty = false;
        public string characterTexturePrefix = "T_C_";
        public string characterTextureSuffixColor = "_B";
        public string characterTextureSuffixNormal = "_N";
        public string characterTextureSuffixMix = "_MRA";
        public string characterTextureSuffixMask = "_Mask";
        public string characterTextureSuffixCube = "_Cube";

        #endregion

        #region 特效贴图规则选项

        public bool effectTextureProperty = false;
        public string effectTexturePrefix = "T_E_";
        public string effectTextureSuffixColor = "_B";
        public string effectTextureSuffixNomral = "_N";
        public string effectTextureSuffixMask = "_Mask"; //多个单通道混合
        public string effectTextureSuffixGray = "_Gray"; //只有一个通道，或者说三个通道都是一样的

        #endregion

        #region UI贴图规则选项

        public bool uiTextureProperty = false;
        public bool isSpriteTexture = true;
        public string uiTexturePropertyPrefix = "T_UI_";
        public string uiTexturePropertySuffixColor = "_B";
        public string uiTexturePropertySuffixMask = "_Mask";
        public string uiTexturePropertySuffixGray = "_Gray";

        #endregion

        #region 武器贴图规则选项

        public bool weaponTextureProperty = false;
        public string weaponTexturePrefix = "T_W_";
        public string weaponTextureSuffixColor = "_B";
        public string weaponTextureSuffixNormal = "_N";
        public string weaponTextureSuffixMix = "_MRA";
        public string weaponTextureSuffixMask = "_Mask";
        public string weaponTextureSuffixGray = "_Gray";

        #endregion

        //
        [MenuItem("ArtTools/Texture/贴图属性设置", false, 200)]
        public static void ShowWindow()
        {
            TexturePropertySettings window =
                (TexturePropertySettings)EditorWindow.GetWindow(typeof(TexturePropertySettings));
            window.titleContent = new GUIContent("贴图属性设置");

            //设置窗口面板的尺寸
            Vector2 size = new Vector2(400.0f, 600.0f);
            window.minSize = size;
            window.maxSize = size;

            window.Show();
        }

        // 定义点击函数，执行函数体里面的内容
        private void OnGUI()
        {
            //右侧滑动条，当整个页面不能满足显示所有内容时，就需要用到滑动条来拖动显示了
            scrllViewValue = GUILayout.BeginScrollView(scrllViewValue, EditorStyles.label);

            #region 公共属性设置

            Property = EditorGUILayout.Foldout(Property, "公共属性设置");
            if (Property)
            {
                checkAlphaChannel = GUILayout.Toggle(checkAlphaChannel, "是否有Alpha通道");

                isReadWrite = GUILayout.Toggle(isReadWrite, "是否可读写");
                isGenerateMipmaps = GUILayout.Toggle(isGenerateMipmaps, "是否生成Mipmap贴图");

                GUILayout.Space(10);
                GUILayout.BeginHorizontal("纹理平铺模式", EditorStyles.label);
                GUILayout.FlexibleSpace();
                texWarpMode = (TextureWarpMode)EditorGUILayout.EnumPopup(texWarpMode);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal("纹理混合模式", EditorStyles.label);
                GUILayout.FlexibleSpace();
                texFilterMode = (TextureFilterMode)EditorGUILayout.EnumPopup(texFilterMode);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal("纹理尺寸", EditorStyles.label);
                GUILayout.FlexibleSpace();
                sceneTextureMaxSize = (TextureMaxSize)EditorGUILayout.EnumPopup(sceneTextureMaxSize);
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10);

            #endregion

            GUILayout.BeginVertical();

            #region 场景属性面板绘制

            sceneTextureProperty = EditorGUILayout.Foldout(sceneTextureProperty, "场景命名规则设置");
            if (sceneTextureProperty)
            {
                sceneTexturePrefix = EditorGUILayout.TextField("   贴图前缀", sceneTexturePrefix);
                sceneTextureSuffixColor = EditorGUILayout.TextField("   颜色贴图后缀", sceneTextureSuffixColor);
                sceneTextureSuffixNormal = EditorGUILayout.TextField("   法线贴图后缀", sceneTextureSuffixNormal);
                sceneTextureSuffixMix = EditorGUILayout.TextField("   金属粗糙后缀", sceneTextureSuffixMix);
                sceneTextureSuffixMask = EditorGUILayout.TextField("   多通道混合后缀", sceneTextureSuffixMask);
                sceneTextureSuffixGray = EditorGUILayout.TextField("   灰度图后缀", sceneTextureSuffixGray);
                sceneTextureSuffixCube = EditorGUILayout.TextField("   CubeMap后缀", sceneTextureSuffixCube);
            }

            if (GUILayout.Button("批处理场景贴图属性"))
            {
                AddTextureData();
                SettingSceneTextureProperty();
            }

            GUILayout.Space(10);

            #endregion

            #region 角色属性面板绘制

            characterTextureProperty = EditorGUILayout.Foldout(characterTextureProperty, "角色命名规则设置");
            if (characterTextureProperty)
            {
                characterTexturePrefix = EditorGUILayout.TextField("   贴图前缀", characterTexturePrefix);
                characterTextureSuffixColor = EditorGUILayout.TextField("   颜色贴图后缀", characterTextureSuffixColor);
                characterTextureSuffixNormal = EditorGUILayout.TextField("   法线贴图后缀", characterTextureSuffixNormal);
                characterTextureSuffixMix = EditorGUILayout.TextField("   金属粗糙后缀", characterTextureSuffixMix);
                characterTextureSuffixMask = EditorGUILayout.TextField("   多通道混合后缀", characterTextureSuffixMask);
                characterTextureSuffixCube = EditorGUILayout.TextField("   CubeMap贴图后缀", characterTextureSuffixCube);
            }

            if (GUILayout.Button("批处理角色贴图属性"))
            {
                AddTextureData();
                SettingCharacterTextureProperty();
            }

            GUILayout.Space(10);

            #endregion

            #region 特效属性面板绘制

            effectTextureProperty = EditorGUILayout.Foldout(effectTextureProperty, "特效命名规则设置");
            if (effectTextureProperty)
            {
                effectTexturePrefix = EditorGUILayout.TextField("   贴图前缀", effectTexturePrefix);
                effectTextureSuffixColor = EditorGUILayout.TextField("   颜色贴图后缀", effectTextureSuffixColor);
                effectTextureSuffixNomral = EditorGUILayout.TextField("   法线贴图后缀", effectTextureSuffixNomral);
                effectTextureSuffixMask = EditorGUILayout.TextField("   多通道混合后缀", effectTextureSuffixMask);
                effectTextureSuffixGray = EditorGUILayout.TextField("   灰度图后缀", effectTextureSuffixGray);
            }

            if (GUILayout.Button("批处理特效贴图属性"))
            {
                AddTextureData();
                SettingEffectTextureProperty();
            }

            GUILayout.Space(10);

            #endregion

            #region UI属性面板绘制

            uiTextureProperty = EditorGUILayout.Foldout(uiTextureProperty, "UI命名规则设置");
            if (uiTextureProperty)
            {
                isSpriteTexture = GUILayout.Toggle(isSpriteTexture, "是否使用精灵图");
                uiTexturePropertyPrefix = EditorGUILayout.TextField("   贴图前缀", uiTexturePropertyPrefix);
                uiTexturePropertySuffixColor = EditorGUILayout.TextField("   颜色贴图后缀", uiTexturePropertySuffixColor);
                uiTexturePropertySuffixMask = EditorGUILayout.TextField("   多通道混合贴图", uiTexturePropertySuffixMask);
                uiTexturePropertySuffixGray = EditorGUILayout.TextField("   单通道(三通道一致)", uiTexturePropertySuffixGray);
            }

            if (GUILayout.Button("批处理UI贴图属性"))
            {
                AddTextureData();
                SettingsUITextureProperty();
            }

            GUILayout.Space(10);

            #endregion

            #region 武器属性面板绘制

            weaponTextureProperty = EditorGUILayout.Foldout(weaponTextureProperty, "武器命名规则设置");
            if (weaponTextureProperty)
            {
                weaponTexturePrefix = EditorGUILayout.TextField("   贴图前缀", weaponTexturePrefix);
                weaponTextureSuffixColor = EditorGUILayout.TextField("   颜色贴图后缀", weaponTextureSuffixColor);
                weaponTextureSuffixNormal = EditorGUILayout.TextField("   法线贴图后缀", weaponTextureSuffixNormal);
                weaponTextureSuffixMix = EditorGUILayout.TextField("   金属粗糙混合贴图", weaponTextureSuffixMix);
                weaponTextureSuffixMask = EditorGUILayout.TextField("   多通道混合贴图", weaponTextureSuffixMask);
                weaponTextureSuffixGray = EditorGUILayout.TextField("   单通道(三通道一致)", weaponTextureSuffixGray);
            }

            if (GUILayout.Button("批处理武器贴图属性"))
            {
                AddTextureData();
                SettingsWeaponTextureProperty();
            }

            GUILayout.Space(10);

            #endregion

            GUILayout.EndVertical();
            
            GUILayout.EndScrollView();
        }

        // 选择贴图并返回
        private Object[] GetSelectedTextureObjs()
        {
            Object[] selectedTextures = Selection.GetFiltered(typeof(Texture), SelectionMode.Assets);
            //测试输出选中的贴图名称
            // for (int i = 0; i < selectedTextures.Length; i++)
            // {
            //     Debug.Log("TextureName:---" + selectedTextures[i]);
            // }
            return selectedTextures;
        }

        // 添加选择贴图到列表数据
        private void AddTextureData()
        {
            _textureImporters.Clear();
            texturePaths.Clear();

            Object[] selectedTextureData = GetSelectedTextureObjs();
            foreach (Object texture in selectedTextureData)
            {
                string texturePath = AssetDatabase.GetAssetPath(texture);
                texturePaths.Add(texturePath);
                TextureImporter textureImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;
                if (textureImporter != null)
                {
                    _textureImporters.Add(textureImporter);
                }
            }

            foreach (Texture2D texture in selectedTextureData)
            {
                textures2D.Add(texture);
            }
        }

        private void SettingCommonProperty()
        {
            for (int i = 0; i < _textureImporters.Count; i++)
            {
                string textureName = System.IO.Path.GetFileNameWithoutExtension(texturePaths[i]);

                //是否启用透明通道检查
                if (checkAlphaChannel == true)
                {
                    //如果启用了透明通道，则检测该贴图是否具有透明通道，如果没有仍然设置为None
                    AssetDatabase.ImportAsset(texturePaths[i]);
                    if (_textureImporters[i]
                        .DoesSourceTextureHaveAlpha()) //DoesSourceTextureHaveAlpha(),源贴图是否有Alpha通道,返回值是一个布尔型
                    {
                        _textureImporters[i].alphaSource = TextureImporterAlphaSource.FromInput;
                        _textureImporters[i].alphaIsTransparency = true;
                    }
                    else
                    {
                        _textureImporters[i].alphaSource = TextureImporterAlphaSource.None;
                        _textureImporters[i].alphaIsTransparency = false;
                    }
                }
                else //如果没有Alpha通道，则直接设置为false
                {
                    AssetDatabase.ImportAsset(texturePaths[i]);
                    _textureImporters[i].alphaSource = TextureImporterAlphaSource.None;
                    _textureImporters[i].alphaIsTransparency = false;
                }

                //只要前缀是符合场景前缀规则的，首先将最大尺寸修改
                switch (sceneTextureMaxSize)
                {
                    case TextureMaxSize.MaxSize128:
                        if (_textureImporters[i].maxTextureSize <= 128)
                        {
                            //这里不能加下面内容，会打断循环，如果希望目前尺寸走到这一块时，符合当前条件，则继续往下走,不用再当前条件里面有任何事情执行
                            // continue;    
                            // break;
                        }
                        else
                        {
                            _textureImporters[i].maxTextureSize = 128;
                        }

                        break;
                    case TextureMaxSize.MaxSize256:
                        if (_textureImporters[i].maxTextureSize <= 256)
                        {
                            // continue;
                        }
                        else
                        {
                            _textureImporters[i].maxTextureSize = 256;
                        }

                        break;
                    case TextureMaxSize.MaxSize512:
                        if (_textureImporters[i].maxTextureSize <= 512)
                        {
                            // continue;
                        }
                        else
                        {
                            _textureImporters[i].maxTextureSize = 512;
                        }

                        break;
                    case TextureMaxSize.MaxSize1024:
                        if (_textureImporters[i].maxTextureSize <= 1024)
                        {
                            // continue;
                        }
                        else
                        {
                            _textureImporters[i].maxTextureSize = 1024;
                        }

                        break;
                    case TextureMaxSize.MaxSize2048:
                        if (_textureImporters[i].maxTextureSize <= 2048)
                        {
                            // continue;
                        }
                        else
                        {
                            _textureImporters[i].maxTextureSize = 2048;
                        }

                        break;
                }

                //设置纹理可读写属性
                _textureImporters[i].isReadable = isReadWrite;

                //设置Mipmap属性
                _textureImporters[i].mipmapEnabled = isGenerateMipmaps;


                switch (texWarpMode)
                {
                    case TextureWarpMode.Repeat:
                        _textureImporters[i].wrapMode = TextureWrapMode.Repeat;
                        break;
                    case TextureWarpMode.Clamp:
                        _textureImporters[i].wrapMode = TextureWrapMode.Clamp;
                        break;
                }


                if (texFilterMode == TextureFilterMode.Point)
                {
                    _textureImporters[i].filterMode = UnityEngine.FilterMode.Point;
                }
                else if (texFilterMode == TextureFilterMode.Billinear)
                {
                    _textureImporters[i].filterMode = UnityEngine.FilterMode.Bilinear;
                }
                else if (texFilterMode == TextureFilterMode.Trilinear)
                {
                    _textureImporters[i].filterMode = UnityEngine.FilterMode.Trilinear;
                }
            }
        }

        // 场景贴图属性
        private void SettingSceneTextureProperty()
        {
            SettingCommonProperty();

            for (int i = 0; i < _textureImporters.Count; i++)
            {
                string textureName = System.IO.Path.GetFileNameWithoutExtension(texturePaths[i]);

                //判断是否
                if (textureName.StartsWith(sceneTexturePrefix) == true)
                {
                    if (textureName.EndsWith(sceneTextureSuffixColor))
                    {
                        _textureImporters[i].textureType = TextureImporterType.Default;
                        _textureImporters[i].textureShape = TextureImporterShape.Texture2D;
                        _textureImporters[i].sRGBTexture = true;

                    }
                    else if (textureName.EndsWith(sceneTextureSuffixNormal))
                    {
                        _textureImporters[i].textureType = TextureImporterType.NormalMap;
                        _textureImporters[i].textureShape = TextureImporterShape.Texture2D;
                        _textureImporters[i].sRGBTexture = false;
                    }
                    else if (textureName.EndsWith(sceneTextureSuffixMix))
                    {
                        _textureImporters[i].textureType = TextureImporterType.Default;
                        _textureImporters[i].textureShape = TextureImporterShape.Texture2D;
                        _textureImporters[i].sRGBTexture = false;
                    }
                    else if (textureName.EndsWith(sceneTextureSuffixMask))
                    {
                        _textureImporters[i].textureType = TextureImporterType.Default;
                        _textureImporters[i].textureShape = TextureImporterShape.Texture2D;
                        _textureImporters[i].sRGBTexture = false;
                    }
                    else if (textureName.EndsWith(sceneTextureSuffixGray))
                    {
                        _textureImporters[i].textureType = TextureImporterType.Default;
                        _textureImporters[i].textureShape = TextureImporterShape.Texture2D;
                        _textureImporters[i].sRGBTexture = false;
                    }
                    else if (textureName.EndsWith(sceneTextureSuffixCube))
                    {
                        _textureImporters[i].textureType = TextureImporterType.Default;
                        _textureImporters[i].textureShape = TextureImporterShape.TextureCube;
                        _textureImporters[i].sRGBTexture = true;
                    }

                    //保存纹理
                    _textureImporters[i].SaveAndReimport();
                }
                else
                {
                    Debug.LogWarning("贴图命名规则不符合批处理要求！" + _textureImporters[i].assetPath);
                }
            }
        }

        //角色贴图属性
        private void SettingCharacterTextureProperty()
        {
            //通用变量属性设置
            SettingCommonProperty();

            for (int i = 0; i < _textureImporters.Count; i++)
            {
                string textureName = System.IO.Path.GetFileNameWithoutExtension(texturePaths[i]);

                //判断是否
                if (textureName.StartsWith(characterTexturePrefix) == true)
                {
                    if (textureName.EndsWith(characterTextureSuffixColor))
                    {
                        _textureImporters[i].textureType = TextureImporterType.Default;
                        _textureImporters[i].textureShape = TextureImporterShape.Texture2D;
                        _textureImporters[i].sRGBTexture = true;

                    }
                    else if (textureName.EndsWith(characterTextureSuffixNormal))
                    {
                        _textureImporters[i].textureType = TextureImporterType.NormalMap;
                        _textureImporters[i].textureShape = TextureImporterShape.Texture2D;
                        _textureImporters[i].sRGBTexture = false;
                    }
                    else if (textureName.EndsWith(characterTextureSuffixMix))
                    {
                        _textureImporters[i].textureType = TextureImporterType.Default;
                        _textureImporters[i].textureShape = TextureImporterShape.Texture2D;
                        _textureImporters[i].sRGBTexture = false;
                    }
                    else if (textureName.EndsWith(characterTextureSuffixMask))
                    {
                        _textureImporters[i].textureType = TextureImporterType.Default;
                        _textureImporters[i].textureShape = TextureImporterShape.Texture2D;
                        _textureImporters[i].sRGBTexture = false;
                    }
                    else if (textureName.EndsWith(characterTextureSuffixCube))
                    {
                        _textureImporters[i].textureType = TextureImporterType.Default;
                        _textureImporters[i].textureShape = TextureImporterShape.TextureCube;
                        _textureImporters[i].sRGBTexture = true;
                    }

                    //保存纹理
                    _textureImporters[i].SaveAndReimport();
                }
                else
                {
                    Debug.LogWarning("贴图命名规则不符合批处理要求！" + _textureImporters[i].assetPath);
                }
            }
        }

        private void SettingEffectTextureProperty()
        {
            //通用变量属性设置
            SettingCommonProperty();

            for (int i = 0; i < _textureImporters.Count; i++)
            {
                string textureName = System.IO.Path.GetFileNameWithoutExtension(texturePaths[i]);

                //判断是否
                if (textureName.StartsWith(effectTexturePrefix) == true)
                {
                    if (textureName.EndsWith(effectTextureSuffixColor))
                    {
                        _textureImporters[i].textureType = TextureImporterType.Default;
                        _textureImporters[i].textureShape = TextureImporterShape.Texture2D;
                        _textureImporters[i].sRGBTexture = true;

                    }
                    else if (textureName.EndsWith(effectTextureSuffixNomral))
                    {
                        _textureImporters[i].textureType = TextureImporterType.NormalMap;
                        _textureImporters[i].textureShape = TextureImporterShape.Texture2D;
                        _textureImporters[i].sRGBTexture = false;
                    }
                    else if (textureName.EndsWith(effectTextureSuffixMask))
                    {
                        _textureImporters[i].textureType = TextureImporterType.Default;
                        _textureImporters[i].textureShape = TextureImporterShape.Texture2D;
                        _textureImporters[i].sRGBTexture = false;
                    }
                    else if (textureName.EndsWith(effectTextureSuffixGray))
                    {
                        _textureImporters[i].textureType = TextureImporterType.Default;
                        _textureImporters[i].textureShape = TextureImporterShape.Texture2D;
                        _textureImporters[i].sRGBTexture = false;
                    }

                    //保存纹理
                    _textureImporters[i].SaveAndReimport();
                }
                else
                {
                    Debug.LogWarning("贴图命名规则不符合批处理要求！" + _textureImporters[i].assetPath);
                }
            }
        }

        private void SettingsUITextureProperty()
        {
            //通用变量属性设置
            SettingCommonProperty();

            for (int i = 0; i < _textureImporters.Count; i++)
            {
                string textureName = System.IO.Path.GetFileNameWithoutExtension(texturePaths[i]);

                //判断是否
                if (textureName.StartsWith(uiTexturePropertyPrefix) == true)
                {

                    if (textureName.EndsWith(uiTexturePropertySuffixColor))
                    {
                        if (isSpriteTexture)
                        {
                            _textureImporters[i].textureType = TextureImporterType.Sprite;
                        }
                        else
                        {
                            _textureImporters[i].textureType = TextureImporterType.Default;
                            _textureImporters[i].sRGBTexture = true;
                        }
                    }
                    else if (textureName.EndsWith(uiTexturePropertySuffixMask))
                    {
                        if (isSpriteTexture)
                        {
                            _textureImporters[i].textureType = TextureImporterType.Sprite;
                        }
                        else
                        {
                            _textureImporters[i].textureType = TextureImporterType.Default;
                            _textureImporters[i].sRGBTexture = false;
                            _textureImporters[i].textureShape = TextureImporterShape.Texture2D;
                        }
                    }
                    else if (textureName.EndsWith(uiTexturePropertySuffixGray))
                    {
                        if (isSpriteTexture)
                        {
                            _textureImporters[i].textureType = TextureImporterType.Sprite;
                        }
                        else
                        {
                            _textureImporters[i].textureType = TextureImporterType.Default;
                            _textureImporters[i].sRGBTexture = false;
                        }
                    }

                    //保存纹理
                    _textureImporters[i].SaveAndReimport();
                }
                else
                {
                    Debug.LogWarning("贴图命名规则不符合批处理要求！" + _textureImporters[i].assetPath);
                }
            }
        }

        private void SettingsWeaponTextureProperty()
        {
            //通用变量属性设置
            SettingCommonProperty();

            for (int i = 0; i < _textureImporters.Count; i++)
            {
                string textureName = System.IO.Path.GetFileNameWithoutExtension(texturePaths[i]);

                //判断是否
                if (textureName.StartsWith(weaponTexturePrefix) == true)
                {
                    if (textureName.EndsWith(weaponTextureSuffixColor))
                    {
                        _textureImporters[i].textureType = TextureImporterType.Default;
                        _textureImporters[i].sRGBTexture = true;

                    }
                    else if (textureName.EndsWith(weaponTextureSuffixNormal))
                    {
                        _textureImporters[i].textureType = TextureImporterType.NormalMap;
                        _textureImporters[i].sRGBTexture = false;
                    }
                    else if (textureName.EndsWith(weaponTextureSuffixMix))
                    {
                        _textureImporters[i].textureType = TextureImporterType.Default;
                        _textureImporters[i].sRGBTexture = false;
                    }
                    else if (textureName.EndsWith(weaponTextureSuffixMask))
                    {
                        _textureImporters[i].textureType = TextureImporterType.Default;
                        _textureImporters[i].sRGBTexture = false;
                    }
                    else if (textureName.EndsWith(weaponTextureSuffixGray))
                    {
                        _textureImporters[i].textureType = TextureImporterType.Default;
                        _textureImporters[i].sRGBTexture = false;
                    }

                    //保存纹理
                    _textureImporters[i].SaveAndReimport();
                }
                else
                {
                    Debug.LogWarning("贴图命名规则不符合批处理要求！" + _textureImporters[i].assetPath);
                }
            }
        }
    }
}
