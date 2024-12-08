using OpenTK.Mathematics;

public class UI_Text : UI_Base
{
    public string text = "";
    
    private float fontSize;
    private char[] textArray = [];
    private int charCount;

    private Vector2 textSize;
    
    private Vector3 _textPosition;
    private Vector3 _lastPosition;

    private Vector3 _textOffset = new Vector3(0, 0, 0);
    
    private UiAnchorAlignment _textAlignment = UiAnchorAlignment.MiddleCenter;
    
    public UI_Text()
    {
        name = "UI_Text";
    }
    
    public UI_Text(TextMesh mesh)
    {
        name = "UI_Text";
        this.mesh = mesh;
    }

    public void SetText(string text, float fontSize)
    {
        this.text = text;
        this.fontSize = fontSize;
        textArray = text.ToCharArray();
        charCount = textArray.Length;
        
        memSize = textArray.Length;
        
        textSize = GetTextSize();
        
        if (mesh is TextMesh textMesh)
        {
            textMesh.chars = new int[memSize];
            for (int i = 0; i < memSize; i++)
            {
                textMesh.chars[i] = TextShaderHelper.CharPosition[textArray[i]];
            }
        }
    }
    
    public void SetTextAlignment(UiAnchorAlignment alignment)
    {
        _textAlignment = alignment;
    }
    
    public override void RenderText()
    {
        Align();
        AlignText();
        
        Vector2 size = new Vector2(charCount * 20, 20) * fontSize;
        
        mesh.AddQuad(new Vector3(0, 0, 0), MeshHelper.GenerateTextQuad(size.X, size.Y, 0, memSize, memPos));
    }

    private void AlignText()
    {
        if (!TextAlignment.TryGetValue(_textAlignment, out var value)) return;

        value(this);
        
        _textPosition = position + _textOffset;
    }
    
    #region Text Alignment Functions
    
    private float GetTextWidth() { return memSize * 7 * fontSize; }
    private float GetTextHeight() { return 9 * fontSize; }
    private Vector2 GetTextSize() { return new Vector2(GetTextWidth(), GetTextHeight()); }
    
    private float GetOffsetX() { return (size.X - textSize.X) / 2; } 
    private float GetOffsetXEnd() { return size.X - textSize.X; } 
    private float GetOffsetY() { return (size.Y - textSize.Y) / 2; }
    private float GetOffsetYEnd() { return size.Y - textSize.Y; }


    private static readonly Dictionary<UiAnchorAlignment, Action<UI_Text>> TextAlignment = new Dictionary<UiAnchorAlignment, Action<UI_Text>>
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