using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public abstract class UIElement
{
    public static Vector3 _rotationAxis = new Vector3(0, 0, 1);

    public UIController UIController;
    public UIElement? ParentElement = null;
    public string Name = "";

    // Rendering Settings
    public bool Visible = true;
    public Vector3 Origin = (0, 0, 0);
    public Vector3 Center = (0, 0, 0);
    public Vector3 Pivot = (0, 0, 0);
    public Vector2 Scale = (100, 100);
    public Vector2 newScale = (100, 100);
    public Vector4 Offset = (0, 0, 0, 0);  

    public float Rotation = 0f;
    public bool Rotated = false;
    public bool CanTest = true;
    public bool CanUpdate = false;
    public bool Masked = false;
    public int MaskIndex = 0;
    public float Depth = 0;


    private Vector3 _transformedOrigin = (0, 0, 0);

    public AnchorType AnchorType = AnchorType.MiddleCenter;
    public PositionType PositionType = PositionType.Absolute;
    public UIState State = UIState.Static;

    public float Width = 0;
    public float Height = 0;

    public Matrix4 Transformation = Matrix4.Identity;

    public SerializableEvent? OnHover { get; private set; } = null;
    public SerializableEvent? OnClick { get; private set; } = null;
    public SerializableEvent? OnHold { get; private set; } = null;
    public SerializableEvent? OnRelease { get; private set; } = null;
    public SerializableEvent? OnHoverOut { get; private set; } = null;
    public Action? OnAlign;
    private bool _clicked = false;

    public UIElement() {}
    public UIElement(string name, UIController controller, AnchorType anchorType, PositionType positionType, Vector3 pivot, Vector2 scale, Vector4 offset, float rotation)
    {
        Name = name;
        UIController = controller;
        AnchorType = anchorType;
        PositionType = positionType;
        Pivot = pivot;
        Scale = scale;
        newScale = scale;
        Offset = offset;
        Rotation = rotation;
        CanTest = false;
    }

    public virtual void SetVisibility(bool visible) { Visible = visible; UIController.UpdateVisibility = true; }
    public virtual void SetMasked(bool masked) { Masked = masked; }
    public virtual void SetMaskIndex(int maskIndex) { MaskIndex = maskIndex; }
    public virtual void Move(Vector3 offset) { Origin += offset; GetTransformation(); }


    public virtual void SetScale(Vector2 scale) { Scale = scale; newScale = scale; }
    public virtual void SetOffset(Vector4 offset) { Offset = offset; }
    public virtual void SetAnchorType(AnchorType anchorType) { AnchorType = anchorType; }
    public virtual void SetPositionType(PositionType positionType) { PositionType = positionType; }
    


    public virtual void Generate() { }

    public virtual void CalculateScale() {} // Used in collections

    /// <summary>
    /// Used when you want elements to be aligned at the end of the frame. 
    /// This can prevent you from clicking a button that re-aligns BEFORE it is passed trough the mouse clicking test.
    /// </summary>
    public void QueueAlign()
    {
        UIController.QueueAlign(this);
    }
    
    /// <summary>
    /// Used when you want the element's transformation to be updated at the end of the frame.
    /// This can prevent you from clicking a button that re-aligns BEFORE it is passed trough the mouse clicking test.
    /// </summary>
    public void QueueUpdateTransformation()
    {
        UIController.QueueElementTransformation(this);
    }

    public virtual void Align()
    {
        Masked = false;

        if (PositionType == PositionType.Relative && ParentElement != null)
        {
            Width = ParentElement.newScale.X;
            Height = ParentElement.newScale.Y;
            GetTransformedOrigin();
            Origin = _transformedOrigin + new Vector3(0, 0, 0.01f) + (new Vector3(0f, 0f, 0.01f) * (Depth + ParentElement.Depth)) + ParentElement.Origin;
        }
        else
        {
            Width = Game.Width;
            Height = Game.Height;
            GetTransformedOrigin();
            Origin = _transformedOrigin;
        }

        GetTransformation();
        if ((int)AnchorType >= 9) newScale = _dimensions[(int)AnchorType - 9](Width, Height, Scale, Offset);
        SetScale(newScale);
        Center = Origin + new Vector3(newScale.X / 2, newScale.Y / 2, 0);

        if (Name != "MainPanelStacking")
            return;

        Console.WriteLine();
        Console.WriteLine($"Name: {Name}, PositionType: {PositionType}, AnchorType: {AnchorType}");
        Console.WriteLine($"Origin: {Origin}, Transformed: {_transformedOrigin}, Center: {Center}, Scale: {newScale}, Offset: {Offset}, Width: {Width}, Height: {Height}");
        Console.WriteLine($"Parent: {ParentElement?.Name}, Origin: {ParentElement?.Origin}, Scale: {ParentElement?.newScale}, Offset: {ParentElement?.Offset}, Width: {ParentElement?.Width}, Height: {ParentElement?.Height}");
    }

    public virtual void ResetInit() {}
    public virtual void Delete() { UIController.RemoveElement(this); }
    public virtual void RemoveChild(UIElement element) {}
            

    protected virtual void Internal_UpdateTransformation() {}
    public void UpdateTransformation() 
    {
        if (CanUpdate)
            Internal_UpdateTransformation();
    }

    protected virtual void Internal_UpdateScale() {}
    public void UpdateScale() 
    {
        if (CanUpdate)
            Internal_UpdateScale();
    }

    protected virtual void Internal_UpdateTexture() {}
    public void UpdateTexture() 
    {
        if (CanUpdate)
            Internal_UpdateTexture();
    }

    public virtual bool RemoveElement(UIElement element) { return false;}
    public abstract float GetYScale();
    public abstract float GetXScale();

    # region Setters for Events
    public void SetOnClick(Action action)
    {
        OnClick = new SerializableEvent(action); 
        CanTest = true ;
    } 
    public void SetOnHover(Action action)
    {
        OnHover = new SerializableEvent(action); 
        CanTest = true;
    }
    public void SetOnHold(Action action)
    {
        OnHold = new SerializableEvent(action); 
        CanTest = true;
    }
    public void SetOnRelease(Action action)
    {
        OnRelease = new SerializableEvent(action); 
        CanTest = true;
    }
    public void SetOnHoverOut(Action action)
    {
        OnHoverOut = new SerializableEvent(action); 
        CanTest = true;
    }
    # endregion

    # region Mouse Events
    public virtual bool Test(Vector2 offset = default)
    { 
        if (!CanTest || !Visible) 
            return false;

        TestButtons(IsMouseOver(offset));  
        return true;
    }
    public bool IsMouseOver(Vector2 offset = default)
    {
        Vector2 pos = Input.GetMousePosition();
        return MouseOver(pos, Origin.Xy, offset, Scale);
    }
    private void TestButtons(bool mouseOver)
    {
        if (mouseOver)
        {
            OnHover?.Invoke();

            if (Input.IsMousePressed(MouseButton.Left) && !_clicked)
            {  
                OnClick?.Invoke();
                _clicked = true;
            }
        }
        
        if (_clicked)
        {
            OnHold?.Invoke();
        }
            
        if (Input.IsMouseReleased(MouseButton.Left))
        {
            if (_clicked)
            {
                OnRelease?.Invoke();
                _clicked = false;
            }

            if (mouseOver)
                OnHoverOut?.Invoke();
        }
    }

    private bool MouseOver(Vector2 pos, Vector2 origin, Vector2 offset, Vector2 scale)
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
            Vector2 point1 = Vector3.TransformPosition((origin.X, origin.Y, 0), UIController.ModelMatrix).Xy + offset;
            Vector2 point2 = Vector3.TransformPosition((origin.X + scale.X, origin.Y + scale.Y, 0), UIController.ModelMatrix).Xy + offset;

            return pos.X >= point1.X && pos.X <= point2.X && pos.Y >= point1.Y && pos.Y <= point2.Y;
        }
    }
    # endregion

    public static string GetMethodString(SerializableEvent? e)
    {
        return e == null ? "null" : e.GetMethodString();
    }

    public Vector3 GetTransformedOrigin()
    {
        int index = (int)AnchorType;
        _transformedOrigin = origins[index](Width, Height, Scale, Offset);
        return _transformedOrigin;
    }

    public Matrix4 GetTransformation()
    {
        Transformation = Matrix4.CreateTranslation(Origin);
        return Transformation;
    }

    public List<string> GetBasicDisplayLines(string gapString)
    {
        List<string> lines = [];
        lines.Add(gapString + "    Name: " + Name);
        lines.Add(gapString + "    Pivot: " + Pivot);
        lines.Add(gapString + "    Scale: " + Scale);
        lines.Add(gapString + "    Offset: " + Offset);
        lines.Add(gapString + "    Rotation: " + Rotation);
        lines.Add(gapString + "    AnchorType: " + (int)AnchorType);
        lines.Add(gapString + "    PositionType: " + (int)PositionType);
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

    private static readonly Func<float, float, Vector2, Vector4, Vector2>[] _dimensions =
    [
        (w, h, s, o) => (s.X, h - o.Y - o.W),           // ScaleLeft   
        (w, h, s, o) => (s.X, h - o.Y - o.W),           // ScaleCenter
        (w, h, s, o) => (s.X, h - o.Y - o.W),           // ScaleRight
        (w, h, s, o) => (w - o.X - o.Z, s.Y),           // ScaleTop
        (w, h, s, o) => (w - o.X - o.Z, s.Y),           // ScaleMiddle
        (w, h, s, o) => (w - o.X - o.Z, s.Y),           // ScaleBottom
        (w, h, s, o) => (w - o.X - o.Z, h - o.Y - o.W), // ScaleFull
    ];

    // w : width, h : height, s : scale, o : offset
    private static readonly Func<float, float, Vector2, Vector4, Vector3>[] origins =
    [
        (w, h, s, o) => (o.X,                   o.Y,                   0), // TopLeft
        (w, h, s, o) => (w / 2 - s.X / 2 + o.X, o.Y,                   0), // TopCenter
        (w, h, s, o) => (w - s.X + o.X,         o.Y,                   0), // TopRight
        (w, h, s, o) => (o.X,                   h / 2 - s.Y / 2 + o.Y, 0), // MiddleLeft
        (w, h, s, o) => (w / 2 - s.X / 2 + o.X, h / 2 - s.Y / 2 + o.Y, 0), // MiddleCenter
        (w, h, s, o) => (w - s.X + o.X,         h / 2 - s.Y / 2 + o.Y, 0), // MiddleRight
        (w, h, s, o) => (o.X,                   h - s.Y + o.Y,         0), // BottomLeft
        (w, h, s, o) => (w / 2 - s.X / 2 + o.X, h - s.Y + o.Y,         0), // BottomCenter
        (w, h, s, o) => (w - s.X + o.X,         h - s.Y + o.Y,         0), // BottomRight
        (w, h, s, o) => (o.X,                   o.Y,                   0), // ScaleLeft
        (w, h, s, o) => (w / 2 - s.X / 2 + o.X, o.Y,                   0), // ScaleCenter
        (w, h, s, o) => (w - s.X + o.X,         o.Y,                   0), // ScaleRight
        (w, h, s, o) => (o.X,                   o.Y,                   0), // ScaleTop
        (w, h, s, o) => (o.X,                   h / 2 - s.Y / 2 + o.Y, 0), // ScaleMiddle
        (w, h, s, o) => (o.X,                   h - s.Y + o.Y,         0), // ScaleBottom
        (w, h, s, o) => (o.X,                   o.Y,                   0), // ScaleFull
    ];


    public virtual void Clear() { ParentElement = null; OnClick = null; OnHover = null; OnHold = null; OnRelease = null; _clicked = false; CanTest = false; CanUpdate = false; }
    public virtual List<string> ToLines(int gap) { return []; }

    public override string ToString()
    {
        return $"Name: {Name},\n" +
               $"AnchorType: {AnchorType},\n" +
               $"PositionType: {PositionType},\n" +
               $"Pivot: {Pivot},\n" +
               $"Scale: {Scale},\n" +
               $"NewScale: {newScale},\n" +
               $"Offset: {Offset},\n" +
               $"Rotation: {Rotation},\n" +
               $"State: {State}";
    }
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
    ScaleBottom = 14,
    ScaleFull = 15,
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