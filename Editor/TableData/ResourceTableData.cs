using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewResourceTable", menuName = "ArtTools/Resource Table")]
public class ResourceTableData : ScriptableObject
{
    public enum ResourceStatus
    {
        未开始,
        制作中,
        已完成,
        迭代中
    }
    
    // 定义表格行的可序列化类
    [Serializable]
    public class TableRow
    {
        // 行中的单元格列表，每个单元格存储字符串数据
        public List<string> cells = new List<string>();
        public ResourceStatus status;
    }

    // 定义表格分组的可序列化类
    [Serializable]
    public class TableGroup
    {
        public string groupName;
        // 该分组下的所有行数据
        public List<TableRow> rows = new List<TableRow>();
    }

    // 列标题列表，默认为"名称"、"路径"、"描述"、"组别"
    public List<string> columnTitles = new List<string> {"名称", "路径", "描述","状态"};
    // 所有分组数据的列表
    public List<TableGroup> groups = new List<TableGroup>();

    // void Reset()
    // {
    //     // 添加默认分组
    //     var defaultGroup = new TableGroup { groupName = "Default" };
    //     // defaultGroup.rows.Add(new TableRow { cells = new List<string> { "Player", "Assets/Prefabs/Player.prefab", "主角控制器", "Character" } });
    //     // defaultGroup.rows.Add(new TableRow { cells = new List<string> { "Enemy", "Assets/Prefabs/Enemy.prefab", "敌人基础", "Enemy" } });
    //     // groups.Add(defaultGroup);
    // }
}