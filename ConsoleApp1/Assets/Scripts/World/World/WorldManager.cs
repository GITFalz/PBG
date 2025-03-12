using System.Collections.Concurrent;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ErrorCode = OpenTK.Graphics.OpenGL4.ErrorCode;

public class WorldManager : ScriptingNode
{
    public static HashSet<Vector3i> chunks = [];
    
    public static Dictionary<Vector3i, ChunkData> activeChunks = [];
    public static ConcurrentQueue<Vector3i> chunksToGenerate = [];
    public static ConcurrentQueue<ChunkData> chunksToRegenerate = [];
    public static ConcurrentQueue<ChunkData> chunksToCreate = [];
    public static ConcurrentQueue<ChunkData> chunksToStore = [];

    private static ConcurrentBag<Vector3i> chunksToIgnore = [];
    
    
    public static ShaderProgram _shaderProgram = new ShaderProgram("World/World.vert", "World/World.frag");
    public static ShaderProgram _wireframeShader = new ShaderProgram("World/Wireframe.vert", "World/Wireframe.frag");
    public Texture _textures = new Texture("Test_TextureAtlas.png");
    

    private static RenderType _renderType = RenderType.Solid;
    private Action _render = () => { };

    private int _oldRenderedChunks = 0;




    public ChunkData chunkData = new ChunkData(RenderType.Solid, new Vector3i(0, 0, 0));
    public ShaderProgram pullingShader = new ShaderProgram("World/Pulling.vert", "World/Pulling.frag");
    
    public WorldManager()
    {
        chunks = [];  
        activeChunks = [];
        chunksToGenerate = [];
        chunksToCreate = [];
        chunksToStore = [];
        chunksToIgnore = [];
    
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
        RegenerateChunks();
        
        if (chunksToCreate.TryDequeue(out var chunk))
        {
            if (activeChunks.TryAdd(chunk.GetWorldPosition(), chunk) || activeChunks.ContainsKey(chunk.GetWorldPosition()))
            {
                chunk.SaveChunk();
                chunk.CreateChunk.Invoke();
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

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);

        pullingShader.Bind();
        _textures.Bind();

        Matrix4 model = Matrix4.Identity;
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
            Info.VertexCount += chunk.GridAlignedFaces.Count;
            model = Matrix4.CreateTranslation(chunk.GetWorldPosition());
            GL.UniformMatrix4(modelLocationA, true, ref model);
            chunk.Render.Invoke();
        }

        Shader.Error("After Render End0");

        model = Matrix4.Identity;
        GL.UniformMatrix4(modelLocationA, true, ref model);
        //LodBase.Render();

        Shader.Error("After Render End1");
        
        pullingShader.Unbind();
        _textures.Unbind();

        Shader.Error("After Render End2");

        if (renderCount != _oldRenderedChunks)
        {
            Info.ChunkRenderingText.SetText($"Chunks: {renderCount}").GenerateChars().UpdateText();
            _oldRenderedChunks = renderCount;
        }

        Shader.Error("After Render End3");
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
            Info.VertexCount += chunk.GridAlignedFaces.Count;
            Matrix4 model = Matrix4.CreateTranslation(chunk.GetWorldPosition());
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
                    chunkPositions.Add(new Vector3i(playerChunk.X + x, y, playerChunk.Z + z) * 32);
                }
            }
        }
        
        chunks = chunkPositions;
    }

    public void CheckRenderDistance()
    {
        SetChunks();
        
        int render = World.renderDistance;
        Vector3i playerChunk = new Vector3i(0, 0, 0);
        
        HashSet<Vector3i> chunksToRemove = new HashSet<Vector3i>();
        
        foreach (var chunk in activeChunks)
        {
            chunksToRemove.Add(chunk.Key);
        }
        
        foreach (var chunk in chunks)
        {
            chunksToRemove.Remove(chunk);
            
            if (!activeChunks.ContainsKey(chunk) && !chunksToGenerate.Contains(chunk) && chunksToCreate.All(c => c.GetWorldPosition() != chunk))
            {
                chunksToGenerate.Enqueue(chunk);
            }
        }
        
        chunksToIgnore.Clear();
        
        foreach (var chunk in chunksToRemove)
        {
            chunksToIgnore.Add(chunk);
            
            if (activeChunks.Remove(chunk, out var data))
            {
                data.Clear();
            }
        }
    }

    public static int GetBlock(Vector3i blockPosition, out Block block)
    {
        block = Block.Air;
        Vector3i chunkPosition = VoxelData.BlockToChunkPosition(blockPosition);

        if (!activeChunks.TryGetValue(chunkPosition, out var chunk)) return chunks.Contains(chunkPosition) ? 2 : 1;
        
        block = chunk.blockStorage.GetBlock(VoxelData.BlockToRelativePosition(blockPosition));
        return block.IsAir() ? 1 : 0;
    }

    public static bool GetBlock(Vector3i blockPosition)
    {
        GetBlock(blockPosition, out var block);
        return block.IsSolid();
    }

    public static bool GetBlocks(Vector3i min, Vector3i max, out List<(Vector3i, Block)> blocks)
    {
        blocks = [];
        Vector3i position;

        for (int x = min.X; x <= max.X; x++)
        {
            for (int y = min.Y; y <= max.Y; y++)
            {
                for (int z = min.Z; z <= max.Z; z++)
                {
                    position = (x, y, z);
                    int result = GetBlock(position, out var block);
                    if (result == 0)
                        blocks.Add((position, block));
                }
            }
        }

        return blocks.Count > 0;
    }


    public static bool SetBlock(Vector3i blockPosition, Block block, out ChunkData chunkData)
    {
        chunkData = ChunkData.Empty;

        Vector3i chunkPosition = VoxelData.BlockToChunkPosition(blockPosition);

        if (!activeChunks.TryGetValue(chunkPosition, out var chunk) || !chunks.Contains(chunkPosition))
            return false;

        chunk.blockStorage.SetBlock(VoxelData.BlockToRelativePosition(blockPosition), block);
        chunksToRegenerate.Enqueue(chunk);
        return true;
    }


    private void GenerateChunks()
    {
        if (chunksToGenerate.TryDequeue(out var position))
        {
            if (activeChunks.ContainsKey(position) || chunksToIgnore.Contains(position)) 
                return;

            ChunkData chunkData = new ChunkData(_renderType, VoxelData.ChunkToRelativePosition(position));
            bool loaded = chunkData.LoadChunk();
            chunkData.Save = !loaded;
            ThreadPool.QueueAction(() => GenerateChunk(chunkData, loaded));
        }
    }

    private void RegenerateChunks()
    {
        if (chunksToRegenerate.TryDequeue(out var chunkData))
        {
            Vector3i position = chunkData.GetWorldPosition();
            if (!activeChunks.ContainsKey(position) || chunksToIgnore.Contains(position)) 
                return;

            chunkData.Save = true;

            ThreadPool.QueueAction(() => GenerateChunk(chunkData, true));
        }
    }

    private static async Task GenerateChunk(ChunkData chunkData, bool loaded, bool create = true)
    {
        await Task.Run(() =>
        {
            Block[] blocks;

            if (!loaded) Chunk.GenerateChunk(ref chunkData, chunkData.GetWorldPosition());

            blocks = chunkData.blockStorage.GetFullBlockArray();
            blocks = Chunk.GenerateOcclusion(chunkData, blocks);
            Chunk.GenerateMesh(blocks, chunkData);
                
            chunksToCreate.Enqueue(chunkData);
        });
    }
    
    public static bool IsBlockChecks(Vector3i[] positions)
    {
        Block block;

        foreach (var position in positions)
        {
            int result = GetBlock(position, out block);

            if (result == 0 || result == 2)
                return true;
        }
        
        return false;
    }
    
    public static bool IsBlockChecks(Vector3i position)
    {
        Block block;
        
        int result = GetBlock(position, out block);

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
            _textures.Delete();
        }
        catch (Exception e)
        {
            // ignored
        }
    }
}