using OpenTK.Mathematics;
using Vortice.Mathematics;

public class StaticPanel : StaticElement
{
    public List<StaticElement> ChildElements;
    public UiMesh Mesh;
    
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
        Panel panel = UI.GeneratePanel(Origin, 64, 64, Scale.X, Scale.Y, 10f, new Vector4(10f, 10f, 10f, 10f));
        Mesh.AddUiElement(panel);
        
        foreach (StaticElement element in ChildElements)
        {
            element.Generate();
        }
    }
}