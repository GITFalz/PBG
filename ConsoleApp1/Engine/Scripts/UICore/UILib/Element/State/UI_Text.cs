using OpenTK.Mathematics;

public abstract class UI_Text : UI_Base
{
    public string text = "";
    protected float fontSize;
    protected char[] textArray = [];
    protected int _charCount;
    
    protected DynamicTextMesh mesh;

    protected Vector2 textSize;
    
    protected Vector3 _textPosition;
    protected Vector3 _lastPosition;

    protected Vector3 _textOffset = new Vector3(0, 0, 0);
    
    protected UiAnchorAlignment _textAlignment = UiAnchorAlignment.MiddleCenter;
    
    public abstract void SetText(string text, float fontSize);
    public abstract void SetMesh(TextMesh mesh);
    public abstract void SetTextAlignment(UiAnchorAlignment alignment);
    
    #region Text Alignment Functions
    
    protected float GetTextWidth() { return _charCount * 7 * fontSize; }
    protected float GetTextHeight() { return 9 * fontSize; }
    protected Vector2 GetTextSize() { return new Vector2(GetTextWidth(), GetTextHeight()); }
    
    protected float GetOffsetX() { return (size.X - textSize.X) / 2; } 
    protected float GetOffsetXEnd() { return size.X - textSize.X; } 
    protected float GetOffsetY() { return (size.Y - textSize.Y) / 2; }
    protected float GetOffsetYEnd() { return size.Y - textSize.Y; }


    protected static readonly Dictionary<UiAnchorAlignment, Action<UI_DynamicText>> TextAlignment = new Dictionary<UiAnchorAlignment, Action<UI_DynamicText>>
    {
        { UiAnchorAlignment.TopLeft, (t) => t._textOffset =      new Vector3(0,                 0,                 0) },
        { UiAnchorAlignment.TopCenter, (t) => t._textOffset =    new Vector3(t.GetOffsetX(),    0,                 0) },
        { UiAnchorAlignment.TopRight, (t) => t._textOffset =     new Vector3(t.GetOffsetXEnd(), 0,                 0) },
        { UiAnchorAlignment.MiddleLeft, (t) => t._textOffset =   new Vector3(0,                 t.GetOffsetY(),    0) },
        { UiAnchorAlignment.MiddleCenter, (t) => t._textOffset = new Vector3(t.GetOffsetX(),    t.GetOffsetY(),    0) },
        { UiAnchorAlignment.MiddleRight, (t) => t._textOffset =  new Vector3(t.GetOffsetXEnd(), t.GetOffsetY(),    0) },
        { UiAnchorAlignment.BottomLeft, (t) => t._textOffset =   new Vector3(0,                 t.GetOffsetYEnd(), 0) },
        { UiAnchorAlignment.BottomCenter, (t) => t._textOffset = new Vector3(t.GetOffsetX(),    t.GetOffsetYEnd(), 0) },
        { UiAnchorAlignment.BottomRight, (t) => t._textOffset =  new Vector3(t.GetOffsetXEnd(), t.GetOffsetYEnd(), 0) }
    };
    
    #endregion
}