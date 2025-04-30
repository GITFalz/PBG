using System.Drawing;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class TextureEditor : BaseEditor
{
    public static ShaderProgram UvShader = new ShaderProgram("Uv/Uv.vert", "Uv/Uv.frag");
    public static ShaderProgram UvEdgeShader = new ShaderProgram("Uv/Edge.vert", "Uv/Edge.frag");
    public static ShaderProgram UvVertShader = new ShaderProgram("Uv/Vertex.vert", "Uv/Vertex.frag");

    private Vector2i _windowPosition = new Vector2i(11, 66);
    private Vector2i _windowSize = new Vector2i(400, 400);
    private Vector2 _distance = new Vector2(200, -100);
    private Vector2 _canvasPosition = new Vector2(0, 0);

    private bool RenderBrushCircle = true;
    private bool MaskCanvas = false;

    public UvMesh UvMesh = new();

    // Color picker
    public int ColorPickerWidth = 300;
    public int ColorPickerHeight = 200;
    public Vector2i ColorPickerPosition = new Vector2i(100, 100);

    private UIController TextureUI = new UIController();

    private UICollection _textureCollection;

    private ColorPicker ColorPicker;

    private bool _regenerateColors = true;

    public struct PositionData
    {
        public HashSet<Uv> Uvs;

        public PositionData() { Uvs = []; }
        public PositionData(params Uv[] uvs ) { Uvs = [.. uvs]; }

        public bool Add(Uv uv) => Uvs.Add(uv);
    }
    public Dictionary<Vector2, PositionData> Vertices = [];
    public List<Uv> SelectedVertices = [];

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

        UvMesh.LoadModel("swordUvs");
    }

    public override void Start(GeneralModelingEditor editor)
    {   
        Started = true;
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

        bool ctrl = Input.IsKeyDown(Keys.LeftControl);
        if (Input.IsMousePressed(MouseButton.Left) && !ctrl)
        {
            PositionClickTest();
        }

        float delta;
        if (ctrl)
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

            // Drawing
            delta = Input.GetMouseScrollDelta().Y;
            if (delta != 0)
            {
                DrawingPanel.Zoom(delta * GameTime.DeltaTime * 20 * (1 + DrawingPanel.DrawingCanvasSize)); 
            }

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

            return;
        }

        if (Input.IsKeyPressed(Keys.N))
            DrawingPanel.SetDrawingMode(DrawingMode.None);
        else if (Input.IsKeyPressed(Keys.M))
            DrawingPanel.SetDrawingMode(DrawingMode.Move);
        else if (Input.IsKeyPressed(Keys.E))
            DrawingPanel.SetDrawingMode(DrawingMode.Eraser);
        else if (Input.IsKeyPressed(Keys.B))
            DrawingPanel.SetDrawingMode(DrawingMode.Brush);
        else if (Input.IsKeyPressed(Keys.H))
            DrawingPanel.SetDrawingMode(DrawingMode.Blur);
        else if (Input.IsKeyDown(Keys.G))
            Handle_VertexMovement();

        if (Input.IsKeyReleased(Keys.G))
            _regenerateColors = true;

        if (_regenerateColors)
        {
            UpdatePositionData();
            GenerateVertexColor();
            _regenerateColors = false;
        }
    }

    public void GenerateVertexColor()
    {
        for (int i = 0; i < UvMesh.UvList.Count; i++)
        {
            Uv uv = UvMesh.UvList[i];
            uv.Color = SelectedVertices.Contains(uv) ? (0.25f, 0.3f, 1) : (0f, 0f, 0f);

            var vertexData = UvMesh.Vertices[i];
            vertexData.Color = new Vector4(uv.Color.X, uv.Color.Y, uv.Color.Z, 1);
           UvMesh.Vertices[i] = vertexData;
        }

        for (int i = 0; i < UvMesh.EdgeList.Count; i++)
        {
            var edge = UvMesh.EdgeList[i];
            if (UvMesh.EdgeColors.Count > i*2)
                UvMesh.EdgeColors[i*2] = edge.A.Color;

            if (UvMesh.EdgeColors.Count > i*2 + 1)
                UvMesh.EdgeColors[i*2 + 1] = edge.B.Color;
        }

        UvMesh.UpdateVertexColors();
        UvMesh.UpdateEdgeColors();
    }

    public void UpdatePositionData()
    {
        Vertices.Clear();
        foreach (var triangle in UvMesh.TriangleList)
        {
            if (triangle.Hidden)
                continue;

            foreach (var uv in triangle.GetUvs())
            {
                if (!Vertices.ContainsKey(uv))
                    Vertices.Add(uv, new(uv));
                else
                    Vertices[uv].Add(uv);
            }
        }
    }

    public void PositionClickTest()
    {  
        if (!Input.IsKeyDown(Keys.LeftShift))
            SelectedVertices.Clear();
        
        Vector2 mousePos = Input.GetMousePosition();
        Vector2? closest = null;
        HashSet<Uv>? closestVerts = null;
    
        foreach (var (vert, data) in Vertices)
        {
            Vector2 position = vert * DrawingPanel.CanvasScale + _windowPosition + DrawingPanel.CanvasPosition;
            float distance = Vector2.Distance(mousePos, position);
            float distanceClosest = closest == null ? 1000 : Vector2.Distance(mousePos, (Vector2)closest);
        
            if (distance < distanceClosest && distance < 10)
            {
                closest = position;
                closestVerts = data.Uvs;
            }
        }

        if (closestVerts != null)
        {
            foreach (var uv in closestVerts)
            {
                if (!SelectedVertices.Remove(uv))
                    SelectedVertices.Add(uv);
            }
        }

        GenerateVertexColor();
    }

    public void Handle_VertexMovement()
    {
        Vector2 mouseDelta = Input.GetMouseDelta();
        if (mouseDelta != Vector2.Zero)
        {
            //Console.WriteLine("Updating " + SelectedVertices.Count + " Uvs");
            for (int i = 0; i < SelectedVertices.Count; i++)
            {
                Uv uv = SelectedVertices[i];
                uv.Value += mouseDelta / DrawingPanel.CanvasScale;
            }

            UvMesh.Init();
            UvMesh.UpdateMesh();
        }
    }

    public override void Render(GeneralModelingEditor editor)
    {
        GL.Enable(EnableCap.DepthTest);

        GL.DepthMask(true);

        TextureUI.RenderNoDepthTest();

        bool ctrl = Input.IsKeyDown(Keys.LeftControl);
        if (RenderBrushCircle && !MaskCanvas && ctrl)
                DrawingPanel.RenderFramebuffer(ColorPicker.Color);

        DrawingPanel.RenderTexture();

        if (RenderBrushCircle && !MaskCanvas && DrawingPanel.DisplayBrushCircle && ctrl)
            DrawingPanel.RenderBrushCircle(DrawingPanel.WindowPosition, DrawingPanel.WindowWidth, DrawingPanel.WindowHeight);
        
        GL.Disable(EnableCap.DepthTest);

        GL.Clear(ClearBufferMask.DepthBufferBit);
        GL.Enable(EnableCap.Blend);
        GL.Enable(EnableCap.CullFace);
        GL.FrontFace(FrontFaceDirection.Ccw);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        GL.Viewport(DrawingPanel.WindowPosition.X, DrawingPanel.WindowPosition.Y, DrawingPanel.WindowWidth, DrawingPanel.WindowHeight);

        UvShader.Bind();

        Vector2 position = DrawingPanel.CanvasPosition;
        Matrix4 model = Matrix4.CreateTranslation((position.X, position.Y, 0));
        Matrix4 projection = Matrix4.CreateOrthographicOffCenter(0, DrawingPanel.WindowWidth, DrawingPanel.WindowHeight, 0, -1, 1);
        Vector2 size = DrawingPanel.CanvasScale;
        float colorAlpha = 0.5f;

        int modelLocation = UvShader.GetLocation("model");
        int projectionLocation = UvShader.GetLocation("projection");
        int sizeLocation = UvShader.GetLocation("size");
        int colorAlphaLocation = UvShader.GetLocation("colorAlpha");

        GL.UniformMatrix4(modelLocation, true, ref model);
        GL.UniformMatrix4(projectionLocation, true, ref projection);
        GL.Uniform2(sizeLocation, size);
        GL.Uniform1(colorAlphaLocation, colorAlpha);

        UvMesh.Render();

        UvShader.Unbind();

        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Always);

        model = Matrix4.CreateTranslation((position.X, position.Y, 0));

        UvEdgeShader.Bind();

        modelLocation = UvShader.GetLocation("model");
        projectionLocation = UvEdgeShader.GetLocation("projection");
        sizeLocation = UvEdgeShader.GetLocation("size");

        GL.UniformMatrix4(modelLocation, true, ref model);
        GL.UniformMatrix4(projectionLocation, true, ref projection);
        GL.Uniform2(sizeLocation, size);

        UvMesh.RenderEdges();

        Shader.Error("Rendering edges error: ");

        UvEdgeShader.Unbind();

        UvVertShader.Bind();

        model = Matrix4.CreateTranslation((position.X, position.Y, 0));

        modelLocation = UvVertShader.GetLocation("model");
        projectionLocation = UvVertShader.GetLocation("projection");
        sizeLocation = UvVertShader.GetLocation("size");

        GL.UniformMatrix4(modelLocation, true, ref model);
        GL.UniformMatrix4(projectionLocation, true, ref projection);
        GL.Uniform2(sizeLocation, size);

        UvMesh.RenderVertices();

        Shader.Error("Rendering vertices error: ");

        UvVertShader.Unbind();

        GL.Disable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Lequal);

        GL.Viewport(0, 0, Game.Width, Game.Height);

        ColorPicker.Render();
    }

    public override void Exit(GeneralModelingEditor editor)
    {
        DrawingPanel.IsDrawing = false;
    }
}