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
        
        anchorType = AnchorType.MiddleCenter;
        positionType = PositionType.Absolute;
    }
    
    public void SetMesh(UiMesh mesh)
    {
        Mesh = mesh;
    }
    
    public override void Generate()
    {
        Align();
        Panel panel = UI.GeneratePanel(origin, 64, 64, scale.X, scale.Y, 10f, new Vector4(10f, 10f, 10f, 10f));
        Mesh.AddUiElement(panel);
    }

    public override void Align()
    {
        if (anchorType == AnchorType.TopLeft)
        {
            position = Vector3.Zero + halfScale;
        }
        else if (anchorType == AnchorType.TopCenter)
        {
            position = new Vector3(Game.width / 2f, halfScale.Y, 0);
        }
        else if (anchorType == AnchorType.TopRight)
        {
            position = new Vector3(Game.width - halfScale.X, halfScale.Y, 0);
        }
        else if (anchorType == AnchorType.MiddleLeft)
        {
            position = new Vector3(halfScale.X, Game.height / 2f, 0);
        }
        else if (anchorType == AnchorType.MiddleCenter)
        {
            position = new Vector3(Game.width / 2f, Game.height / 2f, 0);
        }
        else if (anchorType == AnchorType.MiddleRight)
        {
            position = new Vector3(Game.width - halfScale.X, Game.height / 2f, 0);
        }
        else if (anchorType == AnchorType.BottomLeft)
        {
            position = new Vector3(halfScale.X, Game.height - halfScale.Y, 0);
        }
        else if (anchorType == AnchorType.BottomCenter)
        {
            position = new Vector3(Game.width / 2f, Game.height - halfScale.Y, 0);
        }
        else if (anchorType == AnchorType.BottomRight)
        {
            position = new Vector3(Game.width - halfScale.X, Game.height - halfScale.Y, 0);
        }
        
        origin = position - halfScale;
    }
}