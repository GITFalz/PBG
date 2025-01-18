using OpenTK.Mathematics;

public class Bone
{
    public Bone RootBone;
    public string Name;
    public List<Bone> Children = new List<Bone>();
    
    // Base values
    public Vector3 Pivot = Vector3.Zero;
    public Vector3 End = (0, 2, 0);
    public List<Vector3> Vertices = new List<Vector3>();
    
    // Transformed values
    public Quaternion TransformedRotation;
    public Vector3 TransformedPivot;
    public Vector3 TransformedEnd;
    public List<Vector3> TransformedVertices = new List<Vector3>();

    public Matrix4 localTransform;
    public Matrix4 globalTransform;

    public Bone(Bone rootBone, string name)
    {
        RootBone = rootBone;
        Name = name;
    }

    /// <summary>
    /// Constructor for the root bone
    /// </summary>
    /// <param name="name"></param>
    public Bone(string name)
    {
        RootBone = this;
        Name = name;
    }

    public void SetVertices(List<Vector3> vertices)
    {
        Vertices = vertices;
        TransformedVertices = [.. vertices];
    }
    
    public void Rotate(Quaternion rotation, Vector3 pivot)
    {
        ApplyRotation(rotation, pivot);

        foreach (var child in Children)
        {
            child.RotateChildren(rotation, pivot);
        }
    }

    public void Rotate(Quaternion rotation)
    {
        Rotate(rotation, TransformedPivot);
    }

    private void RotateChildren(Quaternion rotation, Vector3 pivot)
    {
        ApplyRotation(rotation, pivot);

        foreach (var child in Children)
        {
            child.RotateChildren(rotation, pivot);
        }
    }
    
    private void ApplyRotation(Quaternion rotation, Vector3 pivot)
    {
        TransformedPivot = Mathf.RotateAround(TransformedPivot, pivot, rotation);
        TransformedEnd = Mathf.RotateAround(TransformedEnd, pivot, rotation);

        for (int i = 0; i < TransformedVertices.Count; i++)
        {
            TransformedVertices[i] = Mathf.RotateAround(TransformedVertices[i], pivot, rotation);
        }
    }

    public void AddChild(Bone child)
    {
        var names = RootBone.GetChildNames();
        while (names.Contains(child.Name))
        {
            child.Name += $"{names.Count}";
        }
        Children.Add(child);
        child.RootBone = this;
    }

    public bool IsRoot()
    {
        return RootBone == this;
    }

    public List<string> GetChildNames()
    {
        List<string> names = [Name];
        foreach (var child in Children)
        {
            names.AddRange(child.GetChildNames());
        }
        return names;
    }

    public void SetPivot(Vector3 pivot)
    {
        Pivot = pivot;
        TransformedPivot = pivot;
    }

    public void ResetRotation()
    {
        TransformedRotation = Quaternion.Identity;
        TransformedPivot = Pivot;
        TransformedEnd = End;

        foreach (var child in Children)
        {
            child.ResetRotation();
        }
    }

    public void CalculateGlobalTransform()
    {
        globalTransform = IsRoot() ? localTransform : localTransform * RootBone.globalTransform;
        foreach (var child in Children) {
            child.CalculateGlobalTransform();
        }
    }

    public void ResetTransformedVertices()
    {
        TransformedVertices = [.. Vertices];

        foreach (var child in Children)
        {
            child.ResetTransformedVertices();
        }
    }
    
    public List<Vector3> GetTransformedVertices()
    {
        List<Vector3> vertices = [];
        foreach (var vertex in TransformedVertices) { vertices.Add(vertex); }
        foreach (var child in Children) { vertices.AddRange(child.GetTransformedVertices()); }
        return vertices;
    }

    public List<Bone> GetBones()
    {
        List<Bone> bones = [this];
        foreach (var child in Children)
        {
            bones.AddRange(child.GetBones());
        }
        return bones;
    }
}