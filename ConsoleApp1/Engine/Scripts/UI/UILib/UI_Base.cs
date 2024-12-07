using System.Numerics;

namespace ConsoleApp1.Engine.Scripts.Core.UI.UILib;

public abstract class UI_Base : Component
{
    public Vector2 position = Vector2.Zero;
    public Vector2 size = new Vector2(100, 100);
    public Vector4 color = new Vector4(1, 1, 1, 1);
    
    public UiAnchor anchor = UiAnchor.Absolute;
    public UiAnchorAlignment anchorAlignment = UiAnchorAlignment.MiddleCenter;
    
    public override void Update()
    {
        base.Update();
    }

    public abstract void RenderUI();
}

public enum UiAnchor
{
    Absolute,
    Relative
}

public enum UiAnchorAlignment
{
    TopLeft,
    TopCenter,
    TopRight,
    MiddleLeft,
    MiddleCenter,
    MiddleRight,
    BottomLeft,
    BottomCenter,
    BottomRight
}