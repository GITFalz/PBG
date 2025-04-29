using System.Drawing;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class TextureEditor : BaseEditor
{
    // Drawing window
    public Vector2 TextureOffset = new Vector2(0, 0);

    private Vector2i _windowPosition = new Vector2i(11, 66);
    private Vector2i _windowSize = new Vector2i(400, 400);
    private Vector2 _distance = new Vector2(200, -100);
    private Vector2 _canvasPosition = new Vector2(0, 0);

    private bool RenderBrushCircle = true;
    private bool MaskCanvas = false;

    // Color picker
    public int ColorPickerWidth = 300;
    public int ColorPickerHeight = 200;
    public Vector2i ColorPickerPosition = new Vector2i(100, 100);

    private UIController TextureUI = new UIController();

    private UICollection _textureCollection;

    private ColorPicker ColorPicker;

    public TextureEditor()
    {
        ColorPicker = new ColorPicker(ColorPickerWidth, ColorPickerHeight, ColorPickerPosition);

        _ = new DrawingPanel(400, 400);

        _textureCollection = new ("TextureCollection", TextureUI, AnchorType.ScaleFull, PositionType.Absolute, (0, 0, 0), (1, 1), (5, 60, 255, 5), 0);
        
        UIImage BG = new UIImage("TextureEditorBackGround", TextureUI, AnchorType.ScaleFull, PositionType.Absolute, (0.6f, 0.6f, 0.6f, 1f), (0, 0, 0), (0, 0), (0, 0, 0, 0), 0, 11, (10, 0.05f));
        BG.CanTest = true;

        BG.SetOnHover (() => {
            RenderBrushCircle = true;
        });

        _textureCollection.AddElements(BG);

        TextureUI.AddElements(_textureCollection);
    }

    public override void Start(GeneralModelingEditor editor)
    {   
        Started = true;
    }

    public void UpdateColor()
    {
        DrawingPanel.BrushColor = new Vector4(
            Mathf.Lerp(1, ColorPicker.Color.X, ColorPicker.Saturation),
            Mathf.Lerp(1, ColorPicker.Color.Y, ColorPicker.Saturation),
            Mathf.Lerp(1, ColorPicker.Color.Z, ColorPicker.Saturation), 
            1f
        );

        DrawingPanel.BrushColor *= ColorPicker.Brightness;
        DrawingPanel.BrushColor.W = 1f;
    }

    public override void Resize(GeneralModelingEditor editor)
    {
        TextureUI.Resize();
        ColorPicker.Resize();
        ColorPickerPosition.Y += Game.Height - Game.PreviousHeight;
        _windowSize = new Vector2i(Game.Width - 272, Game.Height - 77);
        DrawingPanel.WindowPosition = (_windowPosition.X, Game.Height - (_windowPosition.Y + _windowSize.Y));
        DrawingPanel.WindowWidth = _windowSize.X;
        DrawingPanel.WindowHeight = _windowSize.Y;
    }

    public override void Awake(GeneralModelingEditor editor)
    {
        DrawingPanel.IsDrawing = true;
        _windowSize = new Vector2i(Game.Width - 272, Game.Height - 77);
        DrawingPanel.WindowPosition = (_windowPosition.X, Game.Height - (_windowPosition.Y + _windowSize.Y));
        DrawingPanel.WindowWidth = _windowSize.X;
        DrawingPanel.WindowHeight = _windowSize.Y;
    }

    public override void Update(GeneralModelingEditor editor)
    {
        RenderBrushCircle = false;
        MaskCanvas = false;

        ColorPicker.Update();
        TextureUI.Test();

        Vector2 mousePos = Input.GetMousePosition();

        if (mousePos.X < _windowPosition.X || mousePos.X > _windowPosition.X + _windowSize.X || mousePos.Y < _windowPosition.Y || mousePos.Y > _windowPosition.Y + _windowSize.Y)
            return;

        float delta;
        if (Input.IsKeyDown(Keys.LeftControl))
        {
            delta = Input.GetMouseScrollDelta().Y;
            if (Input.IsKeyDown(Keys.B)) // Brush size
            {
                if (delta != 0)
                {
                    DrawingPanel.BrushSize += delta * GameTime.DeltaTime * 100 * (1 + DrawingPanel.BrushSize);
                    DrawingPanel.BrushSize = Mathf.Clamp(1, 100, DrawingPanel.BrushSize);
                }
            }
            if (Input.IsKeyDown(Keys.F)) // Falloff
            {
                if (delta != 0)
                {
                    DrawingPanel.Falloff += delta * GameTime.DeltaTime * 50 * (1 + DrawingPanel.Falloff);
                    DrawingPanel.Falloff = Mathf.Clamp(0, 5, DrawingPanel.Falloff);
                }
                DrawingPanel.RenderSet = 1;
            }
            if (Input.IsKeyDown(Keys.W)) // brush strength
            {
                if (delta != 0)
                {
                    DrawingPanel.BrushStrength += delta * GameTime.DeltaTime * 10 * (1 + DrawingPanel.BrushStrength);
                    DrawingPanel.BrushStrength = Mathf.Clamp(0, 1, DrawingPanel.BrushStrength);
                }
                DrawingPanel.RenderSet = 2;
            }

            return;
        }

        // Drawing
        delta = Input.GetMouseScrollDelta().Y;
        if (delta != 0)
        {
            DrawingPanel.Zoom(delta * GameTime.DeltaTime * 100 * (1 + DrawingPanel.DrawingCanvasSize)); 
        }

        if (Input.IsKeyPressed(Keys.N))
            DrawingPanel.SetDrawingMode(DrawingMode.None);
        else if (Input.IsKeyPressed(Keys.M))
            DrawingPanel.SetDrawingMode(DrawingMode.Move);
        else if (Input.IsKeyPressed(Keys.E))
            DrawingPanel.SetDrawingMode(DrawingMode.Eraser);
        else if (Input.IsKeyPressed(Keys.B))
            DrawingPanel.SetDrawingMode(DrawingMode.Brush);
        else if (Input.IsKeyPressed(Keys.G))
            DrawingPanel.SetDrawingMode(DrawingMode.Blur);

        if (DrawingPanel.DrawingMode == DrawingMode.Move)
        {
            if (Input.IsMousePressed(MouseButton.Left))
                _distance = mousePos - DrawingPanel.CanvasPosition;

            if (Input.IsMouseDown(MouseButton.Left))
            {
                Vector2 mouseDelta = Input.GetMouseDelta();
                if (mouseDelta != Vector2.Zero)
                {
                    _canvasPosition = mousePos - _distance;
                    DrawingPanel.CanvasPosition.X = (int)_canvasPosition.X;
                    DrawingPanel.CanvasPosition.Y = (int)_canvasPosition.Y;

                    DrawingPanel.SetDrawingCanvasPosition(DrawingPanel.CanvasPosition.X + _windowPosition.X, DrawingPanel.CanvasPosition.Y + (Game.Height - DrawingPanel.WindowHeight) - DrawingPanel.WindowPosition.Y);
                }
            }
        }
    }

    public override void Render(GeneralModelingEditor editor)
    {
        GL.Enable(EnableCap.DepthTest);

        GL.DepthMask(true);

        TextureUI.RenderNoDepthTest();

        if (RenderBrushCircle && !MaskCanvas)
            DrawingPanel.RenderFramebuffer(ColorPicker.Color);

        DrawingPanel.RenderTexture();

        if (RenderBrushCircle && !MaskCanvas && DrawingPanel.DisplayBrushCircle)
            DrawingPanel.RenderBrushCircle(DrawingPanel.WindowPosition, DrawingPanel.WindowWidth, DrawingPanel.WindowHeight);
        ColorPicker.RenderTexture();

        GL.Disable(EnableCap.DepthTest);
    }

    public override void Exit(GeneralModelingEditor editor)
    {
        DrawingPanel.IsDrawing = false;
    }
}