using System.IO;
using FilteredLogger.Addons.FilteredLogger;
using Godot;
using Godot.Collections;
using Logger = FilteredLogger.Addons.FilteredLogger.Logger;

namespace Legacy.addons.FilteredLogger;

public partial class LoggingConsole : CanvasLayer
{

    #region Components

    

  
    [Export] public HBoxContainer TabBox { get; private set; }
    [Export] private RichTextLabel consoleLabel;
    #endregion
    private Dictionary<string, CheckButton> tabs = new Dictionary<string, CheckButton>();

    #region Helpers
    private bool IsInFilter(string category)
    {
        string withfe = $"{category}.log";
        if(tabs.ContainsKey(withfe) && tabs[withfe].IsPressed())
            return true;
        return false;
    }
    

    #endregion
    

    public override void _Ready()
    {
        Load();
        Logger.OnLog += UpdateLog;
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event is InputEventKey inputEventKey)
        {
            if (inputEventKey.Pressed && inputEventKey.Keycode == Key.F1)
            {
                Visible = !Visible;
            }
        }
    }

    private void UpdateLog(object sender, LogEventargs e)
    {
        
        //If we only need to add a message with an existing category, we just add it to the end,
        //otherwise we have to reload the file
        if (e.IsNew)
        {
            AddTab(e.Category);
        }
        else
            consoleLabel.Text += CombineMessage(e.Category, e.Message, e.Timestamp)+"\n";
    }

    public void Load()
    {
        Load(false);
    }
    public void Load(bool renew)
    {
        string fullText = "";
        string path = Logger.GetBinaryFile();
        if (renew)
        {
            foreach (string key in tabs.Keys)
            {
                RemoveTab(key);
            }

            tabs = new Dictionary<string, CheckButton>();
        }

        string result = ReadFile();
        consoleLabel.Text = result;
    }
    
    private void AddTab(string category)
    {
        if (tabs.ContainsKey(category))
        {
            return;
        }

        CheckButton checkButton = new CheckButton();
        checkButton.Text = category;
        checkButton.Name = category;
        tabs[category] = checkButton;
        TabBox.AddChild(checkButton);
        checkButton.Pressed += Load;



    }

    private void RemoveTab(string category)
    {
        tabs[category].Pressed -= Load;
        
        var tab = tabs[category];
        tabs.Remove(category);
        tab.QueueFree();
        
    }

    private string  ReadFile()
    {

        string result = "";
        byte[] bytes = Logger.GetBinary();
        
        MemoryStream stream = new MemoryStream(bytes);
        BinaryReader reader = new BinaryReader(stream);
        while (stream.Position < stream.Length)
        {
            string category = reader.ReadString();
            string timestamp = reader.ReadString();
            string message = reader.ReadString();
            if (!tabs.ContainsKey(category))
            {
                AddTab(category);
            }
            else if (tabs[category].IsPressed())
            {
                result += CombineMessage(category, timestamp, message)+"\n";
            }
        }
        
        return result;
       
    }
    

    private string CombineMessage(string category, string timestamp, string message)
    { 
        category = $"[color=red]{category}[/color]";
        timestamp = $"[color=yellow]{timestamp}[/color]";
        string result = $"[{category}], [{timestamp}],{message}";
        return result;
    }

  
    
}