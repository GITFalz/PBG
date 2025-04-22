public abstract class CWorldGetterNode : CWorldNode
{
    public CWorldGetterNode() : base() { }
    public CWorldGetterNode(string name) : base(name) { } 
    public virtual void SetValue(float value) { } // Only used for the empty node
    public abstract float GetValue();
} 