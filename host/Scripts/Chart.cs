using Godot;
using System;
using CircularBuffer;

public class zoom_t
{
    public Int32 startPt;
    public Int32 pts2Display;
    public float centerData;
    public float[] yValuesPerDiv = new float[13]{1.0f, 2, 5, 10, 20, 50, 100, 200, 500, 1000, 2000, 5000, 10000};
    public UInt32 idxYValuesPerDiv;

    public zoom_t()
    {
        centerData = 0.5f;  // Y origin relative to display Y size
        idxYValuesPerDiv = 6;
        startPt = 0;        // Starting point in the curve data
        pts2Display = 0;    // Number of curve data points displayed,
                            // starting at startPt
    }
}

public class axles
{
    private Node2D parent;

    public float xTimePerDiv;
    public UInt32 nbXDivPrim { get; set; } = 10;
    public UInt32 nbXDivSec { get; set; } = 10;
    public UInt32 nbYDivPrim { get; set; } = 6;
    public UInt32 nbYDivSec { get; set; } = 10;
    public Color originColor { get; set; } = new Color(1,1,1, 0.7F);
    public Color primDivColor { get; set; } = new Color(0.2F,1.0F,0.5F,0.3F);
    public Color secDivColor { get; set; } = new Color(0.5F,0.2F,0.2F,1);

    public axles(Node2D _parent)
    {
        parent = _parent;
    }
    public void setXTimePerDiv(float xTotTime)
    {
        float tmp; float coef = 1.0f;
        // Calculate the division time in 1,2,5,10 units
        tmp = xTotTime / nbXDivPrim;
        while (tmp > 10.0f)
        {
            tmp /= 10.0f;
            coef *= 10.0f;
        }
        while (tmp < 1.0f)
        {
            tmp *= 10.0f;
            coef /= 10.0f;
        }
        if (tmp < 1.5f)
        {
            xTimePerDiv = 1.0f * coef;
        }
        else
        {
            if (tmp < 3.5f)
            {
                xTimePerDiv = 2.0f * coef;
            }
            else
            {
                if (tmp < 7.5f)
                    xTimePerDiv = 5.0f * coef;
                else
                    xTimePerDiv = 10.0f * coef;
            }
        }
    }

    public void draw(zoom_t zoom, Vector2 areaSize, float xTotTime)
    {
        Vector2 ptStart, ptEnd;
        float centerY = zoom.centerData * areaSize.y;
        float stepY = areaSize.y / nbYDivPrim;
        // Y=0
        ptStart.x = 1; 
        ptEnd.x = areaSize.x-1;

        ptStart.y = centerY;
        ptEnd.y = centerY;

        parent.DrawLine(ptStart, ptEnd, originColor, 2);

        // Horizontal
        ptStart.y = centerY + stepY;
        ptEnd.y = centerY + stepY; 
        while(ptStart.y<areaSize.y)
        {
            parent.DrawLine(ptStart, ptEnd, primDivColor);
            ptStart.y += stepY;
            ptEnd.y += stepY;
        }
        ptStart.y = centerY - stepY;
        ptEnd.y = centerY - stepY; 
        while(ptStart.y>0.0f)
        {
            parent.DrawLine(ptStart, ptEnd, primDivColor);
            ptStart.y -= stepY;
            ptEnd.y -= stepY;
        }        

        // Vertical
        float r = xTimePerDiv / xTotTime * areaSize.x;
        ptStart.y = 0;
        ptEnd.y = areaSize.y - 1;
        ptStart.x = 0;
        ptEnd.x = 0;
        while (ptStart.x <= areaSize.x)
        {
            parent.DrawLine(ptStart, ptEnd, primDivColor);
            ptStart.x += r;
            ptEnd.x = ptStart.x;
        } 
    }
}

public class curve
{
    Node2D parent;
    private float minYVal, maxYVal;
    private Int32 dataSize;
    private CircularBuffer<float> data;

    public Color dataColor { get; set; }

    public bool onPlot;
    public bool freezed { get; set; }

    public curve(Node2D _parent)
    {
        dataSize = -1;
        parent = _parent;
        onPlot = false;
    }

    public void addElem(object oY)
    {
        float Y = Convert.ToSingle(oY);
        if (!freezed)
        {
            if (Y > maxYVal) maxYVal = Y;
            if (Y < minYVal) minYVal = Y;

            if (data.IsFull)
                data.PopFront();

            data.PushBack(Y);  
        }
    }

    public void resize(Int32 _size)
    {
        freezed = false;
        dataSize = _size;
        minYVal = Single.MaxValue;
        maxYVal = Single.MinValue;      
        data = new CircularBuffer<float>((int)dataSize);
    }

    public void draw(zoom_t zoom, Vector2 areaSize, UInt32 nbYDivPrim)
    {
        Vector2 plotVal, plotValNext;
        float r;
        float ry =  areaSize.y / (float)nbYDivPrim / zoom.yValuesPerDiv[zoom.idxYValuesPerDiv];

        // Check if Area greater than the number of points to display
        if (areaSize.x > zoom.pts2Display)
        {
            // Yes, then display the line regarding the data number
            r = (float)areaSize.x / (float)zoom.pts2Display;

            // Init
            plotVal.x = 0.0f;
            if (zoom.startPt < data.Size)
                plotVal.y = zoom.centerData * areaSize.y - data[zoom.startPt] * ry;
            else
                plotVal.y = 0.0f;

            for (Int32 i=0; i<zoom.pts2Display-1; i++)
            {
                if (zoom.startPt + (i+1) < data.Size)
                {
                    plotValNext.x = (float)(i+1) * r;
                    plotValNext.y = zoom.centerData * areaSize.y - data[zoom.startPt + (i+1)] * ry;

                    parent.DrawLine(plotVal, plotValNext, dataColor);

                    plotVal = plotValNext;
                }
            }
        }
        else
        {
            // No, then display for each pixel and seek value in data
            r = (float)zoom.pts2Display / (float)areaSize.x;

            // Init
            plotVal.x = 0.0f;
            if (zoom.startPt < data.Size)
                plotVal.y = zoom.centerData * areaSize.y - data[zoom.startPt] * ry;
            else
                plotVal.y = 0.0f;           

            for (Int32 i=0; i<areaSize.x-1; i++)
            {
                if (zoom.startPt + (int)((i+1)*r) < data.Size)
                {
                    plotValNext.x = i+1;
                    plotValNext.y = zoom.centerData * areaSize.y - data[zoom.startPt + (int)((i+1)*r)] * ry;

                    parent.DrawLine(plotVal, plotValNext, dataColor);

                    plotVal = plotValNext;
                }
            }
        }
    }
}

public class Chart : Node2D
{    
    // Classes instances references
    private MainTop mainTopInst;
    private ColorRect colorRectInst;
    private HBoxContainer controlAreaInst;
    private Label windowTimeSelectedInst;
    private Label pointsDisplayedInst;
    private Label XDivInst;
    private Label YDivInst;
    private Label hwSampleTimeInst;
    private Label samplePerElementInst;
    private Label totalTimeDisplayedInst;
    private Button freezeInst;
    private Label nbElementsSampledInst;
    private OptionButton nbPointsPerCurveInst;

    //
    private Random rnd;
    private bool initDone = false;
    //
    private Resource pointer, closedhand;

    public curve[] curves;
    private UInt32 nbCurves;
    private axles axles;

    //
    private zoom_t zoom;
    private UInt32 nbElementsSampled = 1;
    private Int32 nbOfPointsPerCurve;
    private float xTotTime;

    private Vector2 getDrawArea() {
        return colorRectInst.RectSize;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //Instances
        mainTopInst = GetNode<MainTop>("/root/Main");
        colorRectInst = GetNode<ColorRect>("../");
        XDivInst = GetNode<Label>("../XDiv");
        YDivInst = GetNode<Label>("../YDiv"); 
        controlAreaInst = GetNode<HBoxContainer>("../../controlArea");
        windowTimeSelectedInst = controlAreaInst.FindNode("windowTimeSelected") as Label;
        pointsDisplayedInst = controlAreaInst.FindNode("pointsDisplayed") as Label;
        hwSampleTimeInst = controlAreaInst.FindNode("hwSampleTime") as Label;
        samplePerElementInst = controlAreaInst.FindNode("samplePerElement") as Label;
        nbElementsSampledInst = controlAreaInst.FindNode("nbElementsSampled") as Label;
        nbPointsPerCurveInst = controlAreaInst.FindNode("nbPointsPerCurve") as OptionButton;
        totalTimeDisplayedInst = controlAreaInst.FindNode("totalTimeDisplayed") as Label;
        nbPointsPerCurveInst.Connect("item_selected", this, nameof(nbPointsPerCurve_Pressed));
        freezeInst = controlAreaInst.FindNode("freeze") as Button;
        freezeInst.Connect("pressed", this, nameof(freeze_Pressed));

        nbOfPointsPerCurve = 1000;
        // Init drawing parameters
        zoom = new zoom_t();
        axles = new axles(this);
        rnd = new Random();

        pointer = ResourceLoader.Load("res://icons/pointer@1x.png");
    	closedhand = ResourceLoader.Load("res://icons/closedhand.png");

        colorRectInst.RectClipContent = true;
    }
    public override void _Draw()
    {
        float yTot;
        if (initDone)
        {
            yTot = getDrawArea().y;

            // Draw the h and v axles + origin axle
            axles.draw(zoom, getDrawArea(), xTotTime);
            
            // Draw the curves
            for(UInt32 c=0; c<nbCurves; c++)
                if (curves[c].onPlot)
                    curves[c].draw(zoom, getDrawArea(), axles.nbYDivPrim);

            // Display the scale
            // X
            if (xTotTime>1.0f)
                totalTimeDisplayedInst.Text = xTotTime.ToString() + "s";
            else
                totalTimeDisplayedInst.Text = (xTotTime*1000.0f).ToString() + "ms";

            if (axles.xTimePerDiv>1.0f)
                XDivInst.Text = "X: " + axles.xTimePerDiv.ToString() + "s/div";
            else
                XDivInst.Text = "X: " + (axles.xTimePerDiv*1000.0f).ToString() + "ms/div";

            // Y
            YDivInst.Text = "Y: " + zoom.yValuesPerDiv[zoom.idxYValuesPerDiv].ToString() + "/div";
        }
    }
    public override void _Process(float delta)
    {
        Update();
    }

    public void updateDisplayParameters()
    {
        for(UInt32 c=0; c<nbCurves; c++)
            curves[c].resize(nbOfPointsPerCurve);
        // Reinit drawing parameters
        zoom.startPt = 0;
        zoom.pts2Display = nbOfPointsPerCurve;
        zoom.idxYValuesPerDiv = 6;
        xTotTime = mainTopInst.configData.sampleTimeHW * (float)zoom.pts2Display * (float)nbElementsSampled;
        axles.setXTimePerDiv(xTotTime);

        pointsDisplayedInst.Text = zoom.pts2Display.ToString();
        hwSampleTimeInst.Text = (mainTopInst.configData.sampleTimeHW*1000.0f).ToString()+"ms ";
        samplePerElementInst.Text = (mainTopInst.configData.sampleTimeHW*1000.0f*nbElementsSampled).ToString()+"ms ";
    }

    public void nbPointsPerCurve_Pressed(int index)
    {
        nbOfPointsPerCurve = Convert.ToInt32(nbPointsPerCurveInst.Text);
        updateDisplayParameters();
    }
    
    public void setNbElementsSampled(UInt32 elements)
    {
        nbElementsSampledInst.Text = elements.ToString();
        if (elements==0)
            elements = 1;
        nbElementsSampled = elements;
        updateDisplayParameters();
    }
    public void freeze_Pressed()
    {
        if (initDone)
        {
            for(UInt32 c=0; c<nbCurves; c++)
                if (curves[c].onPlot)
                    curves[c].freezed = !curves[c].freezed;
        }
    }

    /*
    * This function is called prior to any drawing attempt
    */
    public void setCurvesNumber(UInt32 _nbCurves)
    {
        nbCurves = _nbCurves;
        curves = new curve[nbCurves];
        for(UInt32 c=0; c<nbCurves; c++)
        {
            curves[c] = new curve(this);
            curves[c].dataColor = new Color((float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble());
            curves[c].onPlot = false;
        }
        updateDisplayParameters();

        // Init done, enable drawing
        initDone = true;
    }

    private bool dragging = false;
    private bool ctrlPressed = false;
    private Vector2 mousePosWhenLeftPressed;
    private Int32 startDataPtWhenLeftPressed;
    private float centerDataWhenLeftPressed;
    public override void _Input(InputEvent inputEvent)
    {
        Vector2 drawRectPos = new Vector2(colorRectInst.GetGlobalRect().Position);
        Vector2 pos = new Vector2();
        Vector2 drawArea = getDrawArea();
        Int32 idxPtFixeData;
        float rx;

        if (inputEvent is InputEventKey keyEvent)
        {
            if (OS.GetScancodeString(keyEvent.Scancode) == "Control")
            {
                ctrlPressed = keyEvent.Pressed;
            }     
        }

        if (inputEvent is InputEventMouseButton mouseEvent)
        {
            // pos relative to drawing area
            pos = mouseEvent.Position - drawRectPos;

            if (pos.x > 0 && pos.y > 0 && pos.x < drawArea.x && pos.y < drawArea.y)
            {
                if (mouseEvent.Pressed)
                    switch ((ButtonList)mouseEvent.ButtonIndex)
                    {
                        case ButtonList.Left:
                            mousePosWhenLeftPressed = pos;
                            startDataPtWhenLeftPressed = zoom.startPt;
                            centerDataWhenLeftPressed = zoom.centerData;
                            dragging = true;
                            Input.SetCustomMouseCursor(closedhand, Input.CursorShape.Arrow,  new Vector2(15, 15));
                            break;
                    }
                else
                    switch ((ButtonList)mouseEvent.ButtonIndex)
                    {
                        case ButtonList.Left:
                            dragging = false;
                            Input.SetCustomMouseCursor(pointer, Input.CursorShape.Arrow,  new Vector2(15, 15));
                            break;

                        case ButtonList.WheelUp:
                            // X axis zoom
                            if (!ctrlPressed)
                            {
                                rx = pos.x/drawArea.x;
                                idxPtFixeData = zoom.startPt + (Int32)(zoom.pts2Display * rx);
                                zoom.pts2Display -= nbOfPointsPerCurve/50;
                                if (zoom.pts2Display < nbOfPointsPerCurve/50)
                                    zoom.pts2Display = nbOfPointsPerCurve/50;

                                xTotTime = mainTopInst.configData.sampleTimeHW * (float)zoom.pts2Display * (float)nbElementsSampled;
                                axles.setXTimePerDiv(xTotTime);
                                zoom.startPt = idxPtFixeData - (Int32)(zoom.pts2Display * rx);
                                if (zoom.startPt < 0)
                                    zoom.startPt = 0;
                    
                                pointsDisplayedInst.Text = zoom.pts2Display.ToString();
                            }
                            else
                            {
                                if (zoom.idxYValuesPerDiv < 12)
                                    zoom.idxYValuesPerDiv++;
                            }
                            break;

                        case ButtonList.WheelDown:
                            // X axis zoom
                            if (!ctrlPressed)
                            {
                                rx = pos.x/drawArea.x;
                                idxPtFixeData = zoom.startPt + (Int32)(zoom.pts2Display * rx);
                                zoom.pts2Display += nbOfPointsPerCurve/50;
                                if (zoom.pts2Display > nbOfPointsPerCurve)
                                    zoom.pts2Display = nbOfPointsPerCurve;

                                xTotTime = mainTopInst.configData.sampleTimeHW * (float)zoom.pts2Display * (float)nbElementsSampled;
                                axles.setXTimePerDiv(xTotTime);
                                zoom.startPt = idxPtFixeData - (Int32)(zoom.pts2Display * rx);
                                if (zoom.startPt < 0)
                                    zoom.startPt = 0;

                                if (zoom.startPt + zoom.pts2Display > nbOfPointsPerCurve-1)
                                {
                                    zoom.startPt = nbOfPointsPerCurve-1 - zoom.pts2Display;
                                    if (zoom.startPt < 0)
                                        zoom.startPt = 0;
                                }
                            }
                            else
                            {
                                if (zoom.idxYValuesPerDiv > 0)
                                    zoom.idxYValuesPerDiv--;
                            }                                                     

                            pointsDisplayedInst.Text = zoom.pts2Display.ToString();
                            break;                        
                    }    
            }  
            else
            {
                if (!mouseEvent.Pressed)
                    switch ((ButtonList)mouseEvent.ButtonIndex)
                    {
                        case ButtonList.Left:
                            dragging = false;
                            Input.SetCustomMouseCursor(pointer, Input.CursorShape.Arrow,  new Vector2(15, 15));
                            break;
                    } 
            } 

        }

        if (inputEvent is InputEventMouseMotion motionEvent && dragging)
        {
            // pos relative to drawing area
            pos = motionEvent.Position - drawRectPos;

            // X Displacement
            zoom.startPt = startDataPtWhenLeftPressed - (int)((pos.x - mousePosWhenLeftPressed.x) * zoom.pts2Display / drawArea.x);

            if (zoom.startPt < 0)
                zoom.startPt = 0;
            
            if (zoom.startPt + zoom.pts2Display > nbOfPointsPerCurve-1)
            {
                zoom.startPt = nbOfPointsPerCurve-1 - zoom.pts2Display;
                if (zoom.startPt < 0)
                    zoom.startPt = 0;
            }

            // Y Displacement
            zoom.centerData = centerDataWhenLeftPressed + (pos.y - mousePosWhenLeftPressed.y)/drawArea.y;
        }
    }
}
