using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class AnimationEditor : Updateable
{
    public static Symmetry symmetry = Symmetry.None;

    private Camera camera;
    
    private ShaderProgram _shaderProgram;
    private AnimationMesh _playerMesh;
    private ModelMesh _modelMesh;
    
    private ModelNode node;
    private ModelNode node2;
    private ModelNode node3;
    private ModelNode node4;
    private ModelNode node5;
    private ModelNode node6;
    private ModelNode node7;
    
    //Text
    public StaticText text;

    public StaticText xText;
    public StaticText yText;
    public StaticText zText;
    
    public StaticText symmetryText;
    public string? symmetryString = null;
    
    
    private List<ModelNode> nodes = new List<ModelNode>();
    
    private ModelNode selectedNode;
    
    private GameObject go;
    
    public static bool clickedRotate = false;
    public static bool clickedMove = false;
    private bool saveRotate = false;
    private bool saveMove = false;
    private bool regenerateVertexUi = true;
    
    private bool freeCamera = false;

    private int _selectedModel = 0;
    
    private List<Undoable> undoables = new List<Undoable>();
    
    private ModelGrid modelGrid = ModelGrid.Instance;
    
    public VoxelMesh ModelingHelperMesh = new VoxelMesh();
    public ShaderProgram ModelingHelperShader = new ShaderProgram("Model/ModelHelper.vert", "Model/ModelHelper.frag");
    
    public UIController Ui = new UIController();
    public UIController MainUi = new UIController();
    
    
    // Ui
    public StaticText MeshAlphaText;
    public StaticButton MeshAlphaButton;
    
    public StaticText BackfaceCullingText;
    public StaticButton BackfaceCullingButton;

    public StaticText SnappingText;
    public StaticButton SnappingButton;
    public StaticButton SnappingButtonUp;
    public StaticButton SnappingButtonDown;
    
    // Ui values
    public float MeshAlpha = 0.1f;
    public bool BackfaceCulling = true;
    public bool Snapping = false;
    public float SnappingFactor = 1;
    private int SnappingFactorIndex = 0;
    
    
    
    
    private float _mouseX = 0;
    
    
    
    private List<Vertex> _selectedVertices = new List<Vertex>();

    private static readonly Dictionary<int, float> SnappingFactors = new Dictionary<int, float>()
    {
        { 0, 1f },
        { 1, 0.5f },
        { 2, 0.25f },
        { 3, 0.2f },
        { 4, 0.1f }
    };
    
    
    public override void Awake()
    {
        Console.WriteLine("Animation Editor");
        
        camera.SetCameraMode(CameraMode.Free);
        
        camera.position = new Vector3(0, 0, 5);
        camera.pitch = 0;
        camera.yaw = -90;
        
        camera.UpdateVectors();
        
        camera.SetSmoothFactor(true);
        camera.SetPositionSmoothFactor(true);
        
        //_modelMesh.WorldPosition = transform.Position + new Vector3(0, 0, 0);
    }
    
    public override void Start()
    {
        Console.WriteLine("Animation Editor");
        
        camera = new Camera(Game.width, Game.height, new Vector3(0, 0, 5));
        
        _shaderProgram = new ShaderProgram("Model/Model.vert", "Model/Model.frag");
        
        transform.Position = new Vector3(0, 0, 0);
        
        go = new GameObject();

        _modelMesh = new ModelMesh();

        Vertex vertex1 = new Vertex((0, 0, 0));
        Vertex vertex2 = new Vertex((2, 0, 0));
        Vertex vertex3 = new Vertex((0, 2, 0));
        
        Vertex vertex4 = new Vertex((0, 2, 0));
        Vertex vertex5 = new Vertex((2, 0, 0));
        Vertex vertex6 = new Vertex((2, 2, 0));
        
        vertex2.AddSharedVertex(vertex5);
        vertex3.AddSharedVertex(vertex4);
        
        Triangle triangle = new Triangle(vertex1, vertex2, vertex3);
        Triangle triangle2 = new Triangle(vertex4, vertex5, vertex6);
        
        _modelMesh.AddTriangle(triangle);
        _modelMesh.AddTriangle(triangle2);
        
        _modelMesh.Init();
        _modelMesh.GenerateBuffers();
        _modelMesh.UpdateMesh();


        /*
        node = new ModelNode(new ModelGridCell(node, new Vector3i(1, 1, 1)));
        node.CreateNewHexahedron(new Vector3(1, 1, 1));

        node2 = new ModelNode(new ModelGridCell(node2, new Vector3i(0, 0, 0)));
        node2.CreateNewHexahedron(new Vector3(0, 0, 0));

        node3 = new ModelNode(new ModelGridCell(node3, new Vector3i(1, 0, 1)));
        node3.CreateNewHexahedron(new Vector3(1, 0, 1));

        modelGrid.AddModelNode(node);
        modelGrid.AddModelNode(node2);

        selectedNode = node;

        nodes.Add(node);
        nodes.Add(node2);


        _modelMesh = new ModelMesh();
        _modelMesh.AddModelNode(node);
        _modelMesh.AddModelNode(node2);

        _modelMesh.GenerateBuffers();

        VoxelData.GetEntityBoxMesh(ModelingHelperMesh, (1, 1, 1), (-0.5f, -0.5f, -0.5f), 0);
        // 8 small boxes on each corner of the big box
        VoxelData.GetEntityBoxMesh(ModelingHelperMesh, (0.1f, 0.1f, 0.1f), (-0.55f, -0.55f, -0.55f), 2);
        VoxelData.GetEntityBoxMesh(ModelingHelperMesh, (0.1f, 0.1f, 0.1f), (0.45f, -0.55f, -0.55f), 2);
        VoxelData.GetEntityBoxMesh(ModelingHelperMesh, (0.1f, 0.1f, 0.1f), (0.45f, 0.45f, -0.55f), 2);
        VoxelData.GetEntityBoxMesh(ModelingHelperMesh, (0.1f, 0.1f, 0.1f), (-0.55f, 0.45f, -0.55f), 2);
        VoxelData.GetEntityBoxMesh(ModelingHelperMesh, (0.1f, 0.1f, 0.1f), (-0.55f, -0.55f, 0.45f), 2);
        VoxelData.GetEntityBoxMesh(ModelingHelperMesh, (0.1f, 0.1f, 0.1f), (0.45f, -0.55f, 0.45f), 2);
        VoxelData.GetEntityBoxMesh(ModelingHelperMesh, (0.1f, 0.1f, 0.1f), (0.45f, 0.45f, 0.45f), 2);
        VoxelData.GetEntityBoxMesh(ModelingHelperMesh, (0.1f, 0.1f, 0.1f), (-0.55f, 0.45f, 0.45f), 2);

        ModelingHelperMesh.SetFaceColor(0, 0, 3);

        ModelingHelperMesh.GenerateBuffers();

        text = UI.CreateStaticText("10000", AnchorType.TopLeft, null, null, null);
        xText = UI.CreateStaticText("X", AnchorType.MiddleLeft, null, null, null);
        yText = UI.CreateStaticText("Y", AnchorType.MiddleCenter, null, null, null);
        zText = UI.CreateStaticText("Z", AnchorType.MiddleRight, null, null, null);
        symmetryText = UI.CreateStaticText("Symmetry : None", AnchorType.TopRight, null, null, null);

        StaticPanel panel = UI.CreateStaticPanel(AnchorType.TopLeft, null, new Vector3(190, 520, 0), null, null);

        StaticButton buttonX = UI.CreateStaticButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(170, 50, 0), new Vector4(10, 10, 10, 10), null);
        StaticButton buttonY = UI.CreateStaticButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(170, 50, 0), new Vector4(10, 10, 70, 10), null);
        StaticButton buttonZ = UI.CreateStaticButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(170, 50, 0), new Vector4(10, 10, 130, 10), null);

        buttonX.OnClick += () => { AnimationEditor.clickedRotate = true; };
        buttonY.OnClick += () => { AnimationEditor.clickedRotate = true; };
        buttonZ.OnClick += () => { AnimationEditor.clickedRotate = true; };

        buttonX.OnHold += () => { go.transform.Rotation = Mathf.RotateAround(go.transform.Forward, go.transform.Rotation, MathHelper.DegreesToRadians(_mouseX)); };
        buttonY.OnHold += () => { go.transform.Rotation = Mathf.RotateAround(go.transform.Up, go.transform.Rotation, MathHelper.DegreesToRadians(_mouseX)); };
        buttonZ.OnHold += () => { go.transform.Rotation = Mathf.RotateAround(go.transform.Right, go.transform.Rotation, MathHelper.DegreesToRadians(_mouseX)); };


        StaticButton buttonVert1X = UI.CreateStaticButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(10, 10, 200, 10), null);
        StaticButton buttonVert1Y = UI.CreateStaticButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(70, 10, 200, 10), null);
        StaticButton buttonVert1Z = UI.CreateStaticButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(130, 10, 200, 10), null);

        StaticButton buttonVert2X = UI.CreateStaticButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(10, 10, 240, 10), null);
        StaticButton buttonVert2Y = UI.CreateStaticButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(70, 10, 240, 10), null);
        StaticButton buttonVert2Z = UI.CreateStaticButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(130, 10, 240, 10), null);

        StaticButton buttonVert3X = UI.CreateStaticButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(10, 10, 280, 10), null);
        StaticButton buttonVert3Y = UI.CreateStaticButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(70, 10, 280, 10), null);
        StaticButton buttonVert3Z = UI.CreateStaticButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(130, 10, 280, 10), null);

        StaticButton buttonVert4X = UI.CreateStaticButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(10, 10, 320, 10), null);
        StaticButton buttonVert4Y = UI.CreateStaticButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(70, 10, 320, 10), null);
        StaticButton buttonVert4Z = UI.CreateStaticButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(130, 10, 320, 10), null);

        StaticButton buttonVert5X = UI.CreateStaticButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(10, 10, 360, 10), null);
        StaticButton buttonVert5Y = UI.CreateStaticButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(70, 10, 360, 10), null);
        StaticButton buttonVert5Z = UI.CreateStaticButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(130, 10, 360, 10), null);

        StaticButton buttonVert6X = UI.CreateStaticButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(10, 10, 400, 10), null);
        StaticButton buttonVert6Y = UI.CreateStaticButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(70, 10, 400, 10), null);
        StaticButton buttonVert6Z = UI.CreateStaticButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(130, 10, 400, 10), null);

        StaticButton buttonVert7X = UI.CreateStaticButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(10, 10, 440, 10), null);
        StaticButton buttonVert7Y = UI.CreateStaticButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(70, 10, 440, 10), null);
        StaticButton buttonVert7Z = UI.CreateStaticButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(130, 10, 440, 10), null);

        StaticButton buttonVert8X = UI.CreateStaticButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(10, 10, 480, 10), null);
        StaticButton buttonVert8Y = UI.CreateStaticButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(70, 10, 480, 10), null);
        StaticButton buttonVert8Z = UI.CreateStaticButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(130, 10, 480, 10), null);


        buttonVert1X.OnHover += () =>
        {
            ModelCornerColor(0, 2);
            ModelFaceColor(0, 4, 1, [1, 3]);
        };
        buttonVert1Y.OnHover += () =>
        {
            ModelCornerColor(0, 2);
            ModelFaceColor(0, 4, 1, [2, 4]);
        };
        buttonVert1Z.OnHover += () =>
        {
            ModelCornerColor(0, 2);
            ModelFaceColor(0, 4, 1, [0, 5]);
        };


        buttonVert2X.OnHover += () =>
        {
            ModelCornerColor(1, 2);
            ModelFaceColor(0, 4, 3, [1, 3]);
        };
        buttonVert2Y.OnHover += () =>
        {
            ModelCornerColor(1, 2);
            ModelFaceColor(0, 4, 3, [2, 4]);
        };
        buttonVert2Z.OnHover += () =>
        {
            ModelCornerColor(1, 2);
            ModelFaceColor(0, 4, 3, [0, 5]);
        };


        buttonVert3X.OnHover += () =>
        {
            ModelCornerColor(2, 2);
            ModelFaceColor(0, 2, 3, [1, 3]);
        };

        buttonVert3Y.OnHover += () =>
        {
            ModelCornerColor(2, 2);
            ModelFaceColor(0, 2, 3, [2, 4]);
        };

        buttonVert3Z.OnHover += () =>
        {
            ModelCornerColor(2, 2);
            ModelFaceColor(0, 2, 3, [0, 5]);
        };


        buttonVert4X.OnHover += () =>
        {
            ModelCornerColor(3, 2);
            ModelFaceColor(0, 2, 1, [1, 3]);
        };

        buttonVert4Y.OnHover += () =>
        {
            ModelCornerColor(3, 2);
            ModelFaceColor(0, 2, 1, [2, 4]);
        };

        buttonVert4Z.OnHover += () =>
        {
            ModelCornerColor(3, 2);
            ModelFaceColor(0, 2, 1, [0, 5]);
        };


        buttonVert5X.OnHover += () =>
        {
            ModelCornerColor(4, 2);
            ModelFaceColor(5, 4, 1, [1, 3]);
        };

        buttonVert5Y.OnHover += () =>
        {
            ModelCornerColor(4, 2);
            ModelFaceColor(5, 4, 1, [2, 4]);
        };

        buttonVert5Z.OnHover += () =>
        {
            ModelCornerColor(4, 2);
            ModelFaceColor(5, 4, 1, [0, 5]);
        };


        buttonVert6X.OnHover += () =>
        {
            ModelCornerColor(5, 2);
            ModelFaceColor(5, 4, 3, [1, 3]);
        };

        buttonVert6Y.OnHover += () =>
        {
            ModelCornerColor(5, 2);
            ModelFaceColor(5, 4, 3, [2, 4]);
        };

        buttonVert6Z.OnHover += () =>
        {
            ModelCornerColor(5, 2);
            ModelFaceColor(5, 4, 3, [0, 5]);
        };


        buttonVert7X.OnHover += () =>
        {
            ModelCornerColor(6, 2);
            ModelFaceColor(5, 2, 3, [1, 3]);
        };

        buttonVert7Y.OnHover += () =>
        {
            ModelCornerColor(6, 2);
            ModelFaceColor(5, 2, 3, [2, 4]);
        };

        buttonVert7Z.OnHover += () =>
        {
            ModelCornerColor(6, 2);
            ModelFaceColor(5, 2, 3, [0, 5]);
        };


        buttonVert8X.OnHover += () =>
        {
            ModelCornerColor(7, 2);
            ModelFaceColor(5, 2, 1, [1, 3]);
        };

        buttonVert8Y.OnHover += () =>
        {
            ModelCornerColor(7, 2);
            ModelFaceColor(5, 2, 1, [2, 4]);
        };

        buttonVert8Z.OnHover += () =>
        {
            ModelCornerColor(7, 2);
            ModelFaceColor(5, 2, 1, [0, 5]);
        };


        buttonVert1X.OnClick += () => { AnimationEditor.clickedMove = true; };
        buttonVert1Y.OnClick += () => { AnimationEditor.clickedMove = true; };
        buttonVert1Z.OnClick += () => { AnimationEditor.clickedMove = true; };

        buttonVert2X.OnClick += () => { AnimationEditor.clickedMove = true; };
        buttonVert2Y.OnClick += () => { AnimationEditor.clickedMove = true; };
        buttonVert2Z.OnClick += () => { AnimationEditor.clickedMove = true; };

        buttonVert3X.OnClick += () => { AnimationEditor.clickedMove = true; };
        buttonVert3Y.OnClick += () => { AnimationEditor.clickedMove = true; };
        buttonVert3Z.OnClick += () => { AnimationEditor.clickedMove = true; };

        buttonVert4X.OnClick += () => { AnimationEditor.clickedMove = true; };
        buttonVert4Y.OnClick += () => { AnimationEditor.clickedMove = true; };
        buttonVert4Z.OnClick += () => { AnimationEditor.clickedMove = true; };

        buttonVert5X.OnClick += () => { AnimationEditor.clickedMove = true; };
        buttonVert5Y.OnClick += () => { AnimationEditor.clickedMove = true; };
        buttonVert5Z.OnClick += () => { AnimationEditor.clickedMove = true; };

        buttonVert6X.OnClick += () => { AnimationEditor.clickedMove = true; };
        buttonVert6Y.OnClick += () => { AnimationEditor.clickedMove = true; };
        buttonVert6Z.OnClick += () => { AnimationEditor.clickedMove = true; };

        buttonVert7X.OnClick += () => { AnimationEditor.clickedMove = true; };
        buttonVert7Y.OnClick += () => { AnimationEditor.clickedMove = true; };
        buttonVert7Z.OnClick += () => { AnimationEditor.clickedMove = true; };

        buttonVert8X.OnClick += () => { AnimationEditor.clickedMove = true; };
        buttonVert8Y.OnClick += () => { AnimationEditor.clickedMove = true; };
        buttonVert8Z.OnClick += () => { AnimationEditor.clickedMove = true; };


        buttonVert1X.OnHold += () => { selectedNode.MoveVert(0, (_mouseX, 0, 0)); };
        buttonVert1Y.OnHold += () => { selectedNode.MoveVert(0, (0, _mouseX, 0)); };
        buttonVert1Z.OnHold += () => { selectedNode.MoveVert(0, (0, 0, _mouseX)); };

        buttonVert2X.OnHold += () => { selectedNode.MoveVert(1, (_mouseX, 0, 0)); };
        buttonVert2Y.OnHold += () => { selectedNode.MoveVert(1, (0, _mouseX, 0)); };
        buttonVert2Z.OnHold += () => { selectedNode.MoveVert(1, (0, 0, _mouseX)); };

        buttonVert3X.OnHold += () => { selectedNode.MoveVert(2, (_mouseX, 0, 0)); };
        buttonVert3Y.OnHold += () => { selectedNode.MoveVert(2, (0, _mouseX, 0)); };
        buttonVert3Z.OnHold += () => { selectedNode.MoveVert(2, (0, 0, _mouseX)); };

        buttonVert4X.OnHold += () => { selectedNode.MoveVert(3, (_mouseX, 0, 0)); };
        buttonVert4Y.OnHold += () => { selectedNode.MoveVert(3, (0, _mouseX, 0)); };
        buttonVert4Z.OnHold += () => { selectedNode.MoveVert(3, (0, 0, _mouseX)); };

        buttonVert5X.OnHold += () => { selectedNode.MoveVert(4, (_mouseX, 0, 0)); };
        buttonVert5Y.OnHold += () => { selectedNode.MoveVert(4, (0, _mouseX, 0)); };
        buttonVert5Z.OnHold += () => { selectedNode.MoveVert(4, (0, 0, _mouseX)); };

        buttonVert6X.OnHold += () => { selectedNode.MoveVert(5, (_mouseX, 0, 0)); };
        buttonVert6Y.OnHold += () => { selectedNode.MoveVert(5, (0, _mouseX, 0)); };
        buttonVert6Z.OnHold += () => { selectedNode.MoveVert(5, (0, 0, _mouseX)); };

        buttonVert7X.OnHold += () => { selectedNode.MoveVert(6, (_mouseX, 0, 0)); };
        buttonVert7Y.OnHold += () => { selectedNode.MoveVert(6, (0, _mouseX, 0)); };
        buttonVert7Z.OnHold += () => { selectedNode.MoveVert(6, (0, 0, _mouseX)); };

        buttonVert8X.OnHold += () => { selectedNode.MoveVert(7, (_mouseX, 0, 0)); };
        buttonVert8Y.OnHold += () => { selectedNode.MoveVert(7, (0, _mouseX, 0)); };
        buttonVert8Z.OnHold += () => { selectedNode.MoveVert(7, (0, 0, _mouseX)); };

        Ui.AddStaticPanel(panel);

        Ui.SetParentPanel(panel);

        Ui.AddStaticText(symmetryText);

        Ui.AddStaticButton(buttonX);
        Ui.AddStaticButton(buttonY);
        Ui.AddStaticButton(buttonZ);

        Ui.AddStaticButton(buttonVert1X);
        Ui.AddStaticButton(buttonVert1Y);
        Ui.AddStaticButton(buttonVert1Z);

        Ui.AddStaticButton(buttonVert2X);
        Ui.AddStaticButton(buttonVert2Y);
        Ui.AddStaticButton(buttonVert2Z);

        Ui.AddStaticButton(buttonVert3X);
        Ui.AddStaticButton(buttonVert3Y);
        Ui.AddStaticButton(buttonVert3Z);

        Ui.AddStaticButton(buttonVert4X);
        Ui.AddStaticButton(buttonVert4Y);
        Ui.AddStaticButton(buttonVert4Z);

        Ui.AddStaticButton(buttonVert5X);
        Ui.AddStaticButton(buttonVert5Y);
        Ui.AddStaticButton(buttonVert5Z);

        Ui.AddStaticButton(buttonVert6X);
        Ui.AddStaticButton(buttonVert6Y);
        Ui.AddStaticButton(buttonVert6Z);

        Ui.AddStaticButton(buttonVert7X);
        Ui.AddStaticButton(buttonVert7Y);
        Ui.AddStaticButton(buttonVert7Z);

        Ui.AddStaticButton(buttonVert8X);
        Ui.AddStaticButton(buttonVert8Y);
        Ui.AddStaticButton(buttonVert8Z);
        */

        StaticPanel panel = UI.CreateStaticPanel(AnchorType.TopLeft, PositionType.Absolute, 
            new Vector3(300, 500, 0), new Vector4(5, 5, 5, 5), 
            null);
        
        MeshAlphaText = UI.CreateStaticText("alpha: " + MeshAlpha.ToString("F2"), 0.7f, AnchorType.TopLeft, PositionType.Absolute, 
            null, new Vector4(15, 10, 15, 10));
        MeshAlphaButton = UI.CreateStaticButton(AnchorType.TopRight, PositionType.Relative, 
            new Vector3(50, 20, 0), new Vector4(150, 10, 10f, 10), null);
        MeshAlphaButton.TextureIndex = 0;
        
        BackfaceCullingText = UI.CreateStaticText("culling: " + BackfaceCulling, 0.7f, AnchorType.TopLeft, PositionType.Absolute, 
            null, new Vector4(15, 10, 40, 10));
        BackfaceCullingButton = UI.CreateStaticButton(AnchorType.TopRight, PositionType.Relative, 
            new Vector3(50, 20, 0), new Vector4(150, 10, 35, 10), null);
        BackfaceCullingButton.TextureIndex = 0;

        SnappingText = UI.CreateStaticText("snap: " + Snapping, 0.7f, AnchorType.TopLeft, PositionType.Absolute, 
            null, new Vector4(15, 10, 65, 10));
        SnappingButtonUp = UI.CreateStaticButton(AnchorType.TopRight, PositionType.Relative,
            new Vector3(20, 20, 0), new Vector4(90, 10, 60, 10), null);
        SnappingButtonDown = UI.CreateStaticButton(AnchorType.TopRight, PositionType.Relative,
            new Vector3(20, 20, 0), new Vector4(120, 10, 60, 10), null);
        SnappingButton = UI.CreateStaticButton(AnchorType.TopRight, PositionType.Relative,
            new Vector3(50, 20, 0), new Vector4(150, 10, 60, 10), null);
        SnappingButtonUp.TextureIndex = 0;
        SnappingButtonDown.TextureIndex = 0;
        SnappingButton.TextureIndex = 0;
        
        
        
        MeshAlphaButton.OnHold += () =>
        {
            float mouseX = Input.GetMouseDelta().X;
            if (mouseX == 0)
                return;
            
            MeshAlpha += mouseX * GameTime.DeltaTime * 0.2f;
            MeshAlpha = Mathf.Clamp(0, 1, MeshAlpha);
            MeshAlphaText.SetText("alpha: " + MeshAlpha.ToString("F2"));
            MeshAlphaText.Generate();
            MainUi.Update();
        };
        
        BackfaceCullingButton.OnClick += () =>
        {
            BackfaceCulling = !BackfaceCulling;
            BackfaceCullingText.SetText("culling: " + BackfaceCulling);
            BackfaceCullingText.Generate();
            MainUi.Update();
        };

        SnappingButtonUp.OnClick += () =>
        {
            if (SnappingFactorIndex + 1 < SnappingFactors.Count)
            {
                SnappingFactorIndex++;
                SnappingFactor = SnappingFactors[SnappingFactorIndex];
            }
        };
        
        SnappingButtonDown.OnClick += () =>
        {
            if (SnappingFactorIndex > 0)
            {
                SnappingFactorIndex--;
                SnappingFactor = SnappingFactors[SnappingFactorIndex];
            }
        };

        SnappingButton.OnClick += () =>
        {
            Snapping = !Snapping;
            if (Snapping)
                SnappingText.SetText("snap: " + SnappingFactor.ToString("F2"));
            else
                SnappingText.SetText("snap: off");
            SnappingText.Generate();
            MainUi.Update();
        };
        
        MainUi.AddStaticPanel(panel);
        MainUi.AddStaticText(MeshAlphaText);
        MainUi.AddStaticText(BackfaceCullingText);
        MainUi.SetParentPanel(panel);
        MainUi.AddStaticButton(MeshAlphaButton);
        MainUi.AddStaticButton(BackfaceCullingButton);
        MainUi.AddStaticButton(SnappingButton);
        MainUi.AddStaticButton(SnappingButtonUp);
        MainUi.AddStaticButton(SnappingButtonDown);
        
        
        MainUi.Generate();
        Ui.Generate();
    }
    
    public override void Update()
    {
        /*
        ModelingHelperMesh.ResetColor();
        
        _mouseX = InputManager.GetMouseDelta().X * GameTime.DeltaTime * 50;
        Ui.TestButtons();
        
        if (InputManager.IsDown(Keys.P))
        {
            Symmetry oldSymmetry = symmetry;
            
            if (InputManager.IsKeyPressed(Keys.D1))
                symmetry = Symmetry.None;
            if (InputManager.IsKeyPressed(Keys.D2))
                symmetry = Symmetry.X;
            if (InputManager.IsKeyPressed(Keys.D3))
                symmetry = Symmetry.Y;
            if (InputManager.IsKeyPressed(Keys.D4))
                symmetry = Symmetry.Z;
            if (InputManager.IsKeyPressed(Keys.D5))
                symmetry = Symmetry.XY;
            if (InputManager.IsKeyPressed(Keys.D6))
                symmetry = Symmetry.XZ;
            if (InputManager.IsKeyPressed(Keys.D7))
                symmetry = Symmetry.YZ;
            if (InputManager.IsKeyPressed(Keys.D8))
                symmetry = Symmetry.XYZ;

            if (oldSymmetry != symmetry)
                symmetryString = symmetry.ToString();
        }

        if (InputManager.IsKeyPressed(Keys.H))
        {
            nodes.Add(node3);
            modelGrid.AddModelNode(node3);
            _modelMesh.AddModelNode(node3);
            _modelMesh.GenerateBuffers();
        }
        
        if (InputManager.IsKeyPressed(Keys.O))
        {
            _modelMesh.ChangeModelColor(ref _selectedModel, 0);
            _selectedModel++;
            _modelMesh.ChangeModelColor(ref _selectedModel, 1);
            selectedNode = nodes[_selectedModel];
        }
        
        if (Game.cameraMove)
        {
            if (saveRotate)
            {
                Console.WriteLine("Save Rotate");
                saveRotate = false;
                //undoables.Add(new AngleUndo(new Vector3(oldAngleX, oldAngleY, oldAngleZ)));
            }
            
            if (saveMove)
            {
                Console.WriteLine("Save Move");
                saveMove = false;
                undoables.Add(new ModelUndo(node.Copy()));
            }

            if (InputManager.IsDown(Keys.LeftControl) && InputManager.IsKeyPressed(Keys.W))
            {
                /*
                if (undoables.Count > 0)
                {
                    Console.WriteLine("Undo");
                    
                    Undoable undoable = undoables[^1];
                    undoables.RemoveAt(undoables.Count - 1);

                    if (undoable is AngleUndo angleUndo)
                    {
                        go.transform.Rotation = Mathf.RotateAround(go.transform.Forward, go.transform.Rotation, MathHelper.DegreesToRadians(angleUndo.OldAngle.X - oldAngleX));
                        go.transform.Rotation = Mathf.RotateAround(go.transform.Right, go.transform.Rotation, MathHelper.DegreesToRadians(angleUndo.OldAngle.Z - oldAngleZ));
                        go.transform.Rotation = Mathf.RotateAround(go.transform.Up, go.transform.Rotation, MathHelper.DegreesToRadians(angleUndo.OldAngle.Y - oldAngleY));
                        
                        UIManager.x = angleUndo.OldAngle.X;
                        UIManager.y = angleUndo.OldAngle.Y;
                        UIManager.z = angleUndo.OldAngle.Z;
                        
                        oldAngleX = angleUndo.OldAngle.X;
                        oldAngleY = angleUndo.OldAngle.Y;
                        oldAngleZ = angleUndo.OldAngle.Z;
                    }

                    if (undoable is ModelUndo modelUndo)
                    {
                        node = modelUndo.OldNode.Copy();
                    }
                }
                else
                {
                    go.transform.Rotation = Quaternion.Identity;
                    node.CreateNewHexahedron((0, 0, 0));
                }
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                _modelMesh.UpdateNode(i, nodes[i]);
            }
            _modelMesh.Init();
            _modelMesh.UpdateRotation(go.transform.Rotation);
            _modelMesh.Center();
            _modelMesh.UpdateMesh();
        }
        else if (InputManager.IsMouseDown(MouseButton.Left))
        {
            float mouseX = InputManager.GetMouseDelta().X;
            
            Camera.yaw += mouseX * GameTime.DeltaTime * Camera.SENSITIVITY;
            Camera.UpdateVectors();
        }
        
        ModelingHelperMesh.WorldPosition = new Vector3(0, 0, -3);
        
        ModelingHelperMesh.Init();
        ModelingHelperMesh.UpdateRotation(new Quaternion(0, MathHelper.DegreesToRadians(Camera.yaw + 90), 0));
        ModelingHelperMesh.UpdateRotation(new Quaternion(MathHelper.DegreesToRadians(20), 0, 0));
        ModelingHelperMesh.Center();
        ModelingHelperMesh.UpdateTextureIndices();
        ModelingHelperMesh.UpdateMesh();
        
        
        if (symmetryString != null)
        {
            int length = symmetryString.Length;
            for (int i = 0; i < 4 - length; i++)
            {
                symmetryString += " ";
            }
            symmetryText.SetText("Symmetry : " + symmetryString);
            symmetryText.Generate();
            symmetryText.UpdateText();
            symmetryString = null;
        }
        
        base.Update();
        */
        
        MainUi.TestButtons();

        if (Input.IsKeyPressed(Keys.Escape))
        {
            freeCamera = !freeCamera;
            
            if (freeCamera)
            {
                Game.Instance.CursorState = CursorState.Grabbed;
                camera.firstMove = true;
            }
            else
            {
                Game.Instance.CursorState = CursorState.Normal;
            }
        }

        if (Input.IsKeyPressed(Keys.Delete))
        {
            _selectedVertices.Clear();
            GenerateVertexColor();
        }
        
        if (freeCamera)
        {
            camera.Update();
        }

        if (Input.IsKeyPressed(Keys.F))
        {
            if (_selectedVertices.Count == 3)
            {
                Vertex s1 = _selectedVertices[0];
                Vertex s2 = _selectedVertices[1];
                Vertex s3 = _selectedVertices[2];
                
                Vertex v1 = new Vertex(s1);
                Vertex v2 = new Vertex(s2);
                Vertex v3 = new Vertex(s3);
                
                Triangle triangle = new Triangle(v1, v2, v3);
                
                s1.AddSharedVertexToAll(s1.ToList(), v1);
                s2.AddSharedVertexToAll(s2.ToList(), v2);
                s3.AddSharedVertexToAll(s3.ToList(), v3);
                
                _modelMesh.AddTriangle(triangle);
                
                _modelMesh.Init();
                _modelMesh.GenerateBuffers();
                _modelMesh.UpdateMesh();
            }
        }

        if (Input.IsKeyPressed(Keys.I))
        {
            HashSet<Triangle> triangles = new HashSet<Triangle>();
            
            foreach (var vert in _selectedVertices)
            {
                Triangle triangle;
                
                foreach (var svert in vert.SharedVertices)
                {
                    if (svert.ParentTriangle == null || triangles.Contains(svert.ParentTriangle))
                        continue;

                    triangle = svert.ParentTriangle;

                    if (
                        SelectedContainsSharedVertex(triangle.A) &&
                        SelectedContainsSharedVertex(triangle.B) &&
                        SelectedContainsSharedVertex(triangle.C)
                    )
                    {
                        Vertex A = triangle.A;
                        Vertex B = triangle.B;
                        if (_modelMesh.SwapVertices(A, B))
                        {
                            triangle.Invert();
                            _modelMesh.UpdateNormals(triangle);
                        }
                    }

                    triangles.Add(triangle);
                }
                
                if (vert.ParentTriangle == null || triangles.Contains(vert.ParentTriangle))
                    continue;

                triangle = vert.ParentTriangle;

                if (
                    SelectedContainsSharedVertex(triangle.A) &&
                    SelectedContainsSharedVertex(triangle.B) &&
                    SelectedContainsSharedVertex(triangle.C)
                )
                {
                    Vertex A = triangle.A;
                    Vertex B = triangle.B;
                    if (_modelMesh.SwapVertices(A, B))
                    {
                        triangle.Invert();
                        _modelMesh.UpdateNormals(triangle);
                    }
                }

                triangles.Add(triangle);
            }
            
            _modelMesh.Init();
            _modelMesh.UpdateMesh();
        }

        if (Input.IsKeyDown(Keys.LeftControl))
        {
            // Merging
            if (Input.IsKeyPressed(Keys.K) && _selectedVertices.Count >= 2)
            {
                Console.WriteLine("Merging verts");
                
                Vertex s1 = _selectedVertices[0];
                Vector3 position = s1.Position;

                HashSet<Vertex> deletedVertices = new HashSet<Vertex>();
                
                for (int i = 1; i < _selectedVertices.Count; i++)
                {
                    Vertex vert = _selectedVertices[i];
                    s1.AddSharedVertexToAll(s1.ToList(), vert);
                    position += vert.Position;
                    if (
                        vert.GetTwoOtherVertex(out var a, out var b) && (
                        SelectedContainsSharedVertex(a) || 
                        SelectedContainsSharedVertex(b)
                    )) {
                        Console.WriteLine("Remove Triangle");
                        _modelMesh.RemoveTriangle(vert.ParentTriangle);
                        deletedVertices.Add(vert);
                    }
                }
                
                position /= _selectedVertices.Count;
                s1.SetAllPosition(position);

                foreach (var vert in deletedVertices)
                {
                    _selectedVertices.Remove(vert);
                }
                
                for (int i = 1; i < _selectedVertices.Count; i++)
                {
                    _selectedVertices[i].AddSharedVertexToAll(_selectedVertices[i].ToList(), s1);
                    _selectedVertices[i].SetAllPosition(position);
                }
                
                _selectedVertices.Clear();
                
                _modelMesh.Init();
                _modelMesh.GenerateBuffers();
                _modelMesh.UpdateMesh();
                
                Ui.Clear();
                
                regenerateVertexUi = true;
            }
        }
        
        if (Input.IsKeyPressed(Keys.E))
        {
            Console.WriteLine("Extruding verts");
            
            Ui.Clear();
            
            if (_selectedVertices.Count < 2)
                return;
            
            Vertex s1 = _selectedVertices[0];
            Vertex s2 = _selectedVertices[1];

            Vertex v1 = new Vertex(s1);
            Vertex v2 = new Vertex(s1);
            Vertex v3 = new Vertex(s2);
            Vertex v4 = new Vertex(s2);

            Quad quad = new Quad(v1, v2, v3, v4);
            
            Console.WriteLine(quad);
            
            _modelMesh.AddTriangle(quad.A);
            _modelMesh.AddTriangle(quad.B);
            
            _modelMesh.Init();
            _modelMesh.GenerateBuffers();
            _modelMesh.UpdateMesh();
            
            _selectedVertices.Clear();

            s1.AddSharedVertexToAll(s1.ToList(), v1);
            s2.AddSharedVertexToAll(s2.ToList(), v3);
            
            _selectedVertices.Add(v2);
            _selectedVertices.Add(v4);
        }

        if (Input.IsKeyPressed(Keys.G))
        {
            Ui.Clear();
        }
        
        if (Input.IsKeyDown(Keys.E) || Input.IsKeyDown(Keys.G))
        {
            Vector2 mouseDelta = Input.GetMouseDelta() * (GameTime.DeltaTime * 10);
            Vector3 move = camera.right * mouseDelta.X + camera.up * -mouseDelta.Y;

            if (Input.AreKeysDown(out int index, Keys.X, Keys.C, Keys.V))
                move *= AxisIgnore[index];
            
            if (Input.IsKeyDown(Keys.LeftShift))
            {
                //Assuming that the keys are present in the dictionary
                float step;
                if (!Input.AreKeysDown(out Keys? key, Keys.D1, Keys.D2, Keys.D3, Keys.D4))
                    step = 1f;
                else
                    step = StepDictionary[(Keys)key!];

                MoveStepSelectedVertices(move, step);
            }
            else
            {
                MoveSelectedVertices(move);
            }
            
            _modelMesh.RecalculateNormals();
            _modelMesh.Init();
            _modelMesh.UpdateMesh();
        }

        if (Input.IsKeyReleased(Keys.G) || Input.IsKeyReleased(Keys.E))
        {
            regenerateVertexUi = true;
        }
        
        //Generate panels on top of eahc vertex
        if (freeCamera && !regenerateVertexUi)
        {
            Console.WriteLine("Clear Vertex UI");
            Ui.Clear();
            regenerateVertexUi = true;
        }
        
        if (!freeCamera)
        {
            if (regenerateVertexUi)
            {
                Console.WriteLine("Regenerate Vertex UI");
                GenerateVertexPanels();
                regenerateVertexUi = false;
            }
            
            if (Input.IsMousePressed(MouseButton.Left))
            {
                Vector2 mousePos = Input.GetMousePosition();
                Vector2? closest = null;
                Vertex? closestVert = null;
            
                System.Numerics.Matrix4x4 projection = camera.GetNumericsProjectionMatrix();
                System.Numerics.Matrix4x4 view = camera.GetNumericsViewMatrix();
            
                foreach (var vert in _modelMesh.VertexList)
                {
                    Vector2? screenPos = Mathf.WorldToScreen(vert.Position, projection, view);
                    if (screenPos == null)
                        continue;
                    float distance = Vector2.Distance(mousePos, (Vector2)screenPos);
                    float distanceClosest = closest == null ? 1000 : Vector2.Distance(mousePos, (Vector2)closest);
                
                    if (distance < distanceClosest && distance < 10)
                    {
                        closest = screenPos;
                        closestVert = vert;
                    }
                }

                if (closestVert != null && !_selectedVertices.Remove(closestVert))
                    _selectedVertices.Add(closestVert);
                
                GenerateVertexColor();
            }
        }
    }

    public override void Render()
    {
        if (BackfaceCulling)
            GL.Enable(EnableCap.CullFace);
        else
            GL.Disable(EnableCap.CullFace);
        
        GL.Disable(EnableCap.DepthTest);
        
        _shaderProgram.Bind();
        
        MirrorRender(new Vector3(1, 1, 1));
        //MirrorRender(new Vector3(-1, 1, 1));
        
        //_playerMesh.RenderMesh();
        //_modelMesh.RenderMesh();
        
        _shaderProgram.Unbind();
        
        /*
        GL.Viewport(-70, -30, 320, 220);
        
        ModelingHelperShader.Bind();

        model = Matrix4.CreateTranslation(0, 0, 0);
        view = Matrix4.LookAt(new Vector3(0, 0, 0), new Vector3(0, 0, -1f), Vector3.UnitY);
        projection = Matrix4.CreatePerspectiveFieldOfView(
            MathHelper.DegreesToRadians(60),
            160f / 110f, 
            0.1f, 
            1000f
        );
        
        int modelModelLoc = GL.GetUniformLocation(ModelingHelperShader.ID, "model");
        int modelViewLoc = GL.GetUniformLocation(ModelingHelperShader.ID, "view");
        int modelProjectionLoc = GL.GetUniformLocation(ModelingHelperShader.ID, "projection");
        
        GL.UniformMatrix4(modelModelLoc, true, ref model);
        GL.UniformMatrix4(modelViewLoc, true, ref view);
        GL.UniformMatrix4(modelProjectionLoc, true, ref projection);
        
        ModelingHelperMesh.RenderMesh();
        
        ModelingHelperShader.Unbind();
        
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        
        GL.Viewport(0, 0, Game.width, Game.height);
        */
        
        Ui.Render();
        MainUi.Render();
        
        base.Render();
    }

    public void MirrorRender(Vector3 flipping)
    {
        Matrix4 model = Matrix4.CreateScale(flipping);
        Matrix4 view = camera.GetViewMatrix();
        Matrix4 projection = camera.GetProjectionMatrix();

        int modelLocation = GL.GetUniformLocation(_shaderProgram.ID, "model");
        int viewLocation = GL.GetUniformLocation(_shaderProgram.ID, "view");
        int projectionLocation = GL.GetUniformLocation(_shaderProgram.ID, "projection");
        int colorAlphaLocation = GL.GetUniformLocation(_shaderProgram.ID, "colorAlpha");
        
        GL.UniformMatrix4(modelLocation, true, ref model);
        GL.UniformMatrix4(viewLocation, true, ref view);
        GL.UniformMatrix4(projectionLocation, true, ref projection);
        GL.Uniform1(colorAlphaLocation, MeshAlpha);
        
        _modelMesh.RenderMesh();
    }

    public override void Exit()
    {
        camera.SetSmoothFactor(true);
        camera.SetPositionSmoothFactor(true);
        
        base.Exit();
    }
    
    private void GenerateVertexPanels()
    {
        System.Numerics.Matrix4x4 projection = camera.GetNumericsProjectionMatrix();
        System.Numerics.Matrix4x4 view = camera.GetNumericsViewMatrix();
        
        foreach (var vert in _modelMesh.VertexList)
        {
            if (vert.WentThroughOne())
                continue;
            
            vert.WentThrough = true;
            
            Vector2? screenPos = Mathf.WorldToScreen(vert.Position, projection, view);
            if (screenPos == null)
                continue;
                
            Vector2 pos = (Vector2)screenPos;
                
            StaticPanel panel = UI.CreateStaticPanel(AnchorType.TopLeft, PositionType.Free, new Vector3(20, 20, 0), new Vector4(0, 0, 0, 0), null);
            panel.SetPosition(new Vector3(pos.X, pos.Y, 0));
            panel.TextureIndex = SelectedContainsSharedVertex(vert) ? 2 : 1;
                
            Ui.AddStaticPanel(panel);
        }
        
        _modelMesh.ResetVertex();
            
        Ui.Generate();
        Ui.Update();
    }

    public void GenerateVertexColor()
    {
        Ui.ClearUiMesh();
        
        int i = 0;
        foreach (var vert in _modelMesh.VertexList)
        {
            if (vert.WentThroughOne())
                continue;
            
            vert.WentThrough = true;
            
            Ui.SetStaticPanelTexureIndex(i, _selectedVertices.Contains(vert) ? 2 : 1);
            i++;
        }
        
        _modelMesh.ResetVertex();
        
        Ui.GenerateUi();
        Ui.Update();
    }
    
    private bool SelectedContainsSharedVertex(Vertex vertex)
    {
        return vertex.SharedVertices.Any(vert => _selectedVertices.Contains(vert)) || _selectedVertices.Contains(vertex);
    }
    
    private void MoveSelectedVertices(Vector3 move)
    {
        
        foreach (var vert in _selectedVertices)
        {
            vert.MoveVertex(move);
        }
    }
    
    private void MoveStepSelectedVertices(Vector3 move, float step)
    {
        foreach (var vert in _selectedVertices)
        {
            vert.MoveStep(move, step);
        }
    }

    private void ModelFaceColor(int xy, int xz, int yz, int[] xyz)
    {
        switch (symmetry)
        {
            case Symmetry.XY:
                ModelingHelperMesh.SetFaceColor(0, xy, 3);
                break;
            case Symmetry.XZ:
                ModelingHelperMesh.SetFaceColor(0, xz, 3);
                break;
            case Symmetry.YZ:
                ModelingHelperMesh.SetFaceColor(0, yz, 3);
                break;
            case Symmetry.XYZ:
            {
                foreach (var t in xyz)
                {
                    ModelingHelperMesh.SetFaceColor(0, t, 3);
                }

                break;
            }
        }
    }

    private void ModelCornerColor(int index, int color)
    {
        //index is vertex index
        if (index is < 0 or > 8)
            return;
        
        //index is displaced by 1 in mesh
        
        ModelingHelperMesh.SetVoxelColor(index + 1, color);

        int[] indexes = [];
        
        if (symmetry == Symmetry.X)
        {
            indexes = ModelData.SymmetryHelper[index][0];
        }
        else if (symmetry == Symmetry.Y)
        {
            indexes = ModelData.SymmetryHelper[index][1];
        }
        else if (symmetry == Symmetry.Z)
        {
            indexes = ModelData.SymmetryHelper[index][2];
        }
        else if (symmetry == Symmetry.XY)
        {
            indexes = ModelData.SymmetryHelper[index][3];
        }
        else if (symmetry == Symmetry.XZ)
        {
            indexes = ModelData.SymmetryHelper[index][4];
        }
        else if (symmetry == Symmetry.YZ)
        {
            indexes = ModelData.SymmetryHelper[index][5];
        }
        else if (symmetry == Symmetry.XYZ)
        {
            indexes = ModelData.SymmetryHelper[index][6];
        }
        
        for (int i = 0; i < indexes.Length; i++)
        {
            ModelingHelperMesh.SetVoxelColor(indexes[i] + 1, color);
        }
    }
    
    
    private readonly Dictionary<Keys, float> StepDictionary = new()
    {
        {Keys.D1, 0.1f},
        {Keys.D2, 0.25f},
        {Keys.D3, 0.5f},
        {Keys.D4, 1f},
    };
    
    private readonly List<Vector3> AxisIgnore = new()
    {
        new Vector3(0, 1, 1), // X
        new Vector3(1, 0, 1), // Y
        new Vector3(1, 1, 0), // Z
    };
}

public abstract class Undoable
{
    
}

public class AngleUndo(Vector3 oldAngle) : Undoable
{
    public Vector3 OldAngle = oldAngle;
}

public class ModelUndo(ModelNode oldNode) : Undoable
{
    public ModelNode OldNode = oldNode;
}