using OpenTK.Mathematics;

public class UIScrollView : UIPanel
{
    public UIElement? SelectedElement;

    public UIImage SelectionPanel;
 
    public UIMesh? maskMesh;
    public UIMesh? maskedUiMesh;
    public TextMesh? maskedTextMesh;

    public UIScrollView(
        string name, AnchorType anchorType, PositionType positionType, Vector3 color, Vector3 pivot, Vector2 scale, 
        Vector2 selectionScale, Vector4 offset, float rotation, int textureIndex, Vector2 slice, 
        UIMesh? uiMesh, UIMesh? maskMesh, UIMesh? maskedUiMesh, TextMesh? maskedTextMesh) : 
        base(name, anchorType, positionType, color, pivot, selectionScale, offset, rotation, textureIndex, slice, uiMesh)
    {
        this.maskedUiMesh = maskedUiMesh;
        this.maskedTextMesh = maskedTextMesh;

        SelectionPanel = new UIImage("SelectionPanel", anchorType, positionType, color, pivot, (scale.X, scale.Y - selectionScale.Y), (offset.X, offset.Y + selectionScale.Y, 0, 0), rotation, -1, (0, 0), maskMesh);
    } 

    public void SetMesh(UIMesh uIMesh, UIMesh maskMesh, UIMesh maskedUiMesh, TextMesh maskedTextMesh)
    {
        this.uIMesh = uIMesh;
        this.maskMesh = maskMesh;
        this.maskedUiMesh = maskedUiMesh;
        this.maskedTextMesh = maskedTextMesh;
    }
}