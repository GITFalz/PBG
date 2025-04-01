using System.Reflection.Metadata;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SharpGen.Runtime;

public class ModelingEditor : BaseEditor
{
    public static RenderType selectionType = RenderType.Vertex;
    public GeneralModelingEditor Editor;
    public ModelMesh Mesh;

    public Dictionary<Keys, Action> PressedAction = new Dictionary<Keys, Action>();
    public Dictionary<Keys, Action> DownAction = new Dictionary<Keys, Action>();
    public Dictionary<Keys, Action> ReleasedAction = new Dictionary<Keys, Action>();
    public Action[] Selection = [ () => { }, () => { }, () => { } ];
    public Action[] Extrusion = [ () => { }, () => { }, () => { } ];
    public Func<bool>[] Deletion = [ () => false, () => false, () => false ];

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


    public Vector2 oldMousePos = Vector2.Zero;
    public bool renderSelection = false;

    // Selection Rendering
    public ShaderProgram selectionShader = new ShaderProgram("Selection/Selection.vert", "Selection/Selection.frag");
    public VAO selectionVao = new();

    public bool CanStash = true;
    public bool CanGenerateBuffers = true;


    public Vertex? TestVertex = null;

    
    public ModelCopy Copy = new();

    public override void Start(GeneralModelingEditor editor)
    {
        Started = true;

        Console.WriteLine("Start Modeling Editor");

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
        renderSelection = false;

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

            // Flip Selection
            if (Input.IsKeyPressed(Keys.H)) Handle_FlipSelection();
            
            // Flipping triangle
            if (Input.IsKeyPressed(Keys.I)) Handle_FlipTriangleNormal();
            
            // Deleting triangle
            if (Input.IsKeyPressed(Keys.D)) Handle_TriangleDeletion();
            
            // Merging
            if (Input.IsKeyPressed(Keys.K) && SelectedVertices.Count >= 2) Handle_VertexMerging();

            // Split vertices
            if (Input.IsKeyPressed(Keys.Q)) Handle_VertexSpliting();
            
            // Mapping
            if (Input.IsKeyPressed(Keys.M)) 
            {
                StashMesh();

                CanStash = false;
                CanGenerateBuffers = false;

                Handle_SelectAllVertices();
                List<Triangle> triangles = GetSelectedFullTriangles().ToList();

                Vector3 offset = (20, 0, 0);

                while (triangles.Count > 0)
                {
                    SelectedTriangles = triangles[0].GetTriangleRegion([]).ToList();
                    SelectedVertices = GetVertices(SelectedTriangles).ToList();
                    triangles.RemoveAll(t => SelectedTriangles.Contains(t));

                    Handle_SeperateSelection();
                    MoveSelectedVertices(offset);
                    Handle_Flattening();

                    if (SelectedVertices.Count != 0)
                    {
                        Mathf.GetSmallestBoundingBox(SelectedVertices, out Vector3 min, out Vector3 max);
                        List<Triangle> tris = GetSelectedFullTriangles().ToList();
                        if (tris.Count > 0)
                        {
                            Triangle first = tris[0];
                            first.UpdateNormal();

                            if (Vector3.Dot(first.Normal, (0, 1, 0)) < 0)
                            {
                                Vector3 center = SelectedVertices[0];
                                for (int i = 1; i < SelectedVertices.Count; i++)
                                {
                                    center += SelectedVertices[i];
                                }
                                center /= SelectedVertices.Count;

                                foreach (var vert in SelectedVertices)
                                {
                                    vert.SetPosition(Mathf.RotatePoint(vert, center, (1, 0, 0), 180f));
                                }
                            }
                        }

                        offset.X += 20;
                    }
                }

                CanStash = true;
                CanGenerateBuffers = true;

                Mesh.CheckUselessVertices();

                Mesh.RecalculateNormals();
                Mesh.Init();
                Mesh.GenerateBuffers();

                UpdateVertexPosition();
                //GenerateVertexColor();
            }

            // Seperate selection
            if (Input.IsKeyPressed(Keys.L)) Handle_SeperateSelection();

            // Combining Duplicate Vertices
            if (Input.IsKeyPressed(Keys.G))
            {
                StashMesh();
                Mesh.CombineDuplicateVertices();

                if (!CanGenerateBuffers)
                    return;

                Mesh.Init();
                Mesh.RecalculateNormals();
                Mesh.GenerateBuffers();

                UpdateVertexPosition();
                GenerateVertexColor();
            }

            if (Input.IsKeyPressed(Keys.T))
            {
                if (SelectedVertices.Count == 0)
                    return;

                TestVertex = SelectedVertices[0];
            }

            if (Input.IsKeyPressed(Keys.X))
            {
                if (TestVertex == null)
                    return;

                SelectedVertices.Clear();

                foreach (var vert in Mesh.VertexList)
                {
                    if (vert.Name == TestVertex.Name)
                    {
                        SelectedVertices.Add(vert);
                    }
                }

                GenerateVertexColor();
            }
        }
        else
        {
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
                Mesh.CheckUselessEdges();
                Mesh.CheckUselessTriangles();
                UpdateVertexPosition();
            }
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
                oldMousePos = Input.GetMousePosition();
                Selection[(int)selectionType]();
            }

            if (Input.IsMouseDown(MouseButton.Left))
            {
                renderSelection = true;
                
                Vector2 mousePos = Input.GetMousePosition();
                Vector2 max = Mathf.Max(mousePos, oldMousePos);
                Vector2 min = Mathf.Min(mousePos, oldMousePos);
                float distance = Vector2.Distance(mousePos, oldMousePos);
                bool regenColor = false;

                if (distance < 5)
                    return;

                foreach (var vert in Vertices)
                {
                    Vector2 vPos = vert.Value;
                    if (vPos.X >= min.X && vPos.X <= max.X && vPos.Y >= min.Y && vPos.Y <= max.Y)
                    {
                        if (!SelectedVertices.Contains(vert.Key))
                        {
                            regenColor = true;
                            SelectedVertices.Add(vert.Key);
                        }
                    }
                    else
                    {
                        if (!Input.IsKeyDown(Keys.LeftShift) && SelectedVertices.Contains(vert.Key))
                        {
                            regenColor = true;
                            SelectedVertices.Remove(vert.Key);
                        }
                    }
                }

                if (regenColor)
                    GenerateVertexColor();
            }
        }
    }

    public override void Render(GeneralModelingEditor editor)
    {
        editor.RenderModel();
        Ui.Render();

        if (renderSelection)
        {
            selectionShader.Bind();

            Matrix4 model = Matrix4.CreateTranslation((oldMousePos.X, oldMousePos.Y, 0));
            Matrix4 projection = UIController.OrthographicProjection;
            Vector2 selectionSize = Input.GetMousePosition() - oldMousePos;
            Vector3 color = new Vector3(1, 0.5f, 0.25f);

            var modelLoc = GL.GetUniformLocation(selectionShader.ID, "model");
            var projectionLoc = GL.GetUniformLocation(selectionShader.ID, "projection");
            var selectionSizeLoc = GL.GetUniformLocation(selectionShader.ID, "selectionSize");
            var colorLoc = GL.GetUniformLocation(selectionShader.ID, "color");

            GL.UniformMatrix4(modelLoc, true, ref model);
            GL.UniformMatrix4(projectionLoc, true, ref projection);
            GL.Uniform2(selectionSizeLoc, selectionSize);
            GL.Uniform3(colorLoc, color);

            selectionVao.Bind();

            GL.DrawArrays(PrimitiveType.Lines, 0, 8);

            selectionVao.Unbind();

            selectionShader.Unbind();
        }
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
        GetLastMesh();
        
        Mesh.Init();
        Mesh.GenerateBuffers();

        UpdateVertexPosition();
        GenerateVertexColor();
    }

    public void Handle_Copy()
    {
        Copy.Clear();
        Copy.selectedVertices = [.. SelectedVertices];
        Copy.selectedEdges = [.. GetSelectedFullEdges()];
        Copy.selectedTriangles = [.. GetSelectedFullTriangles()];

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
    public void Handle_Paste(bool stash = true)
    {
        if (stash)
            StashMesh();

        ModelCopy copy = Copy.Copy();
        SelectedVertices = copy.newSelectedVertices;

        Mesh.AddCopy(copy);

        if (!CanGenerateBuffers)
            return;

        Mesh.Init();
        Mesh.GenerateBuffers();
        UpdateVertexPosition();
        GenerateVertexColor();
    }

    public void Handle_Flattening()
    {
        Handle_Flattening(GetSelectedFullTriangles().ToList());
    }

    public void Handle_Flattening(List<Triangle> triangles)
    {
        if (triangles.Count == 0)
            return;

        Triangle first = triangles[0];

        Vector3 rotationAxis = Vector3.Cross(first.Normal, (0, 1, 0));

        if (rotationAxis.Length != 0)
        {
            float angle = MathHelper.RadiansToDegrees(Vector3.CalculateAngle(first.Normal, (0, 1, 0)));
            Vector3 center = first.Center();
            Vector3 rotatedNormal = Mathf.RotatePoint(first.Normal, Vector3.Zero, rotationAxis, angle);

            if (Vector3.Dot(rotatedNormal, (0, 1, 0)) < 0)
                angle += 180f;
            
            foreach (var vert in GetVertices(triangles))
                vert.SetPosition(Mathf.RotatePoint(vert, center, rotationAxis, angle));
        }

        first.FlattenRegion(triangles);

        if (!CanGenerateBuffers)
            return;

        Mesh.Init();
        Mesh.RecalculateNormals();
        Mesh.UpdateVertices();
        
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

        if (!CanGenerateBuffers)
            return;

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
            Vertex newVertex = vertex.Copy();
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
    public void Handle_TriangleDeletion(bool stash = true)
    {
        if (stash)
            StashMesh();
        
        Console.WriteLine("Deletinga");
        if (!Deletion[(int)selectionType]())
            return;

        if (!CanGenerateBuffers)
            return;

        Mesh.Init();
        Mesh.GenerateBuffers();

        UpdateVertexPosition();
        GenerateVertexColor();
    }
    public bool HandleVertexDeletion()
    {
        if (SelectedVertices.Count == 0)
            return false;

        foreach (var vert in SelectedVertices)
        {
            Mesh.RemoveVertex(vert);
        }
        SelectedVertices.Clear();

        return true;
    }

    public bool HandleEdgeDeletion()
    {
        if (SelectedVertices.Count == 0)
            return false;

        HashSet<Edge> edges = GetSelectedFullEdges();
        foreach (var edge in edges)
        {
            Mesh.RemoveEdge(edge);
        }

        SelectedVertices.Clear();
        SelectedEdges.Clear();

        return true;
    }

    public bool HandleTriangleDeletion()
    {
        HashSet<Triangle> triangles = GetSelectedFullTriangles();

        if (triangles.Count > 0)
        {
            foreach (var triangle in triangles)
            {
                Mesh.RemoveTriangle(triangle);
            }
            SelectedVertices.Clear();

            return true;
        }
        return false;
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
                    triangle.UpdateNormal();
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
            if (ModelSettings.GridAligned && ModelSettings.Snapping)
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
            Vector2? screenPos = Mathf.WorldToScreen(vert, projection, view);
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

        if (!CanGenerateBuffers)
            return;
        
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

        Vector3 axis = Game.camera.front * ModelSettings.axis;
        if (axis.Length == 0) return;
        axis.Normalize();

        float mouseDelta = Input.GetMouseDelta().X * (GameTime.DeltaTime * 100);
        rotation += mouseDelta;

        if (ModelSettings.Snapping)
        {
            if (Mathf.Abs(rotation) >= ModelSettings.SnappingFactor)
                rotation = ModelSettings.SnappingFactor * Mathf.Sign(rotation);
            else    
                return;
        }

        foreach (var vert in SelectedVertices)
        {
            vert.SetPosition(Mathf.RotatePoint(vert, selectedCenter, axis, rotation));
        }

        rotation = 0;

        if (!CanGenerateBuffers)
            return;

        Mesh.RecalculateNormals();
        Mesh.Init();
        Mesh.UpdateVertices();

        UpdateVertexPosition();
        GenerateVertexColor();
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

        float mouseDelta = Input.GetMouseDelta().X * (GameTime.DeltaTime * 50);
        scale += mouseDelta;

        if (ModelSettings.Snapping)
        {
            if (Mathf.Abs(scale) - 1 >= ModelSettings.SnappingFactor)
                scale = ModelSettings.SnappingFactor * Mathf.Sign(scale) + 1;
            else    
                return;
        }

        foreach (var vert in SelectedVertices)
        { 
            Vector3 oldPosition = vert;
            Vector3 direction = vert - selectedCenter;
            Vector3 newPosition = selectedCenter + direction * scale;

            if (ModelSettings.axis.X == 0)
                newPosition.X = oldPosition.X;
            if (ModelSettings.axis.Y == 0)
                newPosition.Y = oldPosition.Y;
            if (ModelSettings.axis.Z == 0)
                newPosition.Z = oldPosition.Z;

            vert.SetPosition(newPosition);
        }

        scale = 1;

        if (!CanGenerateBuffers)
            return;

        Mesh.Init();
        Mesh.UpdateVertices();
    }

    public Vector3 GetSnappingMovement()
    {
        Camera camera = Game.camera;

        Vector2 mouseDelta = Input.GetMouseDelta() * (GameTime.DeltaTime * 10);
        Vector3 move = camera.right * mouseDelta.X + camera.up * -mouseDelta.Y;

        move *= ModelSettings.axis;
        if (move.Length == 0) return Vector3.Zero;
        
        if (ModelSettings.Snapping)
        {
            Vector3 Offset = Vector3.Zero;

            Vector3 snappingOffset = ModelSettings.SnappingOffset;
            float snappingFactor = ModelSettings.SnappingFactor;

            snappingOffset += move;
            if (snappingOffset.X > snappingFactor)
            {
                Offset.X = snappingFactor;
                snappingOffset.X -= snappingFactor;
            }
            if (snappingOffset.X < -snappingFactor)
            {
                Offset.X = -snappingFactor;
                snappingOffset.X += snappingFactor;
            }
            if (snappingOffset.Y > snappingFactor)
            {
                Offset.Y = snappingFactor;
                snappingOffset.Y -= snappingFactor;
            }
            if (snappingOffset.Y < -snappingFactor)
            {
                Offset.Y = -snappingFactor;
                snappingOffset.Y += snappingFactor;
            }
            if (snappingOffset.Z > snappingFactor)
            {
                Offset.Z = snappingFactor;
                snappingOffset.Z -= snappingFactor;
            }
            if (snappingOffset.Z < -snappingFactor)
            {
                Offset.Z = -snappingFactor;
                snappingOffset.Z += snappingFactor;
            }

            ModelSettings.SnappingOffset = snappingOffset;
        
            move = Offset;
        }

        if (ModelSettings.GridAligned)
        {

        }

        return move;
    }

    public void Handle_VertexMerging()
    {        
        if (SelectedVertices.Count < 2)
            return;

        StashMesh();

        ModelMesh modelMesh = Mesh;

        modelMesh.MergeVertices(SelectedVertices);
                
        Vertex first = SelectedVertices[0];
        SelectedVertices = [first];
                
        Ui.Clear();
                
        regenerateVertexUi = true;
    }

    public void Handle_VertexSpliting()
    {
        if (SelectedVertices.Count == 0)
            return;

        StashMesh();

        foreach (var vert in SelectedVertices)
        {
            SplitVertex(vert);
        }

        if (!CanGenerateBuffers)
            return;

        Mesh.RecalculateNormals();
        Mesh.Init();
        Mesh.GenerateBuffers();

        UpdateVertexPosition();
        GenerateVertexColor();
    }   

    public void Handle_SeperateSelection()
    {
        StashMesh();

        Handle_Copy();
        HandleTriangleDeletion();
        Handle_Paste(false);
    }

    public void SplitVertex(Vertex vertex)
    {
        List<Triangle> triangles = [.. vertex.ParentTriangles];
        foreach (var tris in triangles)
        {
            bool replace = false;
            Vertex replacement = new Vertex(vertex + ((tris.Center() - vertex).Normalized() * 0.1f));

            if (tris.AB.Has(vertex) && tris.AB.ParentTriangles.Count > 1)
            {
                Edge ab = new(tris.AB);
                tris.AB = ab;
                Mesh.EdgeList.Add(ab);
                replace = true;
            }
            if (tris.BC.Has(vertex) && tris.BC.ParentTriangles.Count > 1)
            {
                Edge bc = new(tris.BC);
                tris.BC = bc;
                Mesh.EdgeList.Add(bc);
                replace = true;
            }
            if (tris.CA.Has(vertex) && tris.CA.ParentTriangles.Count > 1)
            {
                Edge ca = new(tris.CA);
                tris.CA = ca;
                Mesh.EdgeList.Add(ca);
                replace = true;
            }

            if (replace)
            {
                if (tris.A == vertex)
                    tris.A = replacement;
                else if (tris.B == vertex)
                    tris.B = replacement;
                else if (tris.C == vertex)
                    tris.C = replacement;

                tris.SetVertexTo(vertex, replacement);
                Mesh.AddVertex(replacement, false);
            }
        }

        Mesh.RemoveVertex(vertex);

        if (!CanGenerateBuffers)
            return;

        Mesh.Init();
        Mesh.RecalculateNormals();
        Mesh.GenerateBuffers();

        UpdateVertexPosition();
        GenerateVertexColor();
    }

    public void Handle_GenerateNewFace()
    {
        if (selectionType != RenderType.Vertex || SelectedVertices.Count > 4)
            return;

        StashMesh();

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

        if (!CanGenerateBuffers)
            return;

        Mesh.Init();
        Mesh.GenerateBuffers();

        GenerateVertexColor();
    }

    public void Handle_FlipSelection()
    {
        StashMesh();
        Vector3 center = GetSelectedCenter();

        foreach (var vert in SelectedVertices)
        {
            Vector3 centeredPosition = vert - center;
            centeredPosition.X *= ModelSettings.axis.X == 1 ? -1 : 1;
            centeredPosition.Y *= ModelSettings.axis.Y == 1 ? -1 : 1;
            centeredPosition.Z *= ModelSettings.axis.Z == 1 ? -1 : 1;
            vert.SetPosition(center + centeredPosition);
        }

        if (!CanGenerateBuffers)
            return;

        Mesh.RecalculateNormals();
        Mesh.Init();
        Mesh.UpdateMesh();

        GenerateVertexColor();
        UpdateVertexPosition();
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
        if (!CanStash)
            return;

        string fileName = Editor.currentModelName + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff");
        string folderPath = Path.Combine(Game.undoModelPath, Editor.currentModelName);

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        if (Editor.MeshSaveNames.Count >= maxCount)
        {
            string name = Editor.MeshSaveNames[0];
            string path = Path.Combine(folderPath, name + ".model");
            if (!File.Exists(path))
                throw new FileNotFoundException($"File {path} not found");

            File.Delete(path);
            Editor.MeshSaveNames.RemoveAt(0);
        }
        Console.WriteLine("Stashing mesh");
        Editor.MeshSaveNames.Add(fileName);
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
        if (Editor.MeshSaveNames.Count == 0)
            return;

        string name = Editor.MeshSaveNames[^1];
        string path = Path.Combine(Path.Combine(Game.undoModelPath, Editor.currentModelName), $"{name}.model");

        Console.WriteLine(path);

        if (!File.Exists(path))
            return;

        Console.WriteLine("Getting last mesh");
        
        Editor.MeshSaveNames.RemoveAt(Editor.MeshSaveNames.Count - 1);
        Mesh.LoadModel(name, Path.Combine(Game.undoModelPath, Editor.currentModelName));
    }

    // Helper
    public HashSet<Edge> GetSelectedFullEdges()
    {
        HashSet<Edge> edges = [];
                
        foreach (var vert in SelectedVertices)
        {
            foreach (var edge in vert.ParentEdges)
            {
                if (SelectedVertices.Contains(edge.Not(vert)))
                    edges.Add(edge);
            }
        }

        return edges;
    }

    public HashSet<Triangle> GetSelectedFullTriangles()
    {
        HashSet<Triangle> triangles = [];
                
        foreach (var triangle in GetSelectedTriangles())
        {
            if (IsTriangleFullySelected(triangle))
                triangles.Add(triangle);
        }
        
        return triangles;
    }

    public HashSet<Triangle> GetSelectedTriangles()
    {
        HashSet<Triangle> triangles = [];
                
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

    public List<Vertex> GetVertices(List<Triangle> triangles)
    {
        List<Vertex> vertices = [];

        foreach (var triangle in triangles)
        {
            if (!vertices.Contains(triangle.A))
                vertices.Add(triangle.A);

            if (!vertices.Contains(triangle.B))
                vertices.Add(triangle.B);

            if (!vertices.Contains(triangle.C))
                vertices.Add(triangle.C);
        }

        return vertices;
    }

    public List<Edge> GetEdges(List<Triangle> triangles)
    {
        List<Edge> edges = [];

        foreach (var triangle in triangles)
        {
            if (!edges.Contains(triangle.AB))
                edges.Add(triangle.AB);

            if (!edges.Contains(triangle.BC))
                edges.Add(triangle.BC);

            if (!edges.Contains(triangle.CA))
                edges.Add(triangle.CA);
        }

        return edges;
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
            center += vert;
        }
        return center / SelectedVertices.Count;
    }

    public Vector3 GetAverageNormal(List<Triangle> triangles)
    {
        Vector3 normal = (0, 1, 0);
        if (triangles.Count == 0)
            return normal;

        foreach (var triangle in triangles)
        {
            normal += triangle.Normal;
        }
        return normal / triangles.Count;
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