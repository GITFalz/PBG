using OpenTK.Mathematics;

public class UIImage : UIPanel
{
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
}