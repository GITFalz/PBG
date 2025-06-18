public class HierarchyManager
{
    public List<HierarchySelection> Selections = [];
    
    public void AddSelection(HierarchySelection selection)
    {
        Selections.Add(selection);
    }

    public void Clear()
    {

    }
}

public abstract class HierarchySelection
{
    public HierarchySelection? Parent;
    public string Name = "Element";

    public virtual void Clear() {}
    public virtual void Delete() {}
    public virtual void Delete(HierarchySelection child) { }

    public void DeleteFromParent() { Parent?.Delete(this); }
}

public class GroupSelection : HierarchySelection
{
    public List<HierarchySelection> Children { get; set; } = [];

    public void AddSelection(HierarchySelection selection)
    {
        selection.Parent = this;
        Children.Add(selection);
    }

    public override void Clear()
    {
        foreach (var child in Children)
        {
            child.Clear();
        }
        Children = [];
    }

    public override void Delete()
    {
        foreach (var child in Children)
        {
            child.Delete();
        }
        Children = [];
    }

    public override void Delete(HierarchySelection child) { Children.Remove(child); }
}