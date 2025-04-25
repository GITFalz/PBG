using System.Drawing;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class TextureEditor : BaseEditor
{
    // Drawing window
    public int WindowWidth = 1000;
    public int WindowHeight = 800;
    public Vector2i WindowPosition = new Vector2i(100, 100);
    private Vector2 _windowPosition = new Vector2i(100, 100);
    private Vector2 _distance = new Vector2(200, -100);
    public Vector2i CanvasPosition = new Vector2i(0, 0);
    private Vector2 _canvasPosition = new Vector2(0, 0);

    private bool RenderBrushCircle = true;
    private bool MaskCanvas = false;

    // Color picker
    public int ColorPickerWidth = 300;
    public int ColorPickerHeight = 200;
    public Vector2i ColorPickerPosition = new Vector2i(100, 100);
    private Vector2 _colorPickerPosition = new Vector2i(100, 100);
    private Vector2 _colorPickerDistance = new Vector2(-600, -100);

    private UIController TextureUI = new UIController();

    private UICollection _textureCollection;
    private UIButton _textureButton;

    private Vector3 BrushColor = new Vector3(1, 0, 0);

    private ColorPicker ColorPicker;

    public TextureEditor()
    {
        ColorPicker = new ColorPicker(ColorPickerWidth, ColorPickerHeight, ColorPickerPosition);
    }

    public override void Start(GeneralModelingEditor editor)
    {   
        Started = true;

        _ = new DrawingPanel(400, 400);

        _textureCollection = new ("TextureCollection", TextureUI, AnchorType.TopLeft, PositionType.Absolute, (0, 0, 0), (1, 1), (94, 80, 0, 0), 0);

        _textureButton = new UIButton("TextureEditorMoveButton", TextureUI, AnchorType.TopLeft, PositionType.Absolute, (0.6f, 0.6f, 0.6f, 1f), (0, 0, 0), (WindowWidth + 12, 14), (0, 0, 0, 0), 0, 10, (5, 0.025f), UIState.Interactable);
        UIImage BG = new UIImage("TextureEditorBackGround", TextureUI, AnchorType.TopLeft, PositionType.Absolute, (0.6f, 0.6f, 0.6f, 1f), (0, 0, 0), (WindowWidth + 12, WindowHeight + 12), (0, 14, 0, 0), 0, 1, (10, 0.05f));
        BG.CanTest = true;

        _textureButton.SetOnClick(() => {
            _distance = Input.GetMousePosition() - _windowPosition;
        });

        _textureButton.SetOnHold(DrawingPanelHold);

        BG.SetOnHover (() => {
            RenderBrushCircle = true;
        });

        _textureCollection.AddElements(_textureButton, BG);

        TextureUI.AddElements(_textureCollection);

        UpdateDrawingPanelPosition();
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

    public void DrawingPanelHold()
    {
        Vector2 mouseDelta = Input.GetMouseDelta();
        if (mouseDelta != Vector2.Zero)
        {
            UpdateDrawingPanelPosition();
        }
    }

    public void UpdateDrawingPanelPosition()
    {
        _windowPosition = Input.GetMousePosition() - _distance;
        WindowPosition.X = (int)_windowPosition.X;
        WindowPosition.Y = (int)-_windowPosition.Y + (Game.Height - WindowHeight);

        _textureCollection.SetOffset((_windowPosition.X - 6, _windowPosition.Y - 20, 0, 0));
        _textureCollection.Align();
        _textureCollection.UpdateTransformation();

        DrawingPanel.SetDrawingCanvasPosition(CanvasPosition.X + _windowPosition.X, CanvasPosition.Y + (Game.Height - WindowHeight) - WindowPosition.Y);
    }

    public override void Resize(GeneralModelingEditor editor)
    {
        TextureUI.Resize();
        ColorPicker.Resize();
        WindowPosition.Y += Game.Height - Game.PreviousHeight;
        ColorPickerPosition.Y += Game.Height - Game.PreviousHeight;
    }

    public override void Awake(GeneralModelingEditor editor)
    {
        DrawingPanel.IsDrawing = true;
        DrawingPanel.SetDrawingCanvasPosition(CanvasPosition.X + _windowPosition.X, CanvasPosition.Y + (Game.Height - WindowHeight) - WindowPosition.Y);
    }

    public override void Update(GeneralModelingEditor editor)
    {
        RenderBrushCircle = false;
        MaskCanvas = false;

        ColorPicker.Update();
        TextureUI.Test();

        if (Input.AreAllKeysDown([Keys.LeftControl, Keys.LeftShift, Keys.R]))
        {
            _windowPosition = (100, 100);
            WindowPosition.X = (int)_windowPosition.X;
            WindowPosition.Y = (int)-_windowPosition.Y + (Game.Height - WindowHeight);

            _textureCollection.SetOffset((_windowPosition.X - 6, _windowPosition.Y - 20, 0, 0));
            _textureCollection.Align();
            _textureCollection.UpdateTransformation();

            DrawingPanel.SetDrawingCanvasPosition(CanvasPosition.X + _windowPosition.X, CanvasPosition.Y + (Game.Height - WindowHeight) - WindowPosition.Y);
        }

        Vector2 mousePos = Input.GetMousePosition();

        if (mousePos.X < _windowPosition.X || mousePos.X > _windowPosition.X + WindowWidth || mousePos.Y < _windowPosition.Y || mousePos.Y > _windowPosition.Y + WindowHeight)
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
            DrawingPanel.ZoomAt(mousePos, delta * GameTime.DeltaTime * 100);
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
                _distance = mousePos - CanvasPosition;

            if (Input.IsMouseDown(MouseButton.Left))
            {
                Vector2 mouseDelta = Input.GetMouseDelta();
                if (mouseDelta != Vector2.Zero)
                {
                    _canvasPosition = mousePos - _distance;
                    CanvasPosition.X = (int)_canvasPosition.X;
                    CanvasPosition.Y = (int)_canvasPosition.Y;

                    DrawingPanel.SetDrawingCanvasPosition(CanvasPosition.X + _windowPosition.X, CanvasPosition.Y + (Game.Height - WindowHeight) - WindowPosition.Y);
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

        DrawingPanel.RenderTexture(WindowPosition, WindowWidth, WindowHeight, CanvasPosition.X, CanvasPosition.Y);

        if (RenderBrushCircle && !MaskCanvas && DrawingPanel.DisplayBrushCircle)
            DrawingPanel.RenderBrushCircle(WindowPosition, WindowWidth, WindowHeight);
        ColorPicker.RenderTexture();

        GL.Disable(EnableCap.DepthTest);
    }

    public override void Exit(GeneralModelingEditor editor)
    {
        DrawingPanel.IsDrawing = false;
    }
}