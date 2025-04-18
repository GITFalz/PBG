using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class UIButton : UIPanel
{
    public static UIButton Empty = new();

    public UIButton() : base() { CanTest = false; }
    public UIButton(
        string name,
        UIController controller,
        AnchorType anchorType, 
        PositionType positionType, 
        Vector3 color, 
        Vector3 pivot, 
        Vector2 scale, 
        Vector4 offset, 
        float rotation, 
        int textureIndex, 
        Vector2 slice, 
        UIMesh? uIMesh, 
        UIState state) : 
        base(name, controller, anchorType, positionType, color, pivot, scale, offset, rotation, textureIndex, slice, uIMesh)
    {
        State = state;
        CanTest = true;
    }

    public override void SetUIMesh(UIMesh uIMesh)
    {
        this.uIMesh = uIMesh;
    }

    protected override void Internal_UpdateTexture()
    {
        if (CanGenerate())
            uIMesh.UpdateElementTexture(this);
    }

    protected override void Internal_UpdateTransformation()
    {
        if (CanGenerate())
        {
            uIMesh.UpdateElementTransformation(this);
        } 
    }

    protected override bool CanGenerate()
    {
        return base.CanGenerate() && State != UIState.InvisibleInteractable;
    }

    public override List<string> ToLines(int gap)
    {
        List<string> lines = new List<string>();
        string gapString = new(' ', gap * 4);
        
        lines.Add(gapString + "Button");
        lines.Add(gapString + "{");
        lines.AddRange(GetBasicDisplayLines(gapString));
        lines.Add(gapString + "}");
        
        return lines;
    }
}