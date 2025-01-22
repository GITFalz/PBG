using OpenTK.Mathematics;

public abstract class UIElement
{
    public string Name = "UI Element";
    public string SceneName = "Scene";
    public static Vector3 _rotationAxis = new Vector3(0, 0, 1);

    public UIElement? ParentElement = null;
    public Vector3 Origin = (0, 0, 0);
    public Vector3 Pivot = (0, 0, 0);
    public Vector2 Scale = (100, 100);
    public Vector4 Offset = (0, 0, 0, 0);  
    public float Rotation = 0f;
    public bool Rotated = false;
    public bool test = false;
    public int TextureIndex = 0;
    public int ElementIndex = 0;
    public float Depth = 0;

    public AnchorType AnchorType = AnchorType.MiddleCenter;
    public PositionType PositionType = PositionType.Absolute;
    public UIState State = UIState.Static;

    public float Width = 0;
    public float Height = 0;
    public Vector4 ScreenOffset = new Vector4(0, 0, 0, 0);

    public Matrix4 Transformation = Matrix4.Identity;
    public Panel panel = new();

    public UIElement(string name, AnchorType anchorType, PositionType positionType, Vector3 pivot, Vector2 scale, Vector4 offset, float rotation, int textureIndex)
    {
        Name = name;
        this.AnchorType = anchorType;
        this.PositionType = positionType;
        this.Pivot = pivot;
        this.Scale = scale;
        this.Offset = offset;
        this.Rotation = rotation;
        this.TextureIndex = textureIndex;
    }

    public virtual void Generate() {   }

    public virtual void SetUIMesh(UIMesh uIMesh) { }
    public virtual void SetTextMesh(TextMesh textMesh) { }

    public virtual void Generate(ref int offset) {}
    public virtual List<string> ToLines(int gap) { return []; }
    public virtual bool Test() { return test; }
    public virtual bool Test(Vector2 offset) { return test; }

    public bool IsMouseOver()
    {
        Vector2 pos = Input.GetMousePosition();
        return MouseOver(pos, Origin.Xy, Scale);
    }
    
    public bool IsMouseOver(Vector2 offset)
    {
        Vector2 pos = Input.GetMousePosition();
        return MouseOver(pos, Origin.Xy + offset, Scale);
    }

    private bool MouseOver(Vector2 pos, Vector2 origin, Vector2 scale)
    {
        if (Rotated)
        {
            Vector3 point1 = Mathf.RotateAround((origin.X, origin.Y, 0), Pivot, _rotationAxis, Rotation);
            Vector3 point2 = Mathf.RotateAround((origin.X + scale.X, origin.Y, 0), Pivot, _rotationAxis, Rotation);
            Vector3 point3 = Mathf.RotateAround((origin.X + scale.X, origin.Y + scale.Y, 0), _rotationAxis, Pivot, Rotation);
            Vector3 point4 = Mathf.RotateAround((origin.X, origin.Y + scale.Y, 0), Pivot, _rotationAxis, Rotation);

            return IsPointInRotatedRectangle(pos, [point1.Xy, point2.Xy, point3.Xy, point4.Xy]);
        }
        else
        {
            return pos.X >= origin.X && pos.X <= origin.X + scale.X && pos.Y >= origin.Y && pos.Y <= origin.Y + scale.Y;
        }
    }

    public void Align()
    {
        if (PositionType == PositionType.Free)
            return;

        if (PositionType == PositionType.Relative && ParentElement != null)
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

    public void GenerateUIQuad(out Panel panel, UIMesh uIMesh)
    {
        panel = new Panel();

        Vector3 position1 = Mathf.RotateAround(Origin + (0, 0, Depth),              Pivot, _rotationAxis, Rotation);
        Vector3 position2 = Mathf.RotateAround(Origin + (0, Scale.Y, Depth),        Pivot, _rotationAxis, Rotation);
        Vector3 position3 = Mathf.RotateAround(Origin + (Scale.X, Scale.Y, Depth),  Pivot, _rotationAxis, Rotation);
        Vector3 position4 = Mathf.RotateAround(Origin + (Scale.X, 0, Depth),        Pivot, _rotationAxis, Rotation);
        
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

    public string GetMethodString(SerializableEvent? e)
    {
        return e == null ? "null" : (e.IsStatic ? "Static" : SceneName) + "." + e.TargetName + "." + e.MethodName + (e.FixedParameter == null ? "" : $"({e.FixedParameter})");
    }

    public Vector4 GetTransformedOffset()
    {
        int index = (int)AnchorType;
        return index >= offsets.Length ? (0, 0, 0, 0) : offsets[index](Offset);
    }

    public Vector3 GetTransformedOrigin()
    {
        int index = (int)AnchorType;
        return index >= origins.Length ? (0, 0, 0) : origins[index](Width, Height, Scale, Origin, GetTransformedOffset());
    }

    public List<string> GetBasicDisplayLines(int gap)
    {
        List<string> lines = [];
        string gapString = "";
        for (int i = 0; i < gap; i++)
        {
            gapString += "    ";
        }
        
        lines.Add(gapString + "    Name: " + Name);
        lines.Add(gapString + "    Pivot: " + Pivot);
        lines.Add(gapString + "    Scale: " + Scale);
        lines.Add(gapString + "    Offset: " + Offset);
        lines.Add(gapString + "    Rotation: " + Rotation);
        lines.Add(gapString + "    AnchorType: " + (int)AnchorType);
        lines.Add(gapString + "    PositionType: " + (int)PositionType);
        lines.Add(gapString + "}");
        
        return lines;
    }

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

public enum AnchorType
{
    TopLeft = 0,
    TopCenter = 1,
    TopRight = 2,
    MiddleLeft = 3,
    MiddleCenter = 4,
    MiddleRight = 5,
    BottomLeft = 6, 
    BottomCenter = 7,
    BottomRight = 8, 
    ScaleLeft = 9,
    ScaleCenter = 10,
    ScaleRight = 11,
    ScaleTop = 12,
    ScaleMiddle = 13,
    ScaleBottom = 14
}

public enum PositionType
{
    Absolute,
    Relative,
    Free
}

public enum UIState
{
    Static,
    Interactable,
    InvisibleInteractable,
    Disabled,
}