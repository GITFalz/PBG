using OpenTK.Mathematics;

public class UIImage : UIPanel
{
    public UIImage(string name, AnchorType anchorType, PositionType positionType, Vector3 color, Vector3 pivot, Vector2 scale, Vector4 offset, float rotation, int textureIndex, Vector2 slice, UIMesh? uIMesh) : base(name, anchorType, positionType, color, pivot, scale, offset, rotation, textureIndex, slice, uIMesh)
    {
    }
}