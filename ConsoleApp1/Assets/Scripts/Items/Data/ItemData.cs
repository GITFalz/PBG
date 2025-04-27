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

    public string Name = "empty";
    public int Index = -1;
    public int MaxStackSize = 0;

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

    public void Base()
    {
        Index = ItemDataManager.AllItems.Count - 1;
        ItemDataManager.AllItems.Add(Name, this);
    } 

    public abstract void GenerateIcon();
    public abstract void RenderIcon(Vector2 position, float scale);
    public abstract void RenderIcon(Vector3 position, float scale);
    public bool IsEmpty() => this is EmptyItemData;

    public override bool Equals(object? obj)
    {
        if (obj is ItemData itemData)
        {
            return Index == itemData.Index;
        }
        return false;
    }
}