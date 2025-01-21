using OpenTK.Mathematics;

public abstract class UIElement
{
    public string Name = "UI Element";
    public static Vector3 _rotationAxis = new Vector3(0, 0, 1);

    public UIElement? ParentElement = null;
    public Vector3 Origin = (0, 0, 0);
    public Vector3 Center = (50, 50, 0);
    public Vector3 Pivot = (0, 0, 0);
    public Vector2 Scale = (100, 100);
    public Vector4 Offset = (0, 0, 0, 0);  
    public float Rotation = 0f;
    public int TextureIndex = 0;
    public int ElementIndex = 0;

    public AnchorType anchorType = AnchorType.MiddleCenter;
    public PositionType positionType = PositionType.Absolute;

    public float Width = 0;
    public float Height = 0;
    public Vector4 ScreenOffset = new Vector4(0, 0, 0, 0);

    public Matrix4 Transformation = Matrix4.Identity;

    public UIMesh? uIMesh;
    public Panel panel = new();

    public UIElement(AnchorType anchorType, PositionType positionType, Vector3 pivot, Vector2 scale, Vector4 offset, float rotation, int textureIndex, UIMesh? uIMesh)
    {
        this.anchorType = anchorType;
        this.positionType = positionType;
        this.Pivot = pivot;
        this.Scale = scale;
        this.Offset = offset;
        this.Rotation = rotation;
        this.TextureIndex = textureIndex;
        this.uIMesh = uIMesh;
    }

    public virtual void Generate()
    {   
        Align();

        panel = new Panel();

        Vector3 position1 = Mathf.RotateAround(Origin, Pivot, _rotationAxis, Rotation);
        Vector3 position2 = Mathf.RotateAround(Origin + new Vector3(0, Scale.Y, 0), Pivot, _rotationAxis, Rotation);
        Vector3 position3 = Mathf.RotateAround(Origin + new Vector3(Scale.X, Scale.Y, 0), Pivot, _rotationAxis, Rotation);
        Vector3 position4 = Mathf.RotateAround(Origin + new Vector3(Scale.X, 0, 0), Pivot, _rotationAxis, Rotation);
        
        panel.Vertices.Add(position1);
        panel.Vertices.Add(position2);
        panel.Vertices.Add(position3);
        panel.Vertices.Add(position4);
        
        panel.Uvs.Add(new Vector2(0, 0));
        panel.Uvs.Add(new Vector2(0, 1));
        panel.Uvs.Add(new Vector2(1, 1));
        panel.Uvs.Add(new Vector2(1, 0));
        
        panel.TextUvs.Add(TextureIndex);
        panel.TextUvs.Add(TextureIndex);
        panel.TextUvs.Add(TextureIndex);
        panel.TextUvs.Add(TextureIndex);
        
        panel.UiSizes.Add(new Vector2(Scale.X, Scale.Y));
        panel.UiSizes.Add(new Vector2(Scale.X, Scale.Y));
        panel.UiSizes.Add(new Vector2(Scale.X, Scale.Y));
        panel.UiSizes.Add(new Vector2(Scale.X, Scale.Y));

        uIMesh?.AddElement(this, ref ElementIndex);
    }

    public virtual void Generate(ref int offset) {}

    public void Align()
    {
        if (positionType == PositionType.Free)
            return;

        if (positionType == PositionType.Relative && ParentElement != null)
        {
            Width = ParentElement.Scale.X;
            Height = ParentElement.Scale.Y;

            Transformation = Matrix4.CreateTranslation(GetTransformedOrigin() + (0, 0, 0.01f)) * ParentElement.Transformation;
        }
        else
        {
            Width = Game.width - (ScreenOffset.X + ScreenOffset.Y);
            Height = Game.height - (ScreenOffset.Z + ScreenOffset.W);

            Transformation = Matrix4.CreateTranslation(GetTransformedOrigin());
        }
    }

    public Vector4 GetTransformedOffset()
    {
        int index = (int)anchorType;
        return index >= offsets.Length ? (0, 0, 0, 0) : offsets[index](Offset);
    }

    public Vector3 GetTransformedOrigin()
    {
        int index = (int)anchorType;
        return index >= origins.Length ? (0, 0, 0) : origins[index](Width, Height, Scale, Origin, GetTransformedOffset());
    }

    private static readonly Func<Vector4, Vector4>[] offsets =
    [
        (v) => (v.X, 0, v.Z, 0),
        (v) => (0, 0, v.Z, 0),
        (v) => (0, -v.Y, v.Z, 0),
        (v) => (v.X, 0, 0, 0),
        (v) => (0, 0, 0, 0),
        (v) => (0, -v.Y, 0, 0),
        (v) => (v.X, 0, 0, -v.W),
        (v) => (0, 0, 0, -v.W),
        (v) => (0, -v.Y, 0, -v.W),
        (v) => (v.X, 0, v.Z, -v.W),
        (v) => (0, 0, v.Z, -v.W),
        (v) => (0, -v.Y, v.Z, -v.W),
        (v) => (v.X, -v.Y, v.Z, 0),
        (v) => (v.X, -v.Y, 0, 0),
        (v) => (v.X, -v.Y, 0, -v.W),
    ];

    // w : width, h : height, s : scale, p : position, o : offset
    private static readonly Func<float, float, Vector2, Vector3, Vector4, Vector3>[] origins =
    [
        (w, h, s, p, o) => (p.X + o.X,                  p.Y + o.Z,                  0), // TopLeft
        (w, h, s, p, o) => (p.X + (w / 2) - (s.X / 2),  p.Y + o.Z,                  0), // TopCenter
        (w, h, s, p, o) => (p.X + w + o.Y - s.X,        p.Y + o.Z,                  0), // TopRight
        (w, h, s, p, o) => (p.X + o.X,                  p.Y + (h / 2) - (s.Y / 2),  0), // MiddleLeft
        (w, h, s, p, o) => (p.X + (w / 2) - (s.X / 2),  p.Y + (h / 2) - (s.Y / 2),  0), // MiddleCenter
        (w, h, s, p, o) => (p.X + w + o.Y - s.X,        p.Y + (h / 2) - (s.Y / 2),  0), // MiddleRight
        (w, h, s, p, o) => (p.X + o.X,                  p.Y + h + o.W - s.Y,        0), // BottomLeft
        (w, h, s, p, o) => (p.X + (w / 2) - (s.X / 2),  p.Y + h + o.W - s.Y,        0), // BottomCenter
        (w, h, s, p, o) => (p.X + w + o.Y - s.X,        p.Y + h + o.W - s.Y,        0), // BottomRight
        (w, h, s, p, o) => (p.X + o.X,                  p.Y + o.Z,                  0), // ScaleLeft
        (w, h, s, p, o) => (p.X + (w / 2) - (s.X / 2),  p.Y + o.Z,                  0), // ScaleCenter
        (w, h, s, p, o) => (p.X + w + o.Y - s.X,        p.Y + o.Z,                  0), // ScaleRight
        (w, h, s, p, o) => (p.X + o.X,                  p.Y + o.Z,                  0), // ScaleTop
        (w, h, s, p, o) => (p.X + o.X,                  p.Y + (h / 2) - (s.Y / 2),  0), // ScaleMiddle
        (w, h, s, p, o) => (p.X + o.X,                  p.Y + h + o.W - s.Y,        0), // ScaleBottom
    ];
}