using Godot;
using System;

public class widgetBool : HBoxContainer
{
	// Classes instances
	private MainTop mainTopInst;
	// 
    public Int32 rootVarIdx { get; set; } = -1;
    public Int32 relArrayIdx { get; set; } = -1;
	public Byte bitIndex = 0;
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

	private void _on_Button_SetBit_pressed()
	{
		byte[] byteIndex = new byte[1];

		Byte currentByte = Convert.ToByte(mainTopInst.Fields[rootVarIdx].FieldValues[relArrayIdx].value);
		currentByte |= (Byte)(1 << bitIndex);

		byteIndex[0] = (byte)(rootVarIdx+relArrayIdx);
		string hexIdx = helper.ByteArrayToHexViaLookup32(byteIndex);

		byteIndex[0] = (byte)currentByte;
		string hexValue = helper.ByteArrayToHexViaLookup32(byteIndex);

		string msg = mainTopInst.configData.setValue + hexIdx + "B" + hexValue + "\n";
		if (Menu.port.IsOpen)
			Menu.port.Write(msg);
	}
	private void _on_Button_ClearBit_pressed()
	{
		byte[] byteIndex = new byte[1];

		Byte currentByte = (Byte)mainTopInst.Fields[rootVarIdx].FieldValues[relArrayIdx].value;
		currentByte &= (Byte)~(1 << bitIndex);

		byteIndex[0] = (byte)(rootVarIdx+relArrayIdx);
		string hexIdx = helper.ByteArrayToHexViaLookup32(byteIndex);

		byteIndex[0] = (byte)currentByte;
		string hexValue = helper.ByteArrayToHexViaLookup32(byteIndex);

		string msg = mainTopInst.configData.setValue + hexIdx + "B" + hexValue + "\n";
		if (Menu.port.IsOpen)
			Menu.port.Write(msg);
	}
}




