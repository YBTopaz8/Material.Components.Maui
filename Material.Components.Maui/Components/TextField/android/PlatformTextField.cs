using Android.Content;
using Android.Graphics;
using Android.Text;
using Android.Text.Method;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Microsoft.Maui.Graphics.Platform;
using Color = Microsoft.Maui.Graphics.Color;
using PointF = Microsoft.Maui.Graphics.PointF;
using RectF = Microsoft.Maui.Graphics.RectF;

namespace Material.Components.Maui.Platform;

// CRITICAL FIX: Inherit from EditText, not TextView, to guarantee Android keyboard focus!
public class PlatformTextField : EditText
{
    public new event EventHandler<EditTextChangedEventArgs>? TextChanged;
    public event EventHandler<SelectionChangedArgs>? SelectionChanged;

    private readonly IKeyListener? editableKeyListener;

    public PlatformTextField(Context context, IDrawable? drawable = null)
        : base(context, null, Android.Resource.Attribute.EditTextStyle, 0)
    {
        this.scale = this.Resources?.DisplayMetrics is null ? 1 : this.Resources.DisplayMetrics.Density;
        this.canvas = new PlatformCanvas(context);
        this.scalingCanvas = new ScalingCanvas(this.canvas);
        this.drawable = drawable;

        this.Focusable = true;
        this.FocusableInTouchMode = true;
        this.SetTextIsSelectable(true);
        this.SetCursorVisible(false); // We draw our own Material caret
        
        this.editableKeyListener = this.KeyListener;
    }

    public void UpdateDrawable(IDrawable drawable) => this.drawable = drawable;
    public void UpdateText(string? text) => this.Text = text;
    public void UpdateFontSize(float size) => this.SetTextSize(Android.Util.ComplexUnitType.Dip, size);
    public void UpdateTypeface(Typeface typeface) => this.Typeface = typeface;
    public void UpdateSelection(int index) => Selection.SetSelection(this.EditableText, index);
    public void UpdateSelection(int start, int end) => Selection.SetSelection(this.EditableText, start, end);
    public void UpdateTextAlignment(Android.Views.TextAlignment alignment) => this.TextAlignment = alignment;

    public void UpdatePadding(Thickness padding)
    {
        var left = padding.Left * this.scale;
        var top = padding.Top * this.scale;
        var right = padding.Right * this.scale;
        var bottom = padding.Bottom * this.scale;
        this.SetPadding((int)left, (int)top, (int)right, (int)bottom);
    }
    public void UpdateInputType(Primitives.InputType type)
    {
        if (type == Primitives.InputType.None)
        {
            this.InputType = Android.Text.InputTypes.ClassText | Android.Text.InputTypes.TextFlagMultiLine;
            this.SetSingleLine(false);
            this.ImeOptions = ImeAction.None;
            this.TransformationMethod = null;
            this.KeyListener = this.editableKeyListener;
        }
        else
        {
            this.SetSingleLine(true);
            this.ImeOptions = ImeAction.Next | ImeAction.Done;

            switch (type)
            {
                case Primitives.InputType.Numeric:
                    this.InputType = Android.Text.InputTypes.ClassNumber;
                    this.TransformationMethod = null;
                    // OS-level block: Only allow digits 0-9
                    this.KeyListener = Android.Text.Method.DigitsKeyListener.GetInstance("0123456789");
                    break;

                case Primitives.InputType.DecimalNumber:
                    this.InputType = Android.Text.InputTypes.ClassNumber | Android.Text.InputTypes.NumberFlagDecimal;
                    this.TransformationMethod = null;
                    // OS-level block: Only allow digits, dots, and commas (prevents letters entirely!)
                    this.KeyListener = Android.Text.Method.DigitsKeyListener.GetInstance("0123456789.,");
                    break;

                case Primitives.InputType.Password:
                    this.InputType = Android.Text.InputTypes.ClassText | Android.Text.InputTypes.TextVariationPassword;
                    this.TransformationMethod = PasswordTransformationMethod.Instance;
                    this.KeyListener = this.editableKeyListener;
                    break;

                default:
                    this.InputType = Android.Text.InputTypes.ClassText;
                    this.TransformationMethod = null;
                    this.KeyListener = this.editableKeyListener;
                    break;
            }
        }
    }


    //public void UpdateInputType(Primitives.InputType type)
    //{
    //    if (type == Primitives.InputType.None)
    //    {
    //        this.InputType = Android.Text.InputTypes.ClassText | Android.Text.InputTypes.TextFlagMultiLine;
    //        this.SetSingleLine(false);
    //        this.ImeOptions = ImeAction.None;
    //        this.TransformationMethod = null;
    //    }
    //    else
    //    {
    //        this.SetSingleLine(true);
    //        this.ImeOptions = ImeAction.Next | ImeAction.Done;

    //        switch (type)
    //        {
    //            case Primitives.InputType.Numeric:
    //                this.InputType = Android.Text.InputTypes.ClassNumber;
    //                this.TransformationMethod = null;
    //                break;

    //            case Primitives.InputType.DecimalNumber:
    //                this.InputType = Android.Text.InputTypes.ClassNumber | Android.Text.InputTypes.NumberFlagDecimal;
    //                this.TransformationMethod = null;
    //                break;

    //            case Primitives.InputType.Password:
    //                this.InputType = Android.Text.InputTypes.ClassText | Android.Text.InputTypes.TextVariationPassword;
    //                this.TransformationMethod = PasswordTransformationMethod.Instance;
    //                break;

    //            default:
    //                this.InputType = Android.Text.InputTypes.ClassText;
    //                this.TransformationMethod = null;
    //                break;
    //        }
    //    }
    //}
  
    
    public void UpdateIsReadOnly(bool isReadOnly)
    {
        this.KeyListener = isReadOnly ? null : this.editableKeyListener;
    }

    protected override bool DefaultEditable => true;
    public override bool FreezesText { get => true; set => base.FreezesText = value; }

    protected override void OnTextChanged(Java.Lang.ICharSequence? text, int start, int lengthBefore, int lengthAfter)
    {
        base.OnTextChanged(text, start, lengthBefore, lengthAfter);
        int startSel = base.SelectionStart;
        int endSel = base.SelectionEnd;
        this.TextChanged?.Invoke(this, new EditTextChangedEventArgs(this.Text, new(startSel, endSel)));
    }

    protected override void OnSelectionChanged(int selStart, int selEnd)
    {
        base.OnSelectionChanged(selStart, selEnd);
        this.SelectionChanged?.Invoke(this, new(selStart, selEnd));
    }

    public override bool OnCheckIsTextEditor() => true;
    public override void SetText(Java.Lang.ICharSequence? text, BufferType? type) => base.SetText(text, BufferType.Editable);

    //------------------------------------------------------------------------------------------------//

    public Color? BackgroundColor
    {
        get => this.backgroundColor;
        set
        {
            this.backgroundColor = value;
            this.Invalidate();
        }
    }

    int width;
    int height;
    readonly PlatformCanvas canvas;
    readonly ScalingCanvas scalingCanvas;
    IDrawable? drawable;
    readonly float scale = 1;
    Color? backgroundColor;

    IGraphicsView? graphicsView;
    RectF bounds;
    bool dragStarted;
    PointF[] lastMovedViewPoints = [];
    bool pressedContained = false;

    public override void Draw(Canvas androidCanvas)
    {
        if (this.drawable == null) return;

        var dirtyRect = new RectF(0, 0, this.width, this.height);

        this.canvas.Canvas = androidCanvas;
        if (this.backgroundColor != null)
        {
            this.canvas.FillColor = this.backgroundColor;
            this.canvas.FillRectangle(dirtyRect);
            this.canvas.FillColor = Colors.White;
        }

        this.scalingCanvas.ResetState();
        this.scalingCanvas.Scale(this.scale, this.scale);

        dirtyRect.Height /= this.scale;
        dirtyRect.Width /= this.scale;

        this.drawable.Draw(this.scalingCanvas, dirtyRect);
        this.canvas.Canvas = null;
    }

    protected override void OnSizeChanged(int width, int height, int oldWidth, int oldHeight)
    {
        base.OnSizeChanged(width, height, oldWidth, oldHeight);
        this.width = width;
        this.height = height;
    }

    protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
    {
        base.OnLayout(changed, left, top, right, bottom);
        if (changed)
        {
            var width = right - left;
            var height = bottom - top;
            this.bounds = new RectF(0, 0, width / this.scale, height / this.scale);
        }
    }

    public override bool OnTouchEvent(MotionEvent? e)
    {
        bool handled = base.OnTouchEvent(e);

        if (e is null) return handled;



        var touchCount = e.PointerCount;
        var touchPoints = new PointF[touchCount];
        for (var i = 0; i < touchCount; i++)
            touchPoints[i] = new PointF(e.GetX(i) / this.scale, e.GetY(i) / this.scale);

        var actionMasked = e.Action & MotionEventActions.Mask;

        switch (actionMasked)
        {
            case MotionEventActions.Move:
                this.TouchesMoved(touchPoints);
                break;
            case MotionEventActions.Down:
            case MotionEventActions.PointerDown:
                this.TouchesBegan(touchPoints);
                break;
            case MotionEventActions.Up:
            case MotionEventActions.PointerUp:
                //this.TouchesBegan(touchPoints);
                this.TouchesEnded(touchPoints);
                if (!this.dragStarted)
                {
                    this.RequestFocus();
                    var imm = (InputMethodManager?)this.Context?.GetSystemService(Context.InputMethodService);
                    imm?.ShowSoftInput(this, ShowFlags.Implicit);
                }
                break;
            case MotionEventActions.Cancel:
                this.TouchesCanceled();
                break;
        }

        return true;
    }

    public void TouchesBegan(PointF[] points)
    {
        if (points == null || points.Length == 0) return;

        this.dragStarted = false;
        this.lastMovedViewPoints = points;
        this.graphicsView?.StartInteraction(points);
        this.pressedContained = true;
        if (!this.HasFocus)
        {

            this.RequestFocus();
        }
    }

    public void TouchesMoved(PointF[] points)
    {
        if (points == null || points.Length == 0) return;

        if (!this.dragStarted)
        {
            if (this.lastMovedViewPoints != null && this.lastMovedViewPoints.Length > 0 && points.Length == 1)
            {
                var deltaX = this.lastMovedViewPoints[0].X - points[0].X;
                var deltaY = this.lastMovedViewPoints[0].Y - points[0].Y;

                if (Math.Abs(deltaX) <= 3 && Math.Abs(deltaY) <= 3)
                    return;
            }
            else
            {
                this.lastMovedViewPoints = points;
                return;
            }
        }

        this.lastMovedViewPoints = points;
        this.dragStarted = true;
        this.pressedContained = points.Any(this.bounds.Contains);
        this.graphicsView?.DragInteraction(points);
    }

    public void TouchesEnded(PointF[] points)
    {
        this.graphicsView?.EndInteraction(points, this.pressedContained);

        this.dragStarted = false;
        this.lastMovedViewPoints = [];
       
    }

    public void TouchesCanceled()
    {
        this.pressedContained = false;
        this.dragStarted = false;
        this.lastMovedViewPoints = [];
        this.graphicsView?.CancelInteraction();
    }

    public override bool OnHoverEvent(MotionEvent? e)
    {
        base.OnHoverEvent(e);
        if (e is null) return false;

        var touchCount = e.PointerCount;
        var touchPoints = new PointF[touchCount];
        for (var i = 0; i < touchCount; i++)
            touchPoints[i] = new PointF(e.GetX(i) / this.scale, e.GetY(i) / this.scale);

        var actionMasked = e.Action & MotionEventActions.Mask;

        switch (actionMasked)
        {
            case MotionEventActions.HoverMove:
                this.graphicsView?.MoveHoverInteraction(touchPoints);
                break;
            case MotionEventActions.HoverEnter:
                this.graphicsView?.StartHoverInteraction(touchPoints);
                break;
            case MotionEventActions.HoverExit:
                this.graphicsView?.EndHoverInteraction();
                break;
        }

        return true;
    }

    public void Connect(IGraphicsView graphicsView) => this.graphicsView = graphicsView;
    public void Disconnect() => this.graphicsView = null;
}