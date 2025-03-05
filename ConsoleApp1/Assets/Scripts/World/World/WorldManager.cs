using System.Collections.Concurrent;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ErrorCode = OpenTK.Graphics.OpenGL4.ErrorCode;

public class WorldManager : ScriptingNode
{
    public static WorldManager? Instance;
    
    public HashSet<Vector3i> chunks;
    
    public static ConcurrentDictionary<Vector3i, ChunkData> activeChunks;
    public static ConcurrentQueue<Vector3i> chunksToGenerate;
    public static ConcurrentQueue<ChunkData> chunksToCreate;
    public static ConcurrentQueue<ChunkData> chunksToStore;

    private static ConcurrentBag<Vector3i> chunksToIgnore;
    
    private Task?[] currentTask = new Task?[4];
    private Task?[] storeTask = new Task?[6];
    
    public ChunkRegionData regionData;
    
    
    public static ShaderProgram _shaderProgram = new ShaderProgram("World/World.vert", "World/World.frag");
    public static ShaderProgram _wireframeShader = new ShaderProgram("World/Wireframe.vert", "World/Wireframe.frag");
    public Texture _textureArray = new Texture("Test_TextureAtlas.png");

    private static RenderType _renderType = RenderType.Solid;
    private Action _render = () => { };

    private int _oldRenderedChunks = 0;




    public ChunkData chunkData = new ChunkData(RenderType.Solid, new Vector3i(0, 0, 0));
    public ShaderProgram pullingShader = new ShaderProgram("World/Pulling.vert", "World/Pulling.frag");
    
    public WorldManager()
    {
        Instance ??= this;
        
        chunks = [];
        
        activeChunks = [];
        chunksToGenerate = [];
        chunksToCreate = [];
        chunksToStore = [];
        chunksToIgnore = [];
        
        regionData = new ChunkRegionData(new Vector3i(0, 0, 0));
        regionData.SaveChunk(new Vector3i(0, 1, 0), new ChunkData(_renderType, (0, 1, 0)));
        regionData.SaveChunk(new Vector3i(1, 1, 0), new ChunkData(_renderType, (1, 1, 0)));
    
        _render = _renderType == RenderType.Solid ? RenderSolid : RenderWireframe;
    }
    
    public override void Awake()
    {
        Console.WriteLine("World Manager");
        
        activeChunks.Clear();
        chunksToGenerate.Clear();
        chunksToCreate.Clear();
        chunksToStore.Clear();
        chunksToIgnore.Clear();
        
        SetChunks();
    }

    public void SetRenderType(RenderType type)
    {
        foreach (var chunk in activeChunks)
        {
            chunk.Value.SetRenderType(type);
        }

        _render = type == RenderType.Solid ? RenderSolid : RenderWireframe;
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
                chunk.CreateChunk.Invoke();
            }
            else
            {
                chunksToGenerate.Enqueue(chunk.position);
                chunk.Clear();
            }
        }

        CheckFrustum();
    }

    public override void Render()
    {
        Timer.DisplayTime("Render Start");
        _render.Invoke();
        Timer.DisplayTime("Render End");
    }

    public void RenderSolid()
    {    
        Camera camera = Game.camera;

        int renderCount = 0;
        Info.VertexCount = 0;

        //Shader.Error("Before Render Start");

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);

        //Shader.Error("Before Bind");

        pullingShader.Bind();
        _textureArray.Bind();

        //Shader.Error("After Bind");

        Matrix4 view = camera.viewMatrix;
        Matrix4 projection = camera.projectionMatrix;

        int modelLocationA = GL.GetUniformLocation(pullingShader.ID, "model");
        int viewLocationA = GL.GetUniformLocation(pullingShader.ID, "view");
        int projectionLocationA = GL.GetUniformLocation(pullingShader.ID, "projection");
        int camPosLocationA = GL.GetUniformLocation(pullingShader.ID, "camPos");
        
        GL.UniformMatrix4(viewLocationA, true, ref view);
        GL.UniformMatrix4(projectionLocationA, true, ref projection);
        GL.Uniform3(camPosLocationA, camera.Position); 

        foreach (var (_, chunk) in activeChunks)
        {   
            if (chunk.IsDisabled)
                continue;

            renderCount++;
            Info.VertexCount += chunk.VertexData.Count;
            Matrix4 model = Matrix4.CreateTranslation(chunk.position);
            GL.UniformMatrix4(modelLocationA, true, ref model);
            chunk.Render.Invoke();

            //Shader.Error("After Render");
        }

        //Shader.Error("Before Unbind");
        
        pullingShader.Unbind();
        _textureArray.Unbind();

        //Shader.Error("After Unbind");

        if (renderCount != _oldRenderedChunks)
        {
            Info.ChunkRenderingText.SetText($"Chunks: {renderCount}").GenerateChars().UpdateText();
            _oldRenderedChunks = renderCount;
        }

        Shader.Error("After Render End");
    }

    public void CheckFrustum()
    {
        Camera camera = Game.camera;

        camera.CalculateFrustumPlanes();
        
        foreach (var (_, chunk) in activeChunks)
        {
            chunk.IsDisabled = !chunk.HasBlocks || !camera.FrustumIntersects(chunk.boundingBox);
        }
    }

    public void RenderWireframe()
    {
        Camera camera = Game.camera;
        
        int renderCount = 0;
        Info.VertexCount = 0;

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        
        _wireframeShader.Bind();
        
        Matrix4 view = camera.viewMatrix;
        Matrix4 projection = camera.projectionMatrix;

        int modelLocation = GL.GetUniformLocation(_wireframeShader.ID, "model");
        int viewLocation = GL.GetUniformLocation(_wireframeShader.ID, "view");
        int projectionLocation = GL.GetUniformLocation(_wireframeShader.ID, "projection");
        
        GL.UniformMatrix4(viewLocation, true, ref view);
        GL.UniformMatrix4(projectionLocation, true, ref projection);
        
        foreach (var (_, chunk) in activeChunks)
        {   
            if (chunk.IsDisabled)
                continue;

            renderCount++;
            Info.VertexCount += chunk.VertexData.Count;
            Matrix4 model = Matrix4.CreateTranslation(chunk.position);
            GL.UniformMatrix4(modelLocation, true, ref model);
            chunk.Render.Invoke();
        }
        
        _wireframeShader.Unbind();
        
        GL.Disable(EnableCap.DepthTest);
        GL.Disable(EnableCap.CullFace);

        if (renderCount != _oldRenderedChunks)
        {
            Info.ChunkRenderingText.SetText($"Chunks: {renderCount}").GenerateChars().UpdateText();
            _oldRenderedChunks = renderCount;
        }
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
            for (int y = 0; y < 4; y++)
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
        for (int i = 0; i < 2; i++)
        {
            Task? task = currentTask[i];
            if (task == null)
            {
                task = GenerateChunk();
                await task;
                currentTask[i] = null;
            }
        }
    }

    private static async Task GenerateChunk()
    {
        if (chunksToGenerate.TryDequeue(out var position))
        {
            if (activeChunks.ContainsKey(position) || chunksToIgnore.Contains(position)) return;

            ChunkData chunkData = new ChunkData(_renderType, position);

            await Task.Run(() =>
            {
                Block[] blocks;
                
                Chunk.GenerateChunk(ref chunkData, position);
                //Chunk.GenerateBox(ref chunkData, position, new Vector3i(20, 10, 20), new Vector3i(10, 10, 10));
                
                blocks = chunkData.blockStorage.GetFullBlockArray();
                blocks = Chunk.GenerateOcclusion(chunkData, blocks);
                Chunk.GenerateMesh(blocks, chunkData);
                    
                chunksToCreate.Enqueue(chunkData);
            });
        }
    }
    
    private async void StoreChunks()
    {
        return;
        for (int i = 0; i < 6; i++)
        {
            Task? task = storeTask[i];
            if (task == null)
            {
                task = StoreChunk();
                await task;
                storeTask[i] = null;
            }
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
        foreach (var (_, chunk) in activeChunks)
        {
            chunk.Delete();
        }
        
        try
        {
            _shaderProgram.Delete();
            _wireframeShader.Delete();
            _textureArray.Delete();
        }
        catch (Exception e)
        {
            // ignored
        }
    }
}