using System.Reflection.Metadata;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SharpGen.Runtime;

public class ModelingEditor : BaseEditor
{
    public ModelingBase CurrentMode;

    public ModelingSelectionMode SelectionMode;
    public ModelingEditingMode EditingMode;

    public Vector2 oldMousePos = Vector2.Zero;
    public bool renderSelection = false;

    // Selection Rendering
    public static ShaderProgram selectionShader = new ShaderProgram("Selection/Selection.vert", "Selection/Selection.frag");
    public static VAO selectionVao = new();

    public bool CanStash = true;
    public bool CanGenerateBuffers = true;


    public UIController ModeController;
    public bool ModeClicked = false;

    public UIVerticalCollection ModeCollection;


    public ModelingEditor(GeneralModelingEditor editor) : base(editor)
    {
        Editor = editor;

        SelectionMode = new ModelingSelectionMode(this);
        EditingMode = new ModelingEditingMode(this);

        CurrentMode = EditingMode;

        ModeController = new UIController();

        UIVerticalCollection collection = new("ModeCollection", ModeController, AnchorType.TopLeft, PositionType.Absolute, (0, 0, 0), (200, 30), (5, 60, 0, 0), (0, 0, 0, 0), 5f, 0f);

        UICollection modeCollection = new("TypeCollection", ModeController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (200, 30), (0, 0, 0, 0), 0f);

        UIButton background = new("Background", ModeController, AnchorType.ScaleFull, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (200, 30), (0, 0, 0, 0), 0f, 1, (10f, 0.05f), UIState.Interactable);
        background.SetOnClick(() => 
        {
            ModeCollection?.SetVisibility(!ModeCollection.Visible);
            ModeClicked = true;
        });

        UIText editModeText = new("EditModeText", ModeController, AnchorType.MiddleCenter, PositionType.Relative, (0.6f, 0.6f, 0.6f, 1f), (0, 0, 0), (200, 30), (0, 0, 0, 0), 0);
        editModeText.SetTextCharCount("Edit", 1f);

        UIText selectText = new("SelectText", ModeController, AnchorType.MiddleCenter, PositionType.Relative, (0.6f, 0.6f, 0.6f, 1f), (0, 0, 0), (200, 30), (0, 0, 0, 0), 0f);
        selectText.SetTextCharCount("Select", 1f);
        selectText.SetVisibility(false);

        modeCollection.AddElements(background, editModeText, selectText);

        ModeCollection = new UIVerticalCollection("ModeCollection", ModeController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (200, 30), (0, 0, 0, 0), (0, 0, 0, 0), 5f, 0f);

        UITextButton editButton = new("EditButton", ModeController, AnchorType.TopCenter, PositionType.Relative, (0.6f, 0.6f, 0.6f), (0, 0, 0), (200, 30), (0, 0, 0, 0), 0f, 0, (10f, 0.05f), UIState.Interactable);
        editButton.SetTextCharCount("Edit", 1f);
        editButton.SetOnClick(() =>
        {
            ModeCollection.SetVisibility(false);
            SwitchMode(EditingMode);
            editModeText.SetVisibility(true);
            selectText.SetVisibility(false);
        });

        UITextButton selectButton = new("SelectButton", ModeController, AnchorType.TopCenter, PositionType.Relative, (0.6f, 0.6f, 0.6f), (0, 0, 0), (200, 30), (0, 0, 0, 0), 0f, 0, (10f, 0.05f), UIState.Interactable);
        selectButton.SetTextCharCount("Select", 1f);
        selectButton.SetOnClick(() =>
        {
            ModeCollection.SetVisibility(false);
            SwitchMode(SelectionMode);
            editModeText.SetVisibility(false);
            selectText.SetVisibility(true);
        });

        ModeCollection.AddElements(editButton, selectButton);
        ModeCollection.SetVisibility(false);

        collection.AddElements(modeCollection, ModeCollection);

        ModeController.AddElements(collection);

        ModelManager.LoadModel("cube");
    }

    public void SwitchMode(ModelingBase mode)
    {
        CurrentMode.Exit();
        CurrentMode = mode;
        CurrentMode.Start();
    }

    public override void Start()
    {
        Console.WriteLine("Start Modeling Editor");

        CurrentMode.Start();

        if (Editor.freeCamera)
        {
            Game.Instance.CursorState = CursorState.Grabbed;
            Game.camera.Unlock();
        }
        else
        {
            Game.Instance.CursorState = CursorState.Normal;
            Game.camera.Lock();
        }
    }

    public override void Resize()
    {
        ModeController.Resize();
        SelectionMode.Resize();
        EditingMode.Resize();
    }

    public override void Awake()
    {
        Console.WriteLine("Awake Modeling Editor");

        Editor.LoadAction = Editor.LoadModel;
        Editor.SaveAction = Editor.SaveModel;

        ModelSettings.WireframeVisible = true;

        if (Model == null)
            return;

        Model.SetModeling();
    }
    
    public override void Update()
    {
        ModeController.Update();

        if (Input.IsMousePressed(MouseButton.Left) && !ModeClicked)
        {
            ModeCollection.SetVisibility(false);
        }

        if (Model == null)
            return;

        renderSelection = false;

        CurrentMode.Update();

        if (Input.IsKeyPressed(Keys.Escape))
        {
            Editor.freeCamera = !Editor.freeCamera;
            
            if (Editor.freeCamera)
            {
                Game.Instance.CursorState = CursorState.Grabbed;
                Game.camera.Unlock();
            }
            else
            {
                Game.Instance.CursorState = CursorState.Normal;
                Game.camera.Lock();
                Model.UpdateVertexPosition();
            }
        }
        
        if (!Editor.freeCamera)
        {   
            if (Input.IsMousePressed(MouseButton.Left))
            {
                oldMousePos = Input.GetMousePosition();
            }

            if (Input.IsMouseDown(MouseButton.Left) && !blocked)
            {
                renderSelection = true;
                
                Vector2 mousePos = Input.GetMousePosition();
                Vector2 max = Mathf.Max(mousePos, oldMousePos);
                Vector2 min = Mathf.Min(mousePos, oldMousePos); 
                float distance = Vector2.Distance(mousePos, oldMousePos);
                bool regenColor = false;

                if (distance < 5)
                    return;

                foreach (var vert in Model.Vertices)
                {
                    Vector2 vPos = vert.Value;
                    if (vPos.X >= min.X && vPos.X <= max.X && vPos.Y >= min.Y && vPos.Y <= max.Y)
                    {
                        if (!Model.SelectedVertices.Contains(vert.Key))
                        {
                            regenColor = true;
                            Model.SelectedVertices.Add(vert.Key);
                        }
                    }
                    else
                    {
                        if (!Input.IsKeyDown(Keys.LeftShift) && Model.SelectedVertices.Contains(vert.Key))
                        {
                            regenColor = true;
                            Model.SelectedVertices.Remove(vert.Key);
                        }
                    }
                }

                if (regenColor)
                    Model.GenerateVertexColor();
            }
        }

        ModeClicked = false;
    }

    public override void Render()
    {
        Editor.RenderModel();

        CurrentMode.Render();

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

        ModeController.RenderDepthTest();
    }

    public override void Exit()
    {
        Game.camera.SetSmoothFactor(true);
        Game.camera.SetPositionSmoothFactor(true);
        ClearStash();
    }

    public static void SwitchSelection(RenderType st)
    {
        
        PopUp.AddPopUp($"Switched to {st} Selection");
        ModelingEditingMode.selectionType = st;
    }


    public void Handle_Undo()
    {
        if (Model == null)
            return;
            
        GetLastMesh();
        
        Model.Mesh.Init();
        Model.Mesh.RecalculateNormals();
        Model.Mesh.GenerateBuffers();

        Model.UpdateVertexPosition();
        Model.GenerateVertexColor();
    }

    public void Handle_Copy()
    {
        if (Model == null)
            return;

        ModelCopy.CopyInto(Model.Copy, Model.SelectedVertices);
    }

    // Paste
    public void Handle_Paste(bool stash = true)
    {
        if (Model == null)
            return;
            
        if (stash)
            StashMesh();

        ModelCopy copy = Model.Copy.Copy();
        Model.SelectedVertices = copy.newSelectedVertices;

        Model.Mesh.AddCopy(copy);

        if (!CanGenerateBuffers)
            return;

        Model.Mesh.Init();
        Model.Mesh.GenerateBuffers();
        Model.UpdateVertexPosition();
        Model.GenerateVertexColor();
    }

    public static void Paste(ModelCopy copy, ModelMesh mesh)
    {
        mesh.AddCopy(copy.Copy());
    }


    public Vector3 GetSnappingMovement()
    {
        Camera camera = Game.camera;

        Vector2 mouseDelta = Input.GetMouseDelta() * (GameTime.DeltaTime * 10);
        Vector3 move = camera.right * mouseDelta.X + camera.up * -mouseDelta.Y;

        move *= ModelSettings.Axis;
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

    // Stashing
    public void StashMesh(int maxCount = 30)
    {
        if (Model == null || !CanStash)
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
        Model.Mesh.SaveModel(fileName, folderPath);
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
        if (Model == null || Editor.MeshSaveNames.Count == 0)
            return;

        string name = Editor.MeshSaveNames[^1];
        string path = Path.Combine(Path.Combine(Game.undoModelPath, Editor.currentModelName), $"{name}.model");

        Console.WriteLine(path);

        if (!File.Exists(path))
            return;

        Console.WriteLine("Getting last mesh");
        
        Editor.MeshSaveNames.RemoveAt(Editor.MeshSaveNames.Count - 1);
        Model.Mesh.LoadModel(name, Path.Combine(Game.undoModelPath, Editor.currentModelName));
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