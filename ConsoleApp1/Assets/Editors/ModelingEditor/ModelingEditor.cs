using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class ModelingEditor : BaseEditor
{
    public static SelectionType selectionType = SelectionType.Vertex;
    public bool _started = false;

    public static Action<GeneralModelingEditor>? Selection;

    public override void Start(GeneralModelingEditor editor)
    {
        Console.WriteLine("Start Modeling Editor");

        editor.model.SwitchState("Modeling");

        if (!_started)
        {
            _started = true;
        }

        Selection = HandleSelection[(int)selectionType];
    }

    public override void Awake(GeneralModelingEditor editor)
    {
        
    }
    
    public override void Update(GeneralModelingEditor editor)
    {
        if (OldUIController.activeInputField != null)
        {
            if (Input.IsKeyPressed(Keys.Escape) || Input.IsKeyPressed(Keys.Enter))
                OldUIController.activeInputField = null;
            
            return;
        }

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
            editor._selectedVertices.Clear();
            editor.GenerateVertexColor();
        }
        
        if (editor.freeCamera)
        {
            Game.camera.Update();
        }

        if (Input.IsKeyDown(Keys.LeftControl))
        {
            // Select all
            if (Input.IsKeyPressed(Keys.A)) editor.Handle_SelectAllVertices();
            
            // New Face
            if (Input.IsKeyPressed(Keys.F)) editor.Handle_GenerateNewFace();
            
            // Flipping triangle
            if (Input.IsKeyPressed(Keys.I)) editor.Handle_FlipTriangleNormal();
            
            // Deleting triangle
            if (Input.IsKeyPressed(Keys.D)) editor.Handle_TriangleDeletion();
            
            // Merging
            if (Input.IsKeyPressed(Keys.K) && editor._selectedVertices.Count >= 2) editor.Handle_VertexMerging();
        }
        
        // Extrude
        if (Input.IsKeyPressed(Keys.E)) editor.Handle_FaceExtrusion();

        // Move start
        if (Input.IsKeyPressed(Keys.G)) editor.Ui.Clear();
        
        // Moving
        if (Input.IsKeyDown(Keys.E) || Input.IsKeyDown(Keys.G)) editor.Handle_MovingSelectedVertices();

        if (Input.IsKeyReleased(Keys.E))
        {
            ModelSettings.SnappingOffset = Vector3.Zero;
            editor.regenerateVertexUi = true;
        }

        if (Input.IsKeyReleased(Keys.G))
        {
            ModelSettings.SnappingOffset = Vector3.Zero;
            editor.regenerateVertexUi = true;
        }
        
        //Generate panels on top of each vertex
        if (editor.freeCamera && !editor.regenerateVertexUi)
        {
            Console.WriteLine("Clear Vertex UI");
            editor.Ui.Clear();
            editor.regenerateVertexUi = true;
        }
        
        if (!editor.freeCamera)
        {
            if (editor.regenerateVertexUi)
            {
                Console.WriteLine("Regenerate Vertex UI");
                editor.GenerateVertexPanels();
                editor.regenerateVertexUi = false;
            }
            
            if (Input.IsMousePressed(MouseButton.Left))
            {
                Selection?.Invoke(editor);
            }
        }
    }

    public override void Render(GeneralModelingEditor editor)
    {
        editor.RenderModel();
    }

    public override void Exit(GeneralModelingEditor editor)
    {
        Game.camera.SetSmoothFactor(true);
        Game.camera.SetPositionSmoothFactor(true);
    }

    public static void SwitchSelection(SelectionType selectionType)
    {
        PopUp.AddPopUp($"Switched to {selectionType} Selection");
        Selection = HandleSelection[(int)selectionType];
    }

    public static void HandleVertexSelection(GeneralModelingEditor editor)
    {
        if (!Input.IsKeyDown(Keys.LeftShift))
            editor._selectedVertices.Clear();
        
        Vector2 mousePos = Input.GetMousePosition();
        Vector2? closest = null;
        Vertex? closestVert = null;
    
        System.Numerics.Matrix4x4 projection = Game.camera.GetNumericsProjectionMatrix();
        System.Numerics.Matrix4x4 view = Game.camera.GetNumericsViewMatrix();
    
        foreach (var vert in editor.model.modelMesh.VertexList)
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

        if (closestVert != null && !editor._selectedVertices.Remove(closestVert))
            editor._selectedVertices.Add(closestVert);

        editor.GenerateVertexColor();
    }

    public static void HandleEdgeSelection(GeneralModelingEditor editor)
    {
        
    }

    public static void HandleFaceSelection(GeneralModelingEditor editor)
    {

    }

    public static Action<GeneralModelingEditor>[] HandleSelection = 
    [
        HandleVertexSelection,
        HandleEdgeSelection,
        HandleFaceSelection,
    ];
}

public enum SelectionType
{
    Vertex = 0,
    Edge = 1,
    Face = 2,
}

public abstract class Undoable
{
    
}

public class AngleUndo(Vector3 oldAngle) : Undoable
{
    public Vector3 OldAngle = oldAngle;
}