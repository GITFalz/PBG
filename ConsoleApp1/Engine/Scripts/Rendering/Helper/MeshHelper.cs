using OpenTK.Mathematics;

public static class MeshHelper
{
    public static void GenerateMeshIndices(Mesh mesh)
    {
        uint count = (uint)mesh.Vertices.Count;
        
        mesh.Indices.Add(0 + count);
        mesh.Indices.Add(1 + count);
        mesh.Indices.Add(2 + count);
        mesh.Indices.Add(2 + count);
        mesh.Indices.Add(3 + count);
        mesh.Indices.Add(0 + count);
    }
}