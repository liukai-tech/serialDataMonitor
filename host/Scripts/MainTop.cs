using Godot;
using System;

public enum sectionType_e
{
	eVar,
	eCmd,
}

public enum varType_e
{
	eFloat, // 8 chars
	eU32, // 8 chars
	eI32, // 8 chars
	eU16, // 4 chars
	eI16, // 4 chars
	eU8, // 2 chars
	eI8, // 2 chars
	eUnknown
}

public enum cmdType_e
{
	eButton,
}

public class configData_t
{
	public Int32 highestRootIdxVars;
	public Int32 highestIndexCmds;
	public Int32 baudrate;
	public string endian;
	//Commands from monitor to target
	public char reportValueOn;
	public char reportValueOnce;
	public char reportValueOff;
	public char setValue;
	public char executeCMD;
	//Messages from target to monitor
	public char reportValue;
	//Every 1ms a report is sent
	public float sampleTimeHW;
	//Fields layouting
	public Int32 numTabs;
	public string[] tabsNames;
	public Int32[] tabsH;
	public Int32[] tabsV;
}

/*
There is ONE field per Cmd in the .ini file
Cmds have their own index 
*/
public class Command_t
{
	public string name;
	public bool hasData;
	public varType_e dataType;
	private object data;
}

public class Bit_t
{
	public string text;
	public bool canEdit;
	public string color;
}

public class FieldValue_t
{
	public bool canEdit;
	public bool onEdit;
	public bool canPlot;
	public bool boolsOnU8;
	public bool spinBox;
	public Bit_t[] bits;

	// VALUE FIELDS
	public varType_e varType;
	public object value;
	public string unit;
	
	public FieldValue_t(varType_e _varType, bool _boolsOnU8, string _unit)
	{
		varType = _varType;
		boolsOnU8 = _boolsOnU8;
		unit = _unit;

		onEdit = false;

		if (boolsOnU8)
		{
			bits = new Bit_t[8];
			for (Byte bit=0; bit<8; bit++)
				bits[bit] = new Bit_t();
		}
	}
}
// 
/*
There is ONE field per Var in the .ini file
For Var that is an array (hence that has more than one index)
A LUT is used to link the embedded index and the host index
*/
public class Field_t
{
	public string name;
	public sectionType_e sectionType;

	public bool monitor;

	public FieldValue_t[] FieldValues;

	// Widget path for updating value
	public string pathValue;
	
	// Displaying
	public Int32 tab;
	public Int32 screenH;
	public Int32 screenV;
	// Diplayed value = valueMultVisual * FieldValue + valueAddVisual
	
	// For Cmd sectionType
	public Int32 cmdIdx;
	public Int32 cmdData;

	public Field_t(varType_e _varType, Int32 _sizeArray, bool _boolOnU8, string _unit)
	{
		FieldValues = new FieldValue_t[_sizeArray];

		for (UInt32 relArrayIdx=0; relArrayIdx<FieldValues.Length; relArrayIdx++)
		{
			FieldValues[relArrayIdx] = new FieldValue_t(_varType, _boolOnU8, _unit);
		}
	}
}
	
public class MainTop : Panel
{
	// Classes instances
	private Chart chartInst;
	private VBoxContainer VBoxCMDSInst;

	public Field_t[] Fields;
	public Command_t[] Commands;
	public Godot.Collections.Dictionary<int, int> LUTIdx;

	public configData_t configData = new configData_t();
	
	public Godot.Collections.Dictionary<sectionType_e, string> sectionTypes =
		new Godot.Collections.Dictionary<sectionType_e, string>();
	public static Godot.Collections.Dictionary<varType_e, string> fieldVarTypes =
		new Godot.Collections.Dictionary<varType_e, string>();
	public static Godot.Collections.Dictionary<string, string> varErrorsKEYS =
		new Godot.Collections.Dictionary<string, string>();
	public string varErrorsBU8Text, varErrorsBU8Type, varErrorsBU8Array, varErrorTYPE;
	public Godot.Collections.Dictionary<string, string> defErrorsKEYS =
		 new Godot.Collections.Dictionary<string, string>();
	public string defErrorsTAB0, defErrorsTAB1;
	public Godot.Collections.Dictionary<string, string> cmdErrorsKEYS =
		 new Godot.Collections.Dictionary<string, string>();
	public string cmdErrorTYPEDATA;
	
	public override void _Ready()
	{
		// Class instance
		chartInst = GetNode<Chart>("Node/HBox/HSplit/HSplit/VBox/drawEngine/ColorRect/Chart");
		VBoxCMDSInst = GetNode<VBoxContainer>("Node/HBox/HSplit/HSplit/Scene/Panel/VBoxCMDS");

		// Sections types
		sectionTypes.Add(sectionType_e.eVar, "Var");
		sectionTypes.Add(sectionType_e.eCmd, "Cmd");
		
		// For "Var ..." sections
		fieldVarTypes.Add(varType_e.eFloat, "f");
		fieldVarTypes.Add(varType_e.eU32, "W");
		fieldVarTypes.Add(varType_e.eI32, "w");
		fieldVarTypes.Add(varType_e.eU16, "I");
		fieldVarTypes.Add(varType_e.eI16, "i");
		fieldVarTypes.Add(varType_e.eU8, "B");
		fieldVarTypes.Add(varType_e.eI8, "b");
		fieldVarTypes.Add(varType_e.eUnknown, "unknown");

		/*
		* ERRORS MESSAGES
		*/
		//For [Var ...] sections
		varErrorsKEYS.Add("index", "Field must have an index key");
		varErrorsKEYS.Add("screenH", "Field must have a screenH key");
		varErrorsKEYS.Add("screenV", "Field must have a screenH key");
		varErrorsKEYS.Add("type", "Field must have a type key");
		varErrorsKEYS.Add("tab", "Field must have a tab key");
		varErrorTYPE = "Var types must be one of f,W,w,I,i,B,b";		
		varErrorsBU8Text = "If boolsOnU8 = true, at least one textX key must be present";
		varErrorsBU8Type = "If boolsOnU8 = true, the data type must be B";
		varErrorsBU8Array = "If boolsOnU8 = true, sizeArray must be 1 (array of boolsOnU8 not supported)";
		
		//For [DEFAULT] section
		defErrorsKEYS.Add("baudrate", "[DEFAULT] must have a baudrate key");
		defErrorsKEYS.Add("endian", "[DEFAULT] must have an endian key");
		defErrorsKEYS.Add("reportValueOn", "[DEFAULT] must have a reportValueOn key");
		defErrorsKEYS.Add("reportValueOnce", "[DEFAULT] must have a reportValueOnce key");
		defErrorsKEYS.Add("reportValueOff", "[DEFAULT] must have a reportValueOff key");
		defErrorsKEYS.Add("setValue", "[DEFAULT] must have a setValue key");
		defErrorsKEYS.Add("executeCMD", "[DEFAULT] must have a executeCMD key");
		defErrorsKEYS.Add("reportValue", "[DEFAULT] must have a reportValue key");
		defErrorsKEYS.Add("sampleTimeHW", "[DEFAULT] must have a sampleTimeHW key");
		defErrorsKEYS.Add("tabs", "[DEFAULT] must have a tabs key");
		defErrorsTAB0 = "[DEFAULT] tabXName not present";

		//For [Cmd ...] sections
		cmdErrorsKEYS.Add("index", "[Cmd] must have a index key");
		cmdErrorTYPEDATA = "If hasData = true, [Cmd] must have a typeData key";
	}

	public bool testDefault(ConfigFile configMonitor)
	{
		bool hardFault = false;
		string hardFaultMessage;

		foreach (string section in configMonitor.GetSections())
		{
			string sectionType = section.Substring(0, 3).ToLower();
			// "DEFAULT" section
			if ( sectionType == "def") 
			{
				// Mandatory section keys MUST be defined
				foreach (string key in defErrorsKEYS.Keys)
				{
					if (!configMonitor.HasSectionKey(section, key))
					{
						hardFault = true;
						hardFaultMessage = (string)defErrorsKEYS[key];
						GD.Print(hardFaultMessage);
						break;
					}
				}
				
				if (!hardFault)
				{
					// Check the presence of the tabs related Fields
					Int32 tabNumber = (Int32)configMonitor.GetValue("DEFAULT", "tabs");
					for (UInt32 i=0; i<tabNumber; i++)
					{
						if (!configMonitor.HasSectionKey("DEFAULT", "tab"+i.ToString()+"Name"))
						{
							hardFault = true;
							hardFaultMessage = defErrorsTAB0;
							GD.Print(hardFaultMessage);
							break;
						}
					}
				}
			}
		}
		return hardFault;
	}
	public bool testVariables(ConfigFile configMonitor)
	{
		bool hardFault = false;
		string hardFaultMessage;
		
		/**********************************************************************
		*************				TESTS
		***********************************************************************/
		foreach (string section in configMonitor.GetSections())
		{
			string sectionType = section.Substring(0, 3).ToLower();

			// Var sections
			if ( sectionType == "var")
			{
				// Mandatory section keys MUST be defined
				foreach (string key in varErrorsKEYS.Keys)
				{
					if (!configMonitor.HasSectionKey(section, key))
					{
						hardFault = true;
						hardFaultMessage = (string)varErrorsKEYS[key];
						GD.Print(hardFaultMessage);
						break;
					}
				}

				// Section type tests
				string type = (string)configMonitor.GetValue(section, "type");
				hardFault = true;
				foreach (string varType in fieldVarTypes.Values)
				{
					if (varType == type)
					{
						hardFault = false;
						break;
					}
				}
				if (hardFault)
				{
					hardFaultMessage = varErrorTYPE;
					GD.Print(hardFaultMessage);
				}

				// If boolsOnU8 variable
				if ((string)configMonitor.GetValue(section, "boolsOnU8", "false") == "true") 
				{
					// At least one of text0, text1, ..., text7 MUST be defined
					hardFault = true;
					for (UInt32 i=0; i<8; i++)
					{
						if (configMonitor.HasSectionKey(section, "text"+i.ToString()))
						{
							hardFault = false;
							break;
						}	
					}
					if (hardFault)
					{
						hardFaultMessage = varErrorsBU8Text;
						GD.Print(hardFaultMessage);
					}
					// If boolsOnU8="true", the type must be "B"
					if ((string)configMonitor.GetValue(section, "type") != "B") 
					{
						hardFault = true;
						hardFaultMessage = varErrorsBU8Type;
						GD.Print(hardFaultMessage);
						break;
					}
					// If boolsOnU8="true", sizeArray must be 1 (array of boolsOnU8 not supported) 
					if ((string)configMonitor.GetValue(section, "sizeArray", "1") != "1") 
					{
						hardFault = true;
						hardFaultMessage = varErrorsBU8Array;
						GD.Print(hardFaultMessage);
						break;
					}
				}
			}

			if (hardFault)
				break;
		}

		return hardFault;
	}
	public bool testCommands(ConfigFile configMonitor)
	{
		bool hardFault = false;
		string hardFaultMessage;

		foreach (string section in configMonitor.GetSections())
		{
			string sectionType = section.Substring(0, 3).ToLower();
			if ( sectionType == "cmd")
			{
				// Mandatory section keys MUST be defined
				foreach (string key in cmdErrorsKEYS.Keys)
				{
					if (!configMonitor.HasSectionKey(section, key))
					{
						hardFault = true;
						hardFaultMessage = (string)cmdErrorsKEYS[key];
						GD.Print(hardFaultMessage);
						break;
					}
				}	
				// If a command has data then the typeData must be present
				if ((string)configMonitor.GetValue(section, "hasdata", "false") == "true") 
				{	
					if (!configMonitor.HasSectionKey(section, "datatype"))
					{
						hardFault = true;
						hardFaultMessage = (string)cmdErrorTYPEDATA;
						GD.Print(hardFaultMessage);
						break;						
					}
				}			
			}
		}
		return hardFault;
	}
	public void fillDefault(ConfigFile configMonitor)
	{
		foreach (string section in configMonitor.GetSections())
		{
			string sectionType = section.Substring(0, 3).ToLower();	
			if (sectionType == "def")
			{
				configData.baudrate = (Int32)configMonitor.GetValue("DEFAULT", "baudrate");
				configData.endian = (string)configMonitor.GetValue("DEFAULT", "endian");
				configData.reportValueOn = ((string)configMonitor.GetValue("DEFAULT", "reportValueOn"))[0];
				configData.reportValueOnce = ((string)configMonitor.GetValue("DEFAULT", "reportValueOnce"))[0];
				configData.reportValueOff = ((string)configMonitor.GetValue("DEFAULT", "reportValueOff"))[0];
				configData.setValue = ((string)configMonitor.GetValue("DEFAULT", "setValue"))[0];
				configData.executeCMD = ((string)configMonitor.GetValue("DEFAULT", "executeCMD"))[0];
				configData.reportValue = ((string)configMonitor.GetValue("DEFAULT", "reportValue"))[0];
				configData.sampleTimeHW = (float)configMonitor.GetValue("DEFAULT", "sampleTimeHW");
				configData.numTabs = (Int32)configMonitor.GetValue("DEFAULT", "tabs");
				configData.tabsNames = new string[configData.numTabs];
				configData.tabsH = new Int32[configData.numTabs];
				configData.tabsV = new Int32[configData.numTabs];
				for (UInt32 tab=0; tab<configData.numTabs; tab++)
					configData.tabsNames[tab] = (string)configMonitor.GetValue("DEFAULT", "tab"+tab.ToString()+"Name");
			}
		}	
	}
	public void fillVariables(ConfigFile configMonitor)
	{
		/**********************************************************************
		*************				Fill the Fields array
		***********************************************************************/
		// First, find the highest index to size the Fields array
		Int32 highestRootIdxVars = -1;
		Int32 lastElementSizeArray = -1;
		Int32 rootVarIdx, sizeArray;
		foreach (string section in configMonitor.GetSections())
		{
			string sectionType = section.Substring(0, 3).ToLower();
			if (sectionType == "var")
			{
				rootVarIdx = (Int32)configMonitor.GetValue(section, "index");
				if (rootVarIdx>highestRootIdxVars) 
				{
					highestRootIdxVars=rootVarIdx;
					// In case if last rootVarIdx is an array
					lastElementSizeArray = (Int32)configMonitor.GetValue(section, "sizeArray", 1);
				}
			}
		}
		configData.highestRootIdxVars = highestRootIdxVars;
		Fields = new Field_t[configData.highestRootIdxVars+1];
		LUTIdx = new Godot.Collections.Dictionary<int, int>();

		// Second, read all the Fields indexes, if a sizeArray is > 1 then create the
		// related indexes (these indexes are sent by the embedded)
		// Then store all the indexes in a LUT that will translate the embedded index to a
		// host index (will be the same if sizeArray=1)
		foreach (string section in configMonitor.GetSections())
		{
			string sectionType = section.Substring(0, 3).ToLower();

			if (sectionType == "var")
			{		
				// Index
				rootVarIdx = (Int32)configMonitor.GetValue(section, "index");
				sizeArray = (Int32)configMonitor.GetValue(section, "sizeArray", 1);

				if (sizeArray == 1) 
				{
					// Not an array, simply create a one-one link
					LUTIdx.Add(rootVarIdx, rootVarIdx);
				}
				else
				{
					// Array, create the links
					for (Int32 relArrayIdx=0; relArrayIdx<sizeArray; relArrayIdx++)
						LUTIdx.Add(rootVarIdx+relArrayIdx, rootVarIdx);
				}
			}
		}

		// Third, retreive the maximum rows and columns per tab
		for(UInt32 tab=0; tab<configData.numTabs; tab++ )
		{
			Int32 highestRow = -1;
			Int32 highestColumn = -1;
			Int32 row, columns;
			foreach (string section in configMonitor.GetSections())
			{
				string sectionType = section.Substring(0, 3).ToLower();
				if (sectionType == "var")
				{
					if ((Int32)configMonitor.GetValue(section, "tab") == tab)
					{
						columns = (Int32)configMonitor.GetValue(section, "screenH");
						row = (Int32)configMonitor.GetValue(section, "screenV");
						if ( columns >= highestColumn)
							highestColumn = columns;
						if ( row >= highestRow)
							highestRow = row;		
					}				
				}
			}	
			configData.tabsH[tab] = highestColumn+5;
			configData.tabsV[tab] = highestRow+5;
		}

		// Quatro, fill the Fields with the indexes found in .ini
		foreach (string section in configMonitor.GetSections())
		{
			string sectionType = section.Substring(0, 3).ToLower();
			string stmp, sUnit;
			
			if (sectionType == "var")
			{
				// Index
				rootVarIdx = (Int32)configMonitor.GetValue(section, "index");

				// FIELD CREATE: Check the varType for allocating variable(s)
				stmp = (string)configMonitor.GetValue(section, "type");
				varType_e varType = varType_e.eUnknown;
				foreach (varType_e varTypeKey in fieldVarTypes.Keys)
					if (fieldVarTypes[varTypeKey] == stmp)
						{ varType = varTypeKey; break; }

				// FIELD CREATE: Check array size for allocating variable(s)
				sizeArray = (Int32)configMonitor.GetValue(section, "sizeArray", 1);

				// FIELD CREATE: Check if boolsOnU8
				stmp = (string)configMonitor.GetValue(section, "boolsOnU8", "false");
				bool boolOnU8 = (stmp == "true") ? true : false;

				// FIELD CREATE: Check if boolsOnU8
				sUnit = (string)configMonitor.GetValue(section, "unit", "");

				// CREATE FIELD
				Fields[rootVarIdx] = new Field_t(varType, sizeArray, boolOnU8, sUnit);

				// name
				Fields[rootVarIdx].name = section.Substring(4);
				// monitor
				stmp = (string)configMonitor.GetValue(section, "monitor", "true");
				Fields[rootVarIdx].monitor = (stmp == "true") ? true : false;
				// // sectionType
				// stmp = section.Substring(0, 3);
				// foreach (sectionType_e sectionTypeKey in sectionTypes.Keys)
				// 	if (sectionTypes[sectionTypeKey] == stmp)
				// 		{ Fields[rootVarIdx].sectionType = sectionTypeKey; break; }
						
				// Set FieldValues to default found in .ini or set to 0
				for (Int32 relArrayIdx=0; relArrayIdx<Fields[rootVarIdx].FieldValues.Length; relArrayIdx++)
				{
					Fields[rootVarIdx].FieldValues[relArrayIdx].value = configMonitor.GetValue(section, "defVal"+relArrayIdx.ToString(), 0);
						
					// spinBox
					stmp = (string)configMonitor.GetValue(section, "spinBox", "false");
					Fields[rootVarIdx].FieldValues[relArrayIdx].spinBox = (stmp == "true") ? true : false;

					// canEdit
					stmp = (string)configMonitor.GetValue(section, "canEdit", "false");
					Fields[rootVarIdx].FieldValues[relArrayIdx].canEdit = (stmp == "true") ? true : false;

					// canPlot
					stmp = (string)configMonitor.GetValue(section, "canPlot", "false");
					Fields[rootVarIdx].FieldValues[relArrayIdx].canPlot = (stmp == "true") ? true : false;

					if ( Fields[rootVarIdx].FieldValues[relArrayIdx].boolsOnU8 )
					{
						for (Byte bit=0; bit<8; bit++)
						{
							stmp = (string)configMonitor.GetValue(section, "text"+bit.ToString(), "NA");
							Fields[rootVarIdx].FieldValues[relArrayIdx].bits[bit].text = stmp;
							stmp = (string)configMonitor.GetValue(section, "canBitEdit"+bit.ToString(), "false");
							Fields[rootVarIdx].FieldValues[relArrayIdx].bits[bit].canEdit = (stmp == "true") ? true : false;
							stmp = (string)configMonitor.GetValue(section, "color"+bit.ToString(), "green");
							Fields[rootVarIdx].FieldValues[relArrayIdx].bits[bit].color = stmp;
						}
					}
				}
				// tab
				Fields[rootVarIdx].tab = (Int32)configMonitor.GetValue(section, "tab", 0);
				// screenH
				Fields[rootVarIdx].screenH = (Int32)configMonitor.GetValue(section, "screenH");
				// screenV
				Fields[rootVarIdx].screenV = (Int32)configMonitor.GetValue(section, "screenV");
			}
		}
		// Initialize the curves in Chart
		chartInst.setCurvesNumber((UInt32)(configData.highestRootIdxVars + lastElementSizeArray +1));
	}	
	public void fillCommands(ConfigFile configMonitor)
	{
		/**********************************************************************
		*************				Fill the Commands array
		***********************************************************************/
		// First, find the highest index to size the Commands array
		Int32 highestIndexCmds = -1;
		Int32 idx;
		foreach (string section in configMonitor.GetSections())
		{
			string sectionType = section.Substring(0, 3).ToLower();
			if (sectionType == "cmd")
			{
				idx = (Int32)configMonitor.GetValue(section, "index");
				if (idx>highestIndexCmds)
					highestIndexCmds=idx;
			}
		}
		configData.highestIndexCmds = highestIndexCmds;
		Commands = new Command_t[configData.highestIndexCmds+1];

		// Second, fill the Commands with the indexes found in .ini
		foreach (string section in configMonitor.GetSections())
		{
			string sectionType = section.Substring(0, 3).ToLower();
			string stmp;
			
			if (sectionType == "cmd")
			{
				// Index
				idx = (Int32)configMonitor.GetValue(section, "index");
				// Create the field
				Commands[idx] = new Command_t();
				// Name
				Commands[idx].name = section.Substring(4);
				// Command has data?			
				stmp = (string)configMonitor.GetValue(section, "hasdata", "false");
				Commands[idx].hasData = (stmp == "true") ? true : false;
				if (Commands[idx].hasData)
				{
					// Yes, get the data type
					stmp = (string)configMonitor.GetValue(section, "datatype");
					Commands[idx].dataType = varType_e.eUnknown;
					foreach (varType_e varTypeKey in fieldVarTypes.Keys)
						if (fieldVarTypes[varTypeKey] == stmp)
							{ Commands[idx].dataType = varTypeKey; break; }
				}
				// Command widget
				stmp = (string)configMonitor.GetValue(section, "typeWidget", "button");
			}
		}
	}
	public void displayVariables()
	{
		// Create the necessary Tabs
		string tabPath = "Node/HBox/HSplit/HSplit/VBox/TabContainer";
		TabContainer tabNode = GetNode<TabContainer>(tabPath);
		var widget = ResourceLoader.Load("widget.tscn") as PackedScene;

		// First remove children of TabContainer if any
		var children = tabNode.GetChildren();
		foreach (Node c in children)
		{
			tabNode.RemoveChild(c);
			c.QueueFree();
		}

		// FOR EACH TAB
		for (UInt32 tab=0; tab<configData.numTabs; tab++)
		{
			// Add the tab
			var grilleTabInst = new GridContainer();
			grilleTabInst.SizeFlagsHorizontal = (int)(Control.SizeFlags.Fill | Control.SizeFlags.Expand);
			grilleTabInst.Name = configData.tabsNames[tab];
			grilleTabInst.Columns = configData.tabsH[tab]; // columns
			string grillePath = tabPath + "/" + grilleTabInst.Name;
			tabNode.AddChild(grilleTabInst);
			var grilleNode = GetNode(grillePath);

			// FOR EACH COLUMN
			for (UInt32 H=0; H<grilleTabInst.Columns; H++)
			{
				// One row (VBox) per colums
				var rowInst = new VBoxContainer();
				rowInst.SizeFlagsHorizontal = (int)(Control.SizeFlags.Fill | Control.SizeFlags.Expand);
				rowInst.Name = "row" + H.ToString();
				rowInst.SizeFlagsHorizontal = (int)(Control.SizeFlags.Fill | Control.SizeFlags.Expand);
				grilleNode.AddChild(rowInst);
				
				// Add widget to each VBoxContainer
				string rowPath = grillePath+ "/" + rowInst.Name;
				var rowNode = GetNode(rowPath);
				
				// FOR EACH ROW
				for (UInt32 V= 0; V < configData.tabsV[tab]; V++)
				{
					// ***********************************************
					// Seek in the Fields array the index that correspond to current tab, H and V
					// ***********************************************
					Int32 varIdx = indexOfPos(tab, H, V);

					// Display if index found
					if (varIdx >= 0)
					{
						var elemInst = new VBoxContainer();
						elemInst.SizeFlagsHorizontal = (int)(Control.SizeFlags.Fill | Control.SizeFlags.Expand);
						elemInst.Name = "elemH" + H.ToString() + "V" + V.ToString();
						rowNode.AddChild(elemInst);

						string elemPath = rowPath + "/" + elemInst.Name;
						VBoxContainer elemNode = GetNode<VBoxContainer>(elemPath);

						var widgetInst = widget.Instance() as widget;
						widgetInst.varIdx = varIdx;
						widgetInst.sizeArray = Fields[varIdx].FieldValues.Length;
						widgetInst.Name = "WDG";
						elemNode.AddChild(widgetInst);
						
						// Set the element name
						string rootElementPath = elemPath + "/" + widgetInst.Name;
						string nameSectionPath =  rootElementPath + "/Label_SectionName";
						GetNode<Label>(nameSectionPath).Text = Fields[varIdx].name;
						GetNode<Label>(nameSectionPath).HintTooltip = "(id" + varIdx.ToString() + ")";
						
						// Enable/disable the monitoring button regarding the monitor value
						string monitOnOffPath = rootElementPath + "/MonitOnOff";
						if (Fields[varIdx].monitor)
							GetNode<Button>(monitOnOffPath).Visible = true;
						else
							GetNode<Button>(monitOnOffPath).Visible = false;
						
						// Save the element path for monitoring display
						Fields[varIdx].pathValue = elemPath;
		
						// Display
						displayVars(elemPath, varIdx);
					}
				}
			}
		}
	}
	public void displayCommands()
	{
		var Cmds = ResourceLoader.Load("Cmds.tscn") as PackedScene;

		if (configData.highestIndexCmds>=0)
		{
			Label labelCommands = new Label();
			labelCommands.Text = "Commands";
			VBoxCMDSInst.AddChild(labelCommands);
		}

		for (Int32 i=0; i<=configData.highestIndexCmds; i++)
		{
			var cmdInst = Cmds.Instance() as Cmds;
			Button Button_CmdInst = cmdInst.GetNode<Button>("Button_Cmd");
			LineEdit Data_CmdInst = cmdInst.GetNode<LineEdit>("Data_Cmd");

			cmdInst.Name = "CMD" + i.ToString();
			cmdInst.cmdIdx = i;
			VBoxCMDSInst.AddChild(cmdInst);
			
			Button_CmdInst.Text = Commands[i].name;
			if (Commands[i].hasData)
			{
				Data_CmdInst.Visible = true;
				Data_CmdInst.Text = "0";
			}
			else
			{
				Data_CmdInst.Visible = false;
			}
		}	
	}
	private void displayVars(string elemPath, Int32 rootVarIdx)
	{
		VBoxContainer elemNode = GetNode<VBoxContainer>(elemPath);

		var widgetSimpleValue = ResourceLoader.Load("widgetSimpleValue.tscn") as PackedScene;
		var widgetBool = ResourceLoader.Load("widgetBool.tscn") as PackedScene;
		var widgetSpinBox = ResourceLoader.Load("widgetSpinBox.tscn") as PackedScene;

		for (Int32 relArrayIdx=0; relArrayIdx<Fields[rootVarIdx].FieldValues.Length; relArrayIdx++)
		{
			// Field is boolsOnU8
			if (Fields[rootVarIdx].FieldValues[relArrayIdx].boolsOnU8)
			{
				for (Byte bit= 0; bit < 8; bit++)
				{
					widgetBool widgetBoolInst = widgetBool.Instance() as widgetBool;
					widgetBoolInst.Name = "WSV" + relArrayIdx.ToString() + "BIT" + bit.ToString();
					widgetBoolInst.rootVarIdx = rootVarIdx;
					widgetBoolInst.relArrayIdx = relArrayIdx;
					widgetBoolInst.bitIndex = bit;
					elemNode.AddChild(widgetBoolInst);
					
					if (Fields[rootVarIdx].FieldValues[relArrayIdx].bits[bit].text != "NA")
					{
						(widgetBoolInst.FindNode("Label_BitNumber") as Label).Text = "B"+bit.ToString();

						(widgetBoolInst.FindNode("Label_BitName") as Label).Text = 
							 Fields[rootVarIdx].FieldValues[relArrayIdx].bits[bit].text;

						// Show/Hide the edit button related to canBitEditX
						(widgetBoolInst.FindNode("Button_SetBit") as Button).Visible = 
							Fields[rootVarIdx].FieldValues[relArrayIdx].bits[bit].canEdit ? true : false;

						(widgetBoolInst.FindNode("Button_ClearBit") as Button).Visible = 
							Fields[rootVarIdx].FieldValues[relArrayIdx].bits[bit].canEdit ? true : false;
					}
					else
					{
						(widgetBoolInst.FindNode("Label_BitName") as Label).Visible = false;
						(widgetBoolInst.FindNode("Button_SetBit") as Button).Visible = false;
						(widgetBoolInst.FindNode("Button_ClearBit") as Button).Visible = false;
						(widgetBoolInst.FindNode("TextureRect_BitVal") as TextureRect).Visible = false;
					}
				}
			}
			else 
			{
				// // Field is a spin box
				// if (Fields[rootVarIdx].FieldValues[relArrayIdx].spinBox)
				// {
				// 	widgetSpinBox widgetSpinBoxInst = widgetSpinBox.Instance() as widgetSpinBox;
				// 	widgetSpinBoxInst.Name = "WSB" + relArrayIdx.ToString();
				// 	widgetSpinBoxInst.rootVarIdx = rootVarIdx;
				// 	widgetSpinBoxInst.relArrayIdx = relArrayIdx;
				// 	elemNode.AddChild(widgetSpinBoxInst);

				// 	// Display the index if array
				// 	if (Fields[rootVarIdx].FieldValues.Length>1)
				// 		(widgetSpinBoxInst.FindNode("Label_Index") as Label).Text = relArrayIdx.ToString();
					
				// 	// Check for field editing canEdit
				// 	if (!Fields[rootVarIdx].FieldValues[relArrayIdx].canEdit)
				// 		(widgetSpinBoxInst.FindNode("Button_Edit") as Button).Visible = false;

				// 	// Check for plot enabled
				// 	if (!Fields[rootVarIdx].FieldValues[relArrayIdx].canPlot)
				// 		(widgetSpinBoxInst.FindNode("Button_Plot") as Button).Visible = false;
					
				// 	// Display the default FieldValue
				// 	SpinBox SB = (widgetSpinBoxInst.FindNode("SpinBox") as SpinBox);
				// 	double dVal = Convert.ToDouble(Fields[rootVarIdx].FieldValues[relArrayIdx].def);
				// 	SB.Value = Fields[rootVarIdx].valueMultVisual * ( dVal + Fields[rootVarIdx].valueAddVisual );
				// }
				// else
				// // Field is a simple value
				// {
					widgetSimpleValue widgetSimpleValueInst = widgetSimpleValue.Instance() as widgetSimpleValue;
					widgetSimpleValueInst.Name = "WSV" + relArrayIdx.ToString();
					widgetSimpleValueInst.rootVarIdx = rootVarIdx;
					widgetSimpleValueInst.relArrayIdx = relArrayIdx;
					elemNode.AddChild(widgetSimpleValueInst);

					// Display the index if array
					if (Fields[rootVarIdx].FieldValues.Length>1)
						(widgetSimpleValueInst.FindNode("Label_Index") as Label).Text = relArrayIdx.ToString();
					
					// Check for field editing canEdit
					if (!Fields[rootVarIdx].FieldValues[relArrayIdx].canEdit)
						(widgetSimpleValueInst.FindNode("Button_Edit") as Button).Visible = false;

					// Check for plot enabled
					if (!Fields[rootVarIdx].FieldValues[relArrayIdx].canPlot)
						(widgetSimpleValueInst.FindNode("Button_Plot") as Button).Visible = false;
					
					// Display the default FieldValues
					(widgetSimpleValueInst.FindNode("LineEdit_Value") as LineEdit).Text =
										 Fields[rootVarIdx].FieldValues[relArrayIdx].value.ToString();

					// Display the unit
					(widgetSimpleValueInst.FindNode("Unit") as Label).Text =
										 Fields[rootVarIdx].FieldValues[relArrayIdx].unit;
				// }
			}
		}
	}
	private Int32 indexOfPos(UInt32 tab, UInt32 H, UInt32 V)
	{
		Int32 idx;
		for (idx=0; idx<=configData.highestRootIdxVars; idx++)
		{
			if (Fields[idx] != null)
				if (Fields[idx].screenH == H && Fields[idx].screenV == V && Fields[idx].tab == tab)
					break;
		}
		if (idx > configData.highestRootIdxVars)
			idx = -1; // Not found
		return idx;
	}
}




