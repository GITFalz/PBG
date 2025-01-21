using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class ModelingEditor : BaseEditor
{
    public bool _started = false;

    public override void Start(GeneralModelingEditor editor)
    {
        Console.WriteLine("Start Modeling Editor");

        editor.model.SwitchState("Modeling");

        if (!_started)
        {
            _started = true;
        }
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
                editor.camera.firstMove = true;
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
            editor.camera.Update();
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
            editor.model.Mesh.CheckUselessTriangles();
            editor.model.Mesh.CombineDuplicateVertices();
            editor.model.Mesh.RecalculateNormals();
            editor.model.Mesh.InitModel();
            editor.model.Mesh.UpdateMesh();
            
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
                if (!Input.IsKeyDown(Keys.LeftShift))
                    editor._selectedVertices.Clear();
                
                Vector2 mousePos = Input.GetMousePosition();
                Vector2? closest = null;
                Vertex? closestVert = null;
            
                System.Numerics.Matrix4x4 projection = editor.camera.GetNumericsProjectionMatrix();
                System.Numerics.Matrix4x4 view = editor.camera.GetNumericsViewMatrix();
            
                foreach (var vert in editor.model.Mesh.VertexList)
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
        }
    }

    public override void Render(GeneralModelingEditor editor)
    {
        editor.RenderModel();
    }

    public override void Exit(GeneralModelingEditor editor)
    {
        editor.camera.SetSmoothFactor(true);
        editor.camera.SetPositionSmoothFactor(true);
        
        editor.gameObject.Scene.SaveUi();
    }
}

public abstract class Undoable
{
    
}

public class AngleUndo(Vector3 oldAngle) : Undoable
{
    public Vector3 OldAngle = oldAngle;
}