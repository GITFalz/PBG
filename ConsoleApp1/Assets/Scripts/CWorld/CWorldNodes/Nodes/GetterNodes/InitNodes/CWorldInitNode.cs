using OpenTK.Mathematics;

public abstract class CWorldInitNode : CWorldGetterNode
{
    public CWorldInitNode() : base() { }
    public abstract void Init(Vector2 position);
}