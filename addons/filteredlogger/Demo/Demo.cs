using Godot;
using System;
using Logger = FilteredLogger.Addons.FilteredLogger.Logger;

public partial class Demo : Control
{
    public override void _Ready()
    {
        base._Ready();
        CreateHBox(3, 5);

    }

    //creating a grid of buttons
    public HBoxContainer CreateHBox(int columns, int rows)
    {
        HBoxContainer hBoxContainer = new HBoxContainer();
        AddChild(hBoxContainer);
        Logger.Log("Created HBOX", "ObjectCreation");
        for (int i = 0; i < 2; i++)
        {
           CreateVbox(hBoxContainer, i, rows);
           
           
        }

        return hBoxContainer;
    }

    public VBoxContainer CreateVbox(HBoxContainer parent, int column, int buttoncount)
    {
        VBoxContainer vBoxContainer = new VBoxContainer();
        parent.AddChild(vBoxContainer);
        Logger.Log($"Created VBOX in column {column}", "ObjectCreation");
        for (int i = 0; i < buttoncount; i++)
        {
            CreateButtons(vBoxContainer, buttoncount, column);
        }

        return vBoxContainer;
    }
    public void CreateButtons(VBoxContainer parent, int count, int column)
    {
        for (int i = 0; i < count; i++)
        {
            Button button = new Button();
            parent.AddChild(button);
            Logger.Log($"Created BUTTON in column {column}", "ObjectCreation");
            button.Text = $"filler text for  row {count}, {column} column, ";
            
            //in a real application this should be a non lamda method to facilitate up
            int buttonnumber = i;
            button.Pressed += () =>
            {
                Logger.Log($"Pressed BUTTON # {buttonnumber} in column {column}", "Event");
            };
        }
    }
}
