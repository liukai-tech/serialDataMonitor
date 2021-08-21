using Godot;
using System;

public class Cmds : VBoxContainer
{
	// Classes instances
	private MainTop mainTopInst;
    //
    public Int32 cmdIdx;
    private helperFunctions helper;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        mainTopInst = GetNode<MainTop>("/root/Main");
        helper = new helperFunctions();
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }

    public void _on_Button_Cmd_pressed()
    {
        string msg = mainTopInst.configData.executeCMD.ToString();

        LineEdit LineEditInst;

        byte[] byteIndex = new byte[1];
		byteIndex[0] = (byte)cmdIdx;
		string hexIdx = helper.ByteArrayToHexViaLookup32(byteIndex);
        msg += hexIdx;

        if (!mainTopInst.Commands[cmdIdx].hasData)
             msg += "\n";
        else
        {
            // Get the value line
            LineEditInst = GetNode<LineEdit>(this.GetPath() + "/Data_Cmd");
 
            msg += helper.ToMsgHexValue(mainTopInst.Commands[cmdIdx].dataType, LineEditInst.Text,
                     mainTopInst.configData.endian);
            msg += "\n";
        }

        if (Menu.port.IsOpen)
            if (msg!="")
                Menu.port.Write(msg);
    }
}
