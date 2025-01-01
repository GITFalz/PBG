using OpenTK.Mathematics;
using Vortice.Mathematics;

public class StaticPanel : StaticElement
{
    public List<StaticElement> ChildElements;
    public UiMesh Mesh;
    
    public int TextureIndex = 0;
    
    public StaticPanel()
    {
        Name = "Static Panel";
        
        ChildElements = new List<StaticElement>();
        
        AnchorType = AnchorType.MiddleCenter;
        PositionType = PositionType.Absolute;
    }
    
    public void SetMesh(UiMesh mesh)
    {
        Mesh = mesh;
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
        
        Mesh.AddPanel(panel);
        
        foreach (StaticElement element in ChildElements)
        {
            element.Generate();
        }
    }
}