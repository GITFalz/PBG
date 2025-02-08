using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class UIButton : UIElement
{
    public UIMesh? uIMesh;

    public UIButton(string name, AnchorType anchorType, PositionType positionType, Vector3 pivot, Vector2 scale, Vector4 offset, float rotation, int textureIndex, Vector2 slice, UIMesh? uIMesh, UIState state) : base(name, anchorType, positionType, pivot, scale, offset, rotation, textureIndex, slice)
    {
        State = state;
        this.uIMesh = uIMesh;
        test = true;
    }

    public override void SetUIMesh(UIMesh uIMesh)
    {
        this.uIMesh = uIMesh;
    }

    public override void UpdateTexture()
    {
        if (uIMesh == null)
            return;
        uIMesh.UpdateElementTexture(this);
    }

    public override void UpdateTransformation()
    {
        if (uIMesh == null)
            return;
        uIMesh.UpdateElementTransformation(this);
    }

    public override void Generate()
    {
        Align();
        if (State == UIState.InvisibleInteractable || uIMesh == null)
            return;
        GenerateUIQuad(out panel, uIMesh);
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