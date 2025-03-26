using System.Collections.Generic;

public static class ProjectFolderConfig
{
    // First-level folders
    public static readonly string[] FirstLevelFolders =
    {
        "ArtAssets",
        "Editor",
        "Plugins",
        "Presets",
        "Maps",
        "Settings",
        "Scripts",
        "StreamingAssets"
    };

    // Second-level folders (ParentFolder -> SubFolders)
    public static readonly Dictionary<string, string[]> SecondLevelFolders = 
        new Dictionary<string, string[]>
        {
            { "ArtAssets", new  [] { "Animations", "Animator" , "Audio" , "Character", "Scene", "Effect" , "UI" , "Fonts" ,"Weapon" } },
            { "Presets", new [] { "Character" , "Scene"} },
            { "Scripts", new[] { "Shaders" , "Commons" } },
            { "Maps", new[] { "DesignerLevels" , "ArtLevels" , "ProgramLevels" } }
        };
    
    //Third-level folders
    public static readonly Dictionary<string, string[]> ThirdLevelFolders =
        new Dictionary<string, string[]>
        {
            { "ArtAssets/Scene", new[] { "Materials", "Models", "Prefabs", "Textures" } },
            { "Presets/Scene" , new[] { "Models" , "Textures"}}
        };
}
