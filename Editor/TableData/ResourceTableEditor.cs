using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

// 自定义编辑器，用于编辑 ResourceTableData 类型的对象
[CustomEditor(typeof(ResourceTableData))]
public class ResourceTableEditor : Editor
{
    // 序列化属性引用
    private SerializedProperty groupsProp;  // 引用 groups 属性
    private SerializedProperty columnsProp; // 引用 columnTitles 属性
    
    // UI 相关变量
    private Vector2 scrollPos;  // 滚动视图位置
    private string newColumnName = "";  // 新列名输入字段
    private string newGroupName = "New Group";  // 新组名输入字段
    private int columnToDelete = -1;    // 要删除的列索引
    private List<float> columnWidths = new List<float>();
    
    // 常量定义
    private const float defaultColumnWidth = 150f;  // 默认列宽
    private const float minColumnWidth = 80f;       // 最小列宽
    private int descriptionColumnIndex = 2;         // 假设描述是第3列(索引2)
    
    // 分组折叠状态字典
    private Dictionary<int, bool> groupFoldoutStates = new Dictionary<int, bool>();

    // 编辑器启用时调用
    void OnEnable()
    {
        // 获取序列化属性
        groupsProp = serializedObject.FindProperty("groups");
        columnsProp = serializedObject.FindProperty("columnTitles");
        
        // 初始化列宽和分组折叠状态
        InitializeGroupFoldoutStates();
        InitializeColumnWidths();
    }

    // 初始化分组折叠状态
    void InitializeGroupFoldoutStates()
    {
        for (int i = 0; i < groupsProp.arraySize; i++)
        {
            if (!groupFoldoutStates.ContainsKey(i))
            {
                groupFoldoutStates[i] = true; // 默认展开
            }
        }
    }

    // 初始化列宽
    void InitializeColumnWidths()
    {
        if (columnWidths.Count != columnsProp.arraySize)
        {
            columnWidths.Clear();
            for (int i = 0; i < columnsProp.arraySize; i++)
            {
                float width = defaultColumnWidth;
                if (i == 0) width = 150f; // 描述列更宽
                else if (i == 1) width = 250.0f;
                columnWidths.Add(width);
            }
        }
    }

    //自定义Inspector面板
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawToolbar();
        DrawTable();
        serializedObject.ApplyModifiedProperties();
    }

    void DrawToolbar()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        {
            // 添加组按钮
            newGroupName = EditorGUILayout.TextField(newGroupName, GUILayout.Width(120));
            if (GUILayout.Button("+ 组", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                if (!string.IsNullOrEmpty(newGroupName))
                {
                    AddNewGroup(newGroupName);
                    newGroupName = "New Group";
                    GUI.FocusControl(null);
                }
            }

            GUILayout.Space(10);

            // 添加行按钮
            if (GUILayout.Button("+ 行", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                if (groupsProp.arraySize > 0)
                {
                    AddNewRow(groupsProp.arraySize - 1); // 默认添加到最后一个组
                }
            }

            GUILayout.Space(5);

            // 添加列控件
            newColumnName = EditorGUILayout.TextField(newColumnName, GUILayout.Width(120));
            if (GUILayout.Button("+ 列", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                if (!string.IsNullOrEmpty(newColumnName))
                {
                    AddNewColumn(newColumnName);
                    newColumnName = "";
                    GUI.FocusControl(null); // 清除焦点
                }
            }

            // 删除列控件组
            if (columnsProp.arraySize > 0)
            {
                GUILayout.Space(10);
                
                // 删除列下拉菜单
                string[] columnNames = GetColumnNames();
                int newColumnToDelete = EditorGUILayout.Popup(
                    Mathf.Clamp(columnToDelete, 0, columnNames.Length - 1),
                    columnNames,
                    EditorStyles.toolbarPopup,
                    GUILayout.Width(120));
                
                if (newColumnToDelete >= 0 && newColumnToDelete < columnNames.Length)
                {
                    columnToDelete = newColumnToDelete;
                }

                // 删除列按钮 - 直接删除，无确认对话框
                if (GUILayout.Button("删除列", EditorStyles.toolbarButton, GUILayout.Width(60)))
                {
                    if (columnToDelete >= 0 && columnToDelete < columnsProp.arraySize)
                    {
                        DeleteColumn(columnToDelete);
                        columnToDelete = -1; // 重置选择
                    }
                }
            }
        }
        GUILayout.EndHorizontal();
    }

    void DrawTable()
{
    scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandHeight(true));

    // 表格外框
    GUILayout.BeginVertical("Box");
    {
        // 表头行
        GUILayout.BeginHorizontal();
        {
            for (int i = 0; i < columnsProp.arraySize; i++)
            {
                var column = columnsProp.GetArrayElementAtIndex(i);
                GUILayout.Label(column.stringValue, EditorStyles.boldLabel, GUILayout.Width(columnWidths[i]));
            }
            GUILayout.Label("操作", EditorStyles.boldLabel, GUILayout.Width(60));
        }
        GUILayout.EndHorizontal();

        // 标题与内容之间的分割线
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        // 绘制所有分组
        for (int groupIndex = 0; groupIndex < groupsProp.arraySize; groupIndex++)
        {
            SerializedProperty groupProp = groupsProp.GetArrayElementAtIndex(groupIndex);
            SerializedProperty groupNameProp = groupProp.FindPropertyRelative("groupName");
            SerializedProperty rowsProp = groupProp.FindPropertyRelative("rows");

            // 分组折叠框
            bool foldout;
            if (!groupFoldoutStates.TryGetValue(groupIndex, out foldout))
            {
                foldout = true;
                groupFoldoutStates[groupIndex] = foldout;
            }

            // 分组标题行
            GUILayout.BeginHorizontal();
            {
                // 折叠箭头和分组名称
                groupFoldoutStates[groupIndex] = EditorGUILayout.Foldout(foldout, groupNameProp.stringValue, true);
                
                // 删除组按钮
                if (GUILayout.Button("删除组", EditorStyles.miniButton, GUILayout.Width(60)))
                {
                    groupsProp.DeleteArrayElementAtIndex(groupIndex);
                    groupFoldoutStates.Remove(groupIndex);
                    // 需要立即应用修改并退出循环，因为数组大小已改变
                    serializedObject.ApplyModifiedProperties();
                    return;
                }

                // 在当前组添加行按钮
                if (GUILayout.Button("+ 行", EditorStyles.miniButton, GUILayout.Width(40)))
                {
                    AddNewRow(groupIndex);
                }
            }
            GUILayout.EndHorizontal();

            // 分组内容 (行)
            if (groupFoldoutStates[groupIndex])
            {
                // 组内表格内容
                for (int rowIndex = 0; rowIndex < rowsProp.arraySize; rowIndex++)
                {
                    SerializedProperty rowProp = rowsProp.GetArrayElementAtIndex(rowIndex);
                    SerializedProperty cellsProp = rowProp.FindPropertyRelative("cells");

                    // 确保单元格数量与列数匹配
                    while (cellsProp.arraySize < columnsProp.arraySize)
                    {
                        cellsProp.InsertArrayElementAtIndex(cellsProp.arraySize);
                    }

                    // 行开始
                    GUILayout.BeginHorizontal();
                    {
                        for (int colIndex = 0; colIndex < columnsProp.arraySize; colIndex++)
                        {
                            SerializedProperty cellProp = cellsProp.GetArrayElementAtIndex(colIndex);
                            
                            if (colIndex == descriptionColumnIndex)
                            {
                                // 描述列特殊处理 - 默认单行，超宽自动换行
                                EditorStyles.textField.wordWrap = true;
                                
                                // 计算文本高度
                                float textHeight = EditorStyles.textField.CalcHeight(
                                    new GUIContent(cellProp.stringValue), 
                                    columnWidths[colIndex]);
                                
                                // 判断是否需要多行显示
                                bool useMultiLine = textHeight > EditorGUIUtility.singleLineHeight * 1.5f;
                                
                                // 根据需要使用TextArea或TextField
                                string newValue;
                                if (useMultiLine)
                                {
                                    newValue = EditorGUILayout.TextArea(
                                        cellProp.stringValue, 
                                        GUILayout.Width(columnWidths[colIndex]),
                                        GUILayout.Height(textHeight + 4f));
                                }
                                else
                                {
                                    newValue = EditorGUILayout.TextField(
                                        cellProp.stringValue, 
                                        GUILayout.Width(columnWidths[colIndex]));
                                }
                                
                                if (newValue != cellProp.stringValue)
                                {
                                    cellProp.stringValue = newValue;
                                }
                            }
                            else
                            {
                                // 其他列正常显示
                                EditorGUILayout.PropertyField(
                                    cellProp, 
                                    GUIContent.none, 
                                    GUILayout.Width(columnWidths[colIndex]));
                            }
                        }

                        // 删除行按钮
                        if (GUILayout.Button("删除行", EditorStyles.miniButton, GUILayout.Width(60)))
                        {
                            rowsProp.DeleteArrayElementAtIndex(rowIndex);
                            break;
                        }
                    }
                    GUILayout.EndHorizontal();
                }
            }
        }
    }
    GUILayout.EndVertical();

    EditorGUILayout.EndScrollView();
}

    //获取列的名称
    string[] GetColumnNames()
    {
        string[] names = new string[columnsProp.arraySize];
        for (int i = 0; i < columnsProp.arraySize; i++)
        {
            names[i] = columnsProp.GetArrayElementAtIndex(i).stringValue;
        }
        return names;
    }
    
    //添加新组
    void AddNewGroup(string groupName)
    {
        int index = groupsProp.arraySize;
        groupsProp.InsertArrayElementAtIndex(index);
        SerializedProperty newGroup = groupsProp.GetArrayElementAtIndex(index);
        newGroup.FindPropertyRelative("groupName").stringValue = groupName;
        newGroup.FindPropertyRelative("rows").ClearArray();
        groupFoldoutStates[index] = true;
    }

    //添加新行
    void AddNewRow(int groupIndex)
    {
        if (groupIndex < 0 || groupIndex >= groupsProp.arraySize) return;

        SerializedProperty groupProp = groupsProp.GetArrayElementAtIndex(groupIndex);
        SerializedProperty rowsProp = groupProp.FindPropertyRelative("rows");

        int index = rowsProp.arraySize;
        rowsProp.InsertArrayElementAtIndex(index);
        SerializedProperty newRow = rowsProp.GetArrayElementAtIndex(index);
        SerializedProperty cells = newRow.FindPropertyRelative("cells");

        cells.ClearArray();
        for (int i = 0; i < columnsProp.arraySize; i++)
        {
            cells.InsertArrayElementAtIndex(i);
        }
    }

    //添加新列
    void AddNewColumn(string columnName)
    {
        int colIndex = columnsProp.arraySize;
        columnsProp.InsertArrayElementAtIndex(colIndex);
        columnsProp.GetArrayElementAtIndex(colIndex).stringValue = columnName;
        columnWidths.Add(defaultColumnWidth);

        // 为所有组中的所有行添加新列
        for (int groupIndex = 0; groupIndex < groupsProp.arraySize; groupIndex++)
        {
            SerializedProperty groupProp = groupsProp.GetArrayElementAtIndex(groupIndex);
            SerializedProperty rowsProp = groupProp.FindPropertyRelative("rows");

            for (int rowIndex = 0; rowIndex < rowsProp.arraySize; rowIndex++)
            {
                SerializedProperty rowProp = rowsProp.GetArrayElementAtIndex(rowIndex);
                SerializedProperty cells = rowProp.FindPropertyRelative("cells");
                cells.InsertArrayElementAtIndex(cells.arraySize);
            }
        }
    }

    //删除列
    void DeleteColumn(int columnIndex)
    {
        if (columnIndex < 0 || columnIndex >= columnsProp.arraySize) return;

        // 记录操作以便撤销
        Undo.RecordObject(target, "Delete Column");
        
        // 删除列标题
        columnsProp.DeleteArrayElementAtIndex(columnIndex);
        columnWidths.RemoveAt(columnIndex);

        // 调整描述列索引(如果需要)
        if (descriptionColumnIndex >= columnIndex && descriptionColumnIndex > 0)
        {
            descriptionColumnIndex--;
        }

        // 删除所有组中所有行中对应的单元格
        for (int groupIndex = 0; groupIndex < groupsProp.arraySize; groupIndex++)
        {
            SerializedProperty groupProp = groupsProp.GetArrayElementAtIndex(groupIndex);
            SerializedProperty rowsProp = groupProp.FindPropertyRelative("rows");

            for (int rowIndex = 0; rowIndex < rowsProp.arraySize; rowIndex++)
            {
                SerializedProperty rowProp = rowsProp.GetArrayElementAtIndex(rowIndex);
                SerializedProperty cells = rowProp.FindPropertyRelative("cells");
                
                if (columnIndex < cells.arraySize)
                {
                    cells.DeleteArrayElementAtIndex(columnIndex);
                }
            }
        }
    }
}