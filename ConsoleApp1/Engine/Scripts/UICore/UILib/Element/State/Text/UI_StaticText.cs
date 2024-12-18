using OpenTK.Mathematics;

public class UI_StaticText : UI_Text
{
    public string text = "";
    
    private float fontSize;
    private char[] textArray = [];
    private int _charCount;
    private int _charIndex;
    private int _index;
    
    private StaticTextMesh mesh;

    private Vector2 textSize;
    
    private Vector3 _textPosition;
    private Vector3 _lastPosition;

    private Vector3 _textOffset = new Vector3(0, 0, 0);
    
    private UiAnchorAlignment _textAlignment = UiAnchorAlignment.MiddleCenter;
    
    private bool textSet = false;
    
    public UI_StaticText()
    {
        name = "UI_Text";
    }
    
    public UI_StaticText(StaticTextMesh mesh)
    {
        name = "UI_Text";
        this.mesh = mesh;
    }
    
    public UI_StaticText(StaticTextMesh mesh, string text, float fontSize)
    {
        name = "UI_Text";
        this.mesh = mesh;
        SetText(text, fontSize);
    }
    
    public void SetMesh(StaticTextMesh mesh)
    {
        this.mesh = mesh;
    }

    public override void SetText(string text, float fontSize)
    {
        if (textSet)
            return;
        
        this.text = text;
        this.fontSize = fontSize;
        
        textArray = text.ToCharArray();
        _charCount = textArray.Length;
        _charIndex = mesh.charCount;
        _index = mesh.elementCount;
        
        textSize = GetTextSize();
        
        baseSize = new Vector2(20 * _charCount, 20);
        
        for (int i = 0; i < _charCount; i++)
        {
            mesh.chars.Add(TextShaderHelper.CharPosition[textArray[i]]);
        }
        
        Console.WriteLine(mesh.chars.Count);
        
        mesh.charCount += _charCount;
        mesh.elementCount++;
        
        textSet = true;
    }

    public override void SetMesh(TextMesh mesh)
    {
        if (mesh is StaticTextMesh staticTextMesh)
        {
            this.mesh = staticTextMesh;
        }
        else
        {
            this.mesh = new StaticTextMesh();
        }
    }

    public override void SetTextAlignment(UiAnchorAlignment alignment)
    {
        _textAlignment = alignment;
    }
    
    public override void RenderText()
    {
        Align();
        AlignText();
        mesh.AddQuad(position, MeshHelper.GenerateTextQuad(textSize.X, textSize.Y, 0, _charCount, _charIndex));
    }
    
    public void UpdateTextWithSameLength(string text)
    {
        this.text = text;
        textArray = text.ToCharArray();
        
        for (int i = 0; i < _charCount; i++)
        {
            mesh.chars[i] = TextShaderHelper.CharPosition[textArray[i]];
        }
        
        mesh.UpdateMesh();
    }
    
    public void UpdateTextWithDifferentLength(string text)
    {
        this.text = text;
        
        //mesh.Shift()
        
        textArray = text.ToCharArray();
        _charCount = textArray.Length;
        
        for (int i = 0; i < _charCount; i++)
        {
            mesh.chars[i + _index] = TextShaderHelper.CharPosition[textArray[i]];
        }
    }
    
    public void UpdatePosition(Vector3 position)
    {
        this.position = position;
        AlignText();
        mesh.MoveUiElement(_index, _textPosition);
    }

    private void AlignText()
    {
        if (!TextAlignment.TryGetValue(_textAlignment, out var value)) return;

        value(this);
        
        _textPosition = position + _textOffset;
    }
    
    #region Text Alignment Functions
    
    private float GetTextWidth() { return _charCount * 7 * fontSize; }
    private float GetTextHeight() { return 9 * fontSize; }
    private Vector2 GetTextSize() { return new Vector2(GetTextWidth(), GetTextHeight()); }
    
    private float GetOffsetX() { return (size.X - textSize.X) / 2; } 
    private float GetOffsetXEnd() { return size.X - textSize.X; } 
    private float GetOffsetY() { return (size.Y - textSize.Y) / 2; }
    private float GetOffsetYEnd() { return size.Y - textSize.Y; }


    private static readonly Dictionary<UiAnchorAlignment, Action<UI_StaticText>> TextAlignment = new Dictionary<UiAnchorAlignment, Action<UI_StaticText>>
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