using OpenTK.Mathematics;

public class UI_DynamicText : UI_Text
{
    public UI_DynamicText()
    {
        name = "UI_Text";
        mesh = new DynamicTextMesh();
    }
    
    public UI_DynamicText(string text, float fontSize)
    {
        name = "UI_Text";
        mesh = new DynamicTextMesh();
        SetText(text, fontSize);
    }

    public DynamicTextMesh GetMesh()
    {
        return mesh;
    }

    public override void SetText(string text, float fontSize)
    {
        this.text = text;
        this.fontSize = fontSize;
        
        textArray = text.ToCharArray();
        _charCount = textArray.Length;
        
        textSize = GetTextSize();
        
        baseSize = new Vector2(20 * _charCount, 20);
        
        for (int i = 0; i < _charCount; i++)
        {
            mesh.chars.Add(TextShaderHelper.CharPosition[textArray[i]]);
        }
    }
    
    public override void SetMesh(TextMesh mesh)
    {
        if (mesh is DynamicTextMesh dynamicTextMesh)
        {
            this.mesh = dynamicTextMesh;
        }
        else
        {
            this.mesh = new DynamicTextMesh();
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
        mesh.SetQuad(position, MeshHelper.GenerateTextQuad(textSize.X, textSize.Y, 0, _charCount, 0));
    }
    
    public void UpdateText(string text)
    {
        this.text = text;
        textArray = text.ToCharArray();
        
        for (int i = 0; i < _charCount; i++)
        {
            mesh.chars[i] = TextShaderHelper.CharPosition[textArray[i]];
        }
    }
    
    public void UpdatePosition(Vector3 position)
    {
        this.position = position;
        AlignText();
        mesh.MoveUiElement(_textPosition);
    }

    private void AlignText()
    {
        if (!TextAlignment.TryGetValue(_textAlignment, out var value)) return;

        value(this);
        
        _textPosition = position + _textOffset;
    }
}