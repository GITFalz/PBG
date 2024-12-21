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
        
        anchorType = AnchorType.MiddleCenter;
        positionType = PositionType.Absolute;
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
        Align();
        Quad quad = MeshHelper.GenerateTextQuad(5 * 20, 20, 0, 5, 0);
        Mesh.SetQuad(origin + new Vector3(0, 0, 0.01f), quad);
        
        foreach (var character in Characters)
        {
            Mesh.chars.Add(TextShaderHelper.CharPosition[character]);
        }
    }

    public void UpdateText()
    {
        Mesh.UpdateMesh();
    }

    public override void Align()
    {
        Vector3 size = new Vector3(TextSize.X, TextSize.Y, 0);
        Vector3 halfSize = size / 2;
        
        if (anchorType == AnchorType.TopLeft)
        {
            position = Vector3.Zero + halfSize;
        }
        else if (anchorType == AnchorType.TopCenter)
        {
            position = new Vector3(Game.width / 2f, halfSize.Y, 0);
        }
        else if (anchorType == AnchorType.TopRight)
        {
            position = new Vector3(Game.width - halfSize.X, halfSize.Y, 0);
        }
        else if (anchorType == AnchorType.MiddleLeft)
        {
            position = new Vector3(halfSize.X, Game.height / 2f, 0);
        }
        else if (anchorType == AnchorType.MiddleCenter)
        {
            position = new Vector3(Game.width / 2f, Game.height / 2f, 0);
        }
        else if (anchorType == AnchorType.MiddleRight)
        {
            position = new Vector3(Game.width - halfSize.X, Game.height / 2f, 0);
        }
        else if (anchorType == AnchorType.BottomLeft)
        {
            position = new Vector3(halfSize.X, Game.height - halfSize.Y, 0);
        }
        else if (anchorType == AnchorType.BottomCenter)
        {
            position = new Vector3(Game.width / 2f, Game.height - halfSize.Y, 0);
        }
        else if (anchorType == AnchorType.BottomRight)
        {
            position = new Vector3(Game.width - halfSize.X, Game.height - halfSize.Y, 0);
        }
        
        origin = position - halfSize;
    }
}