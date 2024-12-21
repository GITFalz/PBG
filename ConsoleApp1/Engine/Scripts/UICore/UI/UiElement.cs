using OpenTK.Mathematics;

public abstract class UiElement
{
    public string Name = "";
    
    public Vector3 position = Vector3.Zero;
    public Vector3 scale = new Vector3(100, 100, 0);
    public Vector3 rotation = Vector3.Zero;
    
    public Vector3 origin = new Vector3(-50, -50, 0);
    public Vector3 halfScale = new Vector3(50, 50, 0);
    
    public AnchorType anchorType = AnchorType.MiddleCenter;
    public PositionType positionType = PositionType.Absolute;

    public abstract void Generate();
    public abstract void Align();
    
    public void SetAnchorType(AnchorType anchor)
    {
        anchorType = anchor;
    }
    
    public void SetPositionType(PositionType type)
    {
        positionType = type;
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