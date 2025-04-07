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
    private UICollection _colorPanelCollection;
    private UIButton _colorButton;

    private Vector3 BrushColor = new Vector3(1, 0, 0);



    public override void Start(GeneralModelingEditor editor)
    {   
        Started = true;

        UIMesh uiMesh = TextureUI.uIMesh;

        _ = new DrawingPanel(400, 400);
        _ = new ColorPicker(ColorPickerWidth, ColorPickerHeight);

        _textureCollection = new ("TextureCollection", AnchorType.TopLeft, PositionType.Absolute, (0, 0, 0), (1, 1), (94, 80, 0, 0), 0);

        _textureButton = new UIButton("TextureEditorMoveButton", AnchorType.TopLeft, PositionType.Absolute, (0.6f, 0.6f, 0.6f), (0, 0, 0), (WindowWidth + 12, 14), (0, 0, 0, 0), 0, 10, (5, 0.025f), uiMesh, UIState.Interactable);
        UIImage BG = new UIImage("TextureEditorBackGround", AnchorType.TopLeft, PositionType.Absolute, (0.6f, 0.6f, 0.6f), (0, 0, 0), (WindowWidth + 12, WindowHeight + 12), (0, 14, 0, 0), 0, 1, (10, 0.05f), uiMesh);
        BG.CanTest = true;

        _textureButton.OnClick = new SerializableEvent(() => {
            _distance = Input.GetMousePosition() - _windowPosition;
            _colorButton.CanTest = false;
        });

        _textureButton.OnHold = new SerializableEvent(DrawingPanelHold);

        _textureButton.OnRelease = new SerializableEvent(() => {
            _colorButton.CanTest = true;
        });

        BG.OnHover = new SerializableEvent(() => {
            RenderBrushCircle = true;
        });

        _colorPanelCollection = new ("ColorPanelCollection", AnchorType.TopLeft, PositionType.Absolute, (0, 0, 0), (1, 1), (94, 80, 0, 0), 0);

        _colorButton = new UIButton("ColorEditorMoveButton", AnchorType.TopLeft, PositionType.Absolute, (0.6f, 0.6f, 0.6f), (0, 0, 0), (ColorPickerWidth + 12, 14), (0, 0, 0, 0), 0, 10, (5, 0.025f), uiMesh, UIState.Interactable);
        UIImage ColorBG = new UIImage("ColorPanelBackGround", AnchorType.TopLeft, PositionType.Absolute, (0.6f, 0.6f, 0.6f), (0, 0, 0), (ColorPickerWidth + 12, ColorPickerHeight + 12), (0, 14, 0, 0), 0, 1, (10, 0.05f), uiMesh);
        UIButton ColorPickSlider = new UIButton("ColorPickSlider", AnchorType.TopLeft, PositionType.Absolute, (0.6f, 0.6f, 0.6f), (0, 0, 0), (12, 12), (0, 14, 0, 0), 0, 10, (5, 0.025f), uiMesh, UIState.Interactable);
        UIButton ColorBarSlider = new UIButton("ColorBarSlider", AnchorType.TopLeft, PositionType.Absolute, (0.6f, 0.6f, 0.6f), (0, 0, 0), (12, 12), (0, ColorPickerHeight + 4, 0, 0), 0, 10, (5, 0.025f), uiMesh, UIState.Interactable);

        _colorButton.Depth = 10;
        ColorBG.Depth = 10;
        ColorPickSlider.Depth = 15;
        ColorBarSlider.Depth = 15;
        ColorBG.CanTest = true;

        _colorButton.OnClick = new SerializableEvent(() => {
            _colorPickerDistance = Input.GetMousePosition() - _colorPickerPosition;
            _textureButton.CanTest = false;
        });

        _colorButton.OnHold = new SerializableEvent(ColorPickerHold);

        _colorButton.OnRelease = new SerializableEvent(() => {
            _textureButton.CanTest = true;
        });

        _colorButton.OnHover = new SerializableEvent(() => {
            MaskCanvas = true;
        });

        ColorBG.OnHold = new SerializableEvent(() => 
        {
            float x = Mathf.Clamp(0, ColorPickerWidth, Input.GetMousePosition().X - ColorPickerPosition.X);
            float y = Input.GetMousePosition().Y - (Game.Height - ColorPickerPosition.Y) + ColorPickerHeight;

            if (y > ColorPickerHeight - 20)
            {
                ColorBarSlider.Offset.X = x;
                ColorBarSlider.Align();
                ColorBarSlider.UpdateTransformation();

                float rX = (Input.GetMousePosition().X - ColorPickerPosition.X) / ColorPickerWidth;
                float h = Mathf.Clamp(0, 360, rX * 360);
                
                Vector3 color = (1, 0, 0);

                if (h < 60) color = (1, h / 60, 0);
                else if (h < 120) color = (1 - (h - 60) / 60, 1, 0);
                else if (h < 180) color = (0, 1, (h - 120) / 60);
                else if (h < 240) color = (0, 1 - (h - 180) / 60, 1);
                else if (h < 300) color = ((h - 240) / 60, 0, 1);
                else color = (1, 0, 1 - (h - 300) / 60);

                ColorPicker.Color = color;
            }

            x = Input.GetMousePosition().X - ColorPickerPosition.X;
            y = (Input.GetMousePosition().Y - (Game.Height - ColorPickerPosition.Y) + ColorPickerHeight);

            if (y <= ColorPickerHeight - 20)
            {
                x = Mathf.Clamp(0, ColorPickerHeight - 20, x);
                y = Mathf.Clamp(0, ColorPickerHeight - 20, y);

                ColorPickSlider.Offset.X = x;
                ColorPickSlider.Offset.Y = y + 14;
                ColorPickSlider.Align();
                ColorPickSlider.UpdateTransformation();

                float rX = x / (ColorPickerHeight - 20);
                float rY = y / (ColorPickerHeight - 20);

                ColorPicker.Saturation = rX;
                ColorPicker.Brightness = 1 - rY;
            }

            UpdateColor();
            MaskCanvas = true;
        });

        ColorBG.OnHover = new SerializableEvent(() => {
            MaskCanvas = true;
        });

        _textureCollection.AddElement(_textureButton, BG);
        _colorPanelCollection.AddElement(_colorButton, ColorBG, ColorPickSlider, ColorBarSlider);

        TextureUI.AddElements(_colorPanelCollection, _textureCollection);

        TextureUI.GenerateBuffers();

        UpdateDrawingPanelPosition();
        UpdateColorPickerPosition();
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

    public void ColorPickerHold()
    {
        Vector2 mouseDelta = Input.GetMouseDelta();
        if (mouseDelta != Vector2.Zero)
        {
            UpdateColorPickerPosition();
        }
    }

    public void UpdateColorPickerPosition()
    {
        _colorPickerPosition = Input.GetMousePosition() - _colorPickerDistance;
        ColorPickerPosition.X = (int)_colorPickerPosition.X;
        ColorPickerPosition.Y = (int)-_colorPickerPosition.Y + (Game.Height - ColorPickerHeight);

        _colorPanelCollection.SetOffset((_colorPickerPosition.X - 6, _colorPickerPosition.Y - 20, 0, 0));
        _colorPanelCollection.Align();
        _colorPanelCollection.UpdateTransformation();
    }

    public override void Resize(GeneralModelingEditor editor)
    {
        TextureUI.OnResize();
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

        Shader.Error("after depth test: ");

        GL.DepthMask(true);

        Shader.Error("after depth mask: ");

        Console.WriteLine("Render TextureEditor");
        TextureUI.Render();

        if (RenderBrushCircle && !MaskCanvas)
            DrawingPanel.RenderFramebuffer();

        DrawingPanel.RenderTexture(WindowPosition, WindowWidth, WindowHeight, CanvasPosition.X, CanvasPosition.Y);

        if (RenderBrushCircle && !MaskCanvas && DrawingPanel.DisplayBrushCircle)
            DrawingPanel.RenderBrushCircle(WindowPosition, WindowWidth, WindowHeight);
        ColorPicker.RenderTexture(ColorPickerPosition, ColorPickerWidth, ColorPickerHeight);

        GL.Disable(EnableCap.DepthTest);
    }

    public override void Exit(GeneralModelingEditor editor)
    {
        DrawingPanel.IsDrawing = false;
    }
}