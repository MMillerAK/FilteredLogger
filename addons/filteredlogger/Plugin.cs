#if TOOLS
using System;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace Legacy.addons.FilteredLogger;
[Tool]
public partial class Plugin  : EditorPlugin
{
    

    public void CreateSettings()
    {
        Dictionary dic = new()
        {
            { "LOG_DIRECTORY", 
                new Dictionary()
                {
                    { "name", "FilteredLogger/Settings/LogDirectory" },
                    { "type", (int)Variant.Type.String },
                    {"hint", (int)PropertyHint.Dir},
                    {"defaultValue" , "res://Logs/"}
                }
            },
            { "LOG_NAME", 
                new Dictionary()
                {
                    { "name", "FilteredLogger/Settings/LogName" },
                    { "type", (int)Variant.Type.String },
                    
                    {"defaultValue" , "Log"}
                }
            },
            { "LOG_DEFAULT_CATEGORY", 
                new Dictionary()
                {
                    { "name", "FilteredLogger/Settings/DefaultCategory" },
                    { "type", (int)Variant.Type.String },
                    
                    {"defaultValue" , "System"}
                }
            },
            { "Write_File", 
                new Dictionary()
                {
                    { "name", "FilteredLogger/Settings/WriteToFile" },
                    { "type", (int)Variant.Type.Bool },
                    {"defaultValue" , "True"}
                        
                }
            },
            { "WRITE_C#_CONSOLE", 
                new Dictionary()
                {
                    { "name", "FilteredLogger/Settings/WriteToC#Console" },
                    { "type", (int)Variant.Type.Bool },
                    {"defaultValue" , "True"}
                    
                }
            },
            { "WRITE_GDScript_CONSOLE", 
                new Dictionary()
                {
                    { "name", "FilteredLogger/Settings/WriteToGodotConsole" },
                    { "type", (int)Variant.Type.Bool },
                    {"defaultValue" , "True"}
                    
                }
            },
        
        };

        foreach (string key in dic.Keys)
        {
            Dictionary value = (Dictionary)dic[key];
            string name = (string)value["name"];
            if (!ProjectSettings.HasSetting((string)value["name"]))
            {
                ProjectSettings.SetSetting((string)value["name"], value["defaultValue"]);
            }
            ProjectSettings.AddPropertyInfo(value);

         
        } 
    }

    
    
    public override void _EnablePlugin()
    {
        // The autoload can be a scene or script file.
        AddAutoloadSingleton(_GetPluginName(), "res://addons/filteredlogger/LoggingConsole.tscn");
    }

    public override void _DisablePlugin()
    {
        RemoveAutoloadSingleton(_GetPluginName());
    }
    
    public override void _EnterTree()
    {
        CreateSettings();
        
    }

    public override void _ExitTree()
    {
        // Clean-up of the plugin goes here.
    }

    public override string _GetPluginName()
    {
        return "FilteredLogger";
    }
    
  
}
#endif
