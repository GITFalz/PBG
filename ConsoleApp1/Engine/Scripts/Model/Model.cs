using ConsoleApp1.Engine.Scripts.Core;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class Model
{
    public string Name = "Model";
    public bool IsShown = true;
    public bool IsSelected = false;

    public string CurrentModelName = "";
    public string TextureName = "empty.png";
    public TextureLocation TextureLocation = TextureLocation.NormalTexture;
    

    public ModelMesh Mesh;

    private static ShaderProgram _animationShader = new ShaderProgram("Model/ModelAnimation.vert", "Model/Model.frag");
    private static int _animationModelLocation = _animationShader.GetLocation("model");
    private static int _animationViewLocation = _animationShader.GetLocation("view");
    private static int _animationProjectionLocation = _animationShader.GetLocation("projection");
    private static int _animationColorAlphaLocation = _animationShader.GetLocation("colorAlpha");

    public Texture Texture = new Texture("empty.png", TextureLocation.NormalTexture);

    public SSBO<Matrix4> BoneMatrices = new SSBO<Matrix4>();
    public List<Matrix4> BoneMatricesList = [];


    public Rig? Rig;
    public string? RigName => Rig?.Name;
    public NormalizedAnimation? Animation;


    // === General model settings === //
    public Vector3 Position
    {
        get => _position;
        set
        {
            _position = value;
            _modelMatrix = Matrix4.CreateRotationX(_rotation.X) * Matrix4.CreateRotationY(_rotation.Y) * Matrix4.CreateRotationZ(_rotation.Z) * Matrix4.CreateTranslation(_position);
        }
    }
    private Vector3 _position = Vector3.Zero;
    public Vector3 Rotation
    {
        get => Mathf.ToDegrees(_rotation);
        set
        {
            _rotation = Mathf.ToRadians(value);
            _modelMatrix = Matrix4.CreateRotationX(_rotation.X) * Matrix4.CreateRotationY(_rotation.Y) * Matrix4.CreateRotationZ(_rotation.Z) * Matrix4.CreateTranslation(_position);
        }
    }
    private Vector3 _rotation = Vector3.Zero;

    private Matrix4 _modelMatrix = Matrix4.Identity;

    public bool RenderBones = false;
    public bool Animate = false;


    public Model()
    {
        Mesh = new ModelMesh(this);
    }

    public Model(Rig rig, NormalizedAnimation animation)
    {
        Mesh = new ModelMesh(this);
        Rig = rig;
        Animation = animation;
    }

    public void InitRig()
    {
        if (Rig == null)
            return;

        Rig.Create();
        Rig.Initialize();

        BoneMatricesList.Clear();
        foreach (var bone in Rig.BonesList)
        {
            BoneMatricesList.Add(bone.FinalMatrix);
        }
        BoneMatrices.Renew(BoneMatricesList);
    }

    public void BindRig()
    {
        if (Rig == null)
            return;

        Mesh.Bind(Rig);
        Mesh.UpdateModel();
    }

    // No need to check if the rig is null here, this method should only be called at a point where the rig is guaranteed to be initialized.
    public void UpdateMatrices()
    {
        Rig.RootBone.UpdateGlobalTransformation();
        foreach (var bone in Rig.BonesList)
        {
            BoneMatricesList[bone.Index] = bone.GlobalAnimatedMatrix;
        }
        BoneMatrices.Update(BoneMatricesList, 0);
    }

    public void Renew(string fileName, TextureLocation textureLocation)
    {
        TextureName = fileName;
        TextureLocation = textureLocation;
        Texture.Renew(fileName, textureLocation);
    }

    public void Reload()
    {
        Texture.Renew(TextureName, TextureLocation);
    }

    public void Update()
    {
        if (Rig == null || Animation == null || !Animate)
            return;

        Animation.Update();
        foreach (var bone in Rig.BonesList)
        {
            var frame = Animation.GetBoneKeyframe(bone.Index);
            bone.Position = frame.Position;
            bone.Rotation = frame.Rotation;
            bone.Scale = frame.Scale;
            bone.LocalAnimatedMatrix = frame.GetLocalTransform(); ;
        }

        UpdateMatrices();
    }

    public void Render()
    {
        var camera = ModelSettings.Camera;
        if (camera == null)
            return;

        _animationShader.Bind();

        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Lequal);

        Matrix4 Model = _modelMatrix;
        Matrix4 view = camera.GetViewMatrix();
        Matrix4 projection = camera.GetProjectionMatrix();

        GL.UniformMatrix4(_animationModelLocation, false, ref Model);
        GL.UniformMatrix4(_animationViewLocation, false, ref view);
        GL.UniformMatrix4(_animationProjectionLocation, false, ref projection);
        GL.Uniform1(_animationColorAlphaLocation, ModelSettings.MeshAlpha);

        Texture.Bind();

        BoneMatrices.Bind(0);
        Mesh.Render();
        BoneMatrices.Unbind();

        Texture.Unbind();

        _animationShader.Unbind();
        
        GL.CullFace(TriangleFace.Back);
    }

    public bool LoadModel(string fileName)
    {
        if (fileName.Length == 0)
        {
            PopUp.AddPopUp("Please enter a model name.");
            return false;
        }

        string folderPath = Path.Combine(Game.undoModelPath, fileName);
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        return Load(fileName);
    }

    public bool Load(string fileName)
    {
        if (Mesh.LoadModel(fileName))
        {
            CurrentModelName = fileName;
            return true;
        }
        return false;
    }

    public void LoadModelFromPath(string path)
    {
        if (Mesh.LoadModelFromPath(path))
        {
            CurrentModelName = Path.GetFileNameWithoutExtension(path);
        }
    }

    public void Unload()
    {
        Mesh.Unload();
    }

    public void Delete()
    {
        Mesh.Delete();
    }
}