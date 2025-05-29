using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class UIButton : UIPanel
{
    public static UIButton Empty = new() { uIMesh = UIMesh.Empty };

    public UIButton() : base() { }

    public UIButton(
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
        Vector2 slice, 
        UIState state) : 
        base(name, controller, anchorType, positionType, color, pivot, scale, offset, rotation, textureIndex, slice)
    {
        State = state;
        CanTest = true;
    }

    public override void SetMaskIndex(int maskedIndex)
    {
        base.SetMaskIndex(maskedIndex);
        if (!CanGenerate())
            return;
        
        uIMesh.UpdateMaskedIndex(this, Masked, MaskIndex);
    }

    public override void SetVisibility(bool visible)
    {
        if (!CanGenerate())
            return;

        if (Visible == visible)
            return;

        base.SetVisibility(visible);
        uIMesh.SetVisibility();
    }

    public override void Generate()
    {
        if (!CanGenerate())
            return;
        SetScale(newScale);
        GenerateUIQuad(uIMesh);    
    }

    public override void Delete(bool baseObnly = false) 
    {
        base.Delete();
        if (baseObnly) return;
        if (CanGenerate())
            uIMesh.RemoveElement(this);
    }

    protected override void Internal_UpdateTransformation()
    {
        if (!CanGenerate())
            return;
        uIMesh.UpdateElementTransformation(this);  
    }

    protected override void Internal_UpdateScale()
    {
        if (!CanGenerate())
            return;
        uIMesh.UpdateElementScale(this);
    }

    protected override void Internal_UpdateTexture()
    {
        if (!CanGenerate())
            return;
        uIMesh.UpdateElementTexture(this);
    }

    protected override void Internal_UpdateColor()
    {
        if (!CanGenerate())
            return;
        uIMesh.UpdateElementColor(this);
    }

    protected bool CanGenerate()
    {
        return State != UIState.InvisibleInteractable;
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