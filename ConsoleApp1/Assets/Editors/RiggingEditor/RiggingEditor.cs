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
    public ShaderProgram _shaderProgram;
    public UIController BoneUi = new UIController();
    public OldModel model;
    public RigMesh Mesh;

    public List<Vertex> SelectedVertices = new();
    public List<BoneVertex> SelectedBones = new();
    public Dictionary<Vertex, Vector2> Vertices = new Dictionary<Vertex, Vector2>();
    public Dictionary<BoneVertex, Vector2> Bones = [];

    private bool regenerateVertexUi = true;

    public RiggingEditor(GeneralModelingEditor editor) : base(editor)
    {
        
    }

    public override void Start(GeneralModelingEditor editor)
    {
        Started = true;

        Console.WriteLine("Start Rigging Editor");

        Mesh = editor.model.rigMesh;
        model = editor.model;
    }

    public override void Resize(GeneralModelingEditor editor)
    {
        
    }

    public override void Awake(GeneralModelingEditor editor)
    {
        Mesh.LoadModel(editor.currentModelName);
        editor.model.SwitchState("Rigging");
        model.UpdateBones();
    }
    
    public override void Render(GeneralModelingEditor editor)
    {
        editor.RenderRigging();
    }

    public override void Update(GeneralModelingEditor editor)
    {
        if (Input.IsKeyPressed(Keys.Escape))
        {
            editor.freeCamera = !editor.freeCamera;
            
            if (editor.freeCamera)
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

        if (editor.freeCamera)
        {
            Game.camera.Update();
        }

        if (Input.AreAllKeysDown(Keys.LeftControl, Keys.LeftShift))
        {
            if (Input.IsKeyPressed(Keys.D) && SelectedBones.Count > 0)
            {
                BoneVertex bone = SelectedBones.First();
                Bone parentBone = bone.Parent;
                Bone childBone = new Bone(parentBone.RootBone, "Child");
                childBone = parentBone.AddChild(childBone);

                BoneVertex childSelection;

                bool endSelected = bone.IsEnd();

                if (!endSelected)
                {
                    childBone.Pivot.Position = parentBone.Pivot.Position;
                    childBone.End.Position = parentBone.Pivot.Position - (parentBone.End.Position - parentBone.Pivot.Position);

                    childSelection = childBone.Pivot;
                }
                else
                {
                    childBone.Pivot.Position = parentBone.End.Position;
                    childBone.End.Position = parentBone.End.Position + (parentBone.End.Position - parentBone.Pivot.Position);

                    childSelection = childBone.End;
                }

                SelectedBones.Clear();
                SelectedBones.Add(childSelection);

                model.UpdateBones();
                UpdateVertexPosition();
                Mesh.Init();
                Mesh.GenerateRigBuffers();
            }

            if (Input.IsKeyPressed(Keys.A) && SelectedBones.Count > 0)
            {
                var bone = SelectedBones.First();
                int index = model.Bones.IndexOf(bone.Parent);

                foreach (var vert in SelectedVertices)
                {
                    vert.Index = index;
                }
            }

            if (Input.IsKeyDown(Keys.D))
            {
                HandleBoneMovement();
            }
        }

        if (Input.IsKeyDown(Keys.G))
        {
            HandleBoneMovement();
        }

        if (Input.AnyKeysReleased(Keys.G, Keys.D))
        {
            UpdateBonePosition();
        }

        if (editor.freeCamera && !regenerateVertexUi)
        {
            Console.WriteLine("Clear Vertex UI");
            regenerateVertexUi = true;
        }
        
        if (!editor.freeCamera)
        {
            if (regenerateVertexUi)
            {
                UpdatePositions();
                regenerateVertexUi = false;
            }
            
            if (Input.IsMousePressed(MouseButton.Left))
            {
                HandleVertexSelection();
                HandleBoneSelection();
                GenerateVertexColor();

                //Console.WriteLine("Selected Vertices: " + SelectedVertices.Count);
            }
        }
    }

    public override void Exit(GeneralModelingEditor editor)
    {
        //editor.model.CurrentAnimation = null;
    }

    // Utility
    public void UpdatePositions()
    {
        System.Numerics.Matrix4x4 projection = Game.camera.GetNumericsProjectionMatrix();
        System.Numerics.Matrix4x4 view = Game.camera.GetNumericsViewMatrix();
    
        UpdateVertexPosition(projection, view);
        UpdateBonePosition(projection, view);

        GenerateVertexColor();
    }

    public void UpdateVertexPosition()
    {
        System.Numerics.Matrix4x4 projection = Game.camera.GetNumericsProjectionMatrix();
        System.Numerics.Matrix4x4 view = Game.camera.GetNumericsViewMatrix();
    
        UpdateVertexPosition(projection, view);

        GenerateVertexColor();
    }

    public void UpdateVertexPosition(System.Numerics.Matrix4x4 projection, System.Numerics.Matrix4x4 view)
    {
        Vertices.Clear();

        foreach (var vert in Mesh.VertexList)
        {
            Vector2? screenPos = Mathf.WorldToScreen(vert, projection, view);
            if (screenPos == null)
                continue;
            
            Vertices.Add(vert, screenPos.Value);
        }
    }

    public void UpdateBonePosition()
    {
        System.Numerics.Matrix4x4 projection = Game.camera.GetNumericsProjectionMatrix();
        System.Numerics.Matrix4x4 view = Game.camera.GetNumericsViewMatrix();
    
        UpdateBonePosition(projection, view);

        GenerateVertexColor();
    }

    public void UpdateBonePosition(System.Numerics.Matrix4x4 projection, System.Numerics.Matrix4x4 view)
    {
        Bones.Clear();

        foreach (var bone in model.rigMesh.RootBone.GetBones())
        {
            (Vector2?, Vector2?) screenPos = (Mathf.WorldToScreen(bone.Pivot.Position, projection, view), Mathf.WorldToScreen(bone.End.Position, projection, view));
            if (screenPos.Item1 == null || screenPos.Item2 == null)
                continue;

            Bones.Add(bone.Pivot, screenPos.Item1.Value);
            Bones.Add(bone.End, screenPos.Item2.Value);
        }
    }

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
    }

    public void HandleBoneSelection()
    {
        if (!Input.IsKeyDown(Keys.LeftShift))
            SelectedBones.Clear();
        
        Vector2 mousePos = Input.GetMousePosition();
        Vector2? closest = null;
        BoneVertex? closestVert = null;

        foreach (var bone in Bones)
        {
            float distance = Vector2.Distance(mousePos, bone.Value);
            float distanceClosest = closest == null ? 1000 : Vector2.Distance(mousePos, closest.Value);
        
            if (distance < distanceClosest && distance < 10)
            {
                closest = bone.Value;
                closestVert = bone.Key;
            }
        }

        if (closestVert != null && !SelectedBones.Remove(closestVert))
            SelectedBones.Add(closestVert);
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

        foreach (var bone in model.rigMesh.RootBone.GetBones())
        {
            int boneIndex = model.Bones.IndexOf(bone) * 2;
            Vector3 color = SelectedBones.Contains(bone.Pivot) ? (1f, 0.3f, 0.25f) : (0.1f, 0.03f, 0.025f);
            if (Mesh.BoneColors.Count > boneIndex)
                Mesh.BoneColors[boneIndex] = color;

            color = SelectedBones.Contains(bone.End) ? (1f, 0.3f, 0.25f) : (0.1f, 0.03f, 0.025f);
            if (Mesh.BoneColors.Count > boneIndex + 1)
                Mesh.BoneColors[boneIndex + 1] = color;
        }

        Mesh.UpdateVertexColors();
        Mesh.UpdateBoneColors();
    }

    public void HandleBoneMovement()
    {
        Vector3 move = GetMovement();
        foreach (var bone in SelectedBones)
        {
            bone.Position += move;
        }
        Mesh.UpdateBonePositions();
    }

    public Vector3 GetMovement()
    {
        Camera camera = Game.camera;

        Vector2 mouseDelta = Input.GetMouseDelta() * (GameTime.DeltaTime * 10);
        Vector3 move = camera.right * mouseDelta.X + camera.up * -mouseDelta.Y;

        return move;
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