using OpenTK.Mathematics;

public class StaticText : StaticElement
{
    public string Text;
    public float FontSize;
    
    public char[] Characters;
    public int CharCount;
    
    public Vector2 TextSize;
    
    public TextMesh Mesh;
    
    public StaticText(string text, float fontSize)
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
            Mesh.chars.Add(TextShaderHelper.CharPosition[character]);
        }
    }

    public void UpdateText()
    {
        Mesh.UpdateMesh();
    }
}