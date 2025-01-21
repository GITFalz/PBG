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
    public OldUIController BoneUi = new OldUIController();
    public Model model;
    public Dictionary<Bone, BoneSelection> SelectedBones = [];

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
        var links = editor.GetLinkPositions(model.Bones);

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
                GenerateBonePanels(links, editor);
                
                BoneUi.Generate();
                editor.regenerateVertexUi = false;
            }
            
            if (Input.IsMousePressed(MouseButton.Left))
            {
                if (!Input.IsKeyDown(Keys.LeftShift))
                {
                    editor._selectedVertices.Clear();
                    SelectedBones.Clear();
                }
                
                Vector2 mousePos = Input.GetMousePosition();
                Vector2? closest = null;
                Vertex? closestVert = null;
                Bone? closestBone = null;
            
                System.Numerics.Matrix4x4 projection = editor.camera.GetNumericsProjectionMatrix();
                System.Numerics.Matrix4x4 view = editor.camera.GetNumericsViewMatrix();

                // Vertex Selection
            
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

                // Bone Selection

                bool pivotSelected = false;
                bool endSelected = false;

                for (int i = 0; i < links.Count; i++)
                {
                    var bone = model.Bones[i];

                    Vector2 linkStart = links[i].A;
                    Vector2 linkEnd = links[i].B;

                    pivotSelected = Vector2.Distance(mousePos, linkStart) < 10;
                    endSelected = Vector2.Distance(mousePos, linkEnd) < 10;

                    if (pivotSelected || endSelected)
                    {   
                        closestBone = bone;
                        break;
                    }

                    if (IsPointCloseToLine(mousePos, linkStart, linkEnd, 10))
                    {
                        closestBone = bone;  
                        pivotSelected = true;
                        endSelected = true;
                        break;
                    }
                }

                if (closestBone != null && !SelectedBones.Remove(closestBone))
                    SelectedBones.Add(closestBone, new BoneSelection(pivotSelected, endSelected));

                GenerateBonePanels(links, editor);
            }
        }

        if (Input.AreAllKeysDown(Keys.LeftControl, Keys.LeftShift))
        {
            if (Input.IsKeyPressed(Keys.D) && SelectedBones.Count > 0)
            {
                var bone = SelectedBones.First();
                Bone parentBone = bone.Key;
                Bone childBone = new Bone(parentBone.RootBone, "child");
                childBone = parentBone.AddChild(childBone);

                BoneSelection childSelection;

                bool endSelected = bone.Value.EndSelected;

                if (!endSelected)
                {
                    childBone.Pivot = parentBone.Pivot;
                    childBone.End = parentBone.Pivot - (parentBone.End - parentBone.Pivot);

                    childSelection = new BoneSelection(true, false);
                }
                else
                {
                    childBone.Pivot = parentBone.End;
                    childBone.End = parentBone.End + (parentBone.End - parentBone.Pivot);

                    childSelection = new BoneSelection(false, true);
                }

                SelectedBones.Remove(parentBone);
                SelectedBones.Add(childBone, childSelection);

                model.UpdateBones();
                links = editor.GetLinkPositions(model.Bones);
            }

            if (Input.IsKeyPressed(Keys.A) && SelectedBones.Count > 0)
            {
                var bone = SelectedBones.First();
                int index = model.Bones.IndexOf(bone.Key);

                foreach (var vert in editor._selectedVertices)
                {
                    vert.SetBoneIndexForAll(index);
                }
            }

            if (Input.IsKeyDown(Keys.D))
            {
                Handle_Movement(links, editor);
            }

            Console.WriteLine(model.Bones.Count);
        }

        if (Input.IsKeyDown(Keys.G))
        {
            Handle_Movement(links, editor);
        }
    }

    public override void Exit(GeneralModelingEditor editor)
    {
        //editor.model.CurrentAnimation = null;
    }

    public void GenerateBonePanels(List<Link<Vector2>> links, GeneralModelingEditor editor)
    {
        BoneUi.ClearUiMesh();
        BoneUi.staticElements.Clear();

        for (int i = 0; i < links.Count; i++)
        {
            Link<Vector2> link = links[i];

            bool pivotSelected = false;
            bool endSelected = false;
            bool bothSelected = false;

            if (SelectedBones.TryGetValue(model.Bones[i], out var selection))
            {
                pivotSelected = selection.PivotSelected;
                endSelected = selection.EndSelected;
                bothSelected = (pivotSelected && endSelected) || (!pivotSelected && !endSelected);
                if (bothSelected)
                {
                    pivotSelected = true;
                    endSelected = true;
                }
            }

            BoneUi.AddStaticElement(editor.GeneratePanelLink(link.A, link.B, bothSelected ? 4 : 3));

            StaticPanel panel1 = UI.CreateStaticPanel(link.A.ToString(), AnchorType.MiddleCenter, PositionType.Free, new Vector3(20, 20, 0), new Vector4(0, 0, 0, 0), null);
            panel1.SetPosition(new Vector3(link.A.X, link.A.Y, 0));
            panel1.TextureIndex = pivotSelected ? 4 : 3;

            StaticPanel panel2 = UI.CreateStaticPanel(link.B.ToString(), AnchorType.MiddleCenter, PositionType.Free, new Vector3(20, 20, 0), new Vector4(0, 0, 0, 0), null);
            panel2.SetPosition(new Vector3(link.B.X, link.B.Y, 0));
            panel2.TextureIndex = endSelected ? 4 : 3;
            
            BoneUi.AddStaticElement(panel1);
            BoneUi.AddStaticElement(panel2);
        }

        BoneUi.Generate();
        BoneUi.Update();

        Console.WriteLine("Bone Panels Generated: " + BoneUi.staticElements.Count);
    }

    public void Handle_Movement(List<Link<Vector2>> links, GeneralModelingEditor editor)
    {
        Vector3 move = editor.GetSnappingMovement();

            foreach (var (bone, pivot) in SelectedBones)
            {
                if (pivot.PivotSelected)
                {
                    bone.Pivot += move;
                }

                if (pivot.EndSelected)
                {
                    bone.End += move;
                }
            }

            GenerateBonePanels(links, editor);
    }

    public static bool IsPointCloseToLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd, float threshold)
    {
        float distance = DistancePointToLine(point, lineStart, lineEnd);
        return distance <= threshold;
    }
    
    private static float DistancePointToLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        Vector2 line = lineEnd - lineStart;
        Vector2 pointVector = point - lineStart;
        float lineLengthSquared = line.LengthSquared;
        
        if (lineLengthSquared == 0f)
        {
            return Vector2.Distance(point, lineStart);
        }
        float t = Mathf.Clamp01(Vector2.Dot(pointVector, line) / lineLengthSquared);
        Vector2 projection = lineStart + t * line;
        return Vector2.Distance(point, projection);
    }
}