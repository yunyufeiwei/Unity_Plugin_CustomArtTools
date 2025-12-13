using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShaderResourceTable", menuName = "ArtTools/Shader Resource Table")]
public class ShaderTableData : ScriptableObject
{
    // 四个模块
    public List<ResourceItem> sceneItems = new List<ResourceItem>();
    public List<ResourceItem> characterItems = new List<ResourceItem>();
    public List<ResourceItem> effectItems = new List<ResourceItem>();
    public List<ResourceItem> uiItems = new List<ResourceItem>();
}

// 资源项，每两行一个小整体
[System.Serializable]
public class ResourceItem
{
    [Header("配置项标题")]
    public string itemName = "";  // 新增：配置项标题，可自定义
    
    [Header("路径/文件名")]
    public string pathOrName = "";  // 第一行：文件路径或文件名
    
    [Header("使用说明")]
    [TextArea(2, 4)]  // 文本区域，2行最小高度，4行最大高度
    public string description = "";  // 第二行：使用说明文字
}