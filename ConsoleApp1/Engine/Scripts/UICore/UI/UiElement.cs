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
    
    public Vector3 Pivot;
    public Quaternion Rotation = Quaternion.Identity;
    
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
        return pos.X >= Origin.X && pos.X <= Origin.X + Scale.X && pos.Y >= Origin.Y && pos.Y <= Origin.Y + Scale.Y;
    }
    
    public bool IsMouseOver(Vector2 offset)
    {
        Vector2 pos = Input.GetMousePosition();
        return pos.X >= Origin.X + offset.X && pos.X <= Origin.X + offset.X + Scale.X && pos.Y >= Origin.Y + offset.Y && pos.Y <= Origin.Y + offset.Y + Scale.Y;
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
    BottomRight,
    ScaleLeft,
    ScaleCenter,
    ScaleRight,
    ScaleTop,
    ScaleMiddle,
    ScaleBottom
}

public enum PositionType
{
    Absolute,
    Relative,
    Free
}