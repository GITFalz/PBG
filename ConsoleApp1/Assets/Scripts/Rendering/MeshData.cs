using OpenTK.Mathematics;

public class MeshData
{
    public List<Vector3> verts = [];
    public List<uint> tris = [];
    public List<Vector2> uvs = [];
    public List<int> tCoords = [];
    public bool IsDisabled = true;

    public Action Enable = () => { };
    public Action Disable = () => { };

    public MeshData()
    {
        Enable = OnEnable;
    }

    public List<Vector3> GetWireFrame()
    {
        List<Vector3> wireFrame = [];
        HashSet<(Vector3, Vector3)> dupe = [];

        Console.WriteLine("Vertex: " + verts.Count);
        Console.WriteLine("Tris: " + tris.Count);

        for (int i = 0; i < tris.Count; i += 3)
        {
            int i0 = (int)tris[i];
            int i1 = (int)tris[i+1];
            int i2 = (int)tris[i+2];

            Vector3 v0 = verts[i0];
            Vector3 v1 = verts[i1];
            Vector3 v2 = verts[i2]; 

            (Vector3, Vector3) edge0 = Sup(v0, v1) ? (v0, v1) : (v1, v0);
            (Vector3, Vector3) edge1 = Sup(v1, v2) ? (v1, v2) : (v2, v1);
            (Vector3, Vector3) edge2 = Sup(v2, v0) ? (v2, v0) : (v0, v2);

            if (dupe.Add(edge0)) wireFrame.AddRange(edge0.Item1, edge0.Item2);
            if (dupe.Add(edge1)) wireFrame.AddRange(edge1.Item1, edge1.Item2);
            if (dupe.Add(edge2)) wireFrame.AddRange(edge2.Item1, edge2.Item2);
        }

        return wireFrame;
    }

    private void OnEnable()
    {
        IsDisabled = false;
        Disable = OnDisable;
        Enable = () => { };
    }

    private void OnDisable()
    {
        IsDisabled = true;
        Enable = OnEnable;
        Disable = () => { };
    }

    private static bool Sup(Vector3 a, Vector3 b)
    {
        return a.X > b.X && a.Y > b.Y && a.Z > b.Z;
    }

    public void Clear()
    {
        verts.Clear();
        tris.Clear();
        uvs.Clear();
        tCoords.Clear();
    }
}