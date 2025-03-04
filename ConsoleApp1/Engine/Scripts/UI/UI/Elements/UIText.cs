using OpenTK.Mathematics;

public class UIText : UIElement
{
    public int MaxCharCount = 20;
    public string Text = "";
    public string finalText = "";
    public float FontSize = 1.5f;
    public char[] Characters = [];
    public int CharCount = 0;
    public List<int> Chars = [];
    public TextMesh? textMesh;
    public TextQuad textQuad = new TextQuad();
    public TextType TextType = TextType.Alphabetic;
    public int TextOffset = 0;

    public UIText(string name, AnchorType anchorType, PositionType positionType, Vector3 pivot, Vector2 scale, Vector4 offset, float rotation, int textureIndex, Vector2 slice, TextMesh? textMesh) : base(name, anchorType, positionType, pivot, scale, offset, rotation)
    {
        this.textMesh = textMesh;
    }

    public override void SetTextMesh(TextMesh textMesh)
    {
        this.textMesh = textMesh;
    }

    public UIText SetText(string text, float fontSize)
    {
        FontSize = fontSize;
        return SetText(text);
    }

    public UIText SetTextType(TextType textType)
    {
        TextType = textType;
        return this;
    }

    public UIText SetMaxCharCount(int maxCharCount)
    {
        MaxCharCount = maxCharCount;
        return this;
    }
    
    public virtual UIText SetText(string text)
    {
        Text = ClampText(text, 0, MaxCharCount);
        finalText = AddSpaces(Text, MaxCharCount);
        CharCount = Text.Length;
        Characters = finalText.ToCharArray();

        Scale = new Vector2(MaxCharCount * (20 * FontSize), 20 * FontSize);
        newScale = Scale;

        return this;
    }

    public override void UpdateTransformation()
    {
        if (textMesh == null)
            return;
        textMesh.UpdateElementTransformation(this);
        textMesh.UpdateMatrices();
    }

    public void UpdateText()
    {
        UIController?.UpdateText();
    }

    public override void Generate()
    {
        GenerateQuad(ref UIController.TextOffset);
        GenerateChars();
    }

    public UIText GenerateChars()
    {
        Chars.Clear();
        foreach (var character in Characters)
        {
            Chars.Add(TextShaderHelper.GetChar(character));
        }
        
        textMesh?.SetCharacters(this, TextOffset);
        return this;
    }

    public void GenerateQuad(ref int offset)
    {
        TextOffset = offset;

        textQuad = new TextQuad();

        Vector3 position1 = Mathf.RotateAround((0,          0,          0), Pivot, _rotationAxis, Rotation);
        Vector3 position2 = Mathf.RotateAround((0,          newScale.Y, 0), Pivot, _rotationAxis, Rotation);
        Vector3 position3 = Mathf.RotateAround((newScale.X, newScale.Y, 0), Pivot, _rotationAxis, Rotation);
        Vector3 position4 = Mathf.RotateAround((newScale.X, 0,          0), Pivot, _rotationAxis, Rotation);
        
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

    public static string ClampText(string text, int min, int max)
    {
        if (text.Length < min)
        {
            return text.PadRight(min, ' ');
        }
        else if (text.Length > max)
        {
            return text[..max];
        }
        return text;
    }

    public static string AddSpaces(string text, int maxCount)
    {
        if (text.Length > maxCount) 
        {
            return text[..maxCount];
        }
        else if (text.Length < maxCount) 
        {
            return text.PadRight(maxCount, ' ');
        }
        return text;
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
        lines.Add(gapString + "    MaxCharCount: " + MaxCharCount);
        lines.Add(gapString + "    TextType: " + (int)TextType);
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