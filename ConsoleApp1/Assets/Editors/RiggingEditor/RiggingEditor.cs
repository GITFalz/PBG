using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class RiggingEditor : BaseEditor
{
    public Camera camera;
    public ShaderProgram _shaderProgram;
    public UIController BoneUi = new UIController();
    public Model model;

    private bool _started = false;

    public override void Start(GeneralModelingEditor editor)
    {
        Console.WriteLine("Start Rigging Editor");

        editor.model.SwitchState("Rigging");
        
        if (!_started)
        {
            model = editor.model;
            Bone rootBone = new Bone("root");
            rootBone.Pivot = (1, 0, -1);
            rootBone.End = (1, 2, -1);
            //model.Bones.Add(rootBone);

            BoneUi.Generate();
            _started = true;
        }
    }

    public override void Awake(GeneralModelingEditor editor)
    {

    }
    
    public override void Render(GeneralModelingEditor editor)
    {
        editor.RenderRigging();
        
        BoneUi.Render();
    }

    public override void Update(GeneralModelingEditor editor)
    {
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

        if (editor.freeCamera)
        {
            editor.camera.Update();
        }

        if (editor.freeCamera && !editor.regenerateVertexUi)
        {
            Console.WriteLine("Clear Vertex UI");
            editor.Ui.Clear();
            BoneUi.Clear();
            editor.regenerateVertexUi = true;
        }
        
        if (!editor.freeCamera)
        {
            if (editor.regenerateVertexUi)
            {
                Console.WriteLine("Regenerate Vertex UI");
                editor.GenerateVertexPanels();
                // Bone Selection
                var links = editor.GetLinkPositions(model.Bones);
                foreach (var link in links)
                {
                    BoneUi.AddStaticElement(editor.GeneratePanelLink(link.A, link.B));

                    StaticPanel panel1 = UI.CreateStaticPanel(link.A.ToString(), AnchorType.TopLeft, PositionType.Free, new Vector3(20, 20, 0), new Vector4(0, 0, 0, 0), null);
                    panel1.SetPosition(new Vector3(link.A.X, link.A.Y, 0));
                    panel1.TextureIndex = 3;

                    StaticPanel panel2 = UI.CreateStaticPanel(link.B.ToString(), AnchorType.TopLeft, PositionType.Free, new Vector3(20, 20, 0), new Vector4(0, 0, 0, 0), null);
                    panel2.SetPosition(new Vector3(link.B.X, link.B.Y, 0));
                    panel2.TextureIndex = 3;
                    
                    BoneUi.AddStaticElement(panel1);
                    BoneUi.AddStaticElement(panel2); 
                }
                
                BoneUi.Generate();
                editor.regenerateVertexUi = false;
            }
            
            if (Input.IsMousePressed(MouseButton.Left))
            {
                if (!Input.IsKeyDown(Keys.LeftShift))
                    editor._selectedVertices.Clear();
                
                // Vertex Selection
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

    public override void Exit(GeneralModelingEditor editor)
    {
        editor.model.CurrentAnimation = null;
    }


    private void VertexPanels()
    {}
}