using OpenTK.Mathematics;

public abstract class UIRender : UIElement
{
    public int TextureIndex = 0;
    public Vector4 Color = (1, 1, 1, 1);
    public Vector4 SizeSlice = (0, 0, 10, 0.15f);
    public UIMesh uIMesh;

    public UIRender() : base() { uIMesh = UIMesh.Empty; } 

    public UIRender(
        string name,
        UIController controller,
        AnchorType anchorType, 
        PositionType positionType, 
        Vector4 color, 
        Vector3 pivot, 
        Vector2 scale, 
        Vector4 offset, 
        float rotation, 
        int textureIndex, 
        Vector4 sizeSlice
    ) : base(name, controller, anchorType, positionType, pivot, scale, offset, rotation)
    {
        TextureIndex = textureIndex;
        Color = color;
        SizeSlice = sizeSlice;
        uIMesh = controller.UIMesh;
    }

    public override void SetMaskIndex(int maskedIndex)
    {
        base.SetMaskIndex(maskedIndex);
        uIMesh.UpdateMaskedIndex(ElementIndex, Masked, MaskIndex);
    }
}