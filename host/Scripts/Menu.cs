using Godot;
using System;
using System.Text;
using System.IO.Ports;

public class Menu : HBoxContainer
{
    // Classes instances
    private MainTop mainTopInst;
    private Chart chartInst;
    private Label labelFooterInfoInst, labelFooterInfoInst2;
    private MenuButton fileMenuInst, openPortMenuInst;
    private PopupMenu openPortMenuElementsInst, fileMenuElementsInst;

    //
    private const int ID_NEW_FILE = 0;
    private const int ID_OPEN_FILE = 1;
    private const int ID_OPEN_FOLDER = 2;
    private const int ID_SAVE = 3;
    private const int ID_EXPORT_PROJECT = 4;
    private const int ID_EXIT = 5;

    private FileDialog openFileDialogInst;

    public static SerialPort port;

    private bool serSendStop = false;

    private bool startupFirstSerialRecieve = true;

    private string[] bufLines = new string[1000];
    private UInt32 flag = 0;
    private UInt32 transmissionErrors = 0;
    private bool portOpennedSinceOneSecond = false;

    private static Texture textOff = ResourceLoader.Load("res://icons/blackButton.png") as Texture;
    private static Texture textGreen = ResourceLoader.Load("res://icons/greenButton.png") as Texture;
    private static Texture textRed = ResourceLoader.Load("res://icons/redButton.png") as Texture;

    public override void _Ready()
    {
        // Seek Chart class instance
        mainTopInst = GetNode<MainTop>("/root/Main");
        chartInst = GetNode<Chart>("../../Node/HBox/HSplit/HSplit/VBox/drawEngine/ColorRect/Chart");
        labelFooterInfoInst = GetNode<Label>("../Footer/LabelFooterInfo");
        labelFooterInfoInst2 = GetNode<Label>("../Footer/LabelFooterInfo2");
        openPortMenuInst = GetNode<MenuButton>("MenuButton_OpenPort");
        openPortMenuElementsInst = openPortMenuInst.GetPopup();
        fileMenuInst = GetNode<MenuButton>("MenuButton_File");
        fileMenuElementsInst = fileMenuInst.GetPopup();
        openFileDialogInst = GetNode<FileDialog>("/root/Main/OpenFileDialog");

        /*
		* FILE MENU
		*/
        // Add the items
        // fileMenu.AddItem("New file", ID_NEW_FILE);
        //fileMenu.AddSeparator();
        fileMenuElementsInst.AddItem("Open .ini file", ID_OPEN_FILE);
        // fileMenu.AddItem("Open folder", ID_OPEN_FOLDER);
        fileMenuElementsInst.AddSeparator();
        // fileMenuElementsInst.AddItem("Save", ID_SAVE);
        // fileMenuElementsInst.AddItem("Export project", ID_EXPORT_PROJECT);
        // fileMenuElementsInst.AddSeparator();
        fileMenuElementsInst.AddItem("Exit", ID_EXIT);
        fileMenuElementsInst.Connect("id_pressed", this, nameof(onFileMenuPressed));

        /*
		* DIALOG PANELS
		*/
        openFileDialogInst.Connect("file_selected", this, nameof(onFileSelected));

        /*
		* OTHER
		*/

        // Port instance
        // port = new SerialPort(portName, MainTop.configData.baudrate, Parity.None, 8, StopBits.One);
        port = new SerialPort();
        port.Parity = Parity.None;
        port.StopBits = StopBits.One;

        // Display on menu available ports
        updatePortListToMenu();
    }

    private void updatePortListToMenu()
    {
        // Scan all available serial ports and put on the first menu
        string[] ports = SerialPort.GetPortNames();

        // Fill the port menu with all the ports found
        var idx = 0;

        openPortMenuElementsInst.Clear();

        if (!port.IsOpen)
        {
            foreach (string port in ports)
                openPortMenuElementsInst.AddItem(port, idx++);
            openPortMenuElementsInst.Connect("id_pressed", this, nameof(onOpenPortPressed));
        }
        else
        {
            // Change to close port
            openPortMenuElementsInst.Clear();
            openPortMenuElementsInst.AddItem("Close", 0);
        }
    }

    private void onOpenPortPressed(int id)
    {
        string portName = openPortMenuElementsInst.GetItemText(id);

        // Port was closed, open it
        if (!port.IsOpen)
        {
            try
            {
                port.PortName = portName;
                port.BaudRate = mainTopInst.configData.baudrate;
                port.Open();
                port.DiscardInBuffer();
                if (port.IsOpen)
                {
                    startupFirstSerialRecieve = true;
                    labelFooterInfoInst.Text = portName + " openned @ " + mainTopInst.configData.baudrate.ToString() + " bauds";
                    updatePortListToMenu();
                }
                else
                {
                    labelFooterInfoInst.Text = "Can't open port";
                }
            }
            catch (Exception ex)
            {
                labelFooterInfoInst.Text = "This port can't be openned (already used?). Error: " + ex.ToString();
            }
        }
        // Port was openned, close it
        else
        {
            try
            {
                port.Close();
                labelFooterInfoInst.Text = "Port closed";
                updatePortListToMenu();
            }
            catch (Exception ex)
            {
                labelFooterInfoInst.Text = "This port can't be openned (already used?). Error: " + ex.ToString();
            }
        }

    }

    private void onFileMenuPressed(int id)
    {
        switch (id)
        {
            case ID_OPEN_FILE:
                openFileDialogInst.PopupCentered();
                break;

            case ID_EXIT:
                // Save the current file
                GetTree().Quit();
                break;
        }
    }

    private void onFileSelected(string file)
    {
        ConfigFile initFile = new ConfigFile();
        Error err = initFile.Load(file);
        MainTop MainTopInst = GetNode<MainTop>("/root/Main");

        // Close port if user decide to change the config file
        port.Close();
        labelFooterInfoInst.Text = "Port closed";
        updatePortListToMenu();

        if (err == Error.FileNotFound)
        {
            GD.Print("Device file " + file + " not found, abording...");
        }
        else
        {
            GD.Print("Device file " + file + " found");

            // Fill the Fields and Commands arrays from the [DEFAULT] [Var...] [Cmd...] sections
            // Do some tests on the keys also
            if (!MainTopInst.testDefault(initFile) && !MainTopInst.testVariables(initFile) && 
                                                                !MainTopInst.testCommands(initFile))
            {
                MainTopInst.fillDefault(initFile);
                MainTopInst.fillVariables(initFile);
                MainTopInst.fillCommands(initFile);

                // Display Fields (Var sections)
                MainTopInst.displayVariables();

                // Display commands (Cmd sections)
                MainTopInst.displayCommands();

                // Display port menu
                openPortMenuInst.Disabled = false;
            }
            else
            {
                // Bad file format, hide port menu
                openPortMenuInst.Disabled = true;
            }
        }
    }

    public override void _Process(float delta)
    {
        if (port != null)
        {
            if (port.IsOpen)
            {
                if (!startupFirstSerialRecieve)
                {
                    if (port.BytesToRead > 0)
                    {
                        parseLines(port.ReadExisting());
                    }
                }
                else
                {
                    port.DiscardInBuffer();
                    startupFirstSerialRecieve = false;
                }
            }
        }
    }

    string relicat = "";
    float debugFloat = 0.0f;
    private void parseLines(string serData)
    {
        string line;
        Int32 startPos, endPos;

        startPos = 0;
        endPos = 0;

        serData = relicat + serData;

        // We don't use split() for an obscure raison of performance (see stackoverflow)
        while (endPos != -1 && startPos != -1)
        {
            startPos = serData.IndexOf(':', startPos);
            if (startPos!=-1)
            {
                endPos = serData.IndexOf('\n', startPos);
                if (endPos!=-1)
                {
                    line = serData.Substring(startPos, endPos-startPos);
                    startPos = endPos;

                    // Get the first character witch is the line meaning
                    if (line.Length > 0)
                    {
                        processReportValue(line);
                    }
                }
            }
        }
        if (startPos!=-1)
            relicat = serData.Substring(startPos);
        else
            relicat = "";
    }

    private void processReportValue(string line)
    {
        byte[] bytes;
        string bigEndian;

        if (line.Length > 2)
        {
            try
            {
                // It's a report value, retrieve the variable index
                Int32 varIdx = int.Parse(line.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);

                // Update the receiving timing per index
                chartUpdateTiming(varIdx);

                if (line.Length > 3)
                {
                    // Check field type from Fields array
                    varType_e varType = getVarType(line.Substring(3, 1));
                    
                    switch (varType)
                    { 
                        case varType_e.eFloat:
                            bytes = new byte[4];
                            bytes[0] = Convert.ToByte(line.Substring(4, 2), 16);
                            bytes[1] = Convert.ToByte(line.Substring(6, 2), 16);
                            bytes[2] = Convert.ToByte(line.Substring(8, 2), 16);
                            bytes[3] = Convert.ToByte(line.Substring(10, 2), 16);
                            float floatValue = BitConverter.ToSingle(bytes, 0);
                            updateValue(varIdx, floatValue);
                            break;

                        case varType_e.eU32:
                            bigEndian = line.Substring(10, 2) + line.Substring(8, 2) + line.Substring(6, 2) + line.Substring(4, 2);
                            UInt32 uint32Value = Convert.ToUInt32(bigEndian , 16);
                            updateValue(varIdx, uint32Value);
                            break;
                        case varType_e.eI32:
                            break;

                        case varType_e.eU16:
                            bigEndian = line.Substring(6, 2) + line.Substring(4, 2);
                            UInt16 uint16Value = Convert.ToUInt16(bigEndian , 16);
                            updateValue(varIdx, uint16Value);
                            break;

                        case varType_e.eI16:
                            bigEndian = line.Substring(6, 2) + line.Substring(4, 2);
                            Int16 int16Value = Convert.ToInt16(bigEndian , 16);
                            updateValue(varIdx, int16Value);                        
                            break;

                        case varType_e.eU8:
                            bigEndian = line.Substring(4, 2);
                            Byte uint8Value = Convert.ToByte(bigEndian , 16);
                            updateValue(varIdx, uint8Value);
                            break;

                        case varType_e.eI8:
                        break;
                        case varType_e.eUnknown:
                        break;

                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                String ff = ex.ToString();
                transmissionErrors++;
                labelFooterInfoInst2.Text = transmissionErrors.ToString();
            }
        }  
    }

    private bool keepThisIndex = true;
    private Int32 indexKeeped = -1;
    private UInt32 elementsCounter = 0;
    private UInt32 lastElementsCounter = 0;
    // Timer for updating widget on field receiving
    private void chartUpdateTiming(Int32 varIdx)
    {
        if (keepThisIndex)
        {
            indexKeeped = varIdx;
            keepThisIndex = false;
            elementsCounter = 0;
            lastElementsCounter = 0;
        }
        else
        {
            elementsCounter++;
            if (varIdx == indexKeeped)
            {
                if (lastElementsCounter != elementsCounter)
                {
                    // Number of elements sampled has changed
                    chartInst.setNbElementsSampled(elementsCounter);
                    lastElementsCounter = elementsCounter;
                }
                elementsCounter = 0;
            }
            // reset in case of indexKeeped unselected
            if (elementsCounter >= 100)
                keepThisIndex = true;
        }
    }

    private void updateValue(Int32 idx, object value)
    {
        Int32 rootVarIdx = mainTopInst.LUTIdx[idx];
        Int32 relArrayIdx = idx - rootVarIdx;

        // Update value on the field
        if (!mainTopInst.Fields[rootVarIdx].FieldValues[relArrayIdx].onEdit)
        {
            mainTopInst.Fields[rootVarIdx].FieldValues[relArrayIdx].value = value;

            // Refresh the display
            if (mainTopInst.Fields[rootVarIdx] != null)
            {
                // Refresh the on monitoring flag
                string basePath = mainTopInst.Fields[rootVarIdx].pathValue;
                widget widgetInst = GetNode<widget>("../../" + basePath + "/WDG");
                widgetInst.setOnMonitoring();

                if (!mainTopInst.Fields[rootVarIdx].FieldValues[relArrayIdx].boolsOnU8)
                {
                    // if (!mainTopInst.Fields[rootVarIdx].FieldValues[relArrayIdx].spinBox)
                    // {
                        string fieldValuePath = "../../" + basePath + "/WSV" + relArrayIdx.ToString() + "/LineEdit_Value";
                        LineEdit lineEditInst = GetNode<LineEdit>(fieldValuePath);
                        lineEditInst.Text = value.ToString();
                        // Add point to curve
                        chartInst.curves[rootVarIdx+relArrayIdx].addElem(value);
                    // }
                    // else
                    // {
                    //     string fieldValuePath = "../../" + basePath + "/WSB" + relArrayIdx.ToString() + "/SpinBox";
                    //     SpinBox spinBoxInst = GetNode<SpinBox>(fieldValuePath);
                    //     double dVal = Convert.ToDouble(value);
                    //     dVal = mainTopInst.Fields[rootVarIdx].valueMultVisual * ( dVal + mainTopInst.Fields[rootVarIdx].valueAddVisual );
                    //     spinBoxInst.Value = dVal;
                    //     // Add point to curve
                    //     chartInst.curves[rootVarIdx+relArrayIdx].addElem(dVal);                        
                    // }
                }
                else
                {
                    // Refresh each bit
                    for (byte bit = 0; bit < 8; bit++)
                    {
                        string color = mainTopInst.Fields[rootVarIdx].FieldValues[relArrayIdx].bits[bit].color;
                        bool bitValue = ((Byte)value & (1 << bit)) != 0;
                        TextureRect texture = GetNode<TextureRect>("../../" + basePath + "/WSV" + relArrayIdx.ToString() + "BIT" + bit.ToString() + "/TextureRect_BitVal");
                        if (bitValue)
                        {
                            //"WSV" + i.ToString() + "BIT" + bit.ToString();
                            if (color=="red")
                                texture.Texture = textRed;
                            else
                                texture.Texture = textGreen;
                        }
                        else
                            texture.Texture = textOff;
                    }
                }
            }
        }
    }

    private varType_e getVarType(string sType)
    {
        foreach (varType_e key in MainTop.fieldVarTypes.Keys)
        {
            if (MainTop.fieldVarTypes[key] == sType)
                return key;
        }
        return varType_e.eUnknown;
    }
}



