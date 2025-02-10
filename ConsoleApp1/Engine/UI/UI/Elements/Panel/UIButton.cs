using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class UIButton : UIPanel
{
    public UIButton(string name, AnchorType anchorType, PositionType positionType, Vector3 color, Vector3 pivot, Vector2 scale, Vector4 offset, float rotation, int textureIndex, Vector2 slice, UIMesh? uIMesh, UIState state) : base(name, anchorType, positionType, color, pivot, scale, offset, rotation, textureIndex, slice, uIMesh)
    {
        State = state;
        test = true;
    }

    public override void SetUIMesh(UIMesh uIMesh)
    {
        this.uIMesh = uIMesh;
    }

    public override void UpdateTexture()
    {
        if (CanGenerate())
            uIMesh.UpdateElementTexture(this);
    }

    public override void UpdateTransformation()
    {
        if (CanGenerate())
            uIMesh.UpdateElementTransformation(this);
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