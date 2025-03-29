using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class TextureEditor : BaseEditor
{
    public int WindowWidth = 1000;
    public int WindowHeight = 800;
    public Vector2i WindowPosition = new Vector2i(100, 100);
    private Vector2 _windowPosition = new Vector2i(100, 100);
    private Vector2 _distance = new Vector2(0, 0);
    public Vector2i CanvasPosition = new Vector2i(0, 0);
    private Vector2 _canvasPosition = new Vector2(0, 0);

    private UIController TextureUI = new UIController();

    private UICollection _textureCollection;



    public override void Start(GeneralModelingEditor editor)
    {   
        Started = true;

        UIMesh uiMesh = TextureUI.uIMesh;

        _textureCollection = new ("TextureCollection", AnchorType.TopLeft, PositionType.Absolute, (0, 0, 0), (1000, 800), (94, 80, 0, 0), 0);

        UIButton MoveBarButton = new UIButton("TextureEditorMoveButton", AnchorType.TopLeft, PositionType.Absolute, (0.6f, 0.6f, 0.6f), (0, 0, 0), (1012, 20), (0, 0, 0, 0), 0, 0, (10, 0.05f), uiMesh, UIState.Interactable);
        UIImage BG = new UIImage("TextureEditorBackGround", AnchorType.TopLeft, PositionType.Absolute, (0.6f, 0.6f, 0.6f), (0, 0, 0), (1012, 812), (0, 14, 0, 0), 0, 1, (10, 0.05f), uiMesh);

        MoveBarButton.OnClick = new SerializableEvent(() => {
            _distance = Input.GetMousePosition() - _windowPosition;
        });

        MoveBarButton.OnHold = new SerializableEvent(() => {
            Vector2 mouseDelta = Input.GetMouseDelta();
            if (mouseDelta != Vector2.Zero)
            {
                _windowPosition = Input.GetMousePosition() - _distance;
                WindowPosition.X = (int)_windowPosition.X;
                WindowPosition.Y = (int)-_windowPosition.Y + (Game.Height - WindowHeight);

                _textureCollection.SetOffset((_windowPosition.X - 6, _windowPosition.Y - 20, 0, 0));
                _textureCollection.Align();
                _textureCollection.UpdateTransformation();

                DrawingBuffer.SetDrawingCanvasPosition(CanvasPosition.X + _windowPosition.X, CanvasPosition.Y + (Game.Height - WindowHeight) - WindowPosition.Y);
            }
        });

        _textureCollection.AddElement(MoveBarButton, BG);

        TextureUI.AddElement(_textureCollection);

        TextureUI.GenerateBuffers();
    }

    public override void Resize(GeneralModelingEditor editor)
    {
        TextureUI.OnResize();
        WindowPosition.Y += Game.Height - Game.PreviousHeight;
    }

    public override void Awake(GeneralModelingEditor editor)
    {
        DrawingBuffer.IsDrawing = true;
        DrawingBuffer.SetDrawingCanvasPosition(CanvasPosition.X + _windowPosition.X, CanvasPosition.Y + (Game.Height - WindowHeight) - WindowPosition.Y);
    }

    public override void Update(GeneralModelingEditor editor)
    {
        TextureUI.Test();

        if (Input.AreAllKeysDown([Keys.LeftControl, Keys.LeftShift, Keys.R]))
        {
            _windowPosition = (100, 100);
            WindowPosition.X = (int)_windowPosition.X;
            WindowPosition.Y = (int)-_windowPosition.Y + (Game.Height - WindowHeight);

            _textureCollection.SetOffset((_windowPosition.X - 6, _windowPosition.Y - 20, 0, 0));
            _textureCollection.Align();
            _textureCollection.UpdateTransformation();

            DrawingBuffer.SetDrawingCanvasPosition(CanvasPosition.X + _windowPosition.X, CanvasPosition.Y + (Game.Height - WindowHeight) - WindowPosition.Y);
        }

        Vector2 mousePos = Input.GetMousePosition();

        if (mousePos.X < _windowPosition.X || mousePos.X > _windowPosition.X + WindowWidth || mousePos.Y < _windowPosition.Y || mousePos.Y > _windowPosition.Y + WindowHeight)
            return;

        // Drawing
        float delta = Input.GetMouseScrollDelta().Y;
        if (delta != 0)
        {
            DrawingBuffer.ZoomAt(mousePos, delta * GameTime.DeltaTime * 100);
        }

        if (Input.IsKeyPressed(Keys.N))
            DrawingBuffer.DrawingMode = DrawingMode.None;
        else if (Input.IsKeyPressed(Keys.M))
            DrawingBuffer.DrawingMode = DrawingMode.Move;
        else if (Input.IsKeyPressed(Keys.B))
            DrawingBuffer.DrawingMode = DrawingMode.Brush;

        if (DrawingBuffer.DrawingMode == DrawingMode.Move)
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

                    DrawingBuffer.SetDrawingCanvasPosition(CanvasPosition.X + _windowPosition.X, CanvasPosition.Y + (Game.Height - WindowHeight) - WindowPosition.Y);
                }
            }
        }
    }

    public override void Render(GeneralModelingEditor editor)
    {
        TextureUI.Render();

        DrawingBuffer.RenderTexture(WindowPosition, WindowWidth, WindowHeight, CanvasPosition.X, CanvasPosition.Y);
    }

    public override void Exit(GeneralModelingEditor editor)
    {
        DrawingBuffer.IsDrawing = false;
    }
}