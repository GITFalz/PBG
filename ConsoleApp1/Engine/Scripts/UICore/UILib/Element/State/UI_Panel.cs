using OpenTK.Mathematics;
using StbImageSharp;

public class UI_Panel : UI_Base
{
    public Vector2 textureSize = new Vector2(64, 64);
    
    public UI_Panel()
    {
        name = "UI_Panel";
    }
    
    public override void RenderUI(MeshData meshData)
    {
        Align();
        UI.Generate9Slice(position, textureSize.X, textureSize.Y, size.X, size.Y, 10f, new Vector4(10f, 10f, 10f, 10f), meshData);
    }
}