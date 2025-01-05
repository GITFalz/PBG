using OpenTK.Mathematics;

public class StaticInputField : StaticClickable
{
    public string Text;
    public float FontSize;
    
    public char[] Characters;
    public int CharCount;
    
    public Vector2 TextSize;
    
    public TextType TextType = TextType.All;
    
    public TextMesh TextMesh;
    
    public void SetText(string text, float fontSize)
    {
        FontSize = fontSize;
        text = Format(text);
        SetText(text);
    }
    
    public void SetText(string text)
    {
        Text = text;
        
        Characters = Text.ToCharArray();
        CharCount = Characters.Length;
        
        TextSize = new Vector2(CharCount * (20 * FontSize), 20 * FontSize);
        Scale = new Vector3(TextSize.X, TextSize.Y, 0);
    }

    
    public StaticInputField(string name, string text, float fontSize)
    {
        Name = name;
        
        SetText(text, fontSize);
        
        AnchorType = AnchorType.MiddleCenter;
        PositionType = PositionType.Absolute;
        
        TextMesh = new TextMesh();  
    }
    
    public override void Generate()
    {
        Align();
        
        MeshQuad meshQuad = MeshHelper.GenerateTextQuad(TextSize.X, TextSize.Y, 0, CharCount, 0);
        TextMesh.SetQuad(Origin + new Vector3(0, 0, 1f), meshQuad);
        foreach (var character in Characters)
        {
            TextMesh.chars.Add(TextShaderHelper.GetChar(character));
        }
        
        Panel panel = new Panel();

        Vector3 position = Origin;
        
        panel.Vertices.Add(new Vector3(0, 0, 0) + position);
        panel.Vertices.Add(new Vector3(0, Scale.Y, 0) + position);
        panel.Vertices.Add(new Vector3(Scale.X, Scale.Y, 0) + position);
        panel.Vertices.Add(new Vector3(Scale.X, 0, 0) + position);
        
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
        
        UiMesh.AddPanel(panel);
    }

    public void UpdateText()
    {
        TextMesh.UpdateMesh();
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
    
    public string Format(string text)
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
    
    public override List<string> ToLines(int gap)
    {
        List<string> lines = new List<string>();
        string gapString = "";
        for (int i = 0; i < gap; i++)
        {
            gapString += "    ";
        }
        
        lines.Add(gapString + "Static Input Field");
        lines.Add(gapString + "{");
        lines.Add(gapString + "    Name: " + Name);
        lines.Add(gapString + "    Text: " + Text);
        lines.Add(gapString + "    FontSize: " + FontSize);
        lines.Add(gapString + "    Position: " + Position);
        lines.Add(gapString + "    Scale: " + Scale);
        lines.Add(gapString + "    Offset: " + Offset);
        lines.Add(gapString + "    AnchorType: " + (int)AnchorType);
        lines.Add(gapString + "    PositionType: " + (int)PositionType);
        lines.Add(gapString + "    TextureIndex: " + TextureIndex);
        lines.Add(gapString + "    OnClick: " + (OnClick == null ? "null" : SceneName + "." + OnClick.TargetName + "." + OnClick.MethodName));
        lines.Add(gapString + "    OnHover: " + (OnHover == null ? "null" : SceneName + "." + OnHover.TargetName + "." + OnHover.MethodName));
        lines.Add(gapString + "    OnHold: " + (OnHold == null ? "null" : SceneName + "." + OnHold.TargetName + "." + OnHold.MethodName));
        lines.Add(gapString + "    OnRelease: " + (OnRelease == null ? "null" : SceneName + "." + OnRelease.TargetName + "." + OnRelease.MethodName));
        lines.Add(gapString + "}");
        
        return lines;
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