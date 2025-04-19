using OpenTK.Mathematics;

public abstract class UIPanel : UIElement
{
    public int TextureIndex = 0;
    public Vector3 Color = (1, 1, 1);
    public Vector2 Slice = (10, 0.15f);
    public UIMesh? uIMesh;

    public UIPanel() : base() { CanTest = false; }
    public UIPanel(
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
        UIMesh? uIMesh) : 
        base(name, controller, anchorType, positionType, pivot, scale, offset, rotation)
    {
        TextureIndex = textureIndex;
        Color = color;
        Slice = slice;
        this.uIMesh = uIMesh;
    }

    public override void SetVisibility(bool visible)
    {
        if (uIMesh == null || Visible == visible)
            return;

        base.SetVisibility(visible);
        uIMesh.SetVisibility();
    }

    protected virtual bool CanGenerate()
    {
        return uIMesh != null;
    }

    public override void Generate()
    {
        if (CanGenerate())
            GenerateUIQuad(uIMesh);
    }

    protected override void Internal_UpdateTransformation()
    {
        if (CanGenerate()) 
            uIMesh.UpdateElementTransformation(this);
    }

    protected override void Internal_UpdateScale()
    {
        if (CanGenerate())
            uIMesh.UpdateElementScale(this);
    }

    protected override void Internal_UpdateTexture()
    {
        if (CanGenerate())
            uIMesh.UpdateElementTexture(this);
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

    public void GenerateUIQuad(UIMesh uIMesh)
    {
        uIMesh.AddElement(this, ref ElementIndex);
    }

    public override float GetYScale()
    {
        return newScale.Y;
    }

    public override float GetXScale()
    {
        return newScale.X;
    }
}