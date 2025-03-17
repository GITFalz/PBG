using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class WorldManager : ScriptingNode
{
    public static ShaderProgram _shaderProgram = new ShaderProgram("World/World.vert", "World/World.frag");
    public static ShaderProgram _wireframeShader = new ShaderProgram("World/Wireframe.vert", "World/Wireframe.frag");
    public TextureArray _textures = new TextureArray("Test_TextureAtlas.png", 32, 32);  

    private static RenderType _renderType = RenderType.Solid;
    private Action _render = () => { };

    private int _oldRenderedChunks = 0;

    public Chunk chunkData = new Chunk(RenderType.Solid, new Vector3i(0, 0, 0));
    public ShaderProgram pullingShader = new ShaderProgram("World/Pulling.vert", "World/Pulling.frag");
    
    public WorldManager()
    {
        _render = _renderType == RenderType.Solid ? RenderSolid : RenderWireframe;
    }
    
    public override void Awake()
    {
        Console.WriteLine("World Manager");
        
        ChunkManager.Clear();
        
        SetChunks();
    }

    public void SetRenderType(RenderType type)
    {
        foreach (var chunk in ChunkManager.ActiveChunks)
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
        PopulateChunks();
        GenerateMeshChunks();
        
        if (ChunkManager.CreateQueue.TryDequeue(out var chunk))
        {
            if (ChunkManager.ActiveChunks.ContainsKey(chunk.GetWorldPosition()))
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

        foreach (var (_, chunk) in ChunkManager.ActiveChunks)
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
        
        foreach (var (_, chunk) in ChunkManager.ActiveChunks)
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
        
        foreach (var (_, chunk) in ChunkManager.ActiveChunks)
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

    public HashSet<Vector3i> SetChunks()
    {
        int render = World.renderDistance;

        Vector3i playerChunk = (0, 0, 0);
        Vector2i startPosition = (-1, -1);
        
        HashSet<Vector3i> chunkPositions = new HashSet<Vector3i>();
        
        for (int x = 1; x < render*2+1; x+=2)
        {
            for (int i = 0; i < x; i++)
            {
                startPosition.X++;
                for (int y = -1; y < 4; y++)
                {
                    chunkPositions.Add(((startPosition.X, y, startPosition.Y) + playerChunk) * 32);
                }
            }   

            for (int i = 0; i < x; i++)
            {
                startPosition.Y++;
                for (int y = -1; y < 4; y++)
                {
                    chunkPositions.Add(((startPosition.X, y, startPosition.Y) + playerChunk) * 32);
                }
            }   

            for (int i = 0; i < x; i++)
            {
                startPosition.X--;
                for (int y = -1; y < 4; y++)
                {
                    chunkPositions.Add(((startPosition.X, y, startPosition.Y) + playerChunk) * 32);
                }
            }   

            for (int i = 0; i < x; i++)
            {
                startPosition.Y--;
                for (int y = -1; y < 4; y++)
                {
                    chunkPositions.Add(((startPosition.X, y, startPosition.Y) + playerChunk) * 32);
                }
            }   

            startPosition -= (1, 1);
        }
        
        return chunkPositions;
    }

    public void CheckRenderDistance()
    {
        var chunks = SetChunks();
        
        int render = World.renderDistance;
        Vector3i playerChunk = new Vector3i(0, 0, 0);
        
        HashSet<Vector3i> chunksToRemove = new HashSet<Vector3i>();
        
        foreach (var chunk in ChunkManager.ActiveChunks)
        {
            chunksToRemove.Add(chunk.Key);
        }
        
        foreach (var chunk in chunks)
        {
            chunksToRemove.Remove(chunk);
            
            if (!ChunkManager.ActiveChunks.ContainsKey(chunk) && !ChunkManager.GenerateChunkQueue.Contains(chunk) && ChunkManager.CreateQueue.All(c => c.GetWorldPosition() != chunk))
            {
                ChunkManager.GenerateChunkQueue.Enqueue(chunk);
            }
        }

        ChunkManager.IgnoreList.Clear();

        foreach (var chunk in chunksToRemove)
        {    
            ChunkManager.IgnoreList.Add(chunk);
            if (ChunkManager.ActiveChunks.TryRemove(chunk, out var data))
            {
                data.Clear();
            }
        }
    }

    public static int GetBlock(Vector3i blockPosition, out Block block)
    {
        return GetBlock(blockPosition, out block, out _, out _);
    }

    public static int GetBlock(Vector3i blockPosition, out Block block, out int blockIndex, out int arrayIndex)
    {
        block = Block.Air;
        blockIndex = -1;
        arrayIndex = -1;

        Vector3i chunkPosition = VoxelData.BlockToChunkPosition(blockPosition);

        if (!ChunkManager.ActiveChunks.TryGetValue(chunkPosition, out var chunk)) 
            return -1;
        
        block = chunk.blockStorage.GetBlock(VoxelData.BlockToRelativePosition(blockPosition), out blockIndex, out arrayIndex);
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


    public static bool SetBlock(Vector3i blockPosition, Block block, out Chunk chunkData)
    {
        chunkData = Chunk.Empty;

        Vector3i chunkPosition = VoxelData.BlockToChunkPosition(blockPosition);

        if (!ChunkManager.ActiveChunks.TryGetValue(chunkPosition, out var chunk))
            return false;

        chunk.blockStorage.SetBlock(VoxelData.BlockToRelativePosition(blockPosition), block);
        ChunkManager.RegenerateMeshQueue.Enqueue(chunk);
        return true;
    }


    private void GenerateChunks()
    {
        if (ChunkManager.GenerateChunkQueue.TryDequeue(out var position))
        {
            if (ChunkManager.ActiveChunks.ContainsKey(position) || ChunkManager.IgnoreList.Contains(position)) 
                return;

            Chunk chunkData = new Chunk(_renderType, VoxelData.ChunkToRelativePosition(position));
            bool loaded = chunkData.LoadChunk();
            chunkData.Save = !loaded;
            if (ChunkManager.TryAddActiveChunk(chunkData))
            {
                chunkData.Stage = ChunkStage.Instance;
                ThreadPool.QueueAction(() => GenerateChunk(chunkData, loaded), TaskPriority.High);
            }
        }
    }

    private void RegenerateChunks()
    {
        if (ChunkManager.RegenerateMeshQueue.TryDequeue(out var chunkData))
        {
            Vector3i position = chunkData.GetWorldPosition();
            if (!ChunkManager.ActiveChunks.ContainsKey(position) || ChunkManager.IgnoreList.Contains(position)) 
                return;

            ThreadPool.QueueAction(() => GenerateMeshChunk(chunkData), TaskPriority.Urgent);
        }
    }

    private void PopulateChunks()
    {
        if (ChunkManager.PopulateChunkQueue.TryDequeue(out var chunkData))
        {
            Vector3i position = chunkData.GetWorldPosition();
            if (!ChunkManager.ActiveChunks.ContainsKey(position) || ChunkManager.IgnoreList.Contains(position)) 
                return;

            ThreadPool.QueueAction(() => PopulateChunk(chunkData), TaskPriority.High);
        }
    }

    private void GenerateMeshChunks()
    {
        if (ChunkManager.GenerateMeshQueue.TryDequeue(out var chunkData))
        {
            Vector3i position = chunkData.GetWorldPosition();
            if (!ChunkManager.ActiveChunks.ContainsKey(position) || ChunkManager.IgnoreList.Contains(position)) 
                return;

            ThreadPool.QueueAction(() => GenerateMeshChunk(chunkData), TaskPriority.Normal);
        }
    }

    private static async Task GenerateChunk(Chunk chunkData, bool loaded, bool create = true)
    {
        await Task.Run(() =>
        {
            if (!loaded)
            {
                ChunkGenerator.GenerateChunk(ref chunkData, chunkData.GetWorldPosition());
                chunkData.Stage = ChunkStage.Generated;
                if (chunkData.AllNeighbourChunkStageSuperiorOrEqual(ChunkStage.Generated))
                {
                    ChunkManager.PopulateChunkQueue.Enqueue(chunkData);
                    chunkData.Save = true;
                }

                foreach (var chunk in chunkData.GetNeighbourChunks())
                {
                    if (chunk.Stage == ChunkStage.Generated && chunk.AllNeighbourChunkStageSuperiorOrEqual(ChunkStage.Generated))
                    {
                        ChunkManager.PopulateChunkQueue.Enqueue(chunk);
                        chunk.Save = true;
                    }
                }
            } 
            else
            {
                chunkData.Stage = ChunkStage.Populated;
                ChunkManager.GenerateMeshQueue.Enqueue(chunkData);
                chunkData.Save = false;
            }
        });
    }

    private static async Task PopulateChunk(Chunk chunkData)
    {
        await Task.Run(() =>
        {
            ChunkGenerator.PopulateChunk(ref chunkData);
            chunkData.Stage = ChunkStage.Populated;
            ChunkManager.GenerateMeshQueue.Enqueue(chunkData);
        });
    }

    private static async Task GenerateMeshChunk(Chunk chunkData)
    {
        await Task.Run(() =>
        {
            if (!chunkData.Loaded) ChunkGenerator.GenerateOcclusion(chunkData);
            ChunkGenerator.GenerateMesh(chunkData);
            chunkData.Stage = ChunkStage.Rendered;
            ChunkManager.CreateQueue.Enqueue(chunkData);
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
        foreach (var (_, chunk) in ChunkManager.ActiveChunks)
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