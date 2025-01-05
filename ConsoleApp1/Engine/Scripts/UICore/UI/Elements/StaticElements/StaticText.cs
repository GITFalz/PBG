using OpenTK.Mathematics;

public class StaticText : StaticElement
{
    public string Text;
    public float FontSize;
    
    public char[] Characters;
    public int CharCount;
    
    public Vector2 TextSize;
    
    public TextMesh Mesh;
    
    public StaticText(string name, string text, float fontSize)
    {
        Name = name;
        
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
        SetText(text);
    }
    
    public void SetText(string text)
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

    public override List<string> ToLines(int gap)
    {
        List<string> lines = new List<string>();
        string gapString = "";
        for (int i = 0; i < gap; i++)
        {
            gapString += "    ";
        }
        
        lines.Add(gapString + "Static Text");
        lines.Add(gapString + "{");
        lines.Add(gapString + "    Name: " + Name);
        lines.Add(gapString + "    Text: " + Text);
        lines.Add(gapString + "    FontSize: " + FontSize);
        lines.Add(gapString + "    Position: " + Position);
        lines.Add(gapString + "    Scale: " + Scale);
        lines.Add(gapString + "    Offset: " + Offset);
        lines.Add(gapString + "    AnchorType: " + (int)AnchorType);
        lines.Add(gapString + "    PositionType: " + (int)PositionType);
        lines.Add(gapString + "}");
        
        return lines;
    }

    public void UpdateText()
    {
        Mesh.UpdateMesh();
    }
}