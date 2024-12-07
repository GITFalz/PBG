using OpenTK.Mathematics;
using StbImageSharp;
using Vortice.Mathematics;

public abstract class UI_Base : Component
{
    public Vector3 position = Vector3.Zero;
    public Vector2 size = new Vector2(100, 100);
    
    public Vector4 baseOffset = Vector4.Zero;
    public Vector2 baseSize = new Vector2(100, 100);
    
    public UiAnchor anchor = UiAnchor.Absolute;
    public UiAnchorAlignment anchorAlignment = UiAnchorAlignment.MiddleCenter;

    public override void OnResize()
    {
        base.OnResize();
    }

    public override void Update()
    {
        base.Update();
    }

    public virtual void RenderUI(MeshData meshData)
    {
        
    }

    public virtual void RenderText()
    {
        
    }

    protected void Align()
    {
        size = sizes[anchorAlignment](baseSize, baseOffset);
        position = positions[anchorAlignment](Game.width, Game.height, sizes[anchorAlignment](baseSize, baseOffset), baseOffset);
    }
    
    private Dictionary<UiAnchorAlignment, Func<Vector2, Vector4, Vector2>> sizes = new Dictionary<UiAnchorAlignment, Func<Vector2, Vector4, Vector2>>
    {
        { UiAnchorAlignment.LeftScale, (baseSize, baseOffset) => new Vector2(baseSize.X, Game.height - baseOffset.Z - baseOffset.W) },
        { UiAnchorAlignment.RightScale, (baseSize, baseOffset) => new Vector2(baseSize.X, Game.height - baseOffset.Z - baseOffset.W) },
        { UiAnchorAlignment.TopScale, (baseSize, baseOffset) => new Vector2(Game.width - baseOffset.X - baseOffset.Y, baseSize.Y) },
        { UiAnchorAlignment.BottomScale, (baseSize, baseOffset) => new Vector2(Game.width - baseOffset.X - baseOffset.Y, baseSize.Y) },
        { UiAnchorAlignment.CenterScale, (baseSize, baseOffset) => new Vector2(Game.width - baseOffset.X - baseOffset.Y, Game.height - baseOffset.Z - baseOffset.W) },
        { UiAnchorAlignment.TopLeft, (baseSize, baseOffset) => baseSize },
        { UiAnchorAlignment.TopCenter, (baseSize, baseOffset) => baseSize },
        { UiAnchorAlignment.TopRight, (baseSize, baseOffset) => baseSize },
        { UiAnchorAlignment.MiddleLeft, (baseSize, baseOffset) => baseSize },
        { UiAnchorAlignment.MiddleCenter, (baseSize, baseOffset) => baseSize },
        { UiAnchorAlignment.MiddleRight, (baseSize, baseOffset) => baseSize },
        { UiAnchorAlignment.BottomLeft, (baseSize, baseOffset) => baseSize },
        { UiAnchorAlignment.BottomCenter, (baseSize, baseOffset) => baseSize },
        { UiAnchorAlignment.BottomRight, (baseSize, baseOffset) => baseSize }
    };

    private Dictionary<UiAnchorAlignment, Func<int, int, Vector2, Vector4, Vector3>> positions = new Dictionary<UiAnchorAlignment, Func<int, int, Vector2, Vector4, Vector3>>
    {
        { UiAnchorAlignment.LeftScale, (width, height, size, baseOffset) => new Vector3(baseOffset.X, baseOffset.Z, 0) },
        { UiAnchorAlignment.RightScale, (width, height, size, baseOffset) => new Vector3(Game.width - size.X - baseOffset.Y, baseOffset.Z, 0) },
        { UiAnchorAlignment.TopScale, (width, height, size, baseOffset) => new Vector3(baseOffset.X, baseOffset.Z, 0) },
        { UiAnchorAlignment.BottomScale, (width, height, size, baseOffset) => new Vector3(baseOffset.X, Game.height - size.Y - baseOffset.W, 0) },
        { UiAnchorAlignment.CenterScale, (width, height, size, baseOffset) => new Vector3(baseOffset.X, baseOffset.Z, 0) },
        { UiAnchorAlignment.TopLeft, (width, height, size, baseOffset) => new Vector3(baseOffset.X, baseOffset.Z, 0) },
        { UiAnchorAlignment.TopCenter, (width, height, size, baseOffset) => new Vector3(((float)Game.width / 2) - (size.X / 2), baseOffset.Z, 0) },
        { UiAnchorAlignment.TopRight, (width, height, size, baseOffset) => new Vector3(Game.width - size.X - baseOffset.Y, baseOffset.Z, 0) },
        { UiAnchorAlignment.MiddleLeft, (width, height, size, baseOffset) => new Vector3(baseOffset.X, ((float)Game.height / 2) - (size.Y / 2), 0) },
        { UiAnchorAlignment.MiddleCenter, (width, height, size, baseOffset) => new Vector3(((float)Game.width / 2) - (size.X / 2), ((float)Game.height / 2) - (size.Y / 2), 0) },
        { UiAnchorAlignment.MiddleRight, (width, height, size, baseOffset) => new Vector3(Game.width - size.X - baseOffset.Y, ((float)Game.height / 2) - (size.Y / 2), 0) },
        { UiAnchorAlignment.BottomLeft, (width, height, size, baseOffset) => new Vector3(baseOffset.X, Game.height - size.Y - baseOffset.W, 0) },
        { UiAnchorAlignment.BottomCenter, (width, height, size, baseOffset) => new Vector3(((float)Game.width / 2) - (size.X / 2), Game.height - size.Y - baseOffset.W, 0) },
        { UiAnchorAlignment.BottomRight, (width, height, size, baseOffset) => new Vector3(Game.width - size.X - baseOffset.Y, Game.height - size.Y - baseOffset.W, 0) }
    };
}

public enum UiAnchor
{
    Absolute,
    Relative
}

public enum UiAnchorAlignment
{
    TopLeft, TopCenter, TopRight, MiddleLeft, MiddleCenter, MiddleRight, BottomLeft, BottomCenter, BottomRight,
    LeftScale, RightScale, TopScale, BottomScale, CenterScale
}