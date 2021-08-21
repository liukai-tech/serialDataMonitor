using Godot;
using System;

public class widget : HBoxContainer
{
	// Classes instances
	private MainTop mainTopInst;
	private Button MonitOnOffInst;
	private Timer TimerInst;
	private static Texture textureMonitOff = ResourceLoader.Load("res://icons/monitOff.png") as Texture;
	private static Texture textureMonitOn = ResourceLoader.Load("res://icons/monitOn.png") as Texture;

 	public Int32 varIdx { get; set; } = -1;
	public Int32 sizeArray { get; set; } = 1;

	private bool onMonitoring = false;

	private helperFunctions helper;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		mainTopInst = GetNode<MainTop>("/root/Main");
		MonitOnOffInst = GetNode<Button>("MonitOnOff");
		TimerInst = GetNode<Timer>("Timer");
		helper = new helperFunctions();
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }

	private void _on_MonitOnOff_pressed()
	{
		if (Menu.port != null && varIdx >= 0) 
		{
			byte[] byteIndex = new byte[1];
			byteIndex[0] = (byte)varIdx;
			string hexIdx = helper.ByteArrayToHexViaLookup32(byteIndex);
			byteIndex[0] = (byte)sizeArray;
			string nbFields = helper.ByteArrayToHexViaLookup32(byteIndex);

			if (!onMonitoring)
			{
				string msg = mainTopInst.configData.reportValueOn + hexIdx + nbFields + "\n";
				if (Menu.port.IsOpen)
					Menu.port.Write(msg);
			}
			else
			{
				string msg = mainTopInst.configData.reportValueOff + hexIdx + nbFields + "\n";
				if (Menu.port.IsOpen)
					Menu.port.Write(msg);			
			}
		}
	}

	public void setOnMonitoring()
	{
		onMonitoring = true;
		MonitOnOffInst.AddColorOverride("font_color", new Color(1,0,0,1));
		MonitOnOffInst.AddColorOverride("font_color_hover", new Color(1,0,0,1));
		TimerInst.Start();
	}

	public void _on_Timer_timeout()
	{
		onMonitoring = false;
		MonitOnOffInst.AddColorOverride("font_color", new Color(1,1,1,1));
		MonitOnOffInst.AddColorOverride("font_color_hover", new Color(1,1,1,1));
	}
}

