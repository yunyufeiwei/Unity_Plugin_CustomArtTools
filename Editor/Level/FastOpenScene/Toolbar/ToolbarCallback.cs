using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace yuxuetian
{
    // 定义一个静态类ToolbarCallback，用于封装与Unity编辑器工具栏相关的回调或操作（尽管这里实际上没有实现回调）  
    public static class ToolbarCallback
    {
        // 定义一个静态的Type变量m_toolbarType，用于尝试获取Unity编辑器工具栏的内部类型
        // 注意：这里的"UnityEditor.toolbar"很可能是一个虚构的类型名，因为UnityEditor命名空间下通常不包含直接名为"toolbar"的公开类型 
        static Type _toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
        // 获取GUI视图的类型，用于进一步反射操作
        static Type _guiViewType = typeof(Editor).Assembly.GetType("UnityEditor.GUIView");
        
        // 获取窗口后端的接口类型，用于访问视图树等  
        static Type _iWindowBackendType = typeof(Editor).Assembly.GetType("UnityEditor.IWindowBackend");
        // 访问GUI视图窗口后端的属性
        static PropertyInfo _windowBackend = _guiViewType.GetProperty("windowBackend",BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        // 访问窗口后端中视觉树的属
        static PropertyInfo _viewVisualTree = _iWindowBackendType.GetProperty("visualTree",BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        // 访问IMGUIContainer的m_OnGUIHandler字段，用于注册自定义的GUI处理函数
        static FieldInfo _imguiContainerOnGui = typeof(IMGUIContainer).GetField("m_OnGUIHandler",BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        // 当前找到的Toolbar实例（如果有的话）
        static ScriptableObject _currentToolbar;

        // 委托，用于在工具栏GUI中执行自定义操作
        public static Action OnToolbarGUI;    
        public static Action OnToolbarGUILeft;
        public static Action OnToolbarGUIRight;
        
        // 静态构造函数，用于注册更新事件
        static ToolbarCallback()
        {
            // 取消之前可能存在的注册（尽管这里首次调用应该是空操作） 
            EditorApplication.update -= OnUpdate;
            // 注册更新事件，以便在编辑器更新时查找和修改工具栏
            EditorApplication.update += OnUpdate;
        }

        // 更新函数，用于查找Toolbar实例并注册自定义GUI 
        static void OnUpdate()
        {
            if (_currentToolbar == null)
            {
                // 通过反射查找UnityEditor.Toolbar的实例  
                var toolbars = Resources.FindObjectsOfTypeAll(_toolbarType);
                _currentToolbar = toolbars.Length > 0 ? (ScriptableObject)toolbars[0] : null;
                if (_currentToolbar != null)
                {
                    // 假设Toolbar有一个非公开的m_Root字段，用于访问其根VisualElement
                    var root = _currentToolbar.GetType().GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
                    var rawRoot = root.GetValue(_currentToolbar);
                    var mRoot = rawRoot as VisualElement;

                    // 尝试在工具栏的左侧区域注册自定义GUI  
                    RegisterCallback("ToolbarZoneLeftAlign", OnToolbarGUILeft);
                    RegisterCallback("ToolbarZoneRightAlign", OnToolbarGUIRight);

                    // 辅助方法，用于在指定区域内注册自定义GUI  
                    void RegisterCallback(string root, Action action)
                    {
                        var toolbarZone = mRoot.Q(root);
                        var parent = new VisualElement()
                        {
                            style = 
                            {
                                flexGrow = 1,
                                flexDirection = FlexDirection.Row,
                            }
                        };
                        var container = new IMGUIContainer();
                        container.style.flexGrow = 1;
                        container.onGUIHandler += () =>
                        {
                            action?.Invoke();
                        };
                        // 将容器添加到父容器中
                        parent.Add(container);
                        // 将父容器添加到工具栏区域中
                        toolbarZone.Add(parent);
                    }
                }
            }
        }
        
        // 自定义的OnGUI处理函数，用于在工具栏上绘制自定义GUI
        static void OnGUI()
        {
            var handler = OnToolbarGUI;
            if (handler != null) handler();
        }
    }
}

