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


    public ModelCopy randomCopy = new();

    
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
            if (Input.IsKeyPressed(Keys.M)) Handle_Mapping();

            if (Input.IsKeyPressed(Keys.T))
            {
                Vector3 min = (float.MaxValue, float.MaxValue, float.MaxValue);
                Vector3  max = (float.MinValue, float.MinValue, float.MinValue);

                foreach (var vert in Mesh.VertexList)
                {
                    min = Mathf.Min(min, vert);
                    max = Mathf.Max(max, vert);
                }

                Vector3 bSize = max - min;
                float largestSide = Mathf.Max(bSize.X, bSize.Z);

                // Set the uvs to the new positions / largestSide

                // To apply the uvs we need to:
                // 1. Store the uvs in a list, delete all the triangles we have now, and paste the model we copied at the start of the function
                // 2. Set the uvs in the pasted triangles
                // This is possible because the order of the triangles hasn't changed inside the mesh so the setting of the uvs will be correct

                List<(Vector2, Vector2, Vector2)> triangleUvs = [];

                foreach (var triangle in Mesh.TriangleList)
                {
                    Vector2 uvA = (triangle.A.X / largestSide, triangle.A.Z / largestSide);
                    Vector2 uvB = (triangle.B.X / largestSide, triangle.B.Z / largestSide);
                    Vector2 uvC = (triangle.C.X / largestSide, triangle.C.Z / largestSide);

                    triangleUvs.Add((uvA, uvB, uvC));
                }

                Mesh.Unload();

                Mesh.AddCopy(randomCopy);

                for (int i = 0; i < Mesh.TriangleList.Count; i++)
                {
                    Triangle triangle = Mesh.TriangleList[i];
                    triangle.UvA = triangleUvs[i].Item1;
                    triangle.UvB = triangleUvs[i].Item2;
                    triangle.UvC = triangleUvs[i].Item3;
                }

                Mesh.CheckUselessVertices();

                Mesh.RecalculateNormals();
                Mesh.Init();
                Mesh.GenerateBuffers();

                UpdateVertexPosition();
                GenerateVertexColor();
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
        Mesh.RecalculateNormals();
        Mesh.GenerateBuffers();

        UpdateVertexPosition();
        GenerateVertexColor();
    }

    public void Handle_Copy()
    {
        ModelCopy.CopyInto(Copy, SelectedVertices);
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

    public static void Paste(ModelCopy copy, ModelMesh mesh)
    {
        mesh.AddCopy(copy.Copy());
    }

    public void Handle_Flattening()
    {
        Model.Handle_Flattening(Model.GetFullSelectedTriangles(SelectedVertices));

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

        List<Edge> edges = Model.GetFullSelectedEdges(SelectedVertices);
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
        List<Triangle> triangles = Model.GetFullSelectedTriangles(SelectedVertices);

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

    public static bool TriangleDeletion(ModelMesh mesh, List<Vertex> selectedVertices)
    {
        List<Triangle> triangles = Model.GetFullSelectedTriangles(selectedVertices);

        if (triangles.Count > 0)
        {
            foreach (var triangle in triangles)
            {
                mesh.RemoveTriangle(triangle);
            }
            selectedVertices.Clear();

            return true;
        }
        return false;
    }



    public void Handle_FlipTriangleNormal()
    {
        StashMesh();

        List<Triangle> triangles = Model.GetFullSelectedTriangles(SelectedVertices);
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

    public void Handle_Mapping()
    {
        StashMesh();

        Handle_SelectAllVertices();

        CanStash = false;
        CanGenerateBuffers = false;

        List<Triangle> triangles = Model.GetFullSelectedTriangles(SelectedVertices);
        Dictionary<string, Triangle> trianglesDict = [];
        List<BoundingBoxRegion> boundingBoxes = [];

        for (int i = 0; i < triangles.Count; i++) { triangles[i].ID = i.ToString(); trianglesDict.Add(i.ToString(), triangles[i]); }

        ModelMesh tempMesh = new();

        Vector3 offset = (0, 0, 0);

        Vector3 min = Vector3.Zero;
        Vector3 max = Vector3.Zero;

        ModelCopy copy = new ModelCopy(SelectedVertices);

        // Basic flattening and packing
        while (triangles.Count > 0)
        {
            // Get a region of triangles from a mesh to flatten out
            SelectedTriangles = triangles[0].GetTriangleRegion([]).ToList();
            SelectedVertices = Model.GetVertices(SelectedTriangles);
            triangles.RemoveAll(t => SelectedTriangles.Contains(t));

            // Remove region from the original mesh and flatten the region
            Handle_SeperateSelection();
            MoveSelectedVertices(offset);
            Handle_Flattening();


            // if any vertices are present:
            // 1. Flip the region if the normal of the first triangle is facing down
            // 2. Get the smallest possible bounding box of the selected region and rotate it if needed before moving it next to the last one
            if (SelectedVertices.Count != 0)
            {
                // Flip the region if the normal of the first triangle is facing down
                List<Triangle> tris = Model.GetFullSelectedTriangles(SelectedVertices);
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

                // Get the smallest possible bounding box of the selected region and rotate it if needed before moving it next to the last one
                Mathf.GetSmallestBoundingBox(SelectedVertices, out min, out max);
                Vector3 size = max - min;
                Vector3 vOffset = (offset.X - min.X, 0, -min.Z);
                BoundingBoxRegion region = new BoundingBoxRegion(min + vOffset, max + vOffset, [.. SelectedVertices]);
                boundingBoxes.Add(region);

                foreach (var vert in SelectedVertices)
                {
                    vert.MovePosition(vOffset);
                }

                offset.X += size.X + 1;
            }
        }

        Handle_SelectAllVertices();
        foreach (var tris in Model.GetFullSelectedTriangles(SelectedVertices))
            trianglesDict[tris.ID] = tris;

        // Better packing algorithm

        // Calculate the approximate volume of the packed regions
        float approximateVolume = 0;
        foreach (var region in boundingBoxes)
        {
            Vector2 size = (region.Max.X - region.Min.X + 2, region.Max.Z - region.Min.Z + 2); // +2 to avoid overlapping
            float volume = size.X * size.Y;
            approximateVolume += volume;
        }

        double sideLength = Math.Sqrt(approximateVolume) + 2; // +2 to avoid overlapping

        // Packing algorithm
        bool packed = false;
        while (!packed)
        {
            bool needsPacking = false;

            // Get the last region and remove it from the list so we can test it against the others
            BoundingBoxRegion last = boundingBoxes[^1];
            boundingBoxes.RemoveAt(boundingBoxes.Count - 1);
            Vector3 lastSize = last.Size;

            Vector3 bestMin = Vector3.Zero;
            int bestIndex = 0;
            bool foundAtLeastOne = false;

            for (int i = 0; i < boundingBoxes.Count; i++)
            {
                min = boundingBoxes[i].Min;
                max = boundingBoxes[i].Max;

                if (min.X >= sideLength) // If the region is too far to the right, skip it
                {
                    needsPacking = true;
                    continue;
                }

                Vector3 testMinLeft = (min.X, 0, max.Z + 2); // Tesing the region above the current one on the left side
                Vector3 testMinRight = (max.X - lastSize.X, 0, max.Z + 2); // Tesing the region above the current one on the right side

                bool collidingLeft = false;
                for (int j = 0; j < boundingBoxes.Count; j++)
                {
                    if (i == j)
                        continue;
                    
                    last.SetMin(testMinLeft);
                    var bB = boundingBoxes[j];

                    if (last & bB)
                    {
                        collidingLeft = true;
                        break;
                    }
                }

                if (!collidingLeft)
                {
                    bestIndex = i;

                    if (!foundAtLeastOne || testMinLeft.Z < bestMin.Z) // Only set the best index if it is the first one or if it is smaller than the current best
                    {
                        bestMin = testMinLeft;
                        foundAtLeastOne = true;
                    }
                    continue;
                }

                if (max.X < sideLength) // Only test the right side if the region is not too far to the right
                {
                    bool collidingRight = false;
                    for (int j = 0; j < boundingBoxes.Count; j++)
                    {
                        if (i == j)
                            continue;
                        
                        last.SetMin(testMinRight);
                        var bB = boundingBoxes[j];

                        if (last & bB)
                        {
                            collidingRight = true;
                            break;
                        }
                    }

                    if (!collidingRight)
                    {
                        bestIndex = i;
                        if (!foundAtLeastOne || testMinRight.Z < bestMin.Z)
                        {
                            bestMin = testMinRight;
                            foundAtLeastOne = true;
                        }
                        continue;
                    }
                }
            }

            last.SetMin(bestMin);
            boundingBoxes.Insert(bestIndex, last);

            if (!needsPacking) 
            {
                packed = true;
                break;
            }
        }

        // Move the regions to their new positions
        foreach (var region in boundingBoxes)
        {
            Vector3 o = region.Min - region.OriginalMin;
            foreach (var vert in region.Vertices)
            {
                vert.MovePosition(o);
            }
        }

        // Get the current bounding box
        min = (float.MaxValue, float.MaxValue, float.MaxValue);
        max = (float.MinValue, float.MinValue, float.MinValue);

        Handle_SelectAllVertices();

        foreach (var vert in SelectedVertices)
        {
            min = Mathf.Min(min, vert);
            max = Mathf.Max(max, vert);
        }

        // Move the mesh to 0,0,0
        foreach (var vert in SelectedVertices)
        {
            vert.MovePosition(-min);
        }

        min = Vector3.Zero;
        max -= min;

        Vector3 bSize = max - min;
        float largestSide = Mathf.Max(bSize.X, bSize.Z);

        Mesh.Unload();
        Mesh.AddCopy(copy.Copy());

        Handle_SelectAllVertices();
        triangles = Model.GetFullSelectedTriangles(SelectedVertices);

        foreach (var triangle in triangles)
        {
            Triangle oldTriangle = trianglesDict[triangle.ID];

            triangle.UvA = (oldTriangle.A.X / largestSide, oldTriangle.A.Z / largestSide);
            triangle.UvB = (oldTriangle.B.X / largestSide, oldTriangle.B.Z / largestSide);
            triangle.UvC = (oldTriangle.C.X / largestSide, oldTriangle.C.Z / largestSide);
        }

        CanStash = true;
        CanGenerateBuffers = true;

        Mesh.CheckUselessVertices();

        Mesh.RecalculateNormals();
        Mesh.Init();
        Mesh.GenerateBuffers();

        UpdateVertexPosition();
        GenerateVertexColor();
    }

    public void MoveSelectedVertices(Vector3 move)
    {
        Model.MoveSelectedVertices(move, SelectedVertices);
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
        selectedCenter = Model.GetSelectedCenter(SelectedVertices);
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
        selectedCenter = Model.GetSelectedCenter(SelectedVertices);
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

    public static void SeperateSelection(ModelCopy copy, List<Vertex> vertices, ModelMesh mesh)
    {
        ModelCopy.CopyInto(copy, vertices);
        TriangleDeletion(mesh, vertices);
        Paste(copy, mesh);
    }

    public void SplitVertex(Vertex vertex)
    {
        List<Triangle> triangles = [.. vertex.ParentTriangles];
        foreach (var tris in triangles)
        {
            bool replace = false;
            Vertex replacement = new Vertex(vertex + ((tris.GetCenter() - vertex).Normalized() * 0.1f));

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
        Vector3 center = Model.GetSelectedCenter(SelectedVertices);

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

    // Data
    public readonly List<Vector3> AxisIgnore = new()
    {
        new Vector3(0, 1, 1), // X
        new Vector3(1, 0, 1), // Y
        new Vector3(1, 1, 0), // Z
    };

    private struct BoundingBoxRegion
    {
        public Vector3 Min;
        public Vector3 Max;
        public Vector3 OriginalMin;
        public Vector3 Size;
        public List<Vertex> Vertices;

        public BoundingBoxRegion(Vector3 min, Vector3 max, List<Vertex> vertices)
        {
            Min = min;
            Max = max;
            OriginalMin = min;
            Size = max - min;
            Vertices = vertices;
        }

        public void SetMin(Vector3 min)
        {
            Min = min;
            Max = Min + Size;
        }

        public bool Intersects(BoundingBoxRegion other)
        {
            return (Mathf.Min(Max.X, other.Max.X) >= Mathf.Max(Min.X, other.Min.X)) &&
                (Mathf.Min(Max.Y, other.Max.Y) >= Mathf.Max(Min.Y, other.Min.Y)) &&
                (Mathf.Min(Max.Z, other.Max.Z) >= Mathf.Max(Min.Z, other.Min.Z));
        }

        /// Check if two bounding boxes intersect
        public static bool operator &(BoundingBoxRegion a, BoundingBoxRegion b)
        {
            return a.Intersects(b);
        }
    }
}

public enum RenderType
{
    Vertex = 0,
    Edge = 1,
    Face = 2,
}