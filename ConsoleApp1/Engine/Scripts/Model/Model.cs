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


    private static ShaderProgram _shaderProgram = new ShaderProgram("Model/Model.vert", "Model/Model.frag");
    private static int _modelLocation = _shaderProgram.GetLocation("model");
    private static int _viewLocation = _shaderProgram.GetLocation("view");
    private static int _projectionLocation = _shaderProgram.GetLocation("projection");
    private static int _colorAlphaLocation = _shaderProgram.GetLocation("colorAlpha");
    private static ShaderProgram _animationShader = new ShaderProgram("Model/ModelAnimation.vert", "Model/Model.frag");
    private static int _animationModelLocation = _animationShader.GetLocation("model");
    private static int _animationViewLocation = _animationShader.GetLocation("view");
    private static int _animationProjectionLocation = _animationShader.GetLocation("projection");
    private static int _animationColorAlphaLocation = _animationShader.GetLocation("colorAlpha");
    private static ShaderProgram _activeShader = _shaderProgram;
    private static int _activeModelLocation = _modelLocation;
    private static int _activeViewLocation = _viewLocation; 
    private static int _activeProjectionLocation = _projectionLocation;
    private static int _activeColorAlphaLocation = _colorAlphaLocation;
    public Texture Texture = new Texture("empty.png", TextureLocation.NormalTexture);

    public SSBO<Matrix4> BoneMatrices = new SSBO<Matrix4>();
    public List<Matrix4> BoneMatricesList = [];


    public Rig? Rig;
    public string? RigName => Rig?.Name;
    public Animation? Animation;


    // === General model settings === //
    public Vector3 Position;

    public bool RenderBones = false;
    public bool Animate = false;


    public Model()
    {
        Mesh = new ModelMesh(this);
        Create();
    }

    public void Create()
    {
        if (!RigManager.TryGet("Base", out Rig? rig))
        {
            if (RigManager.Load("Base") && RigManager.TryGet("Base", out rig, false))
            {
                Rig = rig;
            }
            else
            {
                RootBone root = new RootBone("RootBone");
                Rig r = new Rig("Base");
                r.RootBone = root;

                if (!RigManager.Add(r, false))
                    return;

                if (!RigManager.TryGet("Base", out Rig? rigBase, false))
                {
                    PopUp.AddPopUp("Base rig cannot be loaded.");
                    return;
                }

                Rig = rigBase;
            }
        }
        else
        {
            Rig = rig;
        }

        if (Rig == null)
        {
            PopUp.AddPopUp("Rig cannot be loaded.");
            return;
        }

        Rig.Create();
        Rig.Initialize();
        Rig.RootBone.UpdateGlobalTransformation();

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

    public void UpdateMatrices()
    {
        if (Rig == null)
            return;

        BoneMatricesList.Clear();
        foreach (var bone in Rig.BonesList)
        {
            BoneMatricesList.Add(bone.FinalMatrix);
        }
        BoneMatrices.Update(BoneMatricesList, 0);
    }

    public void UpdateRig()
    {
        UpdateMatrices();
    }

    public void SetModeling()
    {
        _activeShader = _shaderProgram;
        _activeModelLocation = _modelLocation;
        _activeViewLocation = _viewLocation;
        _activeProjectionLocation = _projectionLocation;
        _activeColorAlphaLocation = _colorAlphaLocation;
        Mesh.Init();
        Mesh.UpdateModel(); 
    }

    public void SetAnimation()
    {
        _activeShader = _animationShader;
        _activeModelLocation = _animationModelLocation;
        _activeViewLocation = _animationViewLocation;
        _activeProjectionLocation = _animationProjectionLocation;
        _activeColorAlphaLocation = _animationColorAlphaLocation;
        if (Rig != null)
        {
            Mesh.Bind(Rig);
            Mesh.UpdateModel();
        }
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
        if (Rig == null || Animation == null)
            return;

        if (Animate)
        {
            foreach (var bone in Rig.BonesList)
            {
                var frame = Animation.GetFrame(bone.Name);
                if (frame == null)
                    continue;

                bone.Position = frame.Position;
                bone.Rotation = frame.Rotation;
                bone.Scale = frame.Scale;
                bone.LocalAnimatedMatrix = frame.GetLocalTransform(); ;
            }

            Rig.RootBone.UpdateGlobalTransformation();

            foreach (var bone in Rig.BonesList)
            {
                BoneMatricesList[bone.Index] = bone.GlobalAnimatedMatrix;
            }

            BoneMatrices.Update(BoneMatricesList, 0);
        }
    }

    public void Render()
    {
        var camera = ModelSettings.Camera;
        if (camera == null)
            return;

        _activeShader.Bind();

        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Lequal);

        Matrix4 Model = Matrix4.CreateTranslation(Position);
        Matrix4 view = camera.GetViewMatrix();
        Matrix4 projection = camera.GetProjectionMatrix();

        GL.UniformMatrix4(_activeModelLocation, false, ref Model);
        GL.UniformMatrix4(_activeViewLocation, false, ref view);
        GL.UniformMatrix4(_activeProjectionLocation, false, ref projection);
        GL.Uniform1(_activeColorAlphaLocation, ModelSettings.MeshAlpha);

        Texture.Bind();

        if (Rig != null)
        {
            BoneMatrices.Bind(0);
            Mesh.Render();
            BoneMatrices.Unbind();
        }
        else
        {
            Mesh.Render();
        }

        Texture.Unbind();

        _activeShader.Unbind();
        
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

    public void Unload()
    {
        Mesh.Unload();
    }

    public void Delete()
    {
        Mesh.Delete();
    }
}