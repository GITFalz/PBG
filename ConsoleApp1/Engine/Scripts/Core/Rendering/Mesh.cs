using OpenTK.Mathematics;

namespace ConsoleApp1.Engine.Scripts.Core.Rendering;

public class Mesh
{
    public List<Vector3> vertices { get; set; }
    public List<Vector3> uvs { get; set; }
    public List<uint> indices { get; set; }
    public Bounds bounds { get; set; }
    
    public List<Vector2> uvs2D { get; set; }
    public List<int> texIndex { get; set; }

    public Mesh()
    {
        vertices = new List<Vector3>();
        uvs = new List<Vector3>();
        indices = new List<uint>();
        uvs2D = new List<Vector2>();
        texIndex = new List<int>();
    }

    public void CheckConformity()
    {
        uint max = indices.Max();
        
        bool tris = indices.Count % 3 == 0;
        bool uvs = this.uvs.Count == vertices.Count;
        bool verts = vertices.Count > (int)max;
        
        if (!tris)
        {
            throw new Exception("Indices count must be a multiple of 3");
        }
        
        if (!uvs)
        {
            throw new Exception("UVs count must be equal to vertices count");
        }
        
        if (!verts)
        {
            throw new Exception("Indices must not reference non-existent vertices");
        }
    }

    public void SeperateUvs()
    {
        foreach (Vector3 uv in uvs)
        {
            uvs2D.Add(new Vector2(uv.X, uv.Y));
            texIndex.Add((int)uv.Z);
        }
    }

    public void RecalculateBounds()
    {
        Bounds bounds = new Bounds();
        bounds.min = new Vector3(float.MaxValue);
        bounds.max = new Vector3(float.MinValue);

        foreach (Vector3 vertex in vertices)
        {
            bounds.min = Vector3.ComponentMin(bounds.min, vertex);
            bounds.max = Vector3.ComponentMax(bounds.max, vertex);
        }
    }
    
    public struct Bounds
    {
        public Vector3 min;
        public Vector3 max;
    }
}