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
            { "ArtAssets", new  [] { "Animations", "Animator" , "Audio" , "Character", "Scene", "Effect" , "UI" , "Fonts" ,"Weapon" , "Interactive"} },
            { "Presets", new [] { "Character" , "Scene"} },
            { "Scripts", new[] { "Shaders" , "Commons" } },
            { "Maps", new[] { "DesignerLevels" , "ArtLevels" , "ProgramLevels" } }
        };
    
    /// <summary>
    //Third-level folders
    /// </summary>
    public static readonly Dictionary<string, string[]> ThirdLevelFolders =
        new Dictionary<string, string[]>
        {
            { "ArtAssets/Audio",new []{"Music", "Sounds"} },
            { "ArtAssets/Scene", new[]{ "Materials", "Models", "Prefabs", "Textures" } },
            {"ArtAssets/UI",new []{"Background","icon"}},
            { "ArtAssets/Interactive", new[]{"TriggerObjects", "Prop"}},
            { "Presets/Scene", new[] { "Models" , "Textures"}},
            { "Scripts/Shaders", new []{"Character","Scene" , "Effects" , "UI" , "Postprocess"}}
        };
}
