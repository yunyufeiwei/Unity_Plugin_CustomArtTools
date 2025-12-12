// ProjectFolderConfigSO.cs
using System;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 这个脚本是用来创建一个可以在Unity中可视化配置结构的文件
/// </summary>

[CreateAssetMenu(fileName = "ProjectFolderConfig", menuName = "ArtTools/Project Folder Config")]
//ScriptableObject来存储配置
public class ProjectFolderConfigCreate : ScriptableObject
{
    [Serializable]
    public class FolderEntry
    {
        public string folderName;
        public List<string> subFolders = new List<string>();
    }

    [Serializable]
    public class NestedFolderEntry
    {
        public string parentPath;
        public List<string> subFolders = new List<string>();
    }

    // First-level folders
    public List<string> firstLevelFolders = new List<string>();

    // Second-level folders
    public List<FolderEntry> secondLevelFolders = new List<FolderEntry>();

    // Third-level folders
    public List<NestedFolderEntry> thirdLevelFolders = new List<NestedFolderEntry>();

    /// <summary>
    /// Convert to the old format for compatibility
    /// </summary>
    public string[] GetFirstLevelFolders()
    {
        return firstLevelFolders.ToArray();
    }

    public Dictionary<string, string[]> GetSecondLevelFolders()
    {
        var dict = new Dictionary<string, string[]>();
        foreach (var entry in secondLevelFolders)
        {
            dict[entry.folderName] = entry.subFolders.ToArray();
        }
        return dict;
    }

    public Dictionary<string, string[]> GetThirdLevelFolders()
    {
        var dict = new Dictionary<string, string[]>();
        foreach (var entry in thirdLevelFolders)
        {
            dict[entry.parentPath] = entry.subFolders.ToArray();
        }
        return dict;
    }
}