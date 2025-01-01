using OpenTK.Mathematics;

public class StaticText : StaticElement
{
    public string Text;
    
    public char[] Characters;
    public int CharCount;
    
    public Vector2 TextSize;
    
    public TextMesh Mesh;
    
    public StaticText(string text)
    {
        Name = "Static Panel";
        
        Text = text;
        
        Characters = Text.ToCharArray();
        CharCount = Characters.Length;
        
        TextSize = new Vector2(CharCount * 20, 20);
        
        AnchorType = AnchorType.MiddleCenter;
        PositionType = PositionType.Absolute;
    }
    
    public void SetMesh(TextMesh mesh)
    {
        Mesh = mesh;
    }
    
    public void SetText(string text)
    {
        Text = text;
        
        Characters = Text.ToCharArray();
        CharCount = Characters.Length;
        
        TextSize = new Vector2(CharCount * 20, 20);
    }
    
    public override void Generate()
    {
        Align(new Vector3(TextSize.X, TextSize.Y, 0));
        MeshQuad meshQuad = MeshHelper.GenerateTextQuad(CharCount * 20, 20, 0, CharCount, 0);
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