using OpenTK.Mathematics;

public class UIText : UIElement
{
    public int MaxCharCount = 20;
    public string Text = "";
    public float FontSize = 1.5f;
    public char[] Characters = [];
    public int CharCount = 0;
    public List<int> Chars = [];
    public TextMesh? textMesh;
    public TextQuad textQuad = new TextQuad();
    public TextType TextType = TextType.Alphabetic;
    public int TextOffset = 0;

    public UIText(string name, AnchorType anchorType, PositionType positionType, Vector3 pivot, Vector2 scale, Vector4 offset, float rotation, int textureIndex, TextMesh? textMesh) : base(name, anchorType, positionType, pivot, scale, offset, rotation, textureIndex)
    {
        this.textMesh = textMesh;
    }

    public override void SetTextMesh(TextMesh textMesh)
    {
        this.textMesh = textMesh;
    }

    public void SetText(string text, float fontSize)
    {
        FontSize = fontSize;
        SetText(text);
    }
    
    public void SetText(string text)
    {
        Text = text;
        
        Characters = Text.ToCharArray();
        CharCount = Characters.Length;

        ClampText();

        Scale = new Vector2(MaxCharCount * (20 * FontSize), 20 * FontSize);
        newScale = Scale;
    }

    public override void UpdateTransformation()
    {
        if (textMesh == null)
            return;
        textMesh.UpdateElementTransformation(this);
    }

    public void UpdateText()
    {
        textMesh?.UpdateText();
    }

    public override void Generate(ref int offset)
    {
        Align();
        GenerateChars();
        GenerateQuad(ref offset);
    }

    public void GenerateChars()
    {
        Chars.Clear();
        foreach (var character in Characters)
        {
            Chars.Add(TextShaderHelper.GetChar(character));
        }
        textMesh?.SetCharacters(Chars);
    }

    public void GenerateQuad(ref int offset)
    {
        textQuad = new TextQuad();

        Vector3 position1 = Mathf.RotateAround((0,          0,          Depth), Pivot, _rotationAxis, Rotation);
        Vector3 position2 = Mathf.RotateAround((0,          newScale.Y, Depth), Pivot, _rotationAxis, Rotation);
        Vector3 position3 = Mathf.RotateAround((newScale.X, newScale.Y, Depth), Pivot, _rotationAxis, Rotation);
        Vector3 position4 = Mathf.RotateAround((newScale.X, 0,          Depth), Pivot, _rotationAxis, Rotation);
        
        textQuad.Vertices.Add(position1);
        textQuad.Vertices.Add(position2);
        textQuad.Vertices.Add(position3);
        textQuad.Vertices.Add(position4);
        
        textQuad.Uvs.Add(new Vector2(0, 0));
        textQuad.Uvs.Add(new Vector2(0, 1));
        textQuad.Uvs.Add(new Vector2(1, 1));
        textQuad.Uvs.Add(new Vector2(1, 0));
        
        textQuad.TextSize.Add((MaxCharCount, offset));
        textQuad.TextSize.Add((MaxCharCount, offset));
        textQuad.TextSize.Add((MaxCharCount, offset));
        textQuad.TextSize.Add((MaxCharCount, offset));

        textMesh?.AddTextElement(this, ref ElementIndex);

        offset += MaxCharCount;
    }

    private void ClampText()
    {
        if (CharCount > MaxCharCount) 
        {
            Text = Text[..MaxCharCount];
        }
        else if (CharCount < MaxCharCount) 
        {
            Text = Text.PadRight(MaxCharCount, ' ');
        }

        CharCount = MaxCharCount;
        Characters = Text.ToCharArray();
    }

    public override List<string> ToLines(int gap)
    {
        List<string> lines = new List<string>();
        string gapString = new(' ', gap * 4);
        
        lines.Add(gapString + "Text");
        lines.Add(gapString + "{");
        lines.AddRange(GetBasicDisplayLines(gapString));
        lines.Add(gapString + "    Text: " + Text);
        lines.Add(gapString + "    FontSize: " + FontSize);
        lines.Add(gapString + "}");
        
        return lines;
    }
}

public class TextQuad
{
    public List<Vector3> Vertices = [];
    public List<Vector2> Uvs = [];
    public List<Vector2i> TextSize = [];
}