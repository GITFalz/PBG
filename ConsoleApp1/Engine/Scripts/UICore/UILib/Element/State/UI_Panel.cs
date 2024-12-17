using OpenTK.Mathematics;
using StbImageSharp;

public class UI_Panel : UI_Base
{
    public Vector2 textureSize = new Vector2(64, 64);
    
    public UiMesh mesh;
    
    public UI_Panel()
    {
        name = "UI_Panel";
    }
    
    public UI_Panel(UiMesh mesh)
    {
        name = "UI_Panel";
        this.mesh = mesh;
    }
    
    public override void RenderUI()
    {
        //mesh.AddQuad(position, MeshHelper.GenerateTextQuad(size.X, size.Y, 0, memSize, memPos));
        Align();
        UI.Generate9Slice(position, textureSize.X, textureSize.Y, size.X, size.Y, 10f, new Vector4(10f, 10f, 10f, 10f), mesh);
    }
}