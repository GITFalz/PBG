using OpenTK.Mathematics;

public abstract class StaticElement : UiElement
{
    public StaticElement? ParentElement;

    public override void Align()
    {
        Align(Scale);
    }
    public void Align(Vector3 scale)
    {
        float width;
        float height;

        Vector4 offset = Offset;
        
        if (PositionType == PositionType.Absolute || ParentElement == null)
        {
            width = Game.width;
            height = Game.height;
        }
        else
        {
            width = ParentElement.Scale.X;
            height = ParentElement.Scale.Y;
            
            offset.X += ParentElement.Origin.X;
            offset.Y = ParentElement.Origin.X - offset.Y;
            offset.Z += ParentElement.Origin.Y;
            offset.W = ParentElement.Origin.Y - offset.W;
            
            Console.WriteLine("Offset : " + offset);
        }
        
        Vector3 halfScale = scale / 2;
        
        if (AnchorType == AnchorType.TopLeft)
        {
            Position = Vector3.Zero + halfScale + new Vector3(offset.X, offset.Z, 0);
        }
        else if (AnchorType == AnchorType.TopCenter)
        {
            Position = new Vector3(width / 2f, halfScale.Y, 0) + new Vector3(0, offset.Z, 0);
        }
        else if (AnchorType == AnchorType.TopRight)
        {
            Position = new Vector3(width - halfScale.X, halfScale.Y, 0) + new Vector3(offset.Y, offset.Z, 0);
        }
        else if (AnchorType == AnchorType.MiddleLeft)
        {
            Position = new Vector3(halfScale.X, height / 2f, 0) + new Vector3(offset.X, 0, 0);
        }
        else if (AnchorType == AnchorType.MiddleCenter)
        {
            Position = new Vector3(width / 2f, height / 2f, 0) + new Vector3(0, 0, 0);
        }
        else if (AnchorType == AnchorType.MiddleRight)
        {
            Position = new Vector3(width - halfScale.X, height / 2f, 0) + new Vector3(offset.Y, 0, 0);
        }
        else if (AnchorType == AnchorType.BottomLeft)
        {
            Position = new Vector3(halfScale.X, height - halfScale.Y, 0) + new Vector3(offset.X, offset.W, 0);
        }
        else if (AnchorType == AnchorType.BottomCenter)
        {
            Position = new Vector3(width / 2f, height - halfScale.Y, 0) + new Vector3(0, offset.W, 0);
        }
        else if (AnchorType == AnchorType.BottomRight)
        {
            Position = new Vector3(width - halfScale.X, height - halfScale.Y, 0) + new Vector3(offset.Y, offset.W, 0);
        }
        
        Origin = Position - halfScale;
    }
}