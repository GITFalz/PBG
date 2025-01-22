using OpenTK.Mathematics;
using Vulkan;

public abstract class UiElement
{
    private readonly string _name = Guid.NewGuid().ToString();
    public string Name = "";
    
    public Vector3 Position = Vector3.Zero;
    public Vector3 Scale = new Vector3(100, 100, 0);
    public Vector4 Offset = new Vector4(0, 0, 0, 0);
    
    public Vector3 Origin = new Vector3(-50, -50, 0);
    public Vector3 HalfScale = new Vector3(50, 50, 0);
    
    public Vector4 ScreenOffset = new Vector4(0, 0, 0, 0);
    public float Depth = 0;
    
    public Vector3 Pivot;
    public Quaternion Rotation = Quaternion.Identity;
    public bool Rotated = false;
    
    public OriginType OriginType = OriginType.Center;
    
    public AnchorType AnchorType = AnchorType.MiddleCenter;
    public PositionType PositionType = PositionType.Absolute;
    
    public string SceneName = "";

    public abstract void Generate();
    public virtual void Generate(Vector3 offset) { }
    public virtual void Create(Vector3 position) { }
    public abstract void Align();
    public virtual void Reset() {}
    public virtual bool HasChild(UiElement element) { return false; }
    
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
    
    public void SetPosition(Vector3 position)
    {
        Position = position;
        Origin = position - HalfScale;
    }
    
    public virtual void ToFile(string path, int gap = 1) {}
    public virtual List<string> ToLines(int gap) { return new List<string>(); }
    
    public bool IsMouseOver()
    {
        Vector2 pos = Input.GetMousePosition();
        return MouseOver(pos, Origin.Xy, Scale.Xy);
    }
    
    public bool IsMouseOver(Vector2 offset)
    {
        Vector2 pos = Input.GetMousePosition();
        return MouseOver(pos, Origin.Xy + offset, Scale.Xy);
    }

    private bool MouseOver(Vector2 pos, Vector2 origin, Vector2 scale)
    {
        if (Rotated)
        {
            Vector3 point1 = Mathf.RotateAround((origin.X, origin.Y, 0), Pivot, Rotation);
            Vector3 point2 = Mathf.RotateAround((origin.X + scale.X, origin.Y, 0), Pivot, Rotation);
            Vector3 point3 = Mathf.RotateAround((origin.X + scale.X, origin.Y + scale.Y, 0), Pivot, Rotation);
            Vector3 point4 = Mathf.RotateAround((origin.X, origin.Y + scale.Y, 0), Pivot, Rotation);

            return IsPointInRotatedRectangle(pos, [point1.Xy, point2.Xy, point3.Xy, point4.Xy]);
        }
        else
        {
            return pos.X >= origin.X && pos.X <= origin.X + scale.X && pos.Y >= origin.Y && pos.Y <= origin.Y + scale.Y;
        }
    }

    public override bool Equals(object? obj)
    {
        return obj is UiElement element && Name == element.Name;
    }

    public override int GetHashCode()
    {
        return _name.GetHashCode();
    }

    public void Move(Vector4 offset)
    {
        MoveParameters[AnchorType](this, offset);
    }

    private readonly Dictionary<AnchorType, Action<UiElement, Vector4>> MoveParameters = new Dictionary<AnchorType, Action<UiElement, Vector4>>()
    {
        { AnchorType.TopLeft, (element, offset) => { element.Offset += offset; } },
        { AnchorType.TopCenter, (element, offset) => { element.Offset += offset; } },
        { AnchorType.TopRight, (element, offset) => { element.Offset += offset; } },
        { AnchorType.MiddleLeft, (element, offset) => { element.Offset += offset; } },
        { AnchorType.MiddleCenter, (element, offset) => { element.Offset += offset; } },
        { AnchorType.MiddleRight, (element, offset) => { element.Offset += offset; } },
        { AnchorType.BottomLeft, (element, offset) => { element.Offset += offset; } },
        { AnchorType.BottomCenter, (element, offset) => { element.Offset += offset; } },
        { AnchorType.BottomRight, (element, offset) => { element.Offset += offset; } },
        { AnchorType.ScaleLeft, (element, offset) => { element.Offset += (offset.X, offset.Y, 0, 0); } },
        { AnchorType.ScaleCenter, (element, offset) => { element.Offset += (offset.X, offset.Y, 0, 0); } },
        { AnchorType.ScaleRight, (element, offset) => { element.Offset += (offset.X, offset.Y, 0, 0); } },
        { AnchorType.ScaleTop, (element, offset) => { element.Offset += (0, 0, offset.Z, offset.W); } },
        { AnchorType.ScaleMiddle, (element, offset) => { element.Offset += (0, 0, offset.Z, offset.W); } },
        { AnchorType.ScaleBottom, (element, offset) => { element.Offset += (0, 0, offset.Z, offset.W); } }
    };

    private static bool IsPointInRotatedRectangle(Vector2 point, Vector2[] rectanglePoints)
    {
        if (rectanglePoints.Length != 4)
            return false;

        Vector2 edge1 = rectanglePoints[1] - rectanglePoints[0];
        Vector2 edge2 = rectanglePoints[3] - rectanglePoints[0];

        Vector2 pointRelative = point - rectanglePoints[0];

        float dot1 = Vector2.Dot(pointRelative, edge1);
        float dot2 = Vector2.Dot(pointRelative, edge2);

        float edge1LengthSq = Vector2.Dot(edge1, edge1);
        float edge2LengthSq = Vector2.Dot(edge2, edge2);

        return dot1 >= 0 && dot1 <= edge1LengthSq &&
               dot2 >= 0 && dot2 <= edge2LengthSq;
    }
}