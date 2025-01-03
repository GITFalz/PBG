using OpenTK.Mathematics;

public class StaticInputField : StaticElement
{
    public string Text;
    public float FontSize;
    
    public char[] Characters;
    public int CharCount;
    
    public Vector2 TextSize;
    
    public TextMesh Mesh;
    
    public TextType TextType = TextType.Decimal;
    
    public StaticInputField(string text, float fontSize)
    {
        Name = "Static Panel";
        
        SetText(text, fontSize);
        
        AnchorType = AnchorType.MiddleCenter;
        PositionType = PositionType.Absolute;
    }
    
    public void SetMesh(TextMesh mesh)
    {
        Mesh = mesh;
    }
    
    public void SetText(string text, float fontSize)
    {
        FontSize = fontSize;
        text = Format(text);
        SetText(text);
    }
    
    private void SetText(string text)
    {
        Text = text;
        
        Characters = Text.ToCharArray();
        CharCount = Characters.Length;
        
        TextSize = new Vector2(CharCount * (20 * FontSize), 20 * FontSize);
    }
    
    public override void Generate()
    {
        Align(new Vector3(TextSize.X, TextSize.Y, 0));
        MeshQuad meshQuad = MeshHelper.GenerateTextQuad(TextSize.X, TextSize.Y, 0, CharCount, 0);
        Mesh.SetQuad(Origin + new Vector3(0, 0, 1f), meshQuad);
        foreach (var character in Characters)
        {
            Mesh.chars.Add(TextShaderHelper.GetChar(character));
        }
    }

    public void UpdateText()
    {
        Mesh.UpdateMesh();
    }
    
    public void AddCharacter(char character)
    {
        if (!TextShaderHelper.CharExists(character)) return;
        SetText(Format(Text + character));
        Generate();
        UpdateText();
    }
    
    public void RemoveCharacter()
    {
        if (Text.Length <= 0) return;
        SetText(Text[..^1]);
        Generate();
        UpdateText();
    }

    private string Format(string text)
    {
        if (TextType == TextType.Numeric)
        {
            return new string(text.Where(char.IsDigit).ToArray());
        }
        else if (TextType == TextType.Decimal)
        {
            int dotCount = 0;
            return new string(text.Where(c =>
            {
                if (c != '.') 
                    return char.IsDigit(c);
                dotCount++;
                return dotCount <= 1;
            }).ToArray());
        }
        else if (TextType == TextType.Alphabetic)
        {
            return new string(text.Where(char.IsLetter).ToArray());
        }
        else if (TextType == TextType.Alphanumeric)
        {
            return new string(text.Where(char.IsLetterOrDigit).ToArray());
        }
        else
        {
            return text;
        }
    }
}

public enum TextType
{
    Numeric,
    Decimal,
    Alphabetic,
    Alphanumeric,
    All
}