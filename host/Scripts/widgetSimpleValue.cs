using Godot;
using System;

public class widgetSimpleValue : HBoxContainer
{
	// Classes instances
	private MainTop mainTopInst;
	private Chart chartInst;
    private Button buttonPlotInst;
    private Button buttonEditInst;
    private Button buttonCancelInst;
    private LineEdit LineEdit_ValueInst;

    public Int32 rootVarIdx { get; set; } = -1;
    public Int32 relArrayIdx { get; set; } = -1;

    private helperFunctions helper;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // Seek Chart class instance
        mainTopInst = GetNode<MainTop>("/root/Main");
		chartInst = GetNode<Chart>("/root/Main/Node/HBox/HSplit/HSplit/VBox/drawEngine/ColorRect/Chart");
        buttonPlotInst = GetNode<Button>("Button_Plot");
        buttonEditInst = GetNode<Button>("Button_Edit");
        buttonCancelInst = GetNode<Button>("Button_Cancel");
        LineEdit_ValueInst = GetNode<LineEdit>("LineEdit_Value");
        helper = new helperFunctions();

        buttonEditInst.Connect("pressed", this, nameof(_on_Button_Edit_pressed));
        buttonCancelInst.Connect("pressed", this, nameof(_on_Button_Cancel_pressed));
        buttonPlotInst.Connect("pressed", this, nameof(_on_Button_Plot_pressed));
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }

    private void _on_Button_Edit_pressed()
	{
        if (!mainTopInst.Fields[rootVarIdx].FieldValues[relArrayIdx].onEdit)
        {
            mainTopInst.Fields[rootVarIdx].FieldValues[relArrayIdx].onEdit = true;
            buttonEditInst.Text = "Send";
            buttonCancelInst.Disabled = false;
        }
        else
        {
            // Send the value
            sendValue();
            mainTopInst.Fields[rootVarIdx].FieldValues[relArrayIdx].onEdit = false;
            buttonCancelInst.Disabled = true;
            buttonEditInst.Text = "Edit";
        }
    }

    private void _on_Button_Cancel_pressed()
	{
        mainTopInst.Fields[rootVarIdx].FieldValues[relArrayIdx].onEdit = false;
        buttonCancelInst.Disabled = true;
        buttonEditInst.Text = "Edit";
    }
    private void _on_Button_Plot_pressed()
	{
        chartInst.curves[rootVarIdx+relArrayIdx].onPlot = buttonPlotInst.Pressed;
    }

    private void sendValue()
    {
        byte[] byteIndex = new byte[1];
        string msg;
      	byteIndex[0] = (byte)(rootVarIdx+relArrayIdx);
		string hexIdx = helper.ByteArrayToHexViaLookup32(byteIndex);

        msg = mainTopInst.configData.setValue.ToString() + hexIdx;
        msg += helper.ToMsgHexValue(mainTopInst.Fields[rootVarIdx].FieldValues[relArrayIdx].varType, LineEdit_ValueInst.Text,
                     mainTopInst.configData.endian);
        msg += "\n";

		if (Menu.port.IsOpen)
			Menu.port.Write(msg);  
    }
}
