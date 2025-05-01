using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class ModelingEditingMode : ModelingBase 
{
    public static RenderType selectionType = RenderType.Vertex;

    public Action[] Selection = [ () => { }, () => { }, () => { } ];
    public Action[] Extrusion = [ () => { }, () => { }, () => { } ];
    public Func<bool>[] Deletion = [ () => false, () => false, () => false ];

    public Vector3 selectedCenter = Vector3.Zero;
    public float rotation = 0;
    public float scale = 1;

    public bool regenerateVertexUi = true;


    // Ui Elements
    public static UIText BackfaceCullingText;
    public static UIText MeshAlphaText;
    public static UIText MirrorText;
    public static UIText AxisText;

    public UIController ModelingUi;
    
    public ModelingEditingMode(ModelingEditor editor) : base(editor) 
    {
        Selection[0] = HandleVertexSelection;
        Selection[1] = HandleEdgeSelection;
        Selection[2] = HandleFaceSelection;

        Extrusion[0] = HandleVertexExtrusion;
        Extrusion[1] = HandleEdgeExtrusion;
        Extrusion[2] = HandleFaceExtrusion;

        Deletion[0] = HandleVertexDeletion;
        Deletion[1] = HandleEdgeDeletion;
        Deletion[2] = HandleTriangleDeletion;


        ModelingUi = new UIController();

        UICollection mainPanelCollection = new("MainPanelCollection", ModelingUi, AnchorType.ScaleRight, PositionType.Absolute, (0, 0, 0), (250, Game.Height), (-5, 5, 5, 5), 0);

        UIImage mainPanel = new("MainPanel", ModelingUi, AnchorType.ScaleRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (245, Game.Height), (0, 0, 0, 0), 0, 0, (10, 0.05f));

        UIVerticalCollection mainPanelStacking = new("MainPanelStacking", ModelingUi, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (245, 0), (0, 0, 0, 0), (5, 10, 5, 5), 5, 0);



        // Main panel collection
        UICollection cullingCollection = new("CullingCollection", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (225, 20), (0, 0, 0, 0), 0);

        BackfaceCullingText = new("CullingText", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (400, 20), (0, 0, 0, 0), 0);
        BackfaceCullingText.SetText("cull: " + ModelSettings.BackfaceCulling, 1.2f);

        UIButton cullingButton = new("CullingButton", ModelingUi, AnchorType.MiddleRight, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (40, 20), (0, 0, 0, 0), 0, 0, (10, 0.05f), UIState.Static);
        cullingButton.SetOnClick(BackFaceCullingSwitch); 

        cullingCollection.AddElements(BackfaceCullingText, cullingButton);


        // Alpha panel collection
        UICollection alphaCollection = new("AlphaCollection", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (225, 20), (0, 0, 0, 0), 0);

        MeshAlphaText = new("AlphaText", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 20, 0), (400, 20), (0, 0, 0, 0), 0);
        MeshAlphaText.SetText("alpha: " + ModelSettings.MeshAlpha.ToString("F2"), 1.2f);

        UIButton alphaButton = new("AlphaUpButton", ModelingUi, AnchorType.MiddleRight, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (40, 20), (0, 0, 0, 0), 0, 0, (10, 0.05f), UIState.Static);
        alphaButton.SetOnClick(() => { Editor.blocked = true; });
        alphaButton.SetOnHold(AlphaControl);
        alphaButton.SetOnRelease(() => { Editor.blocked = false; });

        alphaCollection.AddElements(MeshAlphaText, alphaButton);


        // Wireframe panel collection
        UICollection wireframeCollection = new("WireframeCollection", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (225, 20), (0, 0, 0, 0), 0);

        UIText WireframeVisibilityText = new("WireframeVisibilityText", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (400, 20), (0, 0, 0, 0), 0);
        WireframeVisibilityText.SetMaxCharCount(12).SetText("frame: " + ModelSettings.WireframeVisible, 1.2f);

        UIButton WireframeVisibilitySwitch = new("WireframeVisibilitySwitch", ModelingUi, AnchorType.MiddleRight, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (40, 20),  (0, 0, 0, 0), 0, 0, (10, 0.05f), UIState.Static);
        WireframeVisibilitySwitch.SetOnClick(() => {
            ModelSettings.WireframeVisible = !ModelSettings.WireframeVisible; 
            WireframeVisibilityText.SetText("frame: " + ModelSettings.WireframeVisible).UpdateCharacters();
        });

        wireframeCollection.AddElements(WireframeVisibilityText, WireframeVisibilitySwitch);


        // Mirror panel collection
        UICollection mirrorStackingCollection = new("MainPanelStacking", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (225, 20), (0, 0, 0, 0), 0);

        UICollection mirrorStacking = new UICollection("MirrorStacking", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (0, 0, 0), (0, 20), (0, 0, 0, 0), 0);

        MirrorText = new UIText("MirrorText", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 60, 0), (200, 20), (0, 0, 0, 0), 0);
        MirrorText.SetMaxCharCount(11).SetText("mirror: " + (ModelSettings.Mirror.X == 1 ? "X" : "-") + (ModelSettings.Mirror.Y == 1 ? "Y" : "-") + (ModelSettings.Mirror.Z == 1 ? "Z" : "-"), 1.2f);
        mirrorStacking.SetScale((MirrorText.Scale.X + 3, 20f));

        UIHorizontalCollection mirrorButtonStacking = new("MirrorButtonStacking", ModelingUi, AnchorType.MiddleRight, PositionType.Relative, (0, 0, 0), (20, 20), (0, 0, 0, 0), (0, 0, 0, 0), 0, 0);

        UIButton mirrorZButton = new("MirrorZButton", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (14, 20), (0, 0, 0, 0), 0, -1, (0, 0), UIState.InvisibleInteractable);
        mirrorZButton.SetOnClick(() => SwitchMirror("Z"));

        UIButton mirrorYButton = new("MirrorYButton", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (14, 20), (0, 0, 0, 0), 0, -1, (0, 0), UIState.InvisibleInteractable);
        mirrorYButton.SetOnClick(() => SwitchMirror("Y"));

        UIButton mirrorXButton = new("MirrorXButton", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (14, 20), (0, 0, 0, 0), 0, -1, (0, 0), UIState.InvisibleInteractable);
        mirrorXButton.SetOnClick(() => SwitchMirror("X"));

        mirrorButtonStacking.AddElements(mirrorXButton, mirrorYButton, mirrorZButton);

        mirrorStacking.AddElements(MirrorText, mirrorButtonStacking);

        UIButton mirrorButton = new("MirrorButton", ModelingUi, AnchorType.MiddleRight, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (40, 20), (0, 0, 0, 0), 0, 0, (10, 0.05f), UIState.Static);
        mirrorButton.SetOnClick(ApplyMirror);

        mirrorStackingCollection.AddElements(mirrorStacking, mirrorButton);



        // Axis panel collection
        UICollection axisStackingCollection = new("AxisStacking", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (225, 20), (0, 0, 0, 0), 0);

        UICollection axisStacking = new UICollection("AxisStacking", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (0, 0, 0), (0, 20), (0, 0, 0, 0), 0);

        AxisText = new UIText("AxisText", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 80, 0), (400, 20), (0, 0, 0, 0), 0);
        AxisText.SetMaxCharCount(9).SetText("axis: " + (ModelSettings.Axis.X == 1 ? "X" : "-") + (ModelSettings.Axis.Y == 1 ? "Y" : "-") + (ModelSettings.Axis.Z == 1 ? "Z" : "-"), 1.2f);
        axisStacking.SetScale((AxisText.newScale.X + 3, 20f));

        UIHorizontalCollection axisButtonStacking = new("AxisButtonStacking", ModelingUi, AnchorType.MiddleRight, PositionType.Relative, (0, 0, 0), (20, 20), (0, 0, 0, 0), (0, 0, 0, 0), 0, 0);

        UIButton axisZButton = new("AxisZButton", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (14, 20), (0, 0, 0, 0), 0, -1, (0, 0), UIState.InvisibleInteractable);
        axisZButton.SetOnClick(() => SwitchAxis("Z"));

        UIButton axisYButton = new("AxisYButton", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (14, 20), (0, 0, 0, 0), 0, -1, (0, 0), UIState.InvisibleInteractable);
        axisYButton.SetOnClick(() => SwitchAxis("Y"));

        UIButton axisXButton = new("AxisXButton", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (14, 20), (0, 0, 0, 0), 0, -1, (0, 0), UIState.InvisibleInteractable);
        axisXButton.SetOnClick(() => SwitchAxis("X"));

        axisButtonStacking.AddElements(axisXButton, axisYButton, axisZButton);

        axisStacking.AddElements(AxisText, axisButtonStacking);

        axisStackingCollection.AddElements(axisStacking);



        // Grid panel collection
        UICollection gridStackingCollection = new("GridStacking", ModelingUi, AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (225, 20), (0, 0, 0, 0), 0);

        UIText GridAlignedText = new("GridAlignedText", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 100, 0), (400, 20), (0, 0, 0, 0), 0);
        GridAlignedText.MaxCharCount = 11;
        GridAlignedText.SetText("grid: " + ModelSettings.GridAligned, 1.2f);

        UIButton gridAlignedButton = new("GridAlignedButton", ModelingUi, AnchorType.MiddleRight, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (40, 20), (0, 0, 0, 0), 0, 0, (10, 0.05f), UIState.Static);
        gridAlignedButton.SetOnClick(() => {
            ModelSettings.GridAligned = !ModelSettings.GridAligned; 
            GridAlignedText.SetText("grid: " + ModelSettings.GridAligned).UpdateCharacters();
        });

        gridStackingCollection.AddElements(GridAlignedText, gridAlignedButton);
        


        // Camera speed panel collection
        UICollection cameraSpeedStacking = new("CameraSpeedStacking", ModelingUi, AnchorType.BottomCenter, PositionType.Relative, (0, 0, 0), (225, 35), (5, 0, 0, 0), 0);

        UIText CameraSpeedTextLabel = new("CameraSpeedTextLabel", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (400, 20), (0, 0, 0, 0), 0);
        CameraSpeedTextLabel.SetTextCharCount("Cam Speed: ", 1.2f);

        UICollection speedStacking = new UICollection("CameraSpeedStacking", ModelingUi, AnchorType.MiddleRight, PositionType.Relative, (0, 0, 0), (0, 20), (0, 0, 0, 0), 0);
        
        UIImage CameraSpeedFieldPanel = new("CameraSpeedTextLabelPanel", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (45, 30), (0, 0, 0, 0), 0, 1, (10, 0.05f));
        
        UIInputField CameraSpeedField = new("CameraSpeedText", ModelingUi, AnchorType.MiddleLeft, PositionType.Relative, (1, 1, 1, 1f), (0, 0, 0), (400, 20), (10, 0, 0, 0), 0, 0, (10, 0.05f));
        
        CameraSpeedField.SetMaxCharCount(2).SetText("50", 1.2f).SetTextType(TextType.Numeric);
        CameraSpeedField.OnTextChange = new SerializableEvent(() => { try { Game.camera.SPEED = int.Parse(CameraSpeedField.Text); } catch { Game.camera.SPEED = 1; } }); 

        speedStacking.SetScale((45, 30f));

        speedStacking.AddElements(CameraSpeedFieldPanel, CameraSpeedField);

        cameraSpeedStacking.AddElements(CameraSpeedTextLabel, speedStacking);


        mainPanelStacking.AddElements(cullingCollection, alphaCollection, wireframeCollection, mirrorStackingCollection, axisStackingCollection, gridStackingCollection);

        mainPanelCollection.AddElements(mainPanel, mainPanelStacking, cameraSpeedStacking);


        // Add elements to ui
        ModelingUi.AddElement(mainPanelCollection);
    }

    public void SwitchMirror(string axis)
    {
        switch (axis)
        {
            case "X":
                ModelSettings.Mirror.X = ModelSettings.Mirror.X == 0 ? 1 : 0;
                break;
            case "Y":
                ModelSettings.Mirror.Y = ModelSettings.Mirror.Y == 0 ? 1 : 0;
                break;
            case "Z":
                ModelSettings.Mirror.Z = ModelSettings.Mirror.Z == 0 ? 1 : 0;
                break;
        }
        
        UpdateMirrorText();
    }

    public void SwitchAxis(string axis)
    {
        switch (axis)
        {
            case "X":
                ModelSettings.Axis.X = ModelSettings.Axis.X == 0 ? 1 : 0;
                break;
            case "Y":
                ModelSettings.Axis.Y = ModelSettings.Axis.Y == 0 ? 1 : 0;
                break;
            case "Z":
                ModelSettings.Axis.Z = ModelSettings.Axis.Z == 0 ? 1 : 0;
                break;
        }
        
        UpdateAxisText();
    }
    
    public void ApplyMirror()
    {
        if (Model == null)
            return;

        Model.Mesh.ApplyMirror();
        Model.Mesh.CombineDuplicateVertices();
        
        Model.Mesh.Init();
        Model.Mesh.GenerateBuffers();
        Model.Mesh.UpdateMesh();
        
        regenerateVertexUi = true; 
        
        ModelSettings.Mirror = new Vector3i(0, 0, 0);
        UpdateMirrorText();
    }
    
    public void AlphaControl()
    {
        float mouseX = Input.GetMouseDelta().X;
        if (mouseX == 0)
            return;
            
        ModelSettings.MeshAlpha += mouseX * GameTime.DeltaTime;
        ModelSettings.MeshAlpha = Mathf.Clamp(0, 1, ModelSettings.MeshAlpha);
        MeshAlphaText.SetText("alpha: " + ModelSettings.MeshAlpha.ToString("F2")).UpdateCharacters();
    }

    public void BackFaceCullingSwitch()
    {
        ModelSettings.BackfaceCulling = !ModelSettings.BackfaceCulling;
        BackfaceCullingText.SetText("cull: " + ModelSettings.BackfaceCulling).UpdateCharacters();
    }

    public void UpdateMirrorText()
    {
        string text = "";
        text += ModelSettings.Mirror.X == 1 ? "X" : "-";
        text += ModelSettings.Mirror.Y == 1 ? "Y" : "-";
        text += ModelSettings.Mirror.Z == 1 ? "Z" : "-";
        
        MirrorText.SetText("mirror: " + text).UpdateCharacters();
    }

    public void UpdateAxisText()
    {
        string text = "";
        text += ModelSettings.Axis.X == 1 ? "X" : "-";
        text += ModelSettings.Axis.Y == 1 ? "Y" : "-";
        text += ModelSettings.Axis.Z == 1 ? "Z" : "-";
        
        AxisText.SetText("axis: " + text).UpdateCharacters();
    }

    public override void Start()
    {
        ModelSettings.WireframeVisible = true;
    }

    public override void Resize()
    {
        ModelingUi.Resize();
    }

    public override void Update()
    {
        ModelingUi.Update();

        if (Model == null)
            return;

        if (!FreeCamera)
        {
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
                if (Input.IsKeyPressed(Keys.K) && Model.SelectedVertices.Count >= 2) Handle_VertexMerging();

                // Split vertices
                if (Input.IsKeyPressed(Keys.Q)) Handle_VertexSpliting();
                
                // Mapping
                if (Input.IsKeyPressed(Keys.M)) Handle_Mapping();

                if (Input.IsKeyPressed(Keys.T)) TestFunction();

                // Seperate selection
                if (Input.IsKeyPressed(Keys.L)) Handle_SeperateSelection();

                // Combining Duplicate Vertices
                if (Input.IsKeyPressed(Keys.G)) CombineDuplicateVertices();

                // Delete model
                if (Input.IsKeyPressed(Keys.Delete)) Model.Delete();
            }
            else
            {
                // Extrude
                if (Input.IsKeyPressed(Keys.E)) Handle_Extrusion();

                // Rotation
                if (Input.IsKeyPressed(Keys.R)) RotationInit();
                if (Input.IsKeyDown(Keys.R)) Handle_RotateSelectedVertices();
                if (Input.IsKeyReleased(Keys.R)) Model.UpdateVertexPosition();

                // Scaling
                if (Input.IsKeyPressed(Keys.S)) ScalingInit();
                if (Input.IsKeyDown(Keys.S)) Handle_ScalingSelectedVertices();
                if (Input.IsKeyReleased(Keys.S)) Model.UpdateVertexPosition();
                
                // Moving
                if (Input.IsKeyPressed(Keys.G)) StashMesh();
                if (Input.IsKeyDown(Keys.E) || Input.IsKeyDown(Keys.G)) Handle_MovingSelectedVertices();

                if (Input.IsKeyReleased(Keys.E) || Input.IsKeyReleased(Keys.G))
                {
                    ModelSettings.SnappingOffset = Vector3.Zero;
                    Model.Mesh.CheckUselessEdges();
                    Model.Mesh.CheckUselessTriangles();
                    regenerateVertexUi = true;
                }
            }

            if (Input.IsMousePressed(MouseButton.Left))
            {
                Selection[(int)selectionType]();
            }
        }

        if (regenerateVertexUi)
        {
            Model.UpdateVertexPosition();
            Model.GenerateVertexColor();
            regenerateVertexUi = false;
        }
    }

    public override void Render()
    {
        ModelingUi.RenderDepthTest();
    }

    public override void Exit()
    {
        
    }


    // Selection
    public void HandleVertexSelection()
    {   
        if (Model == null)
            return;
            
        if (!Input.IsKeyDown(Keys.LeftShift))
            Model.SelectedVertices.Clear();
        
        Vector2 mousePos = Input.GetMousePosition();
        Vector2? closest = null;
        Vertex? closestVert = null;
    
        foreach (var vert in Model.Vertices)
        {
            float distance = Vector2.Distance(mousePos, vert.Value);
            float distanceClosest = closest == null ? 1000 : Vector2.Distance(mousePos, (Vector2)closest);
        
            if (distance < distanceClosest && distance < 10)
            {
                closest = vert.Value;
                closestVert = vert.Key;
            }
        }

        if (closestVert != null && !Model.SelectedVertices.Remove(closestVert))
            Model.SelectedVertices.Add(closestVert);

        Model.GenerateVertexColor();
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
        if (Model == null)
            return;
            
        StashMesh();
        
        Console.WriteLine("Extruding");
        Extrusion[(int)selectionType]();

        if (!CanGenerateBuffers)
            return;

        Model.Mesh.Init();
        Model.Mesh.GenerateBuffers();
        Model.Mesh.UpdateMesh();

        regenerateVertexUi = true;
    }

    public void HandleVertexExtrusion()
    {
        if (Model == null)
            return;
            
        List<Vertex> newVertices = new List<Vertex>();

        foreach (var vertex in Model.SelectedVertices)
        {
            Vertex newVertex = vertex.Copy();
            newVertices.Add(newVertex);

            Model.Mesh.EdgeList.Add(new Edge(vertex, newVertex));
        }

        Model.SelectedVertices.Clear();
        Model.SelectedVertices.AddRange(newVertices);

        Model.GenerateVertexColor();

        Model.Mesh.AddVertices(newVertices);
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
        if (Model == null)
            return;
            
        if (stash)
            StashMesh();
        
        Console.WriteLine("Deletinga");
        if (!Deletion[(int)selectionType]())
            return;

        if (!CanGenerateBuffers)
            return;

        Model.Mesh.Init();
        Model.Mesh.GenerateBuffers();

        regenerateVertexUi = true;
    }
    public bool HandleVertexDeletion()
    {
        if (Model == null)
            return false;
            
        if (Model.SelectedVertices.Count == 0)
            return false;

        foreach (var vert in Model.SelectedVertices)
        {
            Model.Mesh.RemoveVertex(vert);
        }
        Model.SelectedVertices.Clear();

        return true;
    }

    public bool HandleEdgeDeletion()
    {
        if (Model == null)
            return false;
            
        if (Model.SelectedVertices.Count == 0)
            return false;

        List<Edge> edges = Model.GetFullSelectedEdges(Model.SelectedVertices);
        foreach (var edge in edges)
        {
            Model.Mesh.RemoveEdge(edge);
        }

        Model.SelectedVertices.Clear();
        Model.SelectedEdges.Clear();

        return true;
    }

    public bool HandleTriangleDeletion()
    {
        if (Model == null)
            return false;
            
        List<Triangle> triangles = Model.GetFullSelectedTriangles(Model.SelectedVertices);

        if (triangles.Count > 0)
        {
            foreach (var triangle in triangles)
            {
                Model.Mesh.RemoveTriangle(triangle);
            }
            Model.SelectedVertices.Clear();

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
        if (Model == null)
            return;

        StashMesh();

        List<Triangle> triangles = Model.GetFullSelectedTriangles(Model.SelectedVertices);

        if (triangles.Count > 0)
        {
            foreach (var triangle in triangles)
            {
                Vertex A = triangle.A;
                Vertex B = triangle.B;
                if (Model.Mesh.SwapVertices(A, B))
                {
                    triangle.Invert();
                    triangle.UpdateNormal();
                }
            }

            Model.Mesh.Init();
            Model.Mesh.UpdateMesh();
        }
    }

    public void Handle_Mapping()
    {
        if (Model == null)
            return;
            
        StashMesh();

        Handle_SelectAllVertices();

        CanStash = false;
        CanGenerateBuffers = false;

        List<Triangle> triangles = Model.GetFullSelectedTriangles(Model.SelectedVertices);
        Dictionary<string, Triangle> trianglesDict = [];
        List<BoundingBoxRegion> boundingBoxes = [];

        for (int i = 0; i < triangles.Count; i++) { triangles[i].ID = i.ToString(); trianglesDict.Add(i.ToString(), triangles[i]); }

        ModelMesh tempMesh = new(Model);

        Vector3 offset = (0, 0, 0);

        Vector3 min = Vector3.Zero;
        Vector3 max = Vector3.Zero;

        ModelCopy copy = new ModelCopy(Model.SelectedVertices);

        // Basic flattening and packing
        while (triangles.Count > 0)
        {
            // Get a region of triangles from a mesh to flatten out
            Model.SelectedTriangles = triangles[0].GetTriangleRegion([]).ToList();
            Model.SelectedVertices = Model.GetVertices(Model.SelectedTriangles);
            triangles.RemoveAll(t => Model.SelectedTriangles.Contains(t));

            // Remove region from the original mesh and flatten the region
            Handle_SeperateSelection();
            MoveSelectedVertices(offset);
            Handle_Flattening();


            // if any vertices are present:
            // 1. Flip the region if the normal of the first triangle is facing down
            // 2. Get the smallest possible bounding box of the selected region and rotate it if needed before moving it next to the last one
            if (Model.SelectedVertices.Count != 0)
            {
                // Flip the region if the normal of the first triangle is facing down
                List<Triangle> tris = Model.GetFullSelectedTriangles(Model.SelectedVertices);
                if (tris.Count > 0)
                {
                    Triangle first = tris[0];
                    first.UpdateNormal();

                    if (Vector3.Dot(first.Normal, (0, 1, 0)) < 0)
                    {
                        Vector3 center = Model.SelectedVertices[0];
                        for (int i = 1; i < Model.SelectedVertices.Count; i++)
                        {
                            center += Model.SelectedVertices[i];
                        }
                        center /= Model.SelectedVertices.Count;

                        foreach (var vert in Model.SelectedVertices)
                        {
                            vert.SetPosition(Mathf.RotatePoint(vert, center, (1, 0, 0), 180f));
                        }
                    }
                }

                // Get the smallest possible bounding box of the selected region and rotate it if needed before moving it next to the last one
                Mathf.GetSmallestBoundingBox(Model.SelectedVertices, out min, out max);
                Vector3 size = max - min;
                Vector3 vOffset = (offset.X - min.X, 0, -min.Z);
                BoundingBoxRegion region = new BoundingBoxRegion(min + vOffset, max + vOffset, [.. Model.SelectedVertices]);
                boundingBoxes.Add(region);

                foreach (var vert in Model.SelectedVertices)
                {
                    vert.MovePosition(vOffset);
                }

                offset.X += size.X + 1;
            }
        }

        Handle_SelectAllVertices();
        foreach (var tris in Model.GetFullSelectedTriangles(Model.SelectedVertices))
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

        foreach (var vert in Model.SelectedVertices)
        {
            min = Mathf.Min(min, vert);
            max = Mathf.Max(max, vert);
        }

        // Move the mesh to 0,0,0
        foreach (var vert in Model.SelectedVertices)
        {
            vert.MovePosition(-min);
        }

        min = Vector3.Zero;
        max -= min;

        Vector3 bSize = max - min;
        float largestSide = Mathf.Max(bSize.X, bSize.Z);

        Model.Mesh.Unload();
        Model.Mesh.AddCopy(copy.Copy());

        Handle_SelectAllVertices();
        triangles = Model.GetFullSelectedTriangles(Model.SelectedVertices);

        Vector2 uvMin = Vector2.One;
        Vector2 uvMax = Vector2.Zero;

        foreach (var triangle in triangles)
        {
            Triangle oldTriangle = trianglesDict[triangle.ID];

            Vector2 uvA = (oldTriangle.A.X / largestSide, oldTriangle.A.Z / largestSide);
            Vector2 uvB = (oldTriangle.B.X / largestSide, oldTriangle.B.Z / largestSide);
            Vector2 uvC = (oldTriangle.C.X / largestSide, oldTriangle.C.Z / largestSide);

            uvMin = Mathf.Min(uvA, uvB, uvC, uvMin);
            uvMax = Mathf.Max(uvA, uvB, uvC, uvMax); 

            triangle.UvA = uvA;
            triangle.UvB = uvB;
            triangle.UvC = uvC;
        }

        Vector2 uvSize = uvMax - uvMin;
        float smallest = Mathf.Max(uvSize.X, uvSize.Y);
        float multiplier = smallest == 0 ? 1 : 1 / smallest;

        foreach (var triangle in triangles)
        {
            triangle.UvA = (triangle.UvA - uvMin) * multiplier;
            triangle.UvB = (triangle.UvB - uvMin) * multiplier;
            triangle.UvC = (triangle.UvC - uvMin) * multiplier;
        }

        CanStash = true;
        CanGenerateBuffers = true;

        Model.Mesh.CheckUselessVertices();

        Model.Mesh.RecalculateNormals();
        Model.Mesh.Init();
        Model.Mesh.GenerateBuffers();

        regenerateVertexUi = true;
    }

    public void MoveSelectedVertices(Vector3 move)
    {
        if (Model == null)
            return;
            
        Model.MoveSelectedVertices(move, Model.SelectedVertices);
    }


    // Utility

    public void Handle_MovingSelectedVertices()
    {
        if (Model == null)
            return;
            
        Vector3 move = Editor.GetSnappingMovement();
        MoveSelectedVertices(move);

        if (!CanGenerateBuffers)
            return;
        
        Model.Mesh.RecalculateNormals();
        Model.Mesh.Init();
        Model.Mesh.UpdateMesh();
    }

    public void RotationInit()
    {
        if (Model == null)
            return;
            
        StashMesh();
        rotation = 0;
        selectedCenter = Model.GetSelectedCenter(Model.SelectedVertices);
    }

    public void Handle_RotateSelectedVertices()
    {
        if (Model == null)
            return;
            
        if (Model.SelectedVertices.Count == 0)
            return;

        Vector3 axis = Game.camera.front * ModelSettings.Axis;
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

        foreach (var vert in Model.SelectedVertices)
        {
            vert.SetPosition(Mathf.RotatePoint(vert, selectedCenter, axis, rotation));
        }

        rotation = 0;

        if (!CanGenerateBuffers)
            return;

        Model.Mesh.RecalculateNormals();
        Model.Mesh.Init();
        Model.Mesh.UpdateVertices();

        regenerateVertexUi = true;
    }

    public void ScalingInit()
    {
        if (Model == null)
            return;
            
        StashMesh();
        scale = 1;
        selectedCenter = Model.GetSelectedCenter(Model.SelectedVertices);
    }

    public void Handle_ScalingSelectedVertices()
    {
        if (Model == null || Model.SelectedVertices.Count < 2)
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

        foreach (var vert in Model.SelectedVertices)
        { 
            Vector3 oldPosition = vert;
            Vector3 direction = vert - selectedCenter;
            Vector3 newPosition = selectedCenter + direction * scale;

            if (ModelSettings.Axis.X == 0)
                newPosition.X = oldPosition.X;
            if (ModelSettings.Axis.Y == 0)
                newPosition.Y = oldPosition.Y;
            if (ModelSettings.Axis.Z == 0)
                newPosition.Z = oldPosition.Z;

            vert.SetPosition(newPosition);
        }

        scale = 1;

        if (!CanGenerateBuffers)
            return;

        Model.Mesh.Init();
        Model.Mesh.UpdateVertices();
    }

    public void Handle_VertexMerging()
    {        
        if (Model == null || Model.SelectedVertices.Count < 2)
            return;

        StashMesh();

        Model.Mesh.MergeVertices(Model.SelectedVertices);
                
        Vertex first = Model.SelectedVertices[0];
        Model.SelectedVertices = [first];
                
        regenerateVertexUi = true;
    }

    public void Handle_VertexSpliting()
    {
        if (Model == null || Model.SelectedVertices.Count == 0)
            return;

        StashMesh();

        foreach (var vert in Model.SelectedVertices)
        {
            SplitVertex(vert);
        }

        if (!CanGenerateBuffers)
            return;

        Model.Mesh.RecalculateNormals();
        Model.Mesh.Init();
        Model.Mesh.GenerateBuffers();

        regenerateVertexUi = true;
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
        ModelingEditor.Paste(copy, mesh);
    }

    public void SplitVertex(Vertex vertex)
    {
        if (Model == null)
            return;
            
        List<Triangle> triangles = [.. vertex.ParentTriangles];
        foreach (var tris in triangles)
        {
            bool replace = false;
            Vertex replacement = new Vertex(vertex + ((tris.GetCenter() - vertex).Normalized() * 0.1f));

            if (tris.AB.Has(vertex) && tris.AB.ParentTriangles.Count > 1)
            {
                Edge ab = new(tris.AB);
                tris.AB = ab;
                Model.Mesh.EdgeList.Add(ab);
                replace = true;
            }
            if (tris.BC.Has(vertex) && tris.BC.ParentTriangles.Count > 1)
            {
                Edge bc = new(tris.BC);
                tris.BC = bc;
                Model.Mesh.EdgeList.Add(bc);
                replace = true;
            }
            if (tris.CA.Has(vertex) && tris.CA.ParentTriangles.Count > 1)
            {
                Edge ca = new(tris.CA);
                tris.CA = ca;
                Model.Mesh.EdgeList.Add(ca);
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
                Model.Mesh.AddVertex(replacement, false);
            }
        }

        Model.Mesh.RemoveVertex(vertex);

        if (!CanGenerateBuffers)
            return;

        Model.Mesh.Init();
        Model.Mesh.RecalculateNormals();
        Model.Mesh.GenerateBuffers();

        regenerateVertexUi = true;
    }

    public void Handle_GenerateNewFace()
    {
        if (Model == null || selectionType != RenderType.Vertex || Model.SelectedVertices.Count > 4)
            return;

        StashMesh();

        if (Model.SelectedVertices.Count == 2)
        { 
            if (ModelingHelper.Generate_2_Selected(Model.SelectedVertices))
            {
                List<Vertex> nextVertices = [Model.SelectedVertices[2], Model.SelectedVertices[3]];
                ModelingHelper.Generate_4_Selected(Model.SelectedVertices, Model.Mesh);
                Model.SelectedVertices = nextVertices;
            }
        }

        if (Model.SelectedVertices.Count == 3)
        { 
            ModelingHelper.Generate_3_Selected(Model.SelectedVertices, Model.Mesh);
        }

        if (Model.SelectedVertices.Count == 4)
        { 
            ModelingHelper.Generate_4_Selected(Model.SelectedVertices, Model.Mesh);    
        }

        if (!CanGenerateBuffers)
            return;

        Model.Mesh.Init();
        Model.Mesh.GenerateBuffers();

        Model.GenerateVertexColor();
    }

    public void Handle_FlipSelection()
    {
        if (Model == null)
            return;
            
        StashMesh();
        Vector3 center = Model.GetSelectedCenter(Model.SelectedVertices);

        foreach (var vert in Model.SelectedVertices)
        {
            Vector3 centeredPosition = vert - center;
            centeredPosition.X *= ModelSettings.Axis.X == 1 ? -1 : 1;
            centeredPosition.Y *= ModelSettings.Axis.Y == 1 ? -1 : 1;
            centeredPosition.Z *= ModelSettings.Axis.Z == 1 ? -1 : 1;
            vert.SetPosition(center + centeredPosition);
        }

        if (!CanGenerateBuffers)
            return;

        Model.Mesh.RecalculateNormals();
        Model.Mesh.Init();
        Model.Mesh.UpdateMesh();

        regenerateVertexUi = true;
    }

    public void Handle_SelectAllVertices()
    {
        if (Model == null)
            return;

        Model.SelectedVertices.Clear();
                
        foreach (var vert in Model.Mesh.VertexList)
        {
            Model.SelectedVertices.Add(vert);
        }
                
        Model.GenerateVertexColor();
    }

    public void CombineDuplicateVertices()
    {
        if (Model == null)
            return;

        StashMesh();
        Model.Mesh.CombineDuplicateVertices();

        if (!CanGenerateBuffers)
            return;

        Model.Mesh.Init();
        Model.Mesh.RecalculateNormals();
        Model.Mesh.GenerateBuffers();

        regenerateVertexUi = true;
    }

    public void TestFunction()
    {
        if (Model == null)
            return;

        Vector3 min = (float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3  max = (float.MinValue, float.MinValue, float.MinValue);
        
        foreach (var vert in Model.Mesh.VertexList)
        {
            min = Mathf.Min(min, vert);
            max = Mathf.Max(max, vert); 
        }

        Vector3 bSize = max - min;
        float largestSide = Mathf.Max(bSize.X, bSize.Z);

        ModelCopy.CopyInto(Model.randomCopy, Model.Mesh.VertexList);

        // Set the uvs to the new positions / largestSide

        // To apply the uvs we need to:
        // 1. Store the uvs in a list, delete all the triangles we have now, and paste the model we copied at the start of the function
        // 2. Set the uvs in the pasted triangles
        // This is possible because the order of the triangles hasn't changed inside the mesh so the setting of the uvs will be correct

        List<(Vector2, Vector2, Vector2)> triangleUvs = [];

        foreach (var triangle in Model.Mesh.TriangleList)
        {
            Vector2 uvA = (triangle.A.X / largestSide, triangle.A.Z / largestSide);
            Vector2 uvB = (triangle.B.X / largestSide, triangle.B.Z / largestSide);
            Vector2 uvC = (triangle.C.X / largestSide, triangle.C.Z / largestSide);

            triangleUvs.Add((uvA, uvB, uvC));
        }

        Model.Mesh.Unload();

        Model.Mesh.AddCopy(Model.randomCopy);

        for (int i = 0; i < Model.Mesh.TriangleList.Count; i++)
        {
            Triangle triangle = Model.Mesh.TriangleList[i];
            triangle.UvA = triangleUvs[i].Item1;
            triangle.UvB = triangleUvs[i].Item2;
            triangle.UvC = triangleUvs[i].Item3;
        }

        Model.Mesh.CheckUselessVertices();

        Model.Mesh.RecalculateNormals();
        Model.Mesh.Init();
        Model.Mesh.GenerateBuffers();

        regenerateVertexUi = true;
    }

    public void Handle_Flattening()
    {
        if (Model == null)
            return;
            
        Model.Handle_Flattening(Model.GetFullSelectedTriangles(Model.SelectedVertices));

        if (!CanGenerateBuffers)
            return;

        Model.Mesh.Init();
        Model.Mesh.RecalculateNormals();
        Model.Mesh.UpdateVertices();
        
        regenerateVertexUi = true;
    }


    private void StashMesh() => Editor.StashMesh();
    private void Handle_Copy() => Editor.Handle_Copy();
    private void Handle_Paste(bool stash = true) => Editor.Handle_Paste(stash);
    private void Handle_Undo() => Editor.Handle_Undo();
}   