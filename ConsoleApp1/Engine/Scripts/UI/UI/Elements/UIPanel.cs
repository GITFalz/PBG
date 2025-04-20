using OpenTK.Mathematics;

public abstract class UIPanel : UIRender
{
    public UIPanel() : base() {}
    public UIPanel(
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
        base(name, controller, anchorType, positionType, color, pivot, scale, offset, rotation, textureIndex, (scale.X, scale.Y, slice.X, slice.Y))
    {

    }

    public override void SetScale(Vector2 scale)
    {
        base.SetScale(scale);
        SizeSlice.X = scale.X;
        SizeSlice.Y = scale.Y;
    }

    public override void SetVisibility(bool visible)
    {
        if (Visible == visible)
            return;

        base.SetVisibility(visible);
        uIMesh.SetVisibility();
    }

    public override void Generate()
    {
        GenerateUIQuad(uIMesh);    
    }

    protected override void Internal_UpdateTransformation()
    {
        uIMesh.UpdateElementTransformation(this);  
    }

    protected override void Internal_UpdateScale()
    {
        uIMesh.UpdateElementScale(this);
    }

    protected override void Internal_UpdateTexture()
    {
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

    public void GenerateUIQuad(newUIMesh uIMesh)
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