using System.Reflection.Metadata;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SharpGen.Runtime;

public class ModelingEditor : BaseEditor
{
    public static RenderType selectionType = RenderType.Vertex;
    public bool _started = false;
    public GeneralModelingEditor Editor;
    public ModelMesh Mesh;

    public Dictionary<Keys, Action> PressedAction = new Dictionary<Keys, Action>();
    public Dictionary<Keys, Action> DownAction = new Dictionary<Keys, Action>();
    public Dictionary<Keys, Action> ReleasedAction = new Dictionary<Keys, Action>();
    public Action[] Selection = [ () => { }, () => { }, () => { } ];
    public Action[] Extrusion = [ () => { }, () => { }, () => { } ];
    public Action[] Deletion = [ () => { }, () => { }, () => { } ];

    public List<Vertex> SelectedVertices = new();
    public List<Vector3> SelectedVerticesPosition = new();
    public List<Edge> SelectedEdges = new();
    public List<Triangle> SelectedTriangles = new();
    public Dictionary<Vertex, Vector2> Vertices = new Dictionary<Vertex, Vector2>();

    public UIController Ui = new UIController();

    public Vector3 selectedCenter = Vector3.Zero;
    public float rotation = 0;
    public float scale = 1;

    public bool regenerateVertexUi = true;


    public List<string> MeshSaveNames = new List<string>();

    
    public ModelCopy Copy = new();

    public override void Start(GeneralModelingEditor editor)
    {
        Console.WriteLine("Start Modeling Editor");

        if (_started)
        {
            return;
        }

        Editor = editor;
        Mesh = editor.model.modelMesh;

        Ui.GenerateBuffers();
        
        Selection[0] = HandleVertexSelection;
        Selection[1] = HandleEdgeSelection;
        Selection[2] = HandleFaceSelection;

        Extrusion[0] = HandleVertexExtrusion;
        Extrusion[1] = HandleEdgeExtrusion;
        Extrusion[2] = HandleFaceExtrusion;

        Deletion[0] = HandleVertexDeletion;
        Deletion[1] = HandleEdgeDeletion;
        Deletion[2] = HandleTriangleDeletion;

        _started = true;
    }

    public override void Resize(GeneralModelingEditor editor)
    {
        
    }

    public override void Awake(GeneralModelingEditor editor)
    {
        editor.model.SwitchState("Modeling");
        Mesh.LoadModel(editor.currentModelName);
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
                UpdateVertexPosition();
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
            return;
        }

        if (Input.IsKeyDown(Keys.LeftControl))
        {
            // Undo
            if (Input.IsKeyPressed(Keys.Z)) Handle_Undo();

            // Copy
            if (Input.IsKeyPressed(Keys.C)) Handle_Copy();

            // Paste
            if (Input.IsKeyPressed(Keys.V)) Handle_Paste();

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

        // Rotation
        if (Input.IsKeyPressed(Keys.R)) RotationInit();
        if (Input.IsKeyDown(Keys.R)) Handle_RotateSelectedVertices();
        if (Input.IsKeyReleased(Keys.R)) UpdateVertexPosition();

        // Scaling
        if (Input.IsKeyPressed(Keys.S)) ScalingInit();
        if (Input.IsKeyDown(Keys.S)) Handle_ScalingSelectedVertices();
        if (Input.IsKeyReleased(Keys.S)) UpdateVertexPosition();
        
        // Moving
        if (Input.IsKeyPressed(Keys.G)) StashMesh();
        if (Input.IsKeyDown(Keys.E) || Input.IsKeyDown(Keys.G)) Handle_MovingSelectedVertices();

        if (Input.IsKeyReleased(Keys.E) || Input.IsKeyReleased(Keys.G))
        {
            ModelSettings.SnappingOffset = Vector3.Zero;
            Mesh.CombineDuplicateVertices();
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
        ClearStash();
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

    public void Handle_Copy()
    {
        Console.WriteLine("Copy");
        StashMesh();

        Copy.Clear();
        Copy.selectedTriangles = GetSelectedFullTriangles().ToList();

        foreach (var triangle in Copy.selectedTriangles)
        {
            Copy.Add(triangle.A);
            Copy.Add(triangle.B);
            Copy.Add(triangle.C);

            Copy.Add(triangle.AB);
            Copy.Add(triangle.BC);
            Copy.Add(triangle.CA);
        }

        foreach (var vert in Copy.selectedVertices)
        {
            Copy.newSelectedVertices.Add(vert.Copy());
        }

        foreach (var edge in Copy.selectedEdges)
        {
            Copy.newSelectedEdges.Add(new(Copy.GetNewVertex(edge.A), Copy.GetNewVertex(edge.B)));
        }

        foreach (var triangle in Copy.selectedTriangles)
        {
            Copy.newSelectedTriangles.Add(
            new(
                Copy.GetNewVertex(triangle.A), 
                Copy.GetNewVertex(triangle.B), 
                Copy.GetNewVertex(triangle.C), 
                Copy.GetNewEdge(triangle.AB), 
                Copy.GetNewEdge(triangle.BC), 
                Copy.GetNewEdge(triangle.CA)
            ));  
        }
    }

    // Paste
    public void Handle_Paste()
    {
        Console.WriteLine("Paste");
        StashMesh();

        ModelCopy copy = Copy.Copy();
        SelectedVertices = copy.newSelectedVertices;

        Mesh.AddCopy(copy);
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
    
        foreach (var vert in Vertices)
        {
            float distance = Vector2.Distance(mousePos, vert.Value);
            float distanceClosest = closest == null ? 1000 : Vector2.Distance(mousePos, (Vector2)closest);
        
            if (distance < distanceClosest && distance < 10)
            {
                closest = vert.Value;
                closestVert = vert.Key;
            }
        }

        if (closestVert != null && !SelectedVertices.Remove(closestVert))
            SelectedVertices.Add(closestVert);

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


    // Deletion
    public void Handle_TriangleDeletion()
    {
        StashMesh();
        
        Console.WriteLine("Deletinga");
        Deletion[(int)selectionType]();

        UpdateVertexPosition();
        GenerateVertexColor();
    }
    public void HandleVertexDeletion()
    {
        if (SelectedVertices.Count == 0)
            return;

        foreach (var vert in SelectedVertices)
        {
            Mesh.RemoveVertex(vert);
        }
        SelectedVertices.Clear();
                
        Mesh.Init();
        Mesh.GenerateBuffers();
    }

    public void HandleEdgeDeletion()
    {
    }

    public void HandleTriangleDeletion()
    {
        HashSet<Triangle> triangles = GetSelectedFullTriangles();

        if (triangles.Count > 0)
        {
            foreach (var triangle in triangles)
            {
                Mesh.RemoveTriangle(triangle);
            }
            SelectedVertices.Clear();
                
            Mesh.Init();
            Mesh.GenerateBuffers();
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
            vert.Color = SelectedVertices.Contains(vert) ? (0.25f, 0.3f, 1) : (0f, 0f, 0f);

            if (Mesh.VertexColors.Count <= vertIndex)
                continue;
            Mesh.VertexColors[vertIndex] = vert.Color;
        }

        foreach (var edge in Mesh.EdgeList)
        {
            int edgeIndex = Mesh.EdgeList.IndexOf(edge) * 2;
            if (Mesh.EdgeColors.Count > edgeIndex)
                Mesh.EdgeColors[edgeIndex] = edge.A.Color;

            if (Mesh.EdgeColors.Count > edgeIndex + 1)
                Mesh.EdgeColors[edgeIndex + 1] = edge.B.Color;
        }

        Mesh.UpdateVertexColors();
        Mesh.UpdateEdgeColors();
    }

    public void Handle_MovingSelectedVertices()
    {
        Vector3 move = GetSnappingMovement();
        MoveSelectedVertices(move);
        
        Mesh.RecalculateNormals();
        Mesh.Init();
        Mesh.UpdateMesh();
    }

    public void RotationInit()
    {
        StashMesh();
        rotation = 0;
        selectedCenter = GetSelectedCenter();
    }

    public void Handle_RotateSelectedVertices()
    {
        if (SelectedVertices.Count == 0)
            return;

        float mouseDelta = Input.GetMouseDelta().X * (GameTime.DeltaTime * 100);
        rotation += mouseDelta;
        Vector3 axis = Game.camera.front;

        foreach (var vert in SelectedVertices)
        {
            vert.Position = RotatePoint(vert.Position, selectedCenter, axis, rotation);
        }

        rotation = 0;

        Mesh.Init();
        Mesh.UpdateVertices();
    }

    public void ScalingInit()
    {
        StashMesh();
        scale = 1;
        selectedCenter = GetSelectedCenter();
    }

    public void Handle_ScalingSelectedVertices()
    {
        if (SelectedVertices.Count < 2)
            return;

        float mouseDelta = Input.GetMouseDelta().X * (GameTime.DeltaTime * 10);
        scale += mouseDelta;

        foreach (var vert in SelectedVertices)
        {
            Vector3 direction = vert.Position - selectedCenter;
            vert.Position = selectedCenter + direction * scale;
        }

        scale = 1;

        Mesh.Init();
        Mesh.UpdateVertices();
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

    public void Handle_GenerateNewFace()
    {
        StashMesh();

        if (selectionType != RenderType.Vertex || SelectedVertices.Count > 4)
            return;

        if (SelectedVertices.Count == 2)
        { 
            if (ModelingHelper.Generate_2_Selected(SelectedVertices))
            {
                List<Vertex> nextVertices = [SelectedVertices[2], SelectedVertices[3]];
                ModelingHelper.Generate_4_Selected(SelectedVertices, Mesh);
                SelectedVertices = nextVertices;
            }
        }

        if (SelectedVertices.Count == 3)
        { 
            ModelingHelper.Generate_3_Selected(SelectedVertices, Mesh);
        }

        if (SelectedVertices.Count == 4)
        { 
            ModelingHelper.Generate_4_Selected(SelectedVertices, Mesh);    
        }

        Mesh.Init();
        Mesh.GenerateBuffers();

        GenerateVertexColor();
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

    // Stashing
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

    public void ClearStash()
    {
        string folderPath = Path.Combine(Game.undoModelPath, Editor.currentModelName);
        foreach (string file in Directory.GetFiles(folderPath))
        {
            File.Delete(file);
        }
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

    // Helper
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

    public Vector3 GetSelectedCenter()
    {
        Vector3 center = Vector3.Zero;
        if (SelectedVertices.Count == 0)
            return center;

        SelectedVerticesPosition.Clear();
        SelectedVerticesPosition.AddRange(SelectedVertices.Select(v => v.Position));
        foreach (var vert in SelectedVertices)
        {
            center += vert.Position;
        }
        return center / SelectedVertices.Count;
    }

    public static Vector3 RotatePoint(Vector3 point, Vector3 center, Vector3 axis, float degrees)
    {
        float radians = MathHelper.DegreesToRadians(degrees);
        float sinHalfAngle = MathF.Sin(radians / 2);

        axis.Normalize();
        Vector3 relativePoint = point - center;

        Quaternion rotation = new Quaternion(axis * sinHalfAngle, MathF.Cos(radians / 2));
        Quaternion pQuat = new Quaternion(relativePoint, 0);
        Quaternion rotatedQuat = rotation * pQuat * rotation.Inverted();

        return (rotatedQuat.X, rotatedQuat.Y, rotatedQuat.Z) + center;
    }


    // Data
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

public class ModelCopy
{
    public List<Vertex> selectedVertices = [];
    public List<Edge> selectedEdges = [];
    public List<Triangle> selectedTriangles = [];

    public List<Vertex> newSelectedVertices = [];
    public List<Edge> newSelectedEdges = [];
    public List<Triangle> newSelectedTriangles = [];

    public void Clear()
    {
        selectedVertices.Clear();
        selectedEdges.Clear();
        selectedTriangles.Clear();

        newSelectedVertices.Clear();
        newSelectedEdges.Clear();
        newSelectedTriangles.Clear();
    }

    public void Add(Vertex vert) { if (!selectedVertices.Contains(vert)) selectedVertices.Add(vert); }
    public void Add(Edge edge) { if (!selectedEdges.Contains(edge)) selectedEdges.Add(edge); }
    public void Add(Triangle triangle) { if (!selectedTriangles.Contains(triangle)) selectedTriangles.Add(triangle); }

    // Vertex
    public int GetOldIndex(Vertex vert) { return selectedVertices.IndexOf(vert); }
    public Vertex GetOldVertex(int index) { return selectedVertices[index]; }
    public int GetNewIndex(Vertex vert) { return newSelectedVertices.IndexOf(vert); }
    public Vertex GetNewVertex(int index) { return newSelectedVertices[index]; }
    public Vertex GetNewVertex(Vertex oldVertex) { return newSelectedVertices[GetOldIndex(oldVertex)]; }

    // Edge
    public int GetOldIndex(Edge edge) { return selectedEdges.IndexOf(edge); }
    public Edge GetOldEdge(int index) { return selectedEdges[index]; }
    public int GetNewIndex(Edge edge) { return newSelectedEdges.IndexOf(edge); }
    public Edge GetNewEdge(int index) { return newSelectedEdges[index]; }
    public Edge GetNewEdge(Edge oldEdge) { return newSelectedEdges[GetOldIndex(oldEdge)]; }

    public ModelCopy Copy()
    {
        ModelCopy copy = new();

        foreach (var vert in newSelectedVertices)
        {
            copy.newSelectedVertices.Add(vert.Copy());
        }

        foreach (var edge in newSelectedEdges)
        {
            copy.newSelectedEdges.Add(new(copy.GetNewVertex(GetNewIndex(edge.A)), copy.GetNewVertex(GetNewIndex(edge.B))));
        }

        foreach (var triangle in newSelectedTriangles)
        {
            copy.newSelectedTriangles.Add(
            new(
                copy.GetNewVertex(GetNewIndex(triangle.A)), 
                copy.GetNewVertex(GetNewIndex(triangle.B)), 
                copy.GetNewVertex(GetNewIndex(triangle.C)), 
                copy.GetNewEdge(GetNewIndex(triangle.AB)), 
                copy.GetNewEdge(GetNewIndex(triangle.BC)), 
                copy.GetNewEdge(GetNewIndex(triangle.CA))
            ));  
        }

        return copy;
    }
}