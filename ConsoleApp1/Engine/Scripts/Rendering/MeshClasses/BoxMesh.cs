using OpenTK.Mathematics;

public abstract class BoxMesh : Mesh
{
    public List<int> TextureIndices;
    
    public List<Vector3> _transformedVerts;
}