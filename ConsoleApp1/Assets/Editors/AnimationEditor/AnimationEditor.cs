using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class AnimationEditor : Updateable
{
    public static Symmetry symmetry = Symmetry.None;
    
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
    
    private List<ModelNode> nodes = new List<ModelNode>();
    
    private ModelNode selectedNode;
    
    private GameObject go;
    
    private float oldAngleX = 0;
    private float oldAngleY = 0;
    private float oldAngleZ = 0;

    private Vector3 oldVert1 = new Vector3(0, 0, 0);
    private Vector3 oldVert2 = new Vector3(0, 0, 0);
    private Vector3 oldVert3 = new Vector3(0, 0, 0);
    private Vector3 oldVert4 = new Vector3(0, 0, 0);
    private Vector3 oldVert5 = new Vector3(0, 0, 0);
    private Vector3 oldVert6 = new Vector3(0, 0, 0);
    private Vector3 oldVert7 = new Vector3(0, 0, 0);
    private Vector3 oldVert8 = new Vector3(0, 0, 0);
    
    public static bool clickedRotate = false;
    public static bool clickedMove = false;
    private bool saveRotate = false;
    private bool saveMove = false;

    private int _selectedModel = 0;
    
    private List<Undoable> undoables = new List<Undoable>();
    
    private ModelGrid modelGrid = ModelGrid.Instance;
    
    public override void Awake()
    {
        Console.WriteLine("Animation Editor");
        
        Camera.SetCameraMode(CameraMode.Free);
        
        Camera.position = new Vector3(0, 61, 5);
        Camera.pitch = 0;
        Camera.yaw = -90;
        
        Camera.UpdateVectors();
        
        Camera.SetSmoothFactor(false);
        Camera.SetPositionSmoothFactor(false);
        
        _modelMesh.WorldPosition = transform.Position + new Vector3(0, 0, 0);
    }
    
    public override void Start()
    {
        Console.WriteLine("Animation Editor");
        
        _shaderProgram = new ShaderProgram("Model/Model.vert", "Model/Model.frag");
        
        transform.Position = new Vector3(0, 61, 0);
        
        /*
        _playerMesh = new AnimationMesh();
        VoxelData.GenerateStandardMeshBox(_playerMesh, 
            new Vector3(1, 2, 1), 
            new Vector3(-0.5f, 0, -0.5f), 
            new Vector3(0, 0, 0), 
            1
        );
        
        _playerMesh.GenerateBuffers();
        _playerMesh.UpdateMesh();
        */
        
        
        // 1 2 3
        // 4 5 6
        //   7
        
        node = new ModelNode(new ModelGridCell(node, new Vector3i(1, 1, -1)));
        node.CreateNewHexahedron(new Vector3(1, 1, 1));
        
        node2 = new ModelNode(new ModelGridCell(node2, new Vector3i(0, 0, 0)));
        node2.CreateNewHexahedron(new Vector3(0, 0, 0));
        
        node3 = new ModelNode(new ModelGridCell(node3, new Vector3i(1, 0, 0)));
        node3.CreateNewHexahedron(new Vector3(1, 0, 0));

        modelGrid.AddModelNode(node);
        modelGrid.AddModelNode(node2);
        modelGrid.AddModelNode(node3);
        
        selectedNode = node;
        
        nodes.Add(node);
        nodes.Add(node2);
        nodes.Add(node3);

        
        _modelMesh = new ModelMesh();
        _modelMesh.AddModelNode(node);
        _modelMesh.AddModelNode(node2); 
        _modelMesh.AddModelNode(node3);
        
        _modelMesh.GenerateBuffers();
        
        go = new GameObject();
    }
    
    public override void Update()
    {
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
                UIManager.symmetry = symmetry.ToString();
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
            if (clickedRotate)
            {
                clickedRotate = false;
                saveRotate = true;
            }
            
            if (clickedMove)
            {
                clickedMove = false;
                saveMove = true;
            }
            else
            {
                if (oldVert1 != UIManager.vert1)
                    selectedNode.MoveVert(0, UIManager.vert1 - oldVert1);
                if (oldVert2 != UIManager.vert2)
                    selectedNode.MoveVert(1, UIManager.vert2 - oldVert2);
                if (oldVert3 != UIManager.vert3)
                    selectedNode.MoveVert(2, UIManager.vert3 - oldVert3);
                if (oldVert4 != UIManager.vert4)
                    selectedNode.MoveVert(3, UIManager.vert4 - oldVert4);
                if (oldVert5 != UIManager.vert5)
                    selectedNode.MoveVert(4, UIManager.vert5 - oldVert5);
                if (oldVert6 != UIManager.vert6)
                    selectedNode.MoveVert(5, UIManager.vert6 - oldVert6);
                if (oldVert7 != UIManager.vert7)
                    selectedNode.MoveVert(6, UIManager.vert7 - oldVert7);
                if (oldVert8 != UIManager.vert8)
                    selectedNode.MoveVert(7, UIManager.vert8 - oldVert8);
            }
            
            if (saveRotate)
            {
                Console.WriteLine("Save Rotate");
                saveRotate = false;
                undoables.Add(new AngleUndo(new Vector3(oldAngleX, oldAngleY, oldAngleZ)));
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
                */
            }

            go.transform.Rotation = Mathf.RotateAround(go.transform.Forward, go.transform.Rotation, MathHelper.DegreesToRadians(UIManager.x - oldAngleX));
            go.transform.Rotation = Mathf.RotateAround(go.transform.Right, go.transform.Rotation, MathHelper.DegreesToRadians(UIManager.z - oldAngleZ));
            go.transform.Rotation = Mathf.RotateAround(go.transform.Up, go.transform.Rotation, MathHelper.DegreesToRadians(UIManager.y - oldAngleY));

            oldAngleX = UIManager.x;
            oldAngleY = UIManager.y;
            oldAngleZ = UIManager.z;

            oldVert1 = UIManager.vert1;
            oldVert2 = UIManager.vert2;
            oldVert3 = UIManager.vert3;
            oldVert4 = UIManager.vert4;
            oldVert5 = UIManager.vert5;
            oldVert6 = UIManager.vert6;
            oldVert7 = UIManager.vert7;
            oldVert8 = UIManager.vert8;

            _modelMesh.UpdateNode(0, node);
            _modelMesh.UpdateNode(1, node2);
            _modelMesh.UpdateNode(2, node3);
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
        
        base.Update();
    }

    public override void Render()
    {
        _shaderProgram.Bind();
        
        Matrix4 model = Matrix4.Identity;
        Matrix4 view = Camera.viewMatrix;
        Matrix4 projection = Camera.projectionMatrix;

        int modelLocation = GL.GetUniformLocation(_shaderProgram.ID, "model");
        int viewLocation = GL.GetUniformLocation(_shaderProgram.ID, "view");
        int projectionLocation = GL.GetUniformLocation(_shaderProgram.ID, "projection");
        
        GL.UniformMatrix4(modelLocation, true, ref model);
        GL.UniformMatrix4(viewLocation, true, ref view);
        GL.UniformMatrix4(projectionLocation, true, ref projection);
        
        //_playerMesh.RenderMesh();
        _modelMesh.RenderMesh();
        
        _shaderProgram.Unbind();
        base.Render();
    }

    public override void Exit()
    {
        Camera.SetSmoothFactor(true);
        Camera.SetPositionSmoothFactor(true);
        
        base.Exit();
    }
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

