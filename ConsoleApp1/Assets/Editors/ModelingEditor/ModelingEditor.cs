using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class ModelingEditor : BaseEditor
{
    public static RenderType selectionType = RenderType.Vertex;
    public bool _started = false;
    public GeneralModelingEditor Editor;
    public ModelMesh Mesh => Editor.model.modelMesh;

    public Dictionary<Keys, Action> PressedAction = new Dictionary<Keys, Action>();
    public Dictionary<Keys, Action> DownAction = new Dictionary<Keys, Action>();
    public Dictionary<Keys, Action> ReleasedAction = new Dictionary<Keys, Action>();
    public Action[] Selection = [ () => { }, () => { }, () => { } ];
    public Action[] Extrusion = [ () => { }, () => { }, () => { } ];

    public List<Vertex> SelectedVertices = new();
    public List<Vector3> SelectedVerticesPosition = new();
    public List<Edge> SelectedEdges = new();
    public List<Triangle> SelectedTriangles = new();
    public Dictionary<Vertex, Vector2> Vertices = new Dictionary<Vertex, Vector2>();

    public UIController Ui = new UIController();


    public float scale = 1;

    public bool regenerateVertexUi = true;


    public List<string> MeshSaveNames = new List<string>();

    public override void Start(GeneralModelingEditor editor)
    {
        if (_started)
        {
            return;
        }

        Console.WriteLine("Start Modeling Editor");

        Editor = editor;

        Ui.GenerateBuffers();
        
        Selection[0] = HandleVertexSelection;
        Selection[1] = HandleEdgeSelection;
        Selection[2] = HandleFaceSelection;

        Extrusion[0] = HandleVertexExtrusion;
        Extrusion[1] = HandleEdgeExtrusion;
        Extrusion[2] = HandleFaceExtrusion;

        _started = true;
    }

    public override void Awake(GeneralModelingEditor editor)
    {
        editor.model.SwitchState("Modeling");
    }
    
    public override void Update(GeneralModelingEditor editor)
    {
        Ui.Test();

        if (Input.IsKeyPressed(Keys.Escape))
        {
            editor.freeCamera = !editor.freeCamera;
            
            if (editor.freeCamera)
            {
                Game.Instance.CursorState = CursorState.Grabbed;
                Game.camera.firstMove = true;
            }
            else
            {
                Game.Instance.CursorState = CursorState.Normal;
            }
        }

        if (Input.IsKeyPressed(Keys.Delete))
        {
            SelectedVertices.Clear();
            GenerateVertexColor();
        }
        
        if (editor.freeCamera)
        {
            Game.camera.Update();
        }

        if (Input.IsKeyDown(Keys.LeftControl))
        {
            // Undo
            if (Input.IsKeyPressed(Keys.Z)) Handle_Undo();

            // Select all
            if (Input.IsKeyPressed(Keys.A)) Handle_SelectAllVertices();
            
            // New Face
            if (Input.IsKeyPressed(Keys.F)) Handle_GenerateNewFace();
            
            // Flipping triangle
            if (Input.IsKeyPressed(Keys.I)) Handle_FlipTriangleNormal();
            
            // Deleting triangle
            if (Input.IsKeyPressed(Keys.D)) Handle_TriangleDeletion();
            
            // Merging
            if (Input.IsKeyPressed(Keys.K) && SelectedVertices.Count >= 2) Handle_VertexMerging();
        }
        
        // Extrude
        if (Input.IsKeyPressed(Keys.E)) Handle_Extrusion();

        // Scaling
        if (Input.IsKeyPressed(Keys.S)) ScalingInit();
        if (Input.IsKeyDown(Keys.S)) Handle_ScalingSelectedVertices();
        
        // Moving
        if (Input.IsKeyPressed(Keys.G)) StashMesh();
        if (Input.IsKeyDown(Keys.E) || Input.IsKeyDown(Keys.G)) Handle_MovingSelectedVertices();

        if (Input.IsKeyReleased(Keys.E) || Input.IsKeyReleased(Keys.G))
        {
            ModelSettings.SnappingOffset = Vector3.Zero;
            UpdateVertexPosition();
        }
        
        //Generate panels on top of each vertex
        if (editor.freeCamera && !regenerateVertexUi)
        {
            Console.WriteLine("Clear Vertex UI");
            regenerateVertexUi = true;
        }
        
        if (!editor.freeCamera)
        {
            if (regenerateVertexUi)
            {
                UpdateVertexPosition();
                GenerateVertexColor();
                regenerateVertexUi = false;
            }
            
            if (Input.IsMousePressed(MouseButton.Left))
            {
                Selection[(int)selectionType]();
            }
        }
    }

    public override void Render(GeneralModelingEditor editor)
    {
        editor.RenderModel();
        Ui.Render();
    }

    public override void Exit(GeneralModelingEditor editor)
    {
        Game.camera.SetSmoothFactor(true);
        Game.camera.SetPositionSmoothFactor(true);
    }

    public static void SwitchSelection(RenderType st)
    {
        
        PopUp.AddPopUp($"Switched to {st} Selection");
        selectionType = st;
    }


    public void Handle_Undo()
    {
        Console.WriteLine("Undo");
        GetLastMesh();
        Mesh.Init();
        Mesh.GenerateBuffers();

        UpdateVertexPosition();
        GenerateVertexColor();
    }

    // Selection
    public void HandleVertexSelection()
    {
        if (!Input.IsKeyDown(Keys.LeftShift))
            SelectedVertices.Clear();
        
        Vector2 mousePos = Input.GetMousePosition();
        Vector2? closest = null;
        Vertex? closestVert = null;

        Console.WriteLine(Vertices.Count);
    
        foreach (var vert in Vertices)
        {
            float distance = Vector2.Distance(mousePos, vert.Value);
            float distanceClosest = closest == null ? 1000 : Vector2.Distance(mousePos, (Vector2)closest);
        
            if (distance < distanceClosest && distance < 10)
            {
                closest = vert.Value;
                closestVert = vert.Key;
            }

            Console.WriteLine(distance);
        }

        Console.WriteLine(Mesh.VertexColors.Count);

        if (closestVert != null && !SelectedVertices.Remove(closestVert))
            SelectedVertices.Add(closestVert);

        Console.WriteLine(SelectedVertices.Count);

        GenerateVertexColor();
    }

    public void HandleEdgeSelection()
    {
        
    }

    public void HandleFaceSelection()
    {

    }


    // Extrusion
    public void Handle_Extrusion()
    {
        StashMesh();
        
        Console.WriteLine("Extruding");
        Extrusion[(int)selectionType]();

        Mesh.Init();
        Mesh.GenerateBuffers();
        Mesh.UpdateMesh();

        UpdateVertexPosition();
        GenerateVertexColor();
    }

    public void HandleVertexExtrusion()
    {
        List<Vertex> newVertices = new List<Vertex>();

        foreach (var vertex in SelectedVertices)
        {
            Vertex newVertex = new Vertex(vertex.Position);
            newVertices.Add(newVertex);

            Mesh.EdgeList.Add(new Edge(vertex, newVertex));
        }

        SelectedVertices.Clear();
        SelectedVertices.AddRange(newVertices);

        GenerateVertexColor();

        Mesh.AddVertices(newVertices);
    }

    public void HandleEdgeExtrusion()
    {

    }

    public void HandleFaceExtrusion()
    {

    }



    public void MoveSelectedVertices(Vector3 move)
    {
        foreach (var vert in SelectedVertices)
        {
            if (ModelSettings.Snapping)
                vert.SnapPosition(move, ModelSettings.SnappingFactor);
            else
                vert.MovePosition(move);
        }
    }


    // Utility
    public void UpdateVertexPosition()
    {
        Vertices.Clear();

        System.Numerics.Matrix4x4 projection = Game.camera.GetNumericsProjectionMatrix();
        System.Numerics.Matrix4x4 view = Game.camera.GetNumericsViewMatrix();
    
        foreach (var vert in Mesh.VertexList)
        {
            Vector2? screenPos = Mathf.WorldToScreen(vert.Position, projection, view);
            if (screenPos == null)
                continue;
            
            Vertices.Add(vert, screenPos.Value);
        }
    }

    public void GenerateVertexColor()
    {
        foreach (var vert in Mesh.VertexList)
        {
            int vertIndex = Mesh.VertexList.IndexOf(vert);
            Vector3 color = SelectedVertices.Contains(vert) ? (0.25f, 0.3f, 1) : (0f, 0f, 0f);
            if (Mesh.VertexColors.Count <= vertIndex)
                continue;
            Mesh.VertexColors[vertIndex] = color;
        }

        Mesh.UpdateVertexColors();
    }

    public void Handle_MovingSelectedVertices()
    {
        Vector3 move = GetSnappingMovement();
        MoveSelectedVertices(move);
        
        Mesh.RecalculateNormals();
        Mesh.Init();
        Mesh.UpdateMesh();
    }

    public void ScalingInit()
    {
        StashMesh();
        SelectedVerticesPosition.Clear();
        SelectedVerticesPosition.AddRange(SelectedVertices.Select(v => v.Position));
        scale = 1;
    }

    public void Handle_ScalingSelectedVertices()
    {
        if (SelectedVertices.Count < 2)
            return;

        Vector3 center = Vector3.Zero;
        foreach (var vert in SelectedVertices)
        {
            center += vert.Position;
        }
        center /= SelectedVertices.Count;
    }

    public Vector3 GetSnappingMovement()
    {
        Camera camera = Game.camera;

        Vector2 mouseDelta = Input.GetMouseDelta() * (GameTime.DeltaTime * 10);
        Vector3 move = camera.right * mouseDelta.X + camera.up * -mouseDelta.Y;

        if (Input.AreKeysDown(out int index, Keys.X, Keys.C, Keys.V))
            move *= AxisIgnore[index];
        
        if (ModelSettings.Snapping)
        {
            Vector3 Offset = Vector3.Zero;

            Vector3 snappingOffset = ModelSettings.SnappingOffset;
            float snappingFactor = ModelSettings.SnappingFactor;

            snappingOffset += move;
            if (snappingOffset.X > ModelSettings.SnappingFactor)
            {
                Offset.X = ModelSettings.SnappingFactor;
                snappingOffset.X -= ModelSettings.SnappingFactor;
            }
            if (snappingOffset.X < -ModelSettings.SnappingFactor)
            {
                Offset.X = -ModelSettings.SnappingFactor;
                snappingOffset.X += ModelSettings.SnappingFactor;
            }
            if (snappingOffset.Y > ModelSettings.SnappingFactor)
            {
                Offset.Y = ModelSettings.SnappingFactor;
                snappingOffset.Y -= ModelSettings.SnappingFactor;
            }
            if (snappingOffset.Y < -ModelSettings.SnappingFactor)
            {
                Offset.Y = -ModelSettings.SnappingFactor;
                snappingOffset.Y += ModelSettings.SnappingFactor;
            }
            if (snappingOffset.Z > ModelSettings.SnappingFactor)
            {
                Offset.Z = ModelSettings.SnappingFactor;
                snappingOffset.Z -= ModelSettings.SnappingFactor;
            }
            if (snappingOffset.Z < -ModelSettings.SnappingFactor)
            {
                Offset.Z = -ModelSettings.SnappingFactor;
                snappingOffset.Z += ModelSettings.SnappingFactor;
            }

            ModelSettings.SnappingOffset = snappingOffset;
            ModelSettings.SnappingFactor = snappingFactor;
        
            move = Offset;
        }

        return move;
    }

    public void Handle_VertexMerging()
    {
        Console.WriteLine("Merging verts");
        
        StashMesh();
                
        if (SelectedVertices.Count < 2)
            return;

        ModelMesh modelMesh = Mesh;

        modelMesh.MergeVertices(SelectedVertices);
                
        Vertex first = SelectedVertices[0];
        SelectedVertices = [first];
                
        Ui.Clear();
                
        regenerateVertexUi = true;
    }

    public void Handle_TriangleDeletion()
    {
        StashMesh();

        HashSet<Triangle> triangles = GetSelectedFullTriangles();
        ModelMesh modelMesh = Mesh;

        if (triangles.Count > 0)
        {
            foreach (var triangle in triangles)
            {
                modelMesh.RemoveTriangle(triangle);
            }
            SelectedVertices.Clear();
                
            modelMesh.Init();
            modelMesh.GenerateBuffers();
        }
    }

    public void Handle_FlipTriangleNormal()
    {
        StashMesh();

        HashSet<Triangle> triangles = GetSelectedFullTriangles();
        ModelMesh modelMesh = Mesh;

        if (triangles.Count > 0)
        {
            foreach (var triangle in triangles)
            {
                Vertex A = triangle.A;
                Vertex B = triangle.B;
                if (modelMesh.SwapVertices(A, B))
                {
                    triangle.Invert();
                    modelMesh.UpdateNormals(triangle);
                }
            }

            modelMesh.Init();
            modelMesh.UpdateMesh();
        }
    }

    public void Handle_GenerateNewFace()
    {
        StashMesh();

        if (selectionType != RenderType.Vertex)
            return;

        if (SelectedVertices.Count == 3)
        {
            Vertex A = SelectedVertices[0];
            Vertex B = SelectedVertices[1];
            Vertex C = SelectedVertices[2];

            if (A.ShareTriangle(B, C))
                return;

            Console.WriteLine("Generating new face");
            
            Edge AB;
            Edge BC;
            Edge CA;

            Edge? b = A.GetEdgeWith(B);
            if (b == null)
            {
                AB = new Edge(A, B);
                Mesh.EdgeList.Add(AB);
            }
            else
            {
                AB = b;
            }

            Edge? c = B.GetEdgeWith(C);
            if (c == null)
            {
                BC = new Edge(B, C);
                Mesh.EdgeList.Add(BC);
            }
            else
            {
                BC = c;
            }

            Edge? a = C.GetEdgeWith(A);
            if (a == null)
            {
                CA = new Edge(C, A);
                Mesh.EdgeList.Add(CA);
            }
            else
            {
                CA = a;
            }

            Mesh.AddTriangle(new Triangle(A, B, C, AB, BC, CA));
        }

        Mesh.Init();
        Mesh.GenerateBuffers();
    }

    public void Handle_SelectAllVertices()
    {
        Console.WriteLine("Select all");
        SelectedVertices.Clear();
                
        foreach (var vert in Mesh.VertexList)
        {
            SelectedVertices.Add(vert);
        }
                
        GenerateVertexColor();
    }

    public void StashMesh(int maxCount = 30)
    {
        string fileName = Editor.currentModelName + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff");
        string folderPath = Path.Combine(Game.undoModelPath, Editor.currentModelName);

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        if (!(MeshSaveNames.Count < 0) && MeshSaveNames.Count >= maxCount)
        {
            string name = MeshSaveNames[0];
            string path = Path.Combine(folderPath, name);
            if (!File.Exists(path))
                return;

            File.Delete(path);
            MeshSaveNames.RemoveAt(0);
        }
        Console.WriteLine("Stashing mesh");
        MeshSaveNames.Add(fileName);
        Mesh.SaveModel(fileName, folderPath);
    }

    public void GetLastMesh()
    {
        if (MeshSaveNames.Count == 0)
            return;

        string name = MeshSaveNames[MeshSaveNames.Count - 1];
        string path = Path.Combine(Path.Combine(Game.undoModelPath, Editor.currentModelName), $"{name}.model");

        Console.WriteLine(path);

        if (!File.Exists(path))
            return;

        Console.WriteLine("Getting last mesh");
        
        MeshSaveNames.RemoveAt(MeshSaveNames.Count - 1);
        Mesh.LoadModel(name, Path.Combine(Game.undoModelPath, Editor.currentModelName));
    }

    public HashSet<Triangle> GetSelectedFullTriangles()
    {
        HashSet<Triangle> triangles = new HashSet<Triangle>();
                
        foreach (var triangle in GetSelectedTriangles())
        {
            if (IsTriangleFullySelected(triangle))
                triangles.Add(triangle);
        }
        
        return triangles;
    }

    public HashSet<Triangle> GetSelectedTriangles()
    {
        HashSet<Triangle> triangles = new HashSet<Triangle>();
                
        foreach (var vert in SelectedVertices)
        {
            foreach (var triangle in vert.ParentTriangles)
            {
                triangles.Add(triangle);
            }
        }
        
        return triangles;
    }
    
    public bool IsTriangleFullySelected(Triangle triangle)
    {
        return SelectedVertices.Contains(triangle.A) &&
               SelectedVertices.Contains(triangle.B) &&
               SelectedVertices.Contains(triangle.C);
    }

    public readonly List<Vector3> AxisIgnore = new()
    {
        new Vector3(0, 1, 1), // X
        new Vector3(1, 0, 1), // Y
        new Vector3(1, 1, 0), // Z
    };
}

public enum RenderType
{
    Vertex = 0,
    Edge = 1,
    Face = 2,
}