// ProjectFolderCreate.cs
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class ProjectFolderCreate : EditorWindow
{
    private ProjectFolderConfigCreate config;
    private Vector2 scrollPosition;

    //打开窗口，根据配置文件创建目录
    [MenuItem("ArtTools/Create/Folder/Create Project Folders", false, 10)]
    private static void ShowWindow()
    {
        EditorWindow window = GetWindow<ProjectFolderCreate>("Create Project Folders");
        window.titleContent = new GUIContent("创建工程目录");
        window.minSize = new Vector2(500, 650);
    }

    //立即创建文件夹，不需要打开窗口
    [MenuItem("ArtTools/Create/Folder/Create Folders Now", false, 11)]
    private static void CreateFoldersImmediately()
    {
        CreateFoldersFromConfig();
    }

    private void OnEnable()
    {
        // Try to load existing config
        string[] guids = AssetDatabase.FindAssets("t:ProjectFolderConfigCreate");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            config = AssetDatabase.LoadAssetAtPath<ProjectFolderConfigCreate>(path);
        }
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Project Folder Creator", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Configuration section
        EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);
        config = (ProjectFolderConfigCreate)EditorGUILayout.ObjectField("Folder Config",  config, typeof(ProjectFolderConfigCreate), false);

        EditorGUILayout.Space();

        //当没有创建配置文件时，直接使用该脚本中设置的默认目录结构
        if (config == null)
        {
            EditorGUILayout.HelpBox("No configuration file found. Create one first!", MessageType.Warning);
            
            if (GUILayout.Button("Create New Configuration", GUILayout.Height(30)))
            {
                CreateNewConfig();
            }
        }
        else
        {
            if (GUILayout.Button("Open Config Editor", GUILayout.Height(30)))
            {
                Selection.activeObject = config;
            }

            EditorGUILayout.Space();

            // Action buttons
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Create Folders From Config(创建目录结构)", GUILayout.Height(30)))
            {
                //根据创建的配置文件创建目录结构
                CreateFoldersFromConfig();
            }
            EditorGUILayout.Space();
            if (GUILayout.Button("Validate Folder Structure(验证目录结构)", GUILayout.Height(30)))
            {
                //验证文件夹结构
                ValidateFolderStructure();
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Reset to Default Structure",GUILayout.Height(25)))
        {
            ResetToDefaultStructure();
        }

        if (GUILayout.Button("Check Existing Folders",GUILayout.Height(25)))
        {
            CheckExistingFolders();
        }

        EditorGUILayout.EndScrollView();
    }

    private static void CreateFoldersFromConfig()
    {
        //通过这个类的名称来查找脚本
        string[] guids = AssetDatabase.FindAssets("t:ProjectFolderConfigCreate");
        if (guids.Length == 0)
        {
            Debug.LogError("No ProjectFolderConfigSO found. Please create one first.");
            return;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        var config = AssetDatabase.LoadAssetAtPath<ProjectFolderConfigCreate>(path);
        
        if (config == null)
        {
            Debug.LogError("Failed to load configuration.");
            return;
        }

        CreateFolders(config);
    }

    private static void CreateFolders(ProjectFolderConfigCreate config)
    {
        string rootPath = "Assets";

        // Create first-level folders
        foreach (var folder in config.GetFirstLevelFolders())
        {
            CreateFolderIfNotExists(rootPath, folder);
        }

        // Create second-level folders
        foreach (var entry in config.GetSecondLevelFolders())
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

        // Create third-level folders
        foreach (var thirdkey in config.GetThirdLevelFolders())
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
        Debug.Log("Project folders created successfully from configuration!");
    }

    private void CreateNewConfig()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Create Folder Configuration",
            "ProjectFolderConfig",
            "asset",
            "Create a new folder configuration"
        );

        if (!string.IsNullOrEmpty(path))
        {
            var newConfig = CreateInstance<ProjectFolderConfigCreate>();
            
            // Set default values
            newConfig.firstLevelFolders = new List<string>
            {
                "Animator", 
                "Art", 
                "Audio", 
                "Designer", 
                "Editor", 
                "Interactive", 
                "Plugins", 
                "Presets", 
                "Rendering", 
                "Resource", 
                "Scenes", 
                "Settings", 
                "Scripts", 
                "StreamingAssets"
            };

            AssetDatabase.CreateAsset(newConfig, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            config = newConfig;
            Debug.Log($"Created new configuration at: {path}");
        }
    }

    //当没有创建配置文件时，Use Default Config按钮直接使用这里的第一层级结构
    private static void CreateFoldersWithDefaultConfig()
    {
        // Create a temporary config with default values
        var tempConfig = CreateInstance<ProjectFolderConfigCreate>();
        
        tempConfig.firstLevelFolders = new List<string>
        {
            "Animator", 
            "Art", 
            "Audio", 
            "Designer", 
            "Editor", 
            "Interactive", 
            "Plugins", 
            "Presets", 
            "Rendering", 
            "Resource", 
            "Scenes", 
            "Settings", 
            "Scripts", 
            "StreamingAssets"
        };

        CreateFolders(tempConfig);
        DestroyImmediate(tempConfig);
    }

    private static void CreateFolderIfNotExists(string parentPath, string folderName)
    {
        string fullPath = $"{parentPath}/{folderName}";
        
        if (!AssetDatabase.IsValidFolder(fullPath))
        {
            string guid = AssetDatabase.CreateFolder(parentPath, folderName);
            if (string.IsNullOrEmpty(guid))
                Debug.LogError($"Failed to create: {fullPath}");
            else
                Debug.Log($"Created Success: {fullPath}");
        }
        else
        {
            Debug.Log($"Already exists: {fullPath}");
        }
    }

    private void ValidateFolderStructure()
    {
        if (config == null) return;

        string rootPath = "Assets";
        int missingFolders = 0;

        // Validate first-level folders
        foreach (var folder in config.GetFirstLevelFolders())
        {
            string path = $"{rootPath}/{folder}";
            if (!AssetDatabase.IsValidFolder(path))
            {
                Debug.LogWarning($"Missing folder: {path}");
                missingFolders++;
            }
        }

        if (missingFolders == 0)
            Debug.Log("All first-level folders are present.");
        else
            Debug.LogWarning($"Found {missingFolders} missing first-level folders.");
    }

    private void ResetToDefaultStructure()
    {
        if (EditorUtility.DisplayDialog(
            "Reset to Default",
            "This will create a default folder structure. Continue?",
            "Yes", "No"))
        {
            CreateFoldersWithDefaultConfig();
        }
    }

    private void CheckExistingFolders()
    {
        string[] allFolders = AssetDatabase.GetSubFolders("Assets");
        Debug.Log($"Found {allFolders.Length} folders in Assets:");
        
        foreach (var folder in allFolders)
        {
            Debug.Log($"- {folder.Replace("Assets/", "")}");
        }
    }
}