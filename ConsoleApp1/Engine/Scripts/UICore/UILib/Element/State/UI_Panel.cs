using OpenTK.Mathematics;
using StbImageSharp;

public class UI_Panel : UI_Base
{
    public Vector2 textureSize;
    
    public UI_Panel(Vector2 textureSize, UiAnchorAlignment alignment, Vector4 offset, Vector2 size)
    {
        name = "UI_Panel";
        
        this.textureSize = textureSize;
        
        this.baseOffset = offset;
        this.baseSize = size;

        anchorAlignment = alignment;
    }
    
    public UI_Panel(Vector2 textureSize, UiAnchorAlignment alignment, Vector2 size) : this(textureSize, alignment, Vector4.Zero, size) { }
    
    public UI_Panel(Vector2 textureSize, Vector4 offset, Vector2 size) : this(textureSize, UiAnchorAlignment.MiddleCenter, offset, size) { }
    public UI_Panel(Vector2 textureSize, Vector2 size) : this(textureSize, UiAnchorAlignment.MiddleCenter, Vector4.Zero, size) { }
    
    public override void RenderUI(MeshData meshData)
    {
        Align();
        UI.Generate9Slice(position, textureSize.X, textureSize.Y, size.X, size.Y, 10f, new Vector4(10f, 10f, 10f, 10f), meshData);
    }
}