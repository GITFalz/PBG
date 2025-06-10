using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Vortice.Mathematics;

public class Chunk
{
    public static List<Chunk> Chunks = [];

    public const int WIDTH = 32;
    public const int HEIGHT = 32;
    public const int DEPTH = 32;
    
    public static Chunk Empty = new();

    public ChunkStatus Status = ChunkStatus.Empty;    
    private ChunkStatus _lastStatus = ChunkStatus.Empty;

    public Vector3i position = (0, 0, 0);
    public BlockStorage blockStorage;
    public BoundingBox boundingBox = new(new System.Numerics.Vector3(0, 0, 0), new System.Numerics.Vector3(0, 0, 0));

    public List<Vector3> Wireframe = [];

    public Dictionary<Vector3i, ChunkEntry?> NeighbourCunks = new Dictionary<Vector3i, ChunkEntry?>
    {
        { (-1, -1, -1), null }, { (-1, -1,  0), null }, { (-1, -1,  1), null },
        { (-1,  0, -1), null }, { (-1,  0,  0), null }, { (-1,  0,  1), null },
        { (-1,  1, -1), null }, { (-1,  1,  0), null }, { (-1,  1,  1), null },

        { ( 0, -1, -1), null }, { ( 0, -1,  0), null }, { ( 0, -1,  1), null },
        { ( 0,  0, -1), null },                         { ( 0,  0,  1), null },
        { ( 0,  1, -1), null }, { ( 0,  1,  0), null }, { ( 0,  1,  1), null },

        { ( 1, -1, -1), null }, { ( 1, -1,  0), null }, { ( 1, -1,  1), null },
        { ( 1,  0, -1), null }, { ( 1,  0,  0), null }, { ( 1,  0,  1), null },
        { ( 1,  1, -1), null }, { ( 1,  1,  0), null }, { ( 1,  1,  1), null },
    };

    public ChunkMesh Mesh;
    
    public int ChunkCount = 0;

    public bool Save = true;
    public bool Loaded = false;

    public bool BlockRendering => Mesh.BlockRendering;

    public bool IsDisabled { get => Mesh.IsDisabled; set => Mesh.IsDisabled = value; }
    public bool HasBlocks { get => Mesh.HasBlocks; set => Mesh.HasBlocks = value; }

    public bool IsTransparentDisabled { get => Mesh.IsTransparentDisabled; set => Mesh.IsTransparentDisabled = value; }
    public bool HasTransparentBlocks { get => Mesh.HasTransparentBlocks; set => Mesh.HasTransparentBlocks = value; }

    public int VertexCount => Mesh.VertexCount;
    public int TransparentVertexCount => Mesh.TransparentVertexCount;

    public Chunk()
    {
        blockStorage = CornerBlockStorage.Empty;
        Chunks.Add(this);
        Mesh = new ChunkMesh(Vector3i.Zero);
    }

    public Chunk(RenderType renderType, Vector3i position)
    {
        this.position = position;

        blockStorage = new FullBlockStorage();

        System.Numerics.Vector3 min = Mathf.Num(position * 32);
        System.Numerics.Vector3 max = min + new System.Numerics.Vector3(ChunkGenerator.WIDTH, ChunkGenerator.HEIGHT, ChunkGenerator.DEPTH);

        boundingBox = new BoundingBox(min, max);
        Chunks.Add(this);
        Mesh = new ChunkMesh(position);
    }

    public void SetPosition(Vector3i position)
    {
        this.position = position;
        boundingBox.Min = Mathf.Num(position * 32);
        boundingBox.Max = boundingBox.Min + new System.Numerics.Vector3(ChunkGenerator.WIDTH, ChunkGenerator.HEIGHT, ChunkGenerator.DEPTH);
    }

    public void AddFace(Vector3i position, int width, int height, int side, int textureIndex, Vector4i ambientOcclusion)
    {
        Mesh.AddFace(position, width, height, side, textureIndex, ambientOcclusion);
    }

    public void AddTransparentFace(Vector3i position, int width, int height, int side, int textureIndex, Vector4i ambientOcclusion)
    {
        Mesh.AddTransparentFace(position, width, height, side, textureIndex, ambientOcclusion);
    }


    public Block this[int index]
    {
        get => blockStorage[index];
        set => blockStorage[index] = value;
    }

    public Block this[int x, int y, int z]
    {
        get => this[x + (z * 32) + (y * 1024)];
        set => this[x + (z * 32) + (y * 1024)] = value;
    }

    public Block this[Vector3i position]
    {
        get => this[position.X, position.Y, position.Z];
        set => this[position.X, position.Y, position.Z] = value;
    }

    public Vector3i GetWorldPosition()
    {
        return position * 32;
    }

    public Vector3i GetRelativePosition()
    {
        return position;
    }

    public bool CreateChunkSolid()
    {
        return Mesh.CreateChunkSolid();
    }

    public void Clear()
    {
        Mesh.ClearMeshData();

        blockStorage.Clear();
        Wireframe = [];

        Mesh.VertexCount = 0;
        Status = ChunkStatus.Empty;
        Loaded = false;
        Save = true;
    }

    public void ClearMeshData()
    {
        Mesh.ClearMeshData();
    }

    public void Delete()
    {
        Clear();
        Mesh.Delete();
        Chunks.Remove(this);
    }

    public void Reload()
    {
        Mesh.ClearMeshData();
    }

    public void RenderChunk()
    {
        Mesh.RenderChunk();
    }

    public void RenderChunkTransparent()
    {
        Mesh.RenderChunkTransparent();
    }

    public void SaveChunk()
    {
        if (!Save) return;
        ChunkSaver.SaveChunk(this);
    }

    public bool LoadChunk()
    {
        Loaded = ChunkLoader.LoadChunk(this);
        return Loaded;
    }

    public void UpdateNeighbours()
    {
        ChunkCount = 0;
        foreach (var (side, _) in ChunkData.SideChunkIndices)
        {
            Vector3i position = GetRelativePosition() + side;
            if (ChunkManager.GetChunk(position, out ChunkEntry? chunk))
            {
                ChunkCount++;
                NeighbourCunks[side] = chunk;
            }
            else
            {
                NeighbourCunks[side] = null;
            }
        }
    }

    public bool AllChunksStageBetween(ChunkStage stage1, ChunkStage stage2)
    {
        foreach (var (_, chunk) in NeighbourCunks)
        {
            if (chunk == null || chunk.Stage < stage1 || chunk.Stage > stage2)
            {
                return false;
            }
        }

        return true;
    }

    private static readonly int[] SideIndices = [
        12, 21, 15, 4, 10, 13
    ];
}

public enum RenderType
{
    Solid,
    Wireframe
}