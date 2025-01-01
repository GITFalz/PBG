using ConsoleApp1.Assets.Scripts.World.Blocks;
using OpenTK.Mathematics;

public static class VoxelData
{
    public static readonly List<List<Vector3>> voxelVerts = new List<List<Vector3>>()
    {
        new List<Vector3>()
        {
            //Font face
            new Vector3(0, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(1, 1, 0),
            new Vector3(1, 0, 0),
        },

        new List<Vector3>()
        {
            //Right face
            new Vector3(1, 0, 0),
            new Vector3(1, 1, 0),
            new Vector3(1, 1, 1),
            new Vector3(1, 0, 1),
        },

        new List<Vector3>()
        {
            //Top face
            new Vector3(0, 1, 0),
            new Vector3(0, 1, 1),
            new Vector3(1, 1, 1),
            new Vector3(1, 1, 0),
        },

        new List<Vector3>()
        {
            //Left face
            new Vector3(0, 0, 1),
            new Vector3(0, 1, 1),
            new Vector3(0, 1, 0),
            new Vector3(0, 0, 0),
        },

        new List<Vector3>()
        {
            //Bottom face
            new Vector3(0, 0, 1),
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(1, 0, 1),
        },

        new List<Vector3>()
        {
            //Back face
            new Vector3(1, 0, 1),
            new Vector3(1, 1, 1),
            new Vector3(0, 1, 1),
            new Vector3(0, 0, 1),
        }
    };
    
    public static readonly Vector3[] VertexTable = new Vector3[8]
    {
        new Vector3(0f, 0f, 0f),
        new Vector3(1f, 0f, 0f),
        new Vector3(1f, 1f, 0f),
        new Vector3(0f, 1f, 0f),
        new Vector3(0f, 0f, 1f),
        new Vector3(1f, 0f, 1f),
        new Vector3(1f, 1f, 1f),
        new Vector3(0f, 1f, 1f),
    };

    public static readonly int[,] VertexIndexTable = new int[,]
    {
        { 0, 3, 2, 1 },
        { 1, 2, 6, 5 },
        { 2, 3, 7, 6 },
        { 4, 7, 3, 0 },
        { 5, 4, 0, 1 },
        { 5, 6, 7, 4 },
    };

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
    
    public static readonly int[] IndexOffset = { -32, 1, 1024, -1, -1024, 32 };
    
    public static readonly int[,] IndexOffsetLod =
    {
        { -32, 1, 1024, -1, -1024, 32 },
        { -16, 1, 256, -1, -256, 16 },
    };
    
    public static readonly Func<int, int, int>[] IndexOffsetVE =
    {
        (X, Z) => -X,
        (X, Z) => 1,
        (X, Z) => X * Z,
        (X, Z) => -1,
        (X, Z) => -X * Z,
        (X, Z) => X,
    };
    
    public static readonly byte[] ShiftPosition = { 1, 2, 4, 8, 16, 32 };

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
    
    public static bool InBounds(int x, int y, int z, int side, int X, int Y, int Z)
    {
        return side switch
        {
            0 => z - 1 >= 0,
            1 => x + 1 < X,
            2 => y + 1 < Y,
            3 => x - 1 >= 0,
            4 => y - 1 >= 0,
            5 => z + 1 < Z,
            _ => false
        };
    }
    
    public static bool BlockIsValid(Block a, Block b, int side)
    {
        return
            (a.check & VoxelData.ShiftPosition[side]) == 0 &&
            (a.occlusion & VoxelData.ShiftPosition[side]) == 0 &&
            a.blockData != b.blockData;
    }
    
    public static readonly int[] FirstOffsetBase =
    {
        1024, 1024, 32, 1024, 32, 1024
    };

    public static readonly int[][] FirstOffset = 
    {
        new int[]
        {
            1024, 1024, 32, 1024, 32, 1024
        },
        new int[]
        {
            256, 256, 16, 256, 16, 256
        },
    };
    
    public static readonly Func<int, int, int>[] FirstOffsetVe = 
    {
        (X, Z) => X * Z,
        (X, Z) => X * Z,
        (X, Z) => X,
        (X, Z) => X * Z,
        (X, Z) => X,
        (X, Z) => X * Z,
    };
    
    public static readonly int[] SecondOffsetBase = { 1, 32, 1, 32, 1, 1 };
    
    public static readonly int[][] SecondOffset =
    {
        new int[]
        {
            1, 32, 1, 32, 1, 1
        },
        new int[]
        {
            1, 16, 1, 16, 1, 1
        },
    };
    
    public static readonly Func<int, int>[] SecondOffsetVe = 
    {
        (X) => 1,
        (X) => X,
        (X) => 1,
        (X) => X,
        (X) => 1,
        (X) => 1,
    };
    
    public static readonly Func<int, int, int>[] FirstLoopBase =
    {
        (y, z) => 31 - y,
        (y, z) => 31 - y,
        (y, z) => 31 - z,
        (y, z) => 31 - y,
        (y, z) => 31 - z,
        (y, z) => 31 - y,
    };

    public static readonly Func<int, int, int>[][] FirstLoop =
    {
        new Func<int, int, int>[]
        {
            (y, z) => 31 - y,
            (y, z) => 31 - y,
            (y, z) => 31 - z,
            (y, z) => 31 - y,
            (y, z) => 31 - z,
            (y, z) => 31 - y,
        },
        new Func<int, int, int>[]
        {
            (y, z) => 15 - y,
            (y, z) => 15 - y,
            (y, z) => 15 - z,
            (y, z) => 15 - y,
            (y, z) => 15 - z,
            (y, z) => 15 - y,
        },
    };
    
    public static readonly Func<int, int, int, int, int>[] FirstLoopVe =
    {
        (y, z, Y, Z) => Y - y,
        (y, z, Y, Z) => Y - y,
        (y, z, Y, Z) => Z - z,
        (y, z, Y, Z) => Y - y,
        (y, z, Y, Z) => Z - z,
        (y, z, Y, Z) => Y - y,
    };
    
    public static readonly Func<int, int, int, int>[] Loop1 =
    {
        (a, y, z) => a - y,
        (a, y, z) => a - y,
        (a, y, z) => a - z,
        (a, y, z) => a - y,
        (a, y, z) => a - z,
        (a, y, z) => a - y,
    };
    
    public static readonly Func<int, int, int, int>[] Loop2 = 
    {
        (a, x, z) => a - x,
        (a, x, z) => a - z,
        (a, x, z) => a - x,
        (a, x, z) => a - z,
        (a, x, z) => a - x,
        (a, x, z) => a - x,
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
    
    public static readonly Func<int, int, int>[][] SecondLoop = 
    {
        new Func<int, int, int>[]
        {
            (x, z) => 31 - x,
            (x, z) => 31 - z,
            (x, z) => 31 - x,
            (x, z) => 31 - z,
            (x, z) => 31 - x,
            (x, z) => 31 - x,
        },
        new Func<int, int, int>[]
        {
            (x, z) => 15 - x,
            (x, z) => 15 - z,
            (x, z) => 15 - x,
            (x, z) => 15 - z,
            (x, z) => 15 - x,
            (x, z) => 15 - x,
        },
    };
    
    public static readonly Func<int, int, int, int, int>[] SecondLoopVe =
    {
        (x, z, X, Z) => X - x,
        (x, z, X, Z) => Z - z,
        (x, z, X, Z) => X - x,
        (x, z, X, Z) => Z - z,
        (x, z, X, Z) => X - x,
        (x, z, X, Z) => X - x,
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