using OpenTK.Mathematics;

public class SkyboxMesh : Mesh
{
    public SkyboxMesh()
    {
        _vao = new VAO();
        
        Vertices = new List<Vector3>();
        Uvs = new List<Vector2>();
        Indices = new List<uint>();

        for (int i = 0; i < 6; i++)
        {
            uint count = (uint)Vertices.Count;
            
            Indices.Add(count + 1);
            Indices.Add(count + 0);
            Indices.Add(count + 2);
            Indices.Add(count + 2);
            Indices.Add(count + 0);
            Indices.Add(count + 3);
            
            Vector3[] vertices = VoxelData.PositionOffset[i](1, 1);
            
            foreach (Vector3 vertex in vertices)
            {
                Console.WriteLine(vertex + new Vector3(0, 100, 0));
                Vertices.Add(vertex + new Vector3(0, 100, 0));
            }
            
            Uvs.Add(new Vector2(0, 0));
            Uvs.Add(new Vector2(0, 1));
            Uvs.Add(new Vector2(1, 1));
            Uvs.Add(new Vector2(1, 0));
        }
        
        base.GenerateBuffers();
    }
}