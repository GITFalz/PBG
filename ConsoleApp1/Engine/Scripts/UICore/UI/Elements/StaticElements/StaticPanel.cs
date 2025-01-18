using OpenTK.Mathematics;

public class StaticPanel : StaticElement
{
    public List<StaticElement> ChildElements;
    public UiMesh UiMesh;
    
    public int TextureIndex = 0;
    
    private int _uiIndex = 0;
    private bool _generated = false;
    
    public StaticPanel(string name)
    {
        Name = name;
        
        ChildElements = new List<StaticElement>();
        
        AnchorType = AnchorType.MiddleCenter;
        PositionType = PositionType.Absolute;
    }
    
    public override void SetMesh(UiMesh mesh)
    {
        UiMesh = mesh;
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
        var position = OriginType == OriginType.Pivot ? Position : Origin;
        Create(position + offset);
    }
    
    public override void Generate()
    {
        Align();
        var position = OriginType == OriginType.Pivot ? Position : Origin;
        Create(position);
    }

    public override void Create(Vector3 position)
    {
        Panel panel = new Panel();
        
        Vector3 position1 = Mathf.RotateAround(position, Pivot, Rotation);
        Vector3 position2 = Mathf.RotateAround(position + new Vector3(0, Scale.Y, 0), Pivot, Rotation);
        Vector3 position3 = Mathf.RotateAround(position + new Vector3(Scale.X, Scale.Y, 0), Pivot, Rotation);
        Vector3 position4 = Mathf.RotateAround(position + new Vector3(Scale.X, 0, 0), Pivot, Rotation);
        
        panel.Vertices.Add(position1);
        panel.Vertices.Add(position2);
        panel.Vertices.Add(position3);
        panel.Vertices.Add(position4);
        
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
            _generated = true;
        }
        else
            UiMesh.UpdatePanel(panel, _uiIndex);
        
        foreach (StaticElement element in ChildElements)
        {
            element.Generate();
        }
    }
    
    public override void Reset()
    {
        _generated = false;
        _uiIndex = 0;
    }
    
    public override void ToFile(string path, int gap = 1)
    {
        File.WriteAllLines(path, ToLines(gap));
    }

    public override List<string> ToLines(int gap)
    {
        List<string> lines = new List<string>();
        string gapString = "";
        for (int i = 0; i < gap; i++)
        {
            gapString += "    ";
        }
        
        lines.Add(gapString + "Static Panel");
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

public enum OriginType
{
    Pivot,
    Center
}