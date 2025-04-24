using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public abstract class ItemData
{
    public static ShaderProgram IconShader = new ShaderProgram("Utils/Rectangle.vert", "Utils/ArrayImage.frag");
    public static VAO IconVAO = new VAO();

    public static int IconModelLocation = -1;
    public static int IconProjectionLocation = -1;
    public static int IconSizeLocation = -1;
    public static int IconTextureLocation = -1;
    public static int IconIndexLocation = -1;

    protected static FBO FBO => ItemDataManager.FBO;

    protected static List<byte[]> Data => ItemDataManager.Data;

    protected static int CubeModelLocation => ItemDataManager.CubeModelLocation;
    protected static int CubeProjectionLocation => ItemDataManager.CubeProjectionLocation;
    protected static int CubeTextureLocation => ItemDataManager.CubeTextureLocation;
    protected static int CubeIndicesLocation => ItemDataManager.CubeIndicesLocation;

    public int Index;

    static ItemData()
    {
        IconShader.Bind();

        IconModelLocation = IconShader.GetLocation("model");
        IconProjectionLocation = IconShader.GetLocation("projection");
        IconSizeLocation = IconShader.GetLocation("size");
        IconTextureLocation = IconShader.GetLocation("textureArray");
        IconIndexLocation = IconShader.GetLocation("index");

        IconShader.Unbind();

        FBO.Bind();
    }

    public ItemData()
    {
        Index = ItemDataManager.AllItems.Count;
        ItemDataManager.AllItems.Add(this);
    }

    public abstract void GenerateIcon();
    public abstract void RenderIcon(Vector2 position);
}