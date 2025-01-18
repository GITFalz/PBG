using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class Model
{
    public ModelBase CurrentModel;
    public ModelAnimation ModelAnimation = new ModelAnimation();
    public ModelRigging ModelRigging = new ModelRigging();
    public ModelModeling ModelModeling = new ModelModeling();

    public Camera camera;

    public Dictionary<string, Animation> Animations = new Dictionary<string, Animation>();
    public List<Bone> Bones = new List<Bone>();
    public RigMesh Mesh;
    public Animation? CurrentAnimation = null;
    
    public void Init(Bone rootBone, string modelName)
    {
        CurrentModel = ModelModeling;

        Mesh = new RigMesh(rootBone);
        Mesh.LoadModel(modelName);

        //rootBone.SetVertices(Mesh.Vertices);
        Bones = rootBone.GetBones();

        CurrentAnimation = new Animation("TestAnimation");
        Animations.Add(CurrentAnimation.Name, CurrentAnimation);

        CurrentAnimation.AddOrUpdateKeyframe(0, new AnimationKeyframe(0, Quaternion.FromEulerAngles(Mathf.DegreesToRadians((0, 0, 0)))));
        CurrentAnimation.AddOrUpdateKeyframe(0, new AnimationKeyframe(8, Quaternion.FromEulerAngles(Mathf.DegreesToRadians((0, 5, 0)))));
        CurrentAnimation.AddOrUpdateKeyframe(0, new AnimationKeyframe(40, Quaternion.FromEulerAngles(Mathf.DegreesToRadians((0, 85, 0)))));
        CurrentAnimation.AddOrUpdateKeyframe(0, new AnimationKeyframe(48, Quaternion.FromEulerAngles(Mathf.DegreesToRadians((0, 90, 0)))));
        CurrentAnimation.AddOrUpdateKeyframe(0, new AnimationKeyframe(56, Quaternion.FromEulerAngles(Mathf.DegreesToRadians((0, 85, 0)))));
        CurrentAnimation.AddOrUpdateKeyframe(0, new AnimationKeyframe(88, Quaternion.FromEulerAngles(Mathf.DegreesToRadians((0, 5, 0)))));
        CurrentAnimation.AddOrUpdateKeyframe(0, new AnimationKeyframe(96, Quaternion.FromEulerAngles(Mathf.DegreesToRadians((0, 0, 0)))));

        CurrentAnimation.AddOrUpdateKeyframe(1, new AnimationKeyframe(0, Quaternion.FromEulerAngles(Mathf.DegreesToRadians((0, 0, 0)))));
        CurrentAnimation.AddOrUpdateKeyframe(1, new AnimationKeyframe(60, Quaternion.FromEulerAngles(Mathf.DegreesToRadians((180, 0, 0)))));
        CurrentAnimation.AddOrUpdateKeyframe(1, new AnimationKeyframe(120, Quaternion.FromEulerAngles(Mathf.DegreesToRadians((360, 0, 0)))));

        CurrentAnimation.AddOrUpdateKeyframe(2, new AnimationKeyframe(0, Quaternion.FromEulerAngles(Mathf.DegreesToRadians((0, 0, 0)))));
        CurrentAnimation.AddOrUpdateKeyframe(2, new AnimationKeyframe(60, Quaternion.FromEulerAngles(Mathf.DegreesToRadians((0, 180, 0)))));
        CurrentAnimation.AddOrUpdateKeyframe(2, new AnimationKeyframe(120, Quaternion.FromEulerAngles(Mathf.DegreesToRadians((0, 180, 180)))));
        CurrentAnimation.AddOrUpdateKeyframe(2, new AnimationKeyframe(180, Quaternion.FromEulerAngles(Mathf.DegreesToRadians((0, 0, 0)))));

        Mesh.InitModel();
        Mesh.GenerateBuffers();

        CurrentModel.Init(this);
    }

    public void SetCurrentAnimation(string animationName)
    {
        if (Animations.TryGetValue(animationName, out var animation))
            CurrentAnimation = animation;
    }

    public void Update()
    {
        CurrentModel.Update(this);
    }

    public void Render()
    {
        CurrentModel.Render(this);
    }

    public static Matrix4 GetRotationMatrix(Vector3 pivot, Quaternion rotation)
    {
        Matrix4 matrixTranslation = Matrix4.CreateTranslation(-pivot);
        Matrix4 matrixRotation = Matrix4.CreateFromQuaternion(rotation);
        Matrix4 matrixTranslationBack = Matrix4.CreateTranslation(pivot);
        return matrixTranslation * matrixRotation * matrixTranslationBack;
    }

    public void SwitchState(string state)
    {
        switch (state)
        {
            case "Animation":
                CurrentModel = ModelAnimation;
                break;
            case "Rigging":
                CurrentModel = ModelRigging;
                break;
            case "Modeling":
                CurrentModel = ModelModeling;
                break;
        }

        CurrentModel.Init(this);
    }
}

public abstract class ModelBase
{
    public abstract void Init(Model model);
    public abstract void Update(Model model);
    public abstract void Render(Model model);
}

public class ModelAnimation : ModelBase
{
    public AnimationMesh Mesh;
    public Dictionary<string, Animation> Animations = new Dictionary<string, Animation>();
    public List<Bone> Bones = new List<Bone>();
    public Animation? CurrentAnimation = null;
    public ShaderProgram _shaderProgram;

    public override void Init(Model model)
    {
        _shaderProgram = new ShaderProgram("Model/ModelAnimation.vert", "Model/Model.frag");

        Animations = model.Animations;
        Bones = model.Bones;
        CurrentAnimation = model.CurrentAnimation;

        Mesh = model.Mesh.ToAnimationMesh(Bones.Count);
        Mesh.GenerateBuffers();
    }

    public override void Update(Model model)
    {
        if (CurrentAnimation == null)
            return;

        for (int i = Bones.Count - 1; i >= 0; i--)
        {
            var frame = CurrentAnimation.GetFrame(i);

            if (frame == null)
                continue;

            var bone = Bones[i];
            bone.localTransform = Model.GetRotationMatrix(Bones[i].TransformedPivot, (Quaternion)frame);
        }

        Mesh.RootBone.CalculateGlobalTransform();

        for (int i = 0; i < Bones.Count; i++)
        {
            Mesh.BoneMatrices[i] = Bones[i].globalTransform;
        }
        
        Mesh.UpdateBoneMatrices();
    }

    public override void Render(Model model)
    {
        var camera = ModelSettings.Camera;
        if (camera == null)
            return;

        _shaderProgram.Bind();

        Matrix4 Model = Matrix4.Identity;
        Matrix4 view = camera.GetViewMatrix();
        Matrix4 projection = camera.GetProjectionMatrix();

        int modelLocation = GL.GetUniformLocation(_shaderProgram.ID, "model");
        int viewLocation = GL.GetUniformLocation(_shaderProgram.ID, "view");
        int projectionLocation = GL.GetUniformLocation(_shaderProgram.ID, "projection");
        int colorAlphaLocation = GL.GetUniformLocation(_shaderProgram.ID, "colorAlpha");
        
        GL.UniformMatrix4(modelLocation, true, ref Model);
        GL.UniformMatrix4(viewLocation, true, ref view);
        GL.UniformMatrix4(projectionLocation, true, ref projection);
        GL.Uniform1(colorAlphaLocation, 1.0f);

        Mesh.RenderMesh();

        _shaderProgram.Unbind();
    }
}

public class ModelRigging : ModelBase
{
    public ShaderProgram _shaderProgram;

    public override void Init(Model model)
    {
        _shaderProgram = new ShaderProgram("Model/Model.vert", "Model/Model.frag");
    }

    public override void Update(Model model)
    {
        
    }

    public override void Render(Model model)
    {
        var camera = ModelSettings.Camera;
        if (camera == null)
            return;

        _shaderProgram.Bind();

        Matrix4 Model = Matrix4.Identity;
        Matrix4 view = camera.GetViewMatrix();
        Matrix4 projection = camera.GetProjectionMatrix();

        int modelLocation = GL.GetUniformLocation(_shaderProgram.ID, "model");
        int viewLocation = GL.GetUniformLocation(_shaderProgram.ID, "view");
        int projectionLocation = GL.GetUniformLocation(_shaderProgram.ID, "projection");
        int colorAlphaLocation = GL.GetUniformLocation(_shaderProgram.ID, "colorAlpha");
        
        GL.UniformMatrix4(modelLocation, true, ref Model);
        GL.UniformMatrix4(viewLocation, true, ref view);
        GL.UniformMatrix4(projectionLocation, true, ref projection);
        GL.Uniform1(colorAlphaLocation, 1.0f);

        model.Mesh.RenderMesh();

        _shaderProgram.Unbind();
    }
}

public class ModelModeling : ModelBase
{
    public ShaderProgram _shaderProgram;

    public override void Init(Model model)
    {
        Console.WriteLine("Init Model Modeling");
        _shaderProgram = new ShaderProgram("Model/ModelModeling.vert", "Model/Model.frag");
    }

    public override void Update(Model model)
    {
        
    }

    public override void Render(Model model)
    {
        var camera = ModelSettings.Camera;
        if (camera == null)
            return;

        _shaderProgram.Bind();

        foreach (var flip in ModelSettings.Mirrors)
        { 
            Matrix4 Model = Matrix4.CreateScale(flip);
            Matrix4 view = camera.GetViewMatrix();
            Matrix4 projection = camera.GetProjectionMatrix();

            int modelLocation = GL.GetUniformLocation(_shaderProgram.ID, "model");
            int viewLocation = GL.GetUniformLocation(_shaderProgram.ID, "view");
            int projectionLocation = GL.GetUniformLocation(_shaderProgram.ID, "projection");
            int colorAlphaLocation = GL.GetUniformLocation(_shaderProgram.ID, "colorAlpha");
            
            GL.UniformMatrix4(modelLocation, true, ref Model);
            GL.UniformMatrix4(viewLocation, true, ref view);
            GL.UniformMatrix4(projectionLocation, true, ref projection);
            GL.Uniform1(colorAlphaLocation, ModelSettings.MeshAlpha);
            
            model.Mesh.RenderMesh();
        }

        _shaderProgram.Unbind();
    }
}

public static class ModelSettings
{
    public static Camera? Camera;
    public static float MeshAlpha = 1.0f;
    public static Vector3i mirror = (0, 0, 0);
    public static Vector3[] Mirrors => Mirroring[mirror];
    public static readonly Dictionary<Vector3i, Vector3[]> Mirroring = new Dictionary<Vector3i, Vector3[]>()
    {
        { (0, 0, 0), [(1, 1, 1)] },
        { (1, 0, 0), [(1, 1, 1), (-1, 1, 1)] },
        { (0, 1, 0), [(1, 1, 1), (1, -1, 1)] },
        { (0, 0, 1), [(1, 1, 1), (1, 1, -1)] },
        { (1, 1, 0), [(1, 1, 1), (-1, 1, 1), (1, -1, 1), (-1, -1, 1)] },
        { (1, 0, 1), [(1, 1, 1), (-1, 1, 1), (1, 1, -1), (-1, 1, -1)] },
        { (0, 1, 1), [(1, 1, 1), (1, -1, 1), (1, 1, -1), (1, -1, -1)] },
        { (1, 1, 1), [(1, 1, 1), (-1, 1, 1), (1, -1, 1), (-1, -1, 1), (1, 1, -1), (-1, 1, -1), (1, -1, -1), (-1, -1, -1)] }
    };
}