using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenTK.Mathematics;

public class UIPanel(string name, AnchorType anchorType, PositionType positionType, Vector3 pivot, Vector2 scale, Vector4 offset, float rotation, int textureIndex, UIMesh? uIMesh) : UIElement(name, anchorType, positionType, pivot, scale, offset, rotation, textureIndex)
{
    public List<UIElement> Children = [];
    public UIMesh? uIMesh;

    public void AddChild(UIElement child)
    {
        child.PositionType = PositionType.Relative;
        Children.Add(child);
        child.ParentElement = this;
    }

    public override void SetUIMesh(UIMesh uIMesh)
    {
        this.uIMesh = uIMesh;
    }

    public override void Generate()
    {
        Align();
        if (uIMesh == null)
            return;
        GenerateUIQuad(out panel, uIMesh);
    }

    public override List<string> ToLines(int gap)
    {
        List<string> lines = new List<string>();
        string gapString = "";
        for (int i = 0; i < gap; i++)
        {
            gapString += "    ";
        }
        
        lines.Add(gapString + "Panel");
        lines.Add(gapString + "{");
        lines.Add(gapString + "    Count: " + Children.Count);
        lines.AddRange(GetBasicDisplayLines(gap));
        
        return lines;
    }
}