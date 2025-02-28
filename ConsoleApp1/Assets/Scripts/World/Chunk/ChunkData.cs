using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Vortice.Mathematics;

public class ChunkData
{
    public Vector3i position = (0, 0, 0);
    public BlockStorage blockStorage = new(new Vector3i(0, 0, 0));
    public MeshData meshData = new();
    public int maxFaceCount = 0;
    public ChunkData[] sideChunks = [];
    public BoundingBox boundingBox = new(new System.Numerics.Vector3(0, 0, 0), new System.Numerics.Vector3(0, 0, 0));
    
    public VAO chunkVao = new VAO();
    public VBO vertVBO = new VBO([0, 0, 0]);
    public VBO uvVBO = new VBO([(0, 0)]);
    public VBO textureIndexVBO = new VBO([0]);
    public IBO chunkIbo = new IBO([]);

    public List<Vector3> Wireframe = [];

    private VAO _edgeVao = new VAO();
    private VBO _edgeVbo = new VBO([(0, 0, 0)]);

    public bool Save = true;
    public bool isEmpty = true;
    public short filePosition = -1;
    
    private string filePath;

    public Action Render = () => { };
    public Action CreateChunk = () => { };

    public bool IsDisabled = true;
    public bool HasBlocks = false;

    public Action Enable = () => { };
    public Action Disable = () => { };

    public ChunkData(RenderType renderType, Vector3i position)
    {
        this.position = position;

        blockStorage = new BlockStorage(position);
        
        System.Numerics.Vector3 min = new System.Numerics.Vector3(position.X, position.Y, position.Z);
        System.Numerics.Vector3 max = min + new System.Numerics.Vector3(Chunk.WIDTH, Chunk.HEIGHT, Chunk.DEPTH);
        
        boundingBox = new BoundingBox(min, max);
        
        filePath = Path.Combine(Game.chunkPath, $"Chunk_{position.X}_{position.Y}_{position.Z}");

        Render = renderType == RenderType.Solid ? RenderChunk : RenderWireframe;
        CreateChunk = renderType == RenderType.Solid ? CreateChunkSolid : CreateChunkWireframe;
    }

    public void SetRenderType(RenderType type)
    {
        if (type == RenderType.Solid)
        {
            Wireframe.Clear();
            Render = RenderChunk;
            CreateChunk = CreateChunkSolid;
        }
        else if (type == RenderType.Wireframe)
        {
            Wireframe = meshData.GetWireFrame();

            _edgeVbo = new VBO(Wireframe);
            _edgeVao.LinkToVAO(0, 3, _edgeVbo);

            Render = RenderWireframe;
            CreateChunk = CreateChunkWireframe;
        }
    }

    public void Clear()
    {
        meshData.Clear();
        sideChunks = null;
    }

    public void Delete()
    {
        chunkVao.Delete();
        vertVBO.Delete();
        uvVBO.Delete();
        textureIndexVBO.Delete();
        chunkIbo.Delete();

        _edgeVao.Delete();
        _edgeVbo.Delete();
        
        meshData.Clear();
        blockStorage.Clear();
    }
    
    public void CreateChunkSolid()
    {
        chunkVao = new VAO();

        vertVBO = new VBO(meshData.verts);
        uvVBO = new VBO(meshData.uvs);
        textureIndexVBO = new VBO(meshData.tCoords);
        
        chunkVao.LinkToVAO(0, 3, vertVBO);
        chunkVao.LinkToVAO(1, 2, uvVBO);
        chunkVao.LinkToVAO(2, 1, textureIndexVBO);
        
        chunkIbo = new IBO(meshData.tris);
    }

    public void CreateChunkWireframe()
    {
        Wireframe = meshData.GetWireFrame();

        _edgeVao = new VAO();
        _edgeVbo = new VBO(Wireframe);
        
        _edgeVao.LinkToVAO(0, 3, _edgeVbo);
    }
    
    public void RenderChunk()
    {
        chunkVao.Bind();
        chunkIbo.Bind();

        GL.DrawElements(PrimitiveType.Triangles, meshData.tris.Count, DrawElementsType.UnsignedInt, 0);
        
        chunkIbo.Unbind();
        chunkVao.Unbind();
    }

    public void RenderWireframe()
    {
        _edgeVao.Bind();

        GL.LineWidth(2.0f);
        GL.DrawArrays(PrimitiveType.Lines, 0, Wireframe.Count);

        _edgeVao.Unbind();
    }

    public void StoreData()
    {
        int i = 0;
        
        if (!Directory.Exists(filePath))
            Directory.CreateDirectory(filePath);
        
        foreach (var subPos in blockStorage.SubPositions)
        {
            string subFilePath = Path.Combine(filePath, $"SubChunk_{subPos.X}_{subPos.Y}_{subPos.Z}.chunk");
            
            Block?[]? blocks = blockStorage.Blocks[i];
            
            if (blockStorage.BlockCount[i] == 0 || blocks == null)
                SaveEmptyChunk(subFilePath);
            else
                SaveChunk(blocks, subFilePath);
            
            i++;
        }
    }
    
    public void SaveChunk(Block?[] data, string filePath)
    {
        using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
        {
            foreach (Block? block in data)
            {
                if (block == null)
                    writer.Write((short)-1);
                else
                    writer.Write(block.blockData);
            }
        }
    }
    
    public void SaveEmptyChunk(string filePath)
    {
        using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
        {
            writer.Write(-2);
        }
    }
    
    public bool FolderExists()
    {
        return Directory.Exists(filePath);
    }
    
    public void LoadData()
    {
        int i = 0;
        
        if (!Directory.Exists(filePath))
            Directory.CreateDirectory(filePath);
        
        foreach (var subPos in blockStorage.SubPositions)
        {
            string subFilePath = Path.Combine(filePath, $"SubChunk_{subPos.X}_{subPos.Y}_{subPos.Z}.chunk");
            blockStorage.Blocks[i] = LoadChunk(subFilePath, out int count);
            blockStorage.BlockCount[i] = count;
            
            i++;
        }
    }

    public Block?[]? LoadChunk(string subFilePath, out int count)
    {
        count = 0;
        
        using (BinaryReader reader = new BinaryReader(File.Open(subFilePath, FileMode.Open)))
        {
            short value = reader.ReadInt16();
            if (value == -2)
                return null;
            
            Block?[] blocks = new Block?[4096];
            blocks[0] = value == -1 ? null : new Block(value, 0);
            
            for (int i = 1; i < 4096; i++)
            {
                short data = reader.ReadInt16();
                if (data == -1)
                    blocks[i] = null;
                else
                {
                    blocks[i] = new Block(data, 0);
                    count++;
                }
            }
            
            return blocks;
        }
        
        return null;
    }
}

public class BlockStorage
{
    public List<Block?[]?> Blocks;
    public List<Vector3i> SubPositions;
    public int[] BlockCount;

    public BlockStorage(Vector3i position)
    {
        Blocks = new List<Block?[]?>() { null, null, null, null, null, null, null, null };
        BlockCount = [0, 0, 0, 0, 0, 0, 0, 0];
        SubPositions = new List<Vector3i>()
        {
            position,
            position + new Vector3i(16, 0, 0),
            position + new Vector3i(0, 0, 16),
            position + new Vector3i(16, 0, 16),
            position + new Vector3i(0, 16, 0),
            position + new Vector3i(16, 16, 0),
            position + new Vector3i(0, 16, 16),
            position + new Vector3i(16, 16, 16),
        };
    }
    
    public void SetBlock(int x, int y, int z, Block? block)
    {
        int modX = x & 15;
        int modY = y & 15;
        int modZ = z & 15;
        
        int xIndex = x >> 4;
        int yIndex = y >> 4;
        int zIndex = z >> 4;

        int blockIndex = modX + modZ * 16 + modY * 256;
        int arrayIndex = xIndex + zIndex * 2 + yIndex * 4;
        
        if (BlockCount[arrayIndex] == 0 || Blocks[arrayIndex] == null)
            Blocks[arrayIndex] = new Block[4096];
        
        Blocks[arrayIndex][blockIndex] = block;
        BlockCount[arrayIndex]++;
    }
    
    public Block? GetBlockNull(int x, int y, int z)
    {
        int modX = x & 15;
        int modY = y & 15;
        int modZ = z & 15;
        
        int xIndex = x >> 4;
        int yIndex = y >> 4;
        int zIndex = z >> 4;

        int blockIndex = modX + modZ * 16 + modY * 256;
        int arrayIndex = xIndex + zIndex * 2 + yIndex * 4;
        
        Block?[]? blocks = arrayIndex >= Blocks.Count ? null : Blocks[arrayIndex];
        
        if (BlockCount[arrayIndex] == 0 || blocks == null)
            return null;
        
        return blocks[blockIndex];
    }

    public Block GetBlock(int x, int y, int z)
    {
        return GetBlockNull(x, y, z) ?? new Block(-1, 0);
    }
    
    public Block? GetBlock(Vector3i position)
    {
        return GetBlockNull(position.X, position.Y, position.Z);
    }

    public Block?[] GetFullBlockArray()
    {
        Block?[] blocks = new Block?[32768];

        int index = 0;
        for (int y = 0; y < 32; y++)
        {
            for (int z = 0; z < 32; z++)
            {
                for (int x = 0; x < 32; x++)
                {
                    blocks[index] = GetBlockNull(x, y, z);
                    index++;
                }
            }
        }

        return blocks;
    }
    
    public void Clear()
    {
        Blocks.Clear();
    }
}
public struct Bounds
{
    public Vector3 Min;
    public Vector3 Max;

    public Bounds(Vector3 min, Vector3 max)
    {
        Min = min;
        Max = max;
    }
}

public enum RenderType
{
    Solid,
    Wireframe
}