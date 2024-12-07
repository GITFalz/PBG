using OpenTK.Mathematics;
using StbImageSharp;
using Vortice.Mathematics;

public abstract class UI_Base : Component
{
    protected static readonly Vector3 TextOffset = new Vector3(0, 0, 0.001f);
     
    public int memPos = 0;
    public int memSize = 1;
    
    public UI_Base? parent;
    
    public Vector3 position = Vector3.Zero;
    public Vector2 size = new Vector2(100, 100);
    public Vector4 offset = Vector4.Zero;
    
    public Vector4 baseOffset = Vector4.Zero;
    public Vector2 baseSize = new Vector2(100, 100);
    
    public UiAnchor anchor = UiAnchor.Absolute;
    public UiAnchorAlignment anchorAlignment = UiAnchorAlignment.MiddleCenter;
    
    
    public virtual void SetSize(Vector2 s)
    {
        baseSize = s;
    }
    
    public void SetOffset(Vector4 offset)
    {
        baseOffset = offset;
    }
    
    public void SetAnchorAlignment(UiAnchorAlignment alignment)
    {
        anchorAlignment = alignment;
    }
    
    public void SetAnchorReference(UiAnchor a)
    {
        anchor = a;
    }
    
    public void SetParent(UI_Base p)
    {
        parent = p;
    }


    public virtual void SetMem(int pos)
    {
        memPos = pos;
    }
    
    

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

    public virtual void RenderText(MeshData meshData)
    {
        
    }

    protected void Align()
    {
        if (parent != null && anchor == UiAnchor.Relative)
        {
            offset = baseOffset + parent.baseOffset;
            size = sizes[anchorAlignment](parent.size.X, parent.size.Y, baseSize, offset);
            position = positions[anchorAlignment](parent.size.X, parent.size.Y, size, offset) + parent.position + TextOffset;
            
            Console.WriteLine(offset + " " + parent.offset + " " + size + " " + parent.size + " " + position + " " + parent.position);
        }
        else
        
        {
            offset = baseOffset;
            size = sizes[anchorAlignment](Game.width, Game.height, baseSize, offset);
            position = positions[anchorAlignment](Game.width, Game.height, size, offset);
        }
    }
    
    private Dictionary<UiAnchorAlignment, Func<float, float, Vector2, Vector4, Vector2>> sizes = new Dictionary<UiAnchorAlignment, Func<float, float, Vector2, Vector4, Vector2>>
    {
        { UiAnchorAlignment.LeftScale, (width, height, baseSize, baseOffset) => new Vector2(baseSize.X, height - baseOffset.Z - baseOffset.W) },
        { UiAnchorAlignment.RightScale, (width, height, baseSize, baseOffset) => new Vector2(baseSize.X, height - baseOffset.Z - baseOffset.W) },
        { UiAnchorAlignment.TopScale, (width, height, baseSize, baseOffset) => new Vector2(width - baseOffset.X - baseOffset.Y, baseSize.Y) },
        { UiAnchorAlignment.BottomScale, (width, height, baseSize, baseOffset) => new Vector2(width - baseOffset.X - baseOffset.Y, baseSize.Y) },
        { UiAnchorAlignment.CenterScale, (width, height, baseSize, baseOffset) => new Vector2(width - baseOffset.X - baseOffset.Y, height - baseOffset.Z - baseOffset.W) },
        { UiAnchorAlignment.TopLeft, (width, height, baseSize, baseOffset) => baseSize },
        { UiAnchorAlignment.TopCenter, (width, height, baseSize, baseOffset) => baseSize },
        { UiAnchorAlignment.TopRight, (width, height, baseSize, baseOffset) => baseSize },
        { UiAnchorAlignment.MiddleLeft, (width, height, baseSize, baseOffset) => baseSize },
        { UiAnchorAlignment.MiddleCenter, (width, height, baseSize, baseOffset) => baseSize },
        { UiAnchorAlignment.MiddleRight, (width, height, baseSize, baseOffset) => baseSize },
        { UiAnchorAlignment.BottomLeft, (width, height, baseSize, baseOffset) => baseSize },
        { UiAnchorAlignment.BottomCenter, (width, height, baseSize, baseOffset) => baseSize },
        { UiAnchorAlignment.BottomRight, (width, height, baseSize, baseOffset) => baseSize }
    };

    private Dictionary<UiAnchorAlignment, Func<float, float, Vector2, Vector4, Vector3>> positions = new Dictionary<UiAnchorAlignment, Func<float, float, Vector2, Vector4, Vector3>>
    {
        { UiAnchorAlignment.LeftScale, (width, height, size, baseOffset) => new Vector3(baseOffset.X, baseOffset.Z, 0) },
        { UiAnchorAlignment.RightScale, (width, height, size, baseOffset) => new Vector3(width - size.X - baseOffset.Y, baseOffset.Z, 0) },
        { UiAnchorAlignment.TopScale, (width, height, size, baseOffset) => new Vector3(baseOffset.X, baseOffset.Z, 0) },
        { UiAnchorAlignment.BottomScale, (width, height, size, baseOffset) => new Vector3(baseOffset.X, height - size.Y - baseOffset.W, 0) },
        { UiAnchorAlignment.CenterScale, (width, height, size, baseOffset) => new Vector3(baseOffset.X, baseOffset.Z, 0) },
        { UiAnchorAlignment.TopLeft, (width, height, size, baseOffset) => new Vector3(baseOffset.X, baseOffset.Z, 0) },
        { UiAnchorAlignment.TopCenter, (width, height, size, baseOffset) => new Vector3(((float)width / 2) - (size.X / 2), baseOffset.Z, 0) },
        { UiAnchorAlignment.TopRight, (width, height, size, baseOffset) => new Vector3(width - size.X - baseOffset.Y, baseOffset.Z, 0) },
        { UiAnchorAlignment.MiddleLeft, (width, height, size, baseOffset) => new Vector3(baseOffset.X, ((float)height / 2) - (size.Y / 2), 0) },
        { UiAnchorAlignment.MiddleCenter, (width, height, size, baseOffset) => new Vector3(((float)width / 2) - (size.X / 2), ((float)height / 2) - (size.Y / 2), 0) },
        { UiAnchorAlignment.MiddleRight, (width, height, size, baseOffset) => new Vector3(width - size.X - baseOffset.Y, ((float)height / 2) - (size.Y / 2), 0) },
        { UiAnchorAlignment.BottomLeft, (width, height, size, baseOffset) => new Vector3(baseOffset.X, height - size.Y - baseOffset.W, 0) },
        { UiAnchorAlignment.BottomCenter, (width, height, size, baseOffset) => new Vector3(((float)width / 2) - (size.X / 2), height - size.Y - baseOffset.W, 0) },
        { UiAnchorAlignment.BottomRight, (width, height, size, baseOffset) => new Vector3(width - size.X - baseOffset.Y, height - size.Y - baseOffset.W, 0) }
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