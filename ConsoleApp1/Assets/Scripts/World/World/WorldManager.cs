using System.Collections.Concurrent;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class WorldManager : ScriptingNode
{
    public static WorldManager? Instance;
    
    public Camera camera;
    
    public HashSet<Vector3i> chunks;
    
    public ConcurrentDictionary<Vector3i, ChunkData> activeChunks;
    public ConcurrentQueue<Vector3i> chunksToGenerate;
    public ConcurrentQueue<ChunkData> chunksToCreate;
    public ConcurrentQueue<ChunkData> chunksToStore;

    private ConcurrentBag<Vector3i> chunksToIgnore;
    
    private Task? currentTask = null;
    private Task? storeTask = null;
    
    public ChunkRegionData regionData;
    
    
    private ShaderProgram _shaderProgram;
    private Texture _textureArray;
    
    public WorldManager()
    {
        Instance ??= this;
        
        chunks = new HashSet<Vector3i>();
        
        activeChunks = new ConcurrentDictionary<Vector3i, ChunkData>();
        chunksToGenerate = new ConcurrentQueue<Vector3i>();
        chunksToCreate = new ConcurrentQueue<ChunkData>();
        chunksToStore = new ConcurrentQueue<ChunkData>();
        chunksToIgnore = new ConcurrentBag<Vector3i>();
        
        regionData = new ChunkRegionData(new Vector3i(0, 0, 0));
        regionData.SaveChunk(new Vector3i(0, 1, 0), new ChunkData(new Vector3i(0, 1, 0)));
        regionData.SaveChunk(new Vector3i(1, 1, 0), new ChunkData(new Vector3i(1, 1, 0)));
        
        _shaderProgram = new ShaderProgram("World/Default.vert", "World/Default.frag");
    }
    
    public override void Awake()
    {
        Console.WriteLine("World Manager");
        
        _textureArray = new Texture("EditorTiles.png");
        
        activeChunks.Clear();
        chunksToGenerate.Clear();
        chunksToCreate.Clear();
        chunksToStore.Clear();
        chunksToIgnore.Clear();
        
        SetChunks();
    }
    
    public override void Update()
    {
        CheckRenderDistance();
        GenerateChunks();
        StoreChunks();
        
        if (chunksToCreate.TryDequeue(out var chunk))
        {
            if (activeChunks.TryAdd(chunk.position, chunk))
            {
                chunk.CreateChunk();
            }
            else
            {
                chunksToGenerate.Enqueue(chunk.position);
                chunk.Clear();
            }
        }
    }

    public override void Render()
    {   
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);

        _shaderProgram.Bind();
        _textureArray.Bind();
        
        Matrix4 model = Matrix4.Identity;
        Matrix4 view = camera.viewMatrix;
        Matrix4 projection = camera.projectionMatrix;

        int modelLocation = GL.GetUniformLocation(_shaderProgram.ID, "model");
        int viewLocation = GL.GetUniformLocation(_shaderProgram.ID, "view");
        int projectionLocation = GL.GetUniformLocation(_shaderProgram.ID, "projection");
        int camPosLocation = GL.GetUniformLocation(_shaderProgram.ID, "camPos");
        
        GL.UniformMatrix4(modelLocation, true, ref model);
        GL.UniformMatrix4(viewLocation, true, ref view);
        GL.UniformMatrix4(projectionLocation, true, ref projection);
        GL.Uniform3(camPosLocation, camera.Position);
        
        foreach (var chunk in activeChunks)
        {
            chunk.Value.RenderChunk();
        }
        
        _shaderProgram.Unbind();
        _textureArray.Unbind();

        GL.Disable(EnableCap.DepthTest);
        GL.Disable(EnableCap.CullFace);
    }

    public override void Exit()
    {
        Delete();
        base.Exit();
    }

    public void SetChunks()
    {
        int render = World.renderDistance;
        Vector3i playerChunk = new Vector3i(0, 0, 0);
        
        HashSet<Vector3i> chunkPositions = new HashSet<Vector3i>();
        
        for (int x = -render; x < render; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                for (int z = -render; z < render; z++)
                {
                    Vector3i position = new Vector3i(playerChunk.X + x, y, playerChunk.Z + z) * 32;
                    chunkPositions.Add(position);
                }
            }
        }
        
        chunks = chunkPositions;
    }

    public void CheckRenderDistance()
    {
        SetChunks();
        
        int render = World.renderDistance;
        //Vector3i playerChunk = new Vector3i(playerPosition.X / 32, playerPosition.Y / 32, playerPosition.Z / 32);
        Vector3i playerChunk = new Vector3i(0, 0, 0);
        
        HashSet<Vector3i> chunksToRemove = new HashSet<Vector3i>();
        
        foreach (var chunk in activeChunks)
        {
            chunksToRemove.Add(chunk.Key);
        }
        
        foreach (var chunk in chunks)
        {
            chunksToRemove.Remove(chunk);
            
            if (!activeChunks.ContainsKey(chunk) && !chunksToGenerate.Contains(chunk) && chunksToCreate.All(c => c.position != chunk))
            {
                chunksToGenerate.Enqueue(chunk);
            }
        }
        
        chunksToIgnore.Clear();
        
        foreach (var chunk in chunksToRemove)
        {
            chunksToIgnore.Add(chunk);
            
            if (activeChunks.TryRemove(chunk, out var data))
            {
                data.Clear();
            }
        }
    }

    public int GetBlock(Vector3i blockPosition, out Block? block)
    {
        block = null;
        Vector3i chunkPosition = VoxelData.BlockToChunkPosition(blockPosition);

        if (!activeChunks.TryGetValue(chunkPosition, out var chunk)) return chunks.Contains(chunkPosition) ? 2 : 1;
        
        block = chunk.blockStorage.GetBlock(VoxelData.BlockToRelativePosition(blockPosition));
        return block == null ? 1 : 0;
    }

    private async void GenerateChunks()
    {
        if (currentTask == null)
        {
            currentTask = GenerateChunk();
            await currentTask;
            currentTask = null;
        }
    }

    private async Task GenerateChunk()
    {
        if (chunksToGenerate.TryDequeue(out var position))
        {
            if (activeChunks.ContainsKey(position) || chunksToIgnore.Contains(position)) return;

            ChunkData chunkData = new ChunkData(position);
            chunkData.meshData = new MeshData();

            await Task.Run(() =>
            {
                Block?[] blocks;

                /*
                if (!chunkData.FolderExists())
                {
                    
                    chunksToStore.Enqueue(chunkData);
                }
                else
                {
                    chunkData.LoadData();
                    blocks = chunkData.blockStorage.GetFullBlockArray();
                }
                */
                
                Chunk.GenerateFlatChunk(ref chunkData, position);
                Chunk.GenerateBox(ref chunkData, position, new Vector3i(20, 10, 20), new Vector3i(10, 10, 10));
                
                blocks = chunkData.blockStorage.GetFullBlockArray();
                
                Chunk.GenerateOcclusion(blocks);
                Chunk.GenerateMesh(blocks, chunkData);
                    
                chunksToCreate.Enqueue(chunkData);
            });
        }
    }
    
    private async void StoreChunks()
    {
        if (storeTask == null)
        {
            storeTask = StoreChunk();
            await storeTask;
            storeTask = null;
        }
    }
    
    private async Task StoreChunk()
    {
        if (chunksToStore.TryDequeue(out var chunk))
        {
            await Task.Run(() =>
            {
                chunk.StoreData();
            });
        }
    }
    
    public static bool IsBlockChecks(Vector3i[] positions)
    {
        Block? block;

        foreach (var position in positions)
        {
            int result = Instance.GetBlock(position, out block);

            if (result == 0 || result == 2)
                return true;
        }
        
        return false;
    }
    
    public static bool IsBlockChecks(Vector3i position)
    {
        Block? block;
        
        int result = Instance.GetBlock(position, out block);

        if (result == 0 || result == 2)
            return true;
            
        return false;
    }
    
    public void Delete()
    {
        foreach (var chunk in activeChunks)
        {
            chunk.Value.Delete();
        }
        
        try
        {
            _shaderProgram.Delete();
            _textureArray.Delete();
        }
        catch (Exception e)
        {
            // ignored
        }
    }
}