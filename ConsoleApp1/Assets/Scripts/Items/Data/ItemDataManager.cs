using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public static class ItemDataManager
{
    public static Dictionary<string, ItemData> AllItems = [];
    public static EmptyItemData Empty = new EmptyItemData();

    public static FBO FBO = new FBO(64, 64, FBOType.Color);
    public static ShaderProgram BlockShader = new ShaderProgram("Utils/Cube.vert", "Inventory/Data/Block.frag");
    public static VAO VAO = new VAO();
    public static TextureArray Image = new TextureArray([], 64, 64); // Placeholder for the texture array

    public static List<byte[]> Data = new List<byte[]>();

    public static int CubeModelLocation = -1;  
    public static int CubeProjectionLocation = -1;
    public static int CubeTextureLocation = -1;
    public static int CubeIndicesLocation = -1;

    static ItemDataManager()
    {
        CubeModelLocation = BlockShader.GetLocation("model"); 
        CubeProjectionLocation = BlockShader.GetLocation("projection");
        CubeTextureLocation = BlockShader.GetLocation("textureArray");
        CubeIndicesLocation = BlockShader.GetLocation("indices");
    }

    public static void GenerateIcons()
    {
        Data.Clear();

        GL.Enable(EnableCap.CullFace);
        GL.CullFace(TriangleFace.Back);
        GL.Enable(EnableCap.DepthTest);

        GL.Viewport(0, 0, 64, 64);
        
        FBO.Bind();

        GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        
        BlockShader.Bind();
        WorldShader.Textures.Bind(TextureUnit.Texture0);
        VAO.Bind();

        Matrix4 model = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(45)) * Matrix4.CreateRotationX(MathHelper.DegreesToRadians(45+90)) * Matrix4.CreateTranslation(32, 32, 0);
        Matrix4 projection = Matrix4.CreateOrthographicOffCenter(0, 64, 64, 0, -32, 32);

        GL.UniformMatrix4(CubeModelLocation, true, ref model);
        GL.UniformMatrix4(CubeProjectionLocation, true, ref projection);
        GL.Uniform1(CubeTextureLocation, 0);

        foreach (var (_, item) in AllItems)
        {
            item.GenerateIcon();
        }

        VAO.Unbind();
        WorldShader.Textures.Unbind();

        BlockShader.Unbind();

        FBO.Unbind();

        GL.Viewport(0, 0, Game.Width, Game.Height);

        Image.Renew(Data, 64, 64);
    }
    
    public static ItemData GetItem(string name)
    {
        name = AllItems.ContainsKey(name) ? name : "empty";
        return AllItems[name];
    }
}