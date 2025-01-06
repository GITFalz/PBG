using OpenTK.Mathematics;
using Vortice.Mathematics;

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
    
    public override void Generate()
    {
        Align();
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