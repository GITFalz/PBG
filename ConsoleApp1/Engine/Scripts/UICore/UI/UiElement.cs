using OpenTK.Mathematics;
using Vulkan;

public abstract class UiElement
{
    public string Name = "";
    
    public Vector3 Position = Vector3.Zero;
    public Vector3 Scale = new Vector3(100, 100, 0);
    public Vector3 Rotation = Vector3.Zero;
    public Vector4 Offset = new Vector4(0, 0, 0, 0);
    
    public Vector3 Origin = new Vector3(-50, -50, 0);
    public Vector3 HalfScale = new Vector3(50, 50, 0);
    
    public AnchorType AnchorType = AnchorType.MiddleCenter;
    public PositionType PositionType = PositionType.Absolute;

    public abstract void Generate();
    public abstract void Align();
    
    public void SetAnchorType(AnchorType anchor)
    {
        AnchorType = anchor;
    }
    
    public void SetPositionType(PositionType type)
    {
        PositionType = type;
    }
    
    public void SetScale(Vector3 scale)
    {
        Scale = scale;
        HalfScale = scale / 2;
    }
    
    public void SetOffset(Vector4 offset)
    {
        Offset = offset;
    }
}

public enum AnchorType
{
    TopLeft,
    TopCenter,
    TopRight,
    MiddleLeft,
    MiddleCenter,
    MiddleRight,
    BottomLeft,
    BottomCenter,
    BottomRight
}

public enum PositionType
{
    Absolute,
    Relative
}