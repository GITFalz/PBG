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
    public TextMesh textMesh;
    public TextQuad textQuad = new TextQuad();
    public TextType TextType = TextType.Alphabetic;
    public int TextOffset = 0;

    public UIText
    (
        string name, 
        UIController controller, 
        AnchorType anchorType, 
        PositionType positionType, 
        Vector3 pivot, 
        Vector2 scale, 
        Vector4 offset, 
        float rotation, 
        TextMesh textMesh) : 
        base(name, controller, anchorType, positionType, pivot, scale, offset, rotation)
    {
        this.textMesh = textMesh;
    }

    public override void SetVisibility(bool visible)
    {   
        if (textMesh == null || Visible == visible)
            return;

        base.SetVisibility(visible);
        textMesh.SetVisibility();
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

    public UIText SetTextCharCount(string text, float fontSize)
    {
        MaxCharCount = text.Length;
        return SetText(text, fontSize);
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

    public float ParseFloat(float replacement = 0f)
    {
        float value;
        string text = Text;
        if (text.Length == 0 || text.EndsWith(".") || text.EndsWith(","))
            text += "0";
        if (float.TryParse(text, out float scaleValue))
            value = scaleValue;
        else
            value = replacement;
        return value;
    }

    public int ParseInt(int replacement = 0)
    {
        int value;
        string text = Text;
        if (int.TryParse(text, out int scaleValue))
            value = scaleValue;
        else
            value = replacement;
        return value;
    }

    protected override void Internal_UpdateTransformation()
    {
        if (!CanUpdate)
            return;
            
        textMesh.UpdateElementTransformation(this);
        textMesh.UpdateMatrices();
    }

    public void UpdateText()
    {
        if (!CanUpdate)
            return;

        textMesh.UpdateText();
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
        textMesh?.SetCharacters(Chars, TextOffset);
        return this;
    }

    public void GenerateQuad(ref int offset)
    {
        TextOffset = offset;

        textMesh?.AddTextElement(this, ref ElementIndex, offset);

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


    public override float GetYScale()
    {
        return newScale.Y;
    }

    public override float GetXScale()
    {
        return newScale.X;
    }
}

public class TextQuad
{
    public List<Vector3> Vertices = [];
    public List<Vector2> Uvs = [];
    public List<Vector2i> TextSize = [];
}