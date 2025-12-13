/*
 * 用于遍历项目工程中的地图，并绘制在自定义的GUI面板上
 */
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace yuxuetian
{
    //用于遍历项目工程中的所有地图文件，方便打开
    public class OpenSceneList : EditorWindow
    {
        public static EditorWindow window;
        
        static string[] scenesPath;
        static string[] scenesBuildPath;
        static Texture sceneIcon;
        static float toggleHeight = 30;
        static int count;
        static int maxRow = 20;
        static Vector2 itemSize = new Vector2(200, 30);
        
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

            window = EditorWindow.GetWindow<OpenSceneList>();
            window.titleContent = new GUIContent("工程场景列表");

            float itemTotalSizeX = itemSize.x + margin * 2;
            float itemTotalSizeY = itemSize.y + margin * 2;

            int col = Mathf.CeilToInt((float)count / maxRow);
            int row = Mathf.CeilToInt((float)count / col);
            // 计算窗口的总宽度和总高度
            float totalWidth = 10 + itemTotalSizeX * col;
            float totalHeight = 10 + toggleHeight + itemTotalSizeY * (count < row ? count : row);

            //用于将窗口定位到鼠标当前位置附近的逻辑
            Vector2 temp = GUIUtility.GUIToScreenPoint(new Vector2(Event.current.mousePosition.x, Event.current.mousePosition.y));
            window.position = new Rect(temp.x - totalWidth, temp.y + 50, totalWidth, totalHeight);
            
            window.Show();
        }

        public void OnEnable()
        {
            // 获取场景图标的Texture2D，用于填充点击的UI背景元素
            sceneIcon = EditorGUIUtility.IconContent("SceneAsset Icon").image;
            // 获取此组件的视觉元素根节点，用于添加子元素
            var root = this.rootVisualElement;

            // 创建一个滚动视图容器，用于包含可滚动的子元素
            ScrollView scrollView = new ScrollView();
            
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

            // 获取当前活动的场景
            Scene activeScene = SceneManager.GetActiveScene();
    
            //方案一：
            // 查找项目中Assets目录下的所有场景文件的GUID ,并将其存在声明sceneGuids中。这里的t表示type，加一个冒号，之后输入要搜索的内容
            string[] sceneGuids = AssetDatabase.FindAssets("t:scene", new string[] { "Assets" });
            //方案二:
            //如果希望只看到固定的目录下的场景列表，或者使用组合的方式，可以使用下面该行代码方式
            //string[] directories = new string[] { "Assets/Maps", "Assets/Designer" };
            
            foreach (var sceneGuid in sceneGuids)
            {
                //将GUID转换为场景文件的路径
                var scenesPath = AssetDatabase.GUIDToAssetPath(sceneGuid);
                //从路径中提取场景名称
                string name = GetSceneName(scenesPath);
                //创建一个表示场景按钮的UI元素，并将其添加到分组框中  
                var box = BindItem(name, scenesPath);
                groupBox.Add(box);

                //如果当前活动的场景与此场景匹配，则选中该按钮
                if (activeScene.path == scenesPath)
                {
                    SelectButton(box);
                }
            }

            //记录分组框中子元素的数量
            count = groupBox.childCount;
            //将分组框添加到滚动视图中
            scrollView.Add(groupBox);
            //将滚动视图添加到根视觉元素中，以便在编辑器中显示 
            root.Add(scrollView);
        }

        // 定义一个静态方法BindItem，用于创建并绑定一个按钮项  
        // 参数name是按钮上显示的文本，path是与之关联的场景路径
        static Button BindItem(string name, string path)
        {
            // 创建一个新的按钮对象
            var box = new Button();
            
            // 为按钮注册点击事件回调  
            // 当按钮被点击时，首先检查是否保存当前修改的场景（如果用户希望保存）  
            // 然后打开指定的场景路径，并调用SelectButton方法选中该按钮  
            // 最后停止事件传播，防止事件进一步冒泡  
            box.RegisterCallback<ClickEvent>((e) =>
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene(path);
                    SelectButton(e.target as Button);
                }

                e.StopPropagation();
            });
            
            box.style.backgroundColor = defaultColor;       // 设置按钮的背景颜色为默认颜色。注意：defaultColor是在开始就定义好的
            box.style.flexDirection = FlexDirection.Row;    // 设置按钮的布局方向为行（水平方向）
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
            Image icon = new Image();
            icon.style.flexShrink = 0;
            icon.style.width = 16;
            icon.style.height = 16;
            icon.style.marginRight = 5;
            icon.image = sceneIcon;
            
            // 将图标添加到按钮中
            box.Add(icon);
            
            // 创建一个新的标签对象，用于显示按钮上的文本
            Label label = new Label();                 
            label.style.flexGrow = 1.0f;            // 设置标签的布局属性，使其能够增长以填充剩余空间
            label.style.alignItems = Align.Center;  // 设置标签中文本的对齐方式为居中（虽然这里可能更多是为了与图标对齐） 
            label.text = name;                        // 设置标签显示的文本 

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

        // 定义一个静态方法，用于从给定的路径中提取场景名称
        static string GetSceneName(string path)
        {
            path = path.Replace(".unity", "");
            return Path.GetFileName(path);
        }

        void Update()
        {
            // 检查当前聚焦的窗口是否为我们自定义的OpenSceneList窗口  
            // 如果不是，则关闭当前窗口
            if (OpenSceneList.focusedWindow != GetWindow(typeof(OpenSceneList)))
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
