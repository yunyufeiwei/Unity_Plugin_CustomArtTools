using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace yuxuetian
{
    public class OpenSkyboxList : EditorWindow
    {
        public static EditorWindow window;
        
        static string materialPath = "Assets";
        static Texture sceneIcon;
        static Toggle toggle;
        static float toggleHeight = 30;
        static int maxRow = 6;
        static Vector2 itemSize = new Vector2(120, 132);
        
        static int count;
        static Color defaultColor = new Color(0.345f, 0.345f, 0.345f, 1f);
        static Color selectColor = new Color(0.88f, 0.5f, 0.2f, 1f);

        static float margin = 2;
        static float padding = 5;
        static Button selectedButton;

        //如果使用，则可以直接在MenuItem菜单下打开场景选择的窗口，但如果希望在Unity的播放按钮旁边显示，就需要使用自定义的按钮来调用这里的方法
        //[MenuItem("ArtTools/Scene/打开工程场景清单",false,501)]
        public static void ShowWindow()
        {
            if (window != null)
            {
                window.Close();
                window = null;
            }

            window = EditorWindow.GetWindow<OpenSkyboxList>();
            window.titleContent = new GUIContent("天空盒列表");

            float itemTotalSizeX = itemSize.x + margin * 2;
            float itemTotalSizeY = itemSize.y + margin * 2;

            int col = Mathf.CeilToInt((float)count / maxRow);
            int row = Mathf.CeilToInt((float)count / col);
            // 计算窗口的总宽度和总高度
            float totalWidth = 10 + itemTotalSizeX * col;
            float totalHeight = 10 + itemTotalSizeY * (count < row ? count : row);

            //用于将窗口定位到鼠标当前位置附近的逻辑
            Vector2 temp = GUIUtility.GUIToScreenPoint(new Vector2(Event.current.mousePosition.x, Event.current.mousePosition.y));
            window.position = new Rect(temp.x - totalWidth / 2 , temp.y + 50, totalWidth, totalHeight);
            
            window.Show();
        }

        public void OnEnable()
        {
            // 获取场景图标的Texture2D，用于填充点击的UI背景元素
            sceneIcon = EditorGUIUtility.IconContent("SceneAsset Icon").image;
            // 获取此组件的视觉元素根节点，用于添加子元素
            var root = this.rootVisualElement;

            //固定窗口高度，让滑动条正常显示
            toggle = new Toggle("固定窗口");
            toggle.style.fontSize = 20;
            toggle.style.alignSelf = Align.Center;
            toggle.style.unityTextAlign = TextAnchor.MiddleCenter;
            toggle.style.height = toggleHeight;
            toggle.style.marginTop = 0;
            toggle.style.marginBottom = 0;
            toggle.style.marginLeft = 0;
            toggle.style.marginRight = 0;
            toggle.Children().First().style.minWidth = 0;
            root.Add(toggle);
            
            ScrollView scrollView = new ScrollView();              // 创建一个滚动视图容器，用于包含可滚动的子元素
            GroupBox groupBox = new GroupBox();                    // 创建一个分组框，用于组织场景按钮
            groupBox.style.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1f);
            groupBox.style.flexDirection = FlexDirection.Row;   // 设置分组框的Flex方向为行，以便子元素水平排列
            groupBox.style.alignItems = Align.Center;           // 设置子元素在交叉轴上的对齐方式为居中  
            groupBox.style.flexWrap = Wrap.Wrap;                // 允许子元素换行
            groupBox.style.justifyContent = Justify.Center;     // 设置主轴上的对齐方式为居中
            
            //移除分组框的默认内边距，这里的边距就是距离打开窗口上下左右的距离
            groupBox.style.paddingTop = 0;
            groupBox.style.paddingBottom = 0;
            groupBox.style.paddingLeft = 0;
            groupBox.style.paddingRight = 0;

            // 查找项目中Assets目录下的所有场景文件的GUID ,并将其存在声明materialGuids中。这里的t表示type，加一个冒号，之后输入要搜索的内容
            string[] materialGuids = AssetDatabase.FindAssets("t:Material", new[] { materialPath });
            string skyboxPath = RenderSettings.skybox != null ? AssetDatabase.GetAssetPath(RenderSettings.skybox) : "";     
            
            imageDic.Clear();
            foreach (string guid in materialGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
                var shaderName = material.shader?.name.ToLower();
                if (shaderName.Contains("sky") || shaderName.Contains("cubemap"))
                {
                    var box = BindItem(material.name, assetPath, material);
                    groupBox.Add(box);
                    if (skyboxPath == assetPath)
                    {
                        SelectButton(box);
                    }
                }
            }
            count = groupBox.childCount;        //记录分组框中子元素的数量
            scrollView.Add(groupBox);           //将分组框添加到滚动视图中
            root.Add(scrollView);               //将滚动视图添加到根视觉元素中，以便在编辑器中显示 
        }

        //创建一个字典，用于存储所有的Image
        private static Dictionary<Image, Material> imageDic = new Dictionary<Image, Material>();

        // 定义一个静态方法BindItem，用于创建并绑定一个按钮项  
        // 参数name是按钮上显示的文本，path是与之关联的场景路径
        static Button BindItem(string name, string path , Material material)
        {
            // 创建一个新的按钮对象
            var box = new Button(() =>
            {
             EditorGUIUtility.PingObject(material);
             RenderSettings.skybox = material;
            });
            
            // 为按钮注册点击事件回调  
            // 当按钮被点击时，首先检查是否保存当前修改的场景（如果用户希望保存）  
            // 然后打开指定的场景路径，并调用SelectButton方法选中该按钮  
            // 最后停止事件传播，防止事件进一步冒泡  
            box.RegisterCallback<ClickEvent>((e) =>
            {
                EditorGUIUtility.PingObject(material);
                RenderSettings.skybox = material;
                SelectButton(e.target as Button);
                e.StopPropagation();
            });
            
            box.style.backgroundColor = defaultColor;       // 设置按钮的背景颜色为默认颜色。注意：defaultColor是在开始就定义好的
            box.style.flexDirection = FlexDirection.Column;    // 设置按钮的布局方向为行（垂直方向），这样就上面显示图片，下面显示名称
            box.style.alignItems = Align.Center;            // 设置子元素在交叉轴上的对齐方式为居中
            
            // 设置按钮的内边距（padding） 
            box.style.paddingLeft = padding;
            box.style.paddingRight = padding;
            box.style.paddingTop = padding;
            box.style.paddingBottom = padding;
            
            // 设置按钮的外边距（margin）
            box.style.marginLeft = margin;
            box.style.marginRight = margin;
            box.style.marginTop = margin;
            box.style.marginBottom = margin;
            
            //设置按钮的宽高
            box.style.width = itemSize.x;
            box.style.height = itemSize.y;
            box.style.unityTextAlign = TextAnchor.MiddleLeft;   // 设置按钮中文本的对齐方式为中间左对齐
            box.tooltip = path;                                   // 设置按钮的提示信息为场景路径

            //创建一个用来在Button中显示的icon图标
            Image image = new Image();
            image.style.flexShrink = 0;
            image.style.width = 100;
            image.style.height = 100;
            image.image = AssetPreview.GetAssetPreview(material);
            // 将图标添加到按钮中
            box.Add(image);
            imageDic.Add(image,material);
            
            // 创建一个新的标签对象，用于显示按钮上的文本
            Label label = new Label(name);                 
            label.style.flexGrow = 1.0f;            // 设置标签的布局属性，使其能够增长以填充剩余空间
            label.style.alignItems = Align.Center;  // 设置标签中文本的对齐方式为居中（虽然这里可能更多是为了与图标对齐） 
            label.style.marginTop = 5;
            // 将标签添加到按钮中
            box.Add(label);

            return box;
        }

        // 定义一个静态方法，用于选择按钮并改变其背景色
        static void SelectButton(Button button)
        {
            if (selectedButton != null)
            {
                selectedButton.style.backgroundColor = defaultColor;
            }
            selectedButton = button;
            selectedButton.style.backgroundColor = selectColor;
        }

        void Update()
        {
            foreach (var item in imageDic)
            {
                if (item.Key.image == null)
                {
                    item.Key.image = AssetPreview.GetAssetPreview(item.Value);
                }
            }
            // 检查当前聚焦的窗口是否为我们自定义的OpenSceneList窗口  
            // 如果不是，则关闭当前窗口
            if (OpenSkyboxList.focusedWindow != GetWindow<OpenSkyboxList>())
            {
                this.Close();
            }
        }

        // 当对象被销毁时调用此方法
        void OnDestroy()
        {
            // 将窗口引用置为空，帮助垃圾回收
            window = null;
        }
    }
}

