using OpenTK.Mathematics;

public class UIPanel : UIElement
{
    public List<UIElement> Children = new List<UIElement>();
    public UIMesh? uIMesh;

    public UIPanel(string name, AnchorType anchorType, PositionType positionType, Vector3 pivot, Vector2 scale, Vector4 offset, float rotation, int textureIndex, Vector2 slice, UIMesh? uIMesh) : base(name, anchorType, positionType, pivot, scale, offset, rotation, textureIndex, slice)
    {
        this.uIMesh = uIMesh;
    }

    public virtual void AddChild(UIElement child)
    {
        child.PositionType = PositionType.Relative;
        Children.Add(child);
        child.ParentElement = this; 
        
        if (child is UIInputField inputField)
        {
            inputField.Button.PositionType = PositionType.Relative;
            Children.Add(inputField.Button);
            inputField.Button.ParentElement = this;
        }
    }

    public override void SetUIMesh(UIMesh uIMesh)
    {
        this.uIMesh = uIMesh;
    }

    public void SetMeshes(UIMesh uIMesh, TextMesh textMesh)
    {
        foreach (var child in Children)
        {
            if (child is UIPanel panel)
            {
                panel.SetMeshes(uIMesh, textMesh);
            }

            if (child is UIText text)
            {
                text.textMesh = textMesh;
            }

            if (child is UIButton button)
            {
                button.uIMesh = uIMesh;
            }
        }
    }

    public override void Generate()
    {
        Align();
        if (uIMesh == null)
            return;
        GenerateUIQuad(out panel, uIMesh);
    }

    public override void UpdateTransformation()
    {
        if (uIMesh == null)
            return;
        uIMesh.UpdateElementTransformation(this);
    }

    public override void UpdateScale()
    {
        if (uIMesh == null)
            return;
        uIMesh.UpdateElementScale(this);
    }

    public void UpdateAllTransformation()
    {
        UpdateTransformation();
        foreach (var child in Children)
        {
            child.UpdateTransformation();
        }
    }

    public override void UpdateTexture()
    {
        if (uIMesh == null)
            return;
        uIMesh.UpdateElementTexture(this);
    }

    public void AlignAll()
    {
        Align();
        foreach (var child in Children)
        {
            child.Align();
        }
    }

    public override List<string> ToLines(int gap)
    {
        List<string> lines = new List<string>();
        string gapString = new(' ', gap * 4);
        
        lines.Add(gapString + "Panel");
        lines.Add(gapString + "{");
        lines.AddRange(GetBasicDisplayLines(gapString));
        lines.Add(gapString + "    Count: " + Children.Count);

        foreach (var child in Children)
        {
            lines.AddRange(child.ToLines(gap + 1));
        }

        lines.Add(gapString + "}");
        
        return lines;
    }
}