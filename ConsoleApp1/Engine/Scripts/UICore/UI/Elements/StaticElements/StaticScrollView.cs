using OpenTK.Mathematics;

public class StaticScrollView : StaticElement
{
    public UiMesh maskMesh;
    
    private bool _generated = false;
    private int _uiIndex = 0;
    private int _maskIndex = 0;
    
    public List<StaticElement> ChildElements = new List<StaticElement>();
    public Vector2 scrollPosition = Vector2.Zero;
    
    public StaticScrollView(string name)
    {
        Name = name;
        
        AnchorType = AnchorType.MiddleCenter;
        PositionType = PositionType.Absolute;
    }

    public void SetMesh(UiMesh uiMesh, UiMesh mask)
    {
        base.SetMesh(uiMesh);
        maskMesh = mask;
    }
    
    public void AddElement(StaticElement element)
    {
        element.ParentElement = this;
        ChildElements.Add(element);
    }
    
    public override bool HasChild(UiElement element)
    {
        foreach (StaticElement child in ChildElements)
        {
            if (Equals(child, element))
                return true;
            if (child.HasChild(element))
                return true;
        }
        return false;
    }
    
    public override void Generate(Vector3 offset)
    {
        Align();
        Create(Origin + offset);
    }
    
    public override void Generate()
    {
        Align();
        Create(Origin);
    }
    
    public override void Create(Vector3 position)
    {
        Panel panel = new Panel();
        
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
        
        if (!_generated)
        {
            UiMesh.AddPanel(panel, out _uiIndex);
            maskMesh.AddPanel(panel, out _maskIndex);
            _generated = true;
        }
        else
        {
            UiMesh.UpdatePanel(panel, _uiIndex);
            maskMesh.UpdatePanel(panel, _maskIndex);
        }
    }

    public override void Reset()
    {
        _generated = false;
        _uiIndex = 0;
        _maskIndex = 0;
    }
    
    public override List<string> ToLines(int gap)
    {
        List<string> lines = new List<string>();
        string gapString = "";
        for (int i = 0; i < gap; i++)
        {
            gapString += "    ";
        }
        
        lines.Add(gapString + "Static Scroll View");
        lines.Add(gapString + "{");
        lines.Add(gapString + "    Name: " + Name);
        lines.Add(gapString + "    Position: " + Position);
        lines.Add(gapString + "    Scale: " + Scale);
        lines.Add(gapString + "    Offset: " + Offset);
        lines.Add(gapString + "    AnchorType: " + (int)AnchorType);
        lines.Add(gapString + "    PositionType: " + (int)PositionType);
        lines.Add(gapString + "    TextureIndex: " + TextureIndex);
        lines.Add(gapString + "    Elements: " + ChildElements.Count);
        if (ChildElements.Count >= 1)
        {
            foreach (StaticElement element in ChildElements)
            {
                lines.AddRange(element.ToLines(gap + 1));
            }
        }
        lines.Add(gapString + "}");
        
        return lines;
    }
}

public enum ScrollDirection
{
    Vertical,
    Horizontal
}