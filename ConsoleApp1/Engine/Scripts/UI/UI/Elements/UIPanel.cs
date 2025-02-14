using OpenTK.Mathematics;

public abstract class UIPanel : UIElement
{
    public int TextureIndex = 0;
    public Vector3 Color = (1, 1, 1);
    public Vector2 Slice = (10, 0.15f);
    public UIMesh? uIMesh;

    public UIPanel(string name, AnchorType anchorType, PositionType positionType, Vector3 color, Vector3 pivot, Vector2 scale, Vector4 offset, float rotation, int textureIndex, Vector2 slice, UIMesh? uIMesh) : base(name, anchorType, positionType, pivot, scale, offset, rotation)
    {
        TextureIndex = textureIndex;
        Color = color;
        Slice = slice;
        this.uIMesh = uIMesh;
    }

    public override void SetUIMesh(UIMesh uIMesh)
    {
        this.uIMesh = uIMesh;
    }

    protected virtual bool CanGenerate()
    {
        return uIMesh != null;
    }

    public override void Generate()
    {
        if (CanGenerate())
            GenerateUIQuad(out panel, uIMesh);
    }

    public override void UpdateTransformation()
    {
        if (CanGenerate())
            uIMesh.UpdateElementTransformation(this);
    }

    public override void UpdateScale()
    {
        if (CanGenerate())
            uIMesh.UpdateElementScale(this);
    }

    public override void UpdateTexture()
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

    public void GenerateUIQuad(out Panel panel, UIMesh uIMesh)
    {
        panel = GetUIQuad(this);
        uIMesh?.AddElement(this, ref ElementIndex);
    }

    public static Panel GetUIQuad(UIPanel element)
    {
        Panel panel = new Panel();
        
        Vector3 p1 = Mathf.RotateAround((0,                  0,                  element.Depth), element.Pivot, (0, 0, 1), element.Rotation);
        Vector3 p2 = Mathf.RotateAround((0,                  element.newScale.Y, element.Depth), element.Pivot, (0, 0, 1), element.Rotation);
        Vector3 p3 = Mathf.RotateAround((element.newScale.X, element.newScale.Y, element.Depth), element.Pivot, (0, 0, 1), element.Rotation);
        Vector3 p4 = Mathf.RotateAround((element.newScale.X, 0,                  element.Depth), element.Pivot, (0, 0, 1), element.Rotation);
        Vector2 s = element.newScale;
        int t = element.TextureIndex;
        
        panel.Vertices.AddRange(p1, p2, p3, p4);
        panel.Uvs.AddRange((0, 0), (0, 1), (1, 1), (1, 0));
        panel.TextUvs.AddRange(t, t, t, t);
        panel.UiSizes.AddRange(s, s, s, s);

        return panel;
    }
}