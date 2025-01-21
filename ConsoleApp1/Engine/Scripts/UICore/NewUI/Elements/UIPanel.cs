using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenTK.Mathematics;

public class UIPanel : UIElement
{
    public UIPanel(AnchorType anchorType, PositionType positionType, Vector3 pivot, Vector2 scale, Vector4 offset, float rotation, int textureIndex, UIMesh? uIMesh) : base(anchorType, positionType, pivot, scale, offset, rotation, textureIndex, uIMesh)
    {
        Name = "UI Panel";
    }

    public List<UIElement> Children = [];

    public void AddChild(UIElement child)
    {
        child.positionType = PositionType.Relative;
        Children.Add(child);
        child.ParentElement = this;
    }
}