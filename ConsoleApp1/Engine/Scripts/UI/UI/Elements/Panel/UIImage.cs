using OpenTK.Mathematics;

public class UIImage : UIPanel
{
    public static UIImage Empty = new() { uIMesh = UIMesh.Empty };

    public UIImage() : base() { uIMesh = UIMesh.Empty; }

    public UIImage(
        string name, 
        UIController controller, 
        AnchorType anchorType, 
        PositionType positionType, 
        Vector4 color, 
        Vector3 pivot, 
        Vector2 scale, 
        Vector4 offset, 
        float rotation, 
        int textureIndex, 
        Vector2 slice) : 
        base(name, controller, anchorType, positionType, color, pivot, scale, offset, rotation, textureIndex, slice)
    {
    }

    public override List<string> ToLines(int gap)
    {
        List<string> lines = new List<string>();
        string gapString = new(' ', gap * 4);
        
        lines.Add(gapString + "Panel");
        lines.Add(gapString + "{");
        lines.AddRange(GetBasicDisplayLines(gapString));
        lines.Add(gapString + "}");
        
        return lines;
    }
}