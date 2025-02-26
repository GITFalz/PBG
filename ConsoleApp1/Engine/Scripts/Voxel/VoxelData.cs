using OpenTK.Mathematics;

public static class VoxelData
{
    public static readonly uint[] TrisIndexTable = new uint[]
    {
        0, 1, 2, 2, 3, 0
    };

    public static readonly Vector2[] UVTable = new Vector2[]
    {
        new Vector2(0, 0),
        new Vector2(0, 1),
        new Vector2(1, 1),
        new Vector2(1, 0),
    };
    
    public static readonly int[,] IndexOffsetLod =
    {
        { -32, 1, 1024, -1, -1024, 32 },
        { -16, 1, 256, -1, -256, 16 },
    };

    private const int ocmask = 0b100000001;
    
    public static readonly byte[] OcclusionShift = [16, 17, 18, 19, 20, 21];
    public static readonly byte[] CheckShift = [22, 23, 24, 25, 26, 27];

    public static readonly int[] OcclusionMask = [1 << 16, 1 << 17, 1 << 18, 1 << 19, 1 << 20, 1 << 21];
    public static readonly int[] CheckMask = [1 << 22, 1 << 23, 1 << 24, 1 << 25, 1 << 26, 1 << 27];
    public static readonly int[] OcclusionCheckMask = [ocmask << 16, ocmask << 17, ocmask << 18, ocmask << 19, ocmask << 20, ocmask << 21];

    public static bool InBounds(int x, int y, int z, int side, int size)
    {
        return side switch
        {
            0 => z - 1 >= 0,
            1 => x + 1 < size,
            2 => y + 1 < size,
            3 => x - 1 >= 0,
            4 => y - 1 >= 0,
            5 => z + 1 < size,
            _ => false
        };
    }
    
    public static readonly int[] FirstOffsetBase =
    {
        1024, 1024, 32, 1024, 32, 1024
    };
    
    public static readonly int[] SecondOffsetBase = { 1, 32, 1, 32, 1, 1 };
    
    public static readonly Func<int, int, int>[] FirstLoopBase =
    {
        (y, z) => 31 - y,
        (y, z) => 31 - y,
        (y, z) => 31 - z,
        (y, z) => 31 - y,
        (y, z) => 31 - z,
        (y, z) => 31 - y,
    };
    
    public static readonly Func<int, int, int>[] SecondLoopBase = 
    {
        (x, z) => 31 - x,
        (x, z) => 31 - z,
        (x, z) => 31 - x,
        (x, z) => 31 - z,
        (x, z) => 31 - x,
        (x, z) => 31 - x,
    };

    public static readonly Func<float, float, Vector3[]>[] PositionOffset =
    [
        (width, height) => new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, height, 0), new Vector3(width, height, 0), new Vector3(width, 0, 0), }, //Front
        (width, height) => new Vector3[] { new Vector3(1, 0, 0), new Vector3(1, height, 0), new Vector3(1, height, width), new Vector3(1, 0, width), }, //Right
        (width, height) => new Vector3[] { new Vector3(0, 1, 0), new Vector3(0, 1, height), new Vector3(width, 1, height), new Vector3(width, 1, 0), }, //Top
        (width, height) => new Vector3[] { new Vector3(0, 0, width), new Vector3(0, height, width), new Vector3(0, height, 0), new Vector3(0, 0, 0), }, //Left
        (width, height) => new Vector3[] { new Vector3(width, 0, 0), new Vector3(width, 0, height), new Vector3(0, 0, height), new Vector3(0, 0, 0), }, //Bottom
        (width, height) => new Vector3[] { new Vector3(width, 0, 1), new Vector3(width, height, 1), new Vector3(0, height, 1), new Vector3(0, 0, 1), }  //Back
    ];

    public static BoxMesh GetEntityBoxMesh(BoxMesh mesh, Vector3 size, Vector3 offset, int color)
    {
        MeshHelper.GenerateMeshIndices(mesh);
        mesh.Uvs.AddRange(VoxelData.UVTable);

        mesh.Vertices.Add(new Vector3(0, 0, 0) + offset);
        mesh.Vertices.Add(new Vector3(0, size.Y, 0) + offset);
        mesh.Vertices.Add(new Vector3(size.X, size.Y, 0) + offset);
        mesh.Vertices.Add(new Vector3(size.X, 0, 0) + offset);
        
        mesh.TextureIndices.Add(color);
        mesh.TextureIndices.Add(color);
        mesh.TextureIndices.Add(color);
        mesh.TextureIndices.Add(color);
        
        MeshHelper.GenerateMeshIndices(mesh);
        mesh.Uvs.AddRange(VoxelData.UVTable);
        
        mesh.Vertices.Add(new Vector3(size.X, 0, 0) + offset);
        mesh.Vertices.Add(new Vector3(size.X, size.Y, 0) + offset);
        mesh.Vertices.Add(new Vector3(size.X, size.Y, size.Z) + offset);
        mesh.Vertices.Add(new Vector3(size.X, 0, size.Z) + offset);
        
        mesh.TextureIndices.Add(color);
        mesh.TextureIndices.Add(color);
        mesh.TextureIndices.Add(color);
        mesh.TextureIndices.Add(color);
        
        MeshHelper.GenerateMeshIndices(mesh);
        mesh.Uvs.AddRange(VoxelData.UVTable);
        
        mesh.Vertices.Add(new Vector3(0, size.Y, 0) + offset);
        mesh.Vertices.Add(new Vector3(0, size.Y, size.Z) + offset);
        mesh.Vertices.Add(new Vector3(size.X, size.Y, size.Z) + offset);
        mesh.Vertices.Add(new Vector3(size.X, size.Y, 0) + offset);
        
        mesh.TextureIndices.Add(color);
        mesh.TextureIndices.Add(color);
        mesh.TextureIndices.Add(color);
        mesh.TextureIndices.Add(color);
        
        MeshHelper.GenerateMeshIndices(mesh);
        mesh.Uvs.AddRange(VoxelData.UVTable);
        
        mesh.Vertices.Add(new Vector3(0, 0, size.Z) + offset);
        mesh.Vertices.Add(new Vector3(0, size.Y, size.Z) + offset);
        mesh.Vertices.Add(new Vector3(0, size.Y, 0) + offset);
        mesh.Vertices.Add(new Vector3(0, 0, 0) + offset);
        
        mesh.TextureIndices.Add(color);
        mesh.TextureIndices.Add(color);
        mesh.TextureIndices.Add(color);
        mesh.TextureIndices.Add(color);
        
        MeshHelper.GenerateMeshIndices(mesh);
        mesh.Uvs.AddRange(VoxelData.UVTable);
        
        mesh.Vertices.Add(new Vector3(size.X, 0, size.Z) + offset);
        mesh.Vertices.Add(new Vector3(0, 0, size.Z) + offset);
        mesh.Vertices.Add(new Vector3(0, 0, 0) + offset);
        mesh.Vertices.Add(new Vector3(size.X, 0, 0) + offset);
        
        mesh.TextureIndices.Add(color);
        mesh.TextureIndices.Add(color);
        mesh.TextureIndices.Add(color);
        mesh.TextureIndices.Add(color);
        
        MeshHelper.GenerateMeshIndices(mesh);
        mesh.Uvs.AddRange(VoxelData.UVTable);
        
        mesh.Vertices.Add(new Vector3(size.X, 0, size.Z) + offset);
        mesh.Vertices.Add(new Vector3(size.X, size.Y, size.Z) + offset);
        mesh.Vertices.Add(new Vector3(0, size.Y, size.Z) + offset);
        mesh.Vertices.Add(new Vector3(0, 0, size.Z) + offset);
        
        mesh.TextureIndices.Add(color);
        mesh.TextureIndices.Add(color);
        mesh.TextureIndices.Add(color);
        mesh.TextureIndices.Add(color);

        return mesh;
    }

    public static void GenerateStandardMeshBox(BoxMesh mesh, Vector3 size, Vector3 position, Vector3 rotation, int color)
    {
        Quaternion rot = Quaternion.FromEulerAngles(MathHelper.DegreesToRadians(rotation.X), MathHelper.DegreesToRadians(rotation.Y), MathHelper.DegreesToRadians(rotation.Z));
        Vector3 center = new Vector3(0, 0, 0);
        
        MeshHelper.GenerateMeshIndices(mesh);
        mesh.Uvs.AddRange(VoxelData.UVTable);

        AddVertToBoxMesh(mesh, new Vector3(0, 0, 0), rot, position);
        AddVertToBoxMesh(mesh, new Vector3(0, size.Y, 0), rot, position);
        AddVertToBoxMesh(mesh, new Vector3(size.X, size.Y, 0), rot, position);
        AddVertToBoxMesh(mesh, new Vector3(size.X, 0, 0), rot, position);
        
        mesh.TextureIndices.Add(color);
        mesh.TextureIndices.Add(color);
        mesh.TextureIndices.Add(color);
        mesh.TextureIndices.Add(color);
        
        MeshHelper.GenerateMeshIndices(mesh);
        mesh.Uvs.AddRange(VoxelData.UVTable);
        
        AddVertToBoxMesh(mesh, new Vector3(size.X, 0, 0), rot, position);
        AddVertToBoxMesh(mesh, new Vector3(size.X, size.Y, 0), rot, position);
        AddVertToBoxMesh(mesh, new Vector3(size.X, size.Y, size.Z), rot, position);
        AddVertToBoxMesh(mesh, new Vector3(size.X, 0, size.Z), rot, position);
        
        mesh.TextureIndices.Add(color);
        mesh.TextureIndices.Add(color);
        mesh.TextureIndices.Add(color);
        mesh.TextureIndices.Add(color);
        
        MeshHelper.GenerateMeshIndices(mesh);
        mesh.Uvs.AddRange(VoxelData.UVTable);
        
        AddVertToBoxMesh(mesh, new Vector3(0, size.Y, 0), rot, position);
        AddVertToBoxMesh(mesh, new Vector3(0, size.Y, size.Z), rot, position);
        AddVertToBoxMesh(mesh, new Vector3(size.X, size.Y, size.Z), rot, position);
        AddVertToBoxMesh(mesh, new Vector3(size.X, size.Y, 0), rot, position);
        
        mesh.TextureIndices.Add(color);
        mesh.TextureIndices.Add(color);
        mesh.TextureIndices.Add(color);
        mesh.TextureIndices.Add(color);
        
        MeshHelper.GenerateMeshIndices(mesh);
        mesh.Uvs.AddRange(VoxelData.UVTable);
        
        AddVertToBoxMesh(mesh, new Vector3(0, 0, size.Z), rot, position);
        AddVertToBoxMesh(mesh, new Vector3(0, size.Y, size.Z), rot, position);
        AddVertToBoxMesh(mesh, new Vector3(0, size.Y, 0), rot, position);
        AddVertToBoxMesh(mesh, new Vector3(0, 0, 0), rot, position);
        
        mesh.TextureIndices.Add(color);
        mesh.TextureIndices.Add(color);
        mesh.TextureIndices.Add(color);
        mesh.TextureIndices.Add(color);
        
        MeshHelper.GenerateMeshIndices(mesh);
        mesh.Uvs.AddRange(VoxelData.UVTable);
        
        AddVertToBoxMesh(mesh, new Vector3(size.X, 0, size.Z), rot, position);
        AddVertToBoxMesh(mesh, new Vector3(0, 0, size.Z), rot, position);
        AddVertToBoxMesh(mesh, new Vector3(0, 0, 0), rot, position);
        AddVertToBoxMesh(mesh, new Vector3(size.X, 0, 0), rot, position);
        
        mesh.TextureIndices.Add(color);
        mesh.TextureIndices.Add(color);
        mesh.TextureIndices.Add(color);
        mesh.TextureIndices.Add(color);
        
        MeshHelper.GenerateMeshIndices(mesh);
        mesh.Uvs.AddRange(VoxelData.UVTable);
        
        AddVertToBoxMesh(mesh, new Vector3(size.X, 0, size.Z), rot, position);
        AddVertToBoxMesh(mesh, new Vector3(size.X, size.Y, size.Z), rot, position);
        AddVertToBoxMesh(mesh, new Vector3(0, size.Y, size.Z), rot, position);
        AddVertToBoxMesh(mesh, new Vector3(0, 0, size.Z), rot, position);
        
        mesh.TextureIndices.Add(color);
        mesh.TextureIndices.Add(color);
        mesh.TextureIndices.Add(color);
        mesh.TextureIndices.Add(color);
    }

    public static void AddVertToBoxMesh(BoxMesh mesh, Vector3 scale, Quaternion rotation, Vector3 position)
    {
        mesh.Vertices.Add(Mathf.RotateAround(scale + position, new Vector3(0, 0, 0), rotation));
    }
    
    public static Vector3i BlockToChunkPosition(Vector3 position)
    {
        return new Vector3i(
            (int)position.X & ~31,
            (int)position.Y & ~31,
            (int)position.Z & ~31
        );
    }
    
    public static Vector3i BlockToRelativePosition(Vector3 position)
    {
        return new Vector3i(
            (int)position.X & 31,
            (int)position.Y & 31,
            (int)position.Z & 31
        );
    }
}