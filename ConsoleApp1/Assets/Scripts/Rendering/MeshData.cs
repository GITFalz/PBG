using OpenTK.Mathematics;

namespace ConsoleApp1.Assets.Scripts.Rendering;

public class MeshData
{
    public List<Vector3> verts;
    public List<uint> tris;
    public List<Vector2> uvs;
    public List<int> tCoords;

    public MeshData()
    {
        verts = new List<Vector3>();
        tris = new List<uint>();
        uvs = new List<Vector2>();
        tCoords = new List<int>();
    }

    public void AddQuadAtIndex(Quad quad, int i)
    {
        int vertIndex = i * 4;
        int uvIndex = i * 4;
        int triIndex = i * 6;
        
        if (vertIndex < 0 || vertIndex > verts.Count)
            return;
        
        verts.Insert(vertIndex, quad.verts[3]);
        verts.Insert(vertIndex, quad.verts[2]);
        verts.Insert(vertIndex, quad.verts[1]);
        verts.Insert(vertIndex, quad.verts[0]);
        
        uvs.Insert(uvIndex, quad.uvs[3]);
        uvs.Insert(uvIndex, quad.uvs[2]);
        uvs.Insert(uvIndex, quad.uvs[1]);
        uvs.Insert(uvIndex, quad.uvs[0]);
        
        tris.Insert(triIndex, (uint)(vertIndex + 0));
        tris.Insert(triIndex, (uint)(vertIndex + 3));
        tris.Insert(triIndex, (uint)(vertIndex + 2));
        tris.Insert(triIndex, (uint)(vertIndex + 2));
        tris.Insert(triIndex, (uint)(vertIndex + 1));
        tris.Insert(triIndex, (uint)(vertIndex + 0)); 
    }

    public void RemoveQuadAtIndex(int i)
    {
        int vertIndex = i * 4;
        int uvIndex = i * 4;

        if (vertIndex < 0 || vertIndex > verts.Count)
            return;

        verts.RemoveAt(vertIndex);
        verts.RemoveAt(vertIndex);
        verts.RemoveAt(vertIndex);
        verts.RemoveAt(vertIndex);

        uvs.RemoveAt(uvIndex);
        uvs.RemoveAt(uvIndex);
        uvs.RemoveAt(uvIndex);
        uvs.RemoveAt(uvIndex);
        
        for (int t = 0; t < 6; t++)
            tris.RemoveAt(tris.Count - 1);
    }

    public void RemoveLastQuad()
    {
        int index = verts.Count / 4 - 1;
        
        if (index < 0)
            return;
        
        RemoveQuadAtIndex(index);
    }

    public void Clear()
    {
        verts.Clear();
        tris.Clear();
        uvs.Clear();
    }
}

public struct Quad
{
    public Vector3[] verts;
    public Vector2[] uvs;
    public int[] tCoords;

    public Quad(Vector3[] verts, Vector2[] uvs, int[] tCoords)
    {
        this.verts = verts;
        this.uvs = uvs;
        this.tCoords = tCoords;
    }
}