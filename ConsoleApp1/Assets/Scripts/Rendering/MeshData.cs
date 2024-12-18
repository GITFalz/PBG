using OpenTK.Mathematics;

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

    public bool MoveQuad(int index, Vector3 position)
    {
        int vertIndex = index * 4;
        
        if (vertIndex < 0 || vertIndex > verts.Count)
            return false;
        
        verts[vertIndex + 0] += position;
        verts[vertIndex + 1] += position;
        verts[vertIndex + 2] += position;
        verts[vertIndex + 3] += position;

        return true;
    }
    
    public bool MoveQuadToXy(int index, Vector2 position)
    {
        int vertIndex = index * 4;
        
        if (vertIndex < 0 || vertIndex > verts.Count)
            return false;
        
        Console.WriteLine(position);
        
        verts[vertIndex + 0] = new Vector3(position.X, position.Y, verts[vertIndex + 0].Z);
        verts[vertIndex + 1] = new Vector3(position.X, position.Y, verts[vertIndex + 1].Z);
        verts[vertIndex + 2] = new Vector3(position.X, position.Y, verts[vertIndex + 2].Z);
        verts[vertIndex + 3] = new Vector3(position.X, position.Y, verts[vertIndex + 3].Z);

        return true;
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