using UnityEditor;
using UnityEngine;

public class ProjectFolderCreate : EditorWindow
{
    [MenuItem("ArtTools/Folders/Create Project Folder" , false , 10)]

    private static void CustomCreatFolder()
    {
        string rootPath = "Assets";

        // Create first-level folders
        foreach (var folder in ProjectFolderConfig.FirstLevelFolders)
        {
            CreateFolderIfNotExists(rootPath, folder);
        }

        // Create second-level folders
        foreach (var entry in ProjectFolderConfig.SecondLevelFolders)
        {
            string parentFolder = entry.Key;
            string parentPath = $"{rootPath}/{parentFolder}";

            if (AssetDatabase.IsValidFolder(parentPath))
            {
                foreach (var subFolder in entry.Value)
                {
                    CreateFolderIfNotExists(parentPath, subFolder);
                }
            }
            else
            {
                Debug.LogWarning($"Parent folder '{parentFolder}' missing. Skipping its sub-folders.");
            }
        }

        //Create third-level folders
        foreach (var thirdkey in ProjectFolderConfig.ThirdLevelFolders)
        {
            string parentFolder = thirdkey.Key;
            string parentPath = $"{rootPath}/{parentFolder}";

            if (AssetDatabase.IsValidFolder(parentPath))
            {
                foreach (var subFolder in thirdkey.Value)
                {
                    CreateFolderIfNotExists(parentPath, subFolder);
                }
            }
            else
            {
                Debug.LogWarning($"Parent folder '{parentFolder}' missing. Skipping its sub-folders.");
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("Project folders created successfully!");
    }

    //创建目录，需要传递两个参数，分别是父类路径和需要创建的目录名称
    private static void CreateFolderIfNotExists(string parentPath, string folderName)
    {
        //拼接字符串
        string fullPath = $"{parentPath}/{folderName}";
        
        // 检查目标文件夹路径（fullPath）是否不存在于Unity项目中
        if (!AssetDatabase.IsValidFolder(fullPath))
        {
            // 尝试在父路径（parentPath）下创建新文件夹（folderName）
            // 返回GUID（全局唯一标识符），如果创建失败则返回空字符串
            string guid = AssetDatabase.CreateFolder(parentPath, folderName);
            // 检查GUID是否为空（表示创建失败）
            if (string.IsNullOrEmpty(guid))
                Debug.LogError($"Failed to create: {fullPath}");
            else
                Debug.Log($"Created Success: {fullPath}");
        }
        else
        {
            Debug.LogWarning($"Already exists: {fullPath}");
        }
    }
}
