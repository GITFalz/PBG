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

    public void UpdateBones()
    {
        Bones = Mesh.RootBone.GetBones();
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
            bone.localTransform = Model.GetRotationMatrix(Bones[i].Pivot, (Quaternion)frame);
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
    public static Model Model = new Model();

    // Ui values
    public static float MeshAlpha = 1.0f;
    public static bool BackfaceCulling = true;
    public static bool Snapping = false;
    public static float SnappingFactor = 1;
    public static int SnappingFactorIndex = 0;
    public static Vector3 SnappingOffset = new Vector3(0, 0, 0);

    // Ui Elements

    public static UIText BackfaceCullingText;
    public static UIText MeshAlphaText;
    public static UIText SnappingText;
    public static UIText MirrorText;
    public static UIText InputField;
    public static UIText BonePivotX;
    public static UIText BonePivotY;
    public static UIText BonePivotZ;
    public static UIText BoneEndX;
    public static UIText BoneEndY;
    public static UIText BoneEndZ;


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

    /*
    public static void SwitchMirror(string axis)
    {
        switch (axis)
        {
            case "X":
                ModelSettings.mirror.X = ModelSettings.mirror.X == 0 ? 1 : 0;
                break;
            case "Y":
                ModelSettings.mirror.Y = ModelSettings.mirror.Y == 0 ? 1 : 0;
                break;
            case "Z":
                ModelSettings.mirror.Z = ModelSettings.mirror.Z == 0 ? 1 : 0;
                break;
        }
        
        UpdateMirrorText();
    }
    
    public void ApplyMirror()
    {
        model.Mesh.ApplyMirror();
        
        model.Mesh.InitModel();
        model.Mesh.GenerateBuffers();
        model.Mesh.UpdateMesh();
        
        regenerateVertexUi = true;
        
        ModelSettings.mirror = new Vector3i(0, 0, 0);
        UpdateMirrorText();
    }
    
    public void SaveModel()
    {
        string modelName = InputField.Text;
        if (modelName == "cube")
            return;

        model.Mesh.SaveModel(modelName);
        
        model.Mesh.InitModel();
        model.Mesh.GenerateBuffers();
        model.Mesh.UpdateMesh();
    }
    
    public void LoadModel()
    {
        string modelName = InputField.Text;
        model.Mesh.LoadModel(modelName);
        
        model.Mesh.InitModel();
        model.Mesh.GenerateBuffers();
        model.Mesh.UpdateMesh();
    }
    
    public void AssignInputField(string test)
    {
        StaticInputField? inputField = MainUi.GetInputField(test);
        if (inputField != null)
            OldUIController.activeInputField = inputField;
    }
    
    public void AlphaControl()
    {
        float mouseX = Input.GetMouseDelta().X;
        if (mouseX == 0)
            return;
            
        ModelSettings.MeshAlpha += mouseX * GameTime.DeltaTime * 0.2f;
        ModelSettings.MeshAlpha = Mathf.Clamp(0, 1, ModelSettings.MeshAlpha);
        MeshAlphaText.SetText("alpha: " + ModelSettings.MeshAlpha.ToString("F2"));
        MeshAlphaText.Generate();
        MainUi.Update();
    }

    public void BackFaceCullingSwitch()
    {
        ModelSettings.BackfaceCulling = !ModelSettings.BackfaceCulling;
        BackfaceCullingText.SetText("culling: " + ModelSettings.BackfaceCulling);
        BackfaceCullingText.Generate();
        MainUi.Update();
    }

    public void SnappingUpButton()
    {
        if (ModelSettings.SnappingFactorIndex < 4)
        {
            ModelSettings.SnappingFactorIndex++;
            ModelSettings.SnappingFactor = SnappingFactors[ModelSettings.SnappingFactorIndex];
        }

        UpdateSnappingText();
    }
    
    public void SnappingDownButton()
    {
        if (ModelSettings.SnappingFactorIndex > 0)
        {
            ModelSettings.SnappingFactorIndex--;
            ModelSettings.SnappingFactor = SnappingFactors[ModelSettings.SnappingFactorIndex];
        }
        UpdateSnappingText();
    }
    
    public void SnappingSwitchButton()
    {
        ModelSettings.Snapping = !ModelSettings.Snapping;
        UpdateSnappingText();
    }
    
    public void SetBonePivot(string axis)
    {
        if (_selectedBone == null)
            return;
        
        switch (axis)
        {
            case "X":
                _selectedBone.Pivot.X = float.Parse(BonePivotX.Text);
                break;
            case "Y":
                _selectedBone.Pivot.Y = float.Parse(BonePivotY.Text);
                break;
            case "Z":
                _selectedBone.Pivot.Z = float.Parse(BonePivotZ.Text);
                break;
        }
    }
    
    public void SetBoneEnd(string axis)
    {
        if (_selectedBone == null)
            return;
        
        switch (axis)
        {
            case "X":
                _selectedBone.End.X = float.Parse(BoneEndX.Text);
                break;
            case "Y":
                _selectedBone.End.Y = float.Parse(BoneEndY.Text);
                break;
            case "Z":
                _selectedBone.End.Z = float.Parse(BoneEndZ.Text);
                break;
        }
    }
    */
}