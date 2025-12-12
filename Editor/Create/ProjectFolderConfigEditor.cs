using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProjectFolderConfigCreate))]
public class ProjectFolderConfigEditor : Editor
{
    private SerializedProperty firstLevelFoldersProp;
    private SerializedProperty secondLevelFoldersProp;
    private SerializedProperty thirdLevelFoldersProp;

    private void OnEnable()
    {
        firstLevelFoldersProp = serializedObject.FindProperty("firstLevelFolders");
        secondLevelFoldersProp = serializedObject.FindProperty("secondLevelFolders");
        thirdLevelFoldersProp = serializedObject.FindProperty("thirdLevelFolders");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Project Folder Configuration", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // First-level folders
        EditorGUILayout.LabelField("First Level Folders", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("These folders will be created directly under Assets.", MessageType.Info);
        EditorGUILayout.PropertyField(firstLevelFoldersProp, true);
        
        EditorGUILayout.Space();

        // Second-level folders
        EditorGUILayout.LabelField("Second Level Folders", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Parent folder name and its subfolders.", MessageType.Info);
        EditorGUILayout.PropertyField(secondLevelFoldersProp, true);

        EditorGUILayout.Space();

        // Third-level folders
        EditorGUILayout.LabelField("Third Level Folders", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Parent folder path (e.g., 'ArtAssets/Audio') and its subfolders.", MessageType.Info);
        EditorGUILayout.PropertyField(thirdLevelFoldersProp, true);

        EditorGUILayout.Space();

        // Default structure button
        if (GUILayout.Button("Load Default Structure"))
        {
            LoadDefaultStructure();
        }

        serializedObject.ApplyModifiedProperties();
    }

    public void LoadDefaultStructure()
    {
        var config = (ProjectFolderConfigCreate)target;
        
        //第一层级结构
        config.firstLevelFolders.Clear();
        config.firstLevelFolders.AddRange(new[] {
            "Animator", "Art", "Audio", "Designer", "Editor", "Interactive", "Plugins", "Presets", "Rendering", "Resource", 
            "Scenes", "Settings", "Scripts", "StreamingAssets"
        });

        //第二层级结构
        config.secondLevelFolders.Clear();
        config.secondLevelFolders.Add(new ProjectFolderConfigCreate.FolderEntry
        {
            folderName = "Art",
            subFolders = new System.Collections.Generic.List<string> { 
                "Animation", "Character", 
                "Scene", "Effect", "UI", "Fonts", "Weapon"
            }
        });
        config.secondLevelFolders.Add(new ProjectFolderConfigCreate.FolderEntry
        {
            folderName = "Rendering",
            subFolders = new System.Collections.Generic.List<string> { "Common", "Shader" }
        });
        config.secondLevelFolders.Add(new ProjectFolderConfigCreate.FolderEntry
        {
            folderName = "Scripts",
            subFolders = new System.Collections.Generic.List<string> { "MaterialParameter", "Commons" }
        });

        //第二层级结构
        config.thirdLevelFolders.Clear();
        config.thirdLevelFolders.Add(new ProjectFolderConfigCreate.NestedFolderEntry
        {
            parentPath = "Art/Scene",
            subFolders = new System.Collections.Generic.List<string> { "Material", "Prefab", "Model", "Texture" }
        });
        config.thirdLevelFolders.Add(new ProjectFolderConfigCreate.NestedFolderEntry
        {
            parentPath = "Art/Effect",
            subFolders = new System.Collections.Generic.List<string> { "Material", "Model", "Particle", "Prefab", "Texture" }
        });
        config.thirdLevelFolders.Add(new ProjectFolderConfigCreate.NestedFolderEntry
        {
            parentPath = "Rendering/Common",
            subFolders = new System.Collections.Generic.List<string> { "Material", "Texture" }
        });

        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
        
        Debug.Log("Default structure loaded successfully!");
    }
}