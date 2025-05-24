using OpenTK.Mathematics;

public class UIMask : UIPanel
{
    public static UIMask Empty = new() { uIMesh = UIMesh.Empty };

    public MaskData MaskData;

    public UIMask() : base() { uIMesh = UIMesh.Empty; }
    public UIMask(
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
        base(name, controller, anchorType, positionType, color, pivot, scale, offset, rotation, textureIndex, slice)
    {
        MaskData = controller.MaskData; 
    }

    public override void SetScale(Vector2 scale)
    {
        base.SetScale(scale);
    }

    public override void Generate()
    {
        SetScale(newScale);
        MaskData.AddElement(this);
    }

    public override void Delete(bool baseObnly = false) 
    {
        base.Delete();
        if (baseObnly) return;
        MaskData.RemoveElement(this);
    }

    protected override void Internal_UpdateTransformation()
    {
        MaskData.UpdateElementTransformation(this);  
    }

    protected override void Internal_UpdateScale()
    {
        MaskData.UpdateElementScale(this);
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
}