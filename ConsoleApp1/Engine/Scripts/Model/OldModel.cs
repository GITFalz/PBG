using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class OldModel
{
    public ModelBase CurrentModel;
    public ModelAnimation ModelAnimation = new ModelAnimation();
    public ModelRigging ModelRigging = new ModelRigging();
    public ModelModeling ModelModeling = new ModelModeling();

    public Dictionary<string, Animation> Animations = new Dictionary<string, Animation>();
    public List<Bone> Bones = new List<Bone>();
    public AnimationMesh animationMesh = new AnimationMesh();
    public RigMesh rigMesh = new RigMesh();
    public ModelMesh modelMesh = new ModelMesh();
    public Animation? CurrentAnimation = null;

    public Meshes CurrentMesh;
    
    public void Init()
    {
        CurrentModel = ModelModeling;
        CurrentMesh = modelMesh;

        CurrentAnimation = new Animation("TestAnimation");
        Animations.Add(CurrentAnimation.Name, CurrentAnimation);

        /*
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
        */

        modelMesh.Init();
        modelMesh.GenerateBuffers();
        animationMesh.GenerateBuffers();

        CurrentModel.Init(this);
    }

    public void UpdateBones()
    {
        if (CurrentMesh is RigMesh rigMesh)
            Bones = rigMesh.RootBone.GetBones();
        else if (CurrentMesh is AnimationMesh animationMesh) 
        {
            Bones = animationMesh.RootBone.GetBones();
        }
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
    public abstract void Init(OldModel model);
    public abstract void Update(OldModel model);
    public abstract void Render(OldModel model);
}

public class ModelAnimation : ModelBase
{
    public AnimationMesh Mesh;
    public Dictionary<string, Animation> Animations = new Dictionary<string, Animation>();
    public List<Bone> Bones = new List<Bone>();
    public Animation? CurrentAnimation = null;

    private ShaderProgram _shaderProgram = new ShaderProgram("Model/ModelAnimation.vert", "Model/Model.frag");

    public override void Init(OldModel model)
    {
        model.UpdateBones();
        Animations = model.Animations;
        Bones = model.Bones;
        CurrentAnimation = model.CurrentAnimation;

        Mesh = model.animationMesh;
        model.CurrentMesh = Mesh;
    }

    public override void Update(OldModel model)
    {
        if (CurrentAnimation == null)
            return;

        for (int i = Bones.Count - 1; i >= 0; i--)
        {
            var frame = CurrentAnimation.GetFrame(i);

            if (frame == null)
                continue;

            var bone = Bones[i];
            bone.localTransform = OldModel.GetRotationMatrix(Bones[i].Pivot.Position, (Quaternion)frame);
        }

        Mesh.RootBone.CalculateGlobalTransform();

        for (int i = 0; i < Bones.Count; i++)
        {
            try
            {
                Mesh.BoneMatrices[i] = Bones[i].globalTransform;
            }
            catch (Exception)
            {
                Mesh.BoneMatrices.Add(Matrix4.Identity);
            }
        }

        foreach (var bone in Bones)
        {
            //Console.WriteLine($"Bone: {bone.Name}, Global: {bone.globalTransform}, Local: {bone.localTransform}");
        }
        
        Mesh.UpdateBoneMatrices();
    }

    public override void Render(OldModel model)
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

        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Always);

        Model = Matrix4.Identity;

        ModelSettings.EdgeShader.Bind();

        modelLocation = GL.GetUniformLocation(ModelSettings.EdgeShader.ID, "model");
        viewLocation = GL.GetUniformLocation(ModelSettings.EdgeShader.ID, "view");
        projectionLocation = GL.GetUniformLocation(ModelSettings.EdgeShader.ID, "projection");

        GL.UniformMatrix4(modelLocation, true, ref Model);
        GL.UniformMatrix4(viewLocation, true, ref view);
        GL.UniformMatrix4(projectionLocation, true, ref projection);

        model.animationMesh.RenderEdges();

        ModelSettings.EdgeShader.Unbind();

        ModelSettings.VertexShader.Bind();

        modelLocation = GL.GetUniformLocation(ModelSettings.VertexShader.ID, "model");
        viewLocation = GL.GetUniformLocation(ModelSettings.VertexShader.ID, "view");
        projectionLocation = GL.GetUniformLocation(ModelSettings.VertexShader.ID, "projection");

        GL.UniformMatrix4(modelLocation, true, ref Model);
        GL.UniformMatrix4(viewLocation, true, ref view);
        GL.UniformMatrix4(projectionLocation, true, ref projection);

        model.animationMesh.RenderVertices();

        ModelSettings.VertexShader.Unbind();

        GL.Disable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Lequal);
    }
}

public class ModelRigging : ModelBase
{
    private ShaderProgram _shaderProgram = new ShaderProgram("Model/ModelRig.vert", "Model/Model.frag");

    public override void Init(OldModel model)
    {
        model.CurrentMesh = model.rigMesh;
    }

    public override void Update(OldModel model)
    {
        
    }

    public override void Render(OldModel model)
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

        model.rigMesh.RenderMesh();

        _shaderProgram.Unbind();

        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Always);

        Model = Matrix4.Identity;

        ModelSettings.EdgeShader.Bind();

        modelLocation = GL.GetUniformLocation(ModelSettings.EdgeShader.ID, "model");
        viewLocation = GL.GetUniformLocation(ModelSettings.EdgeShader.ID, "view");
        projectionLocation = GL.GetUniformLocation(ModelSettings.EdgeShader.ID, "projection");

        GL.UniformMatrix4(modelLocation, true, ref Model);
        GL.UniformMatrix4(viewLocation, true, ref view);
        GL.UniformMatrix4(projectionLocation, true, ref projection);

        model.rigMesh.RenderEdges();

        ModelSettings.EdgeShader.Unbind();

        ModelSettings.VertexShader.Bind();

        modelLocation = GL.GetUniformLocation(ModelSettings.VertexShader.ID, "model");
        viewLocation = GL.GetUniformLocation(ModelSettings.VertexShader.ID, "view");
        projectionLocation = GL.GetUniformLocation(ModelSettings.VertexShader.ID, "projection");

        GL.UniformMatrix4(modelLocation, true, ref Model);
        GL.UniformMatrix4(viewLocation, true, ref view);
        GL.UniformMatrix4(projectionLocation, true, ref projection);

        model.rigMesh.RenderVertices();

        ModelSettings.VertexShader.Unbind();

        GL.Disable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Lequal);
    }
}

public class ModelModeling : ModelBase
{
    private ShaderProgram _shaderProgram = new ShaderProgram("Model/Model.vert", "Model/Model.frag");
    private Texture _texture = new Texture("dirt_block.png");

    public override void Init(OldModel model)
    {
        model.CurrentMesh = model.modelMesh;
    }

    public override void Update(OldModel model)
    {
        
    }

    public override void Render(OldModel model)
    {
        var camera = ModelSettings.Camera;
        if (camera == null)
            return;

        _shaderProgram.Bind();

        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Lequal);

        Matrix4 Model = Matrix4.Identity;
        Matrix4 view = camera.GetViewMatrix();
        Matrix4 projection = camera.GetProjectionMatrix();

        int modelLocation;
        int viewLocation;
        int projectionLocation;
        int colorAlphaLocation;

        foreach (var flip in ModelSettings.Mirrors)
        { 
            Model = Matrix4.CreateScale(flip);

            modelLocation = GL.GetUniformLocation(_shaderProgram.ID, "model");
            viewLocation = GL.GetUniformLocation(_shaderProgram.ID, "view");
            projectionLocation = GL.GetUniformLocation(_shaderProgram.ID, "projection");
            colorAlphaLocation = GL.GetUniformLocation(_shaderProgram.ID, "colorAlpha");
            
            GL.UniformMatrix4(modelLocation, true, ref Model);
            GL.UniformMatrix4(viewLocation, true, ref view);
            GL.UniformMatrix4(projectionLocation, true, ref projection);
            GL.Uniform1(colorAlphaLocation, ModelSettings.MeshAlpha);

            _texture.Bind();
            
            model.modelMesh.Render();

            _texture.Unbind();
        }

        _shaderProgram.Unbind();

        if (ModelSettings.WireframeVisible)
        {
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Always);

            Model = Matrix4.Identity;

            ModelSettings.EdgeShader.Bind();

            modelLocation = GL.GetUniformLocation(ModelSettings.EdgeShader.ID, "model");
            viewLocation = GL.GetUniformLocation(ModelSettings.EdgeShader.ID, "view");
            projectionLocation = GL.GetUniformLocation(ModelSettings.EdgeShader.ID, "projection");

            GL.UniformMatrix4(modelLocation, true, ref Model);
            GL.UniformMatrix4(viewLocation, true, ref view);
            GL.UniformMatrix4(projectionLocation, true, ref projection);

            model.modelMesh.RenderEdges();

            ModelSettings.EdgeShader.Unbind();

            ModelSettings.VertexShader.Bind();

            modelLocation = GL.GetUniformLocation(ModelSettings.VertexShader.ID, "model");
            viewLocation = GL.GetUniformLocation(ModelSettings.VertexShader.ID, "view");
            projectionLocation = GL.GetUniformLocation(ModelSettings.VertexShader.ID, "projection");

            GL.UniformMatrix4(modelLocation, true, ref Model);
            GL.UniformMatrix4(viewLocation, true, ref view);
            GL.UniformMatrix4(projectionLocation, true, ref projection);

            model.modelMesh.RenderVertices();

            ModelSettings.VertexShader.Unbind();

            GL.Disable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);
        }
    }
}

public static class ModelSettings
{
    public static ShaderProgram VertexShader = new ShaderProgram("Model/ModelVertex.vert", "Model/ModelVertex.frag");
    public static ShaderProgram EdgeShader = new ShaderProgram("Model/ModelEdge.vert", "Model/ModelEdge.frag");

    public static Camera? Camera;
    public static OldModel Model = new OldModel();

    public static RenderType RenderType = RenderType.Vertex;

    // Ui values
    public static float MeshAlpha = 1.0f;
    public static bool WireframeVisible = true;
    public static bool BackfaceCulling = true;
    public static bool Snapping = false;
    public static bool GridAligned = false;
    public static float SnappingFactor = 1;
    public static int SnappingFactorIndex = 0;
    public static Vector3 SnappingOffset = new Vector3(0, 0, 0);

    public static Vector3i mirror = (0, 0, 0);
    public static Vector3i axis = (1, 1, 1);
    public static Vector3[] Mirrors => Mirroring[mirror];
    public static bool[] Swaps => Swapping[mirror];
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

    public static readonly Dictionary<Vector3i, bool[]> Swapping = new Dictionary<Vector3i, bool[]>()
    {
        { (0, 0, 0), [false] },
        { (1, 0, 0), [false, true] },
        { (0, 1, 0), [false, true] },
        { (0, 0, 1), [false, true] },
        { (1, 1, 0), [false, true, true, false] },
        { (1, 0, 1), [false, true, true, false] },
        { (0, 1, 1), [false, true, true, false] },
        { (1, 1, 1), [false, true, true, false, true, false, false, true] }
    };
}