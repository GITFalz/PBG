using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class WorldManager : ScriptingNode
{
    public static ShaderProgram _shaderProgram = new ShaderProgram("World/World.vert", "World/World.frag");
    public static ShaderProgram _wireframeShader = new ShaderProgram("World/Wireframe.vert", "World/Wireframe.frag");
    public static TextureArray _textures = new TextureArray("Test_TextureAtlas.png", 32, 32);  

    private static RenderType _renderType = RenderType.Solid;
    private Action _render = () => { };

    private int _oldRenderedChunks = 0;

    public Chunk chunkData = new Chunk(RenderType.Solid, new Vector3i(0, 0, 0));
    public ShaderProgram DepthPrePassShader = new ShaderProgram("World/Pulling.vert");
    public ShaderProgram pullingShader = new ShaderProgram("World/Pulling.vert", "World/Pulling.frag");

    public FBO DepthPrepassFBO;
    public Matrix4 viewMatrix;
    public Matrix4 projectionMatrix;

    private Vector3i _currentPlayerChunk = Vector3i.Zero;
    private Vector3i _lastPlayerPosition = (int.MaxValue, int.MaxValue, int.MaxValue);


    public Matrix4 GetViewMatrix()
    {
        //viewMatrix = Matrix4.LookAt(Position, Position + front, up);
        viewMatrix = Matrix4.LookAt((10, 100, 10), (0, 90, 0), (0, 1, 0));
        return viewMatrix;
    }
    
    public Matrix4 GetProjectionMatrix()
    {
        float zoom = 10;
        Matrix4.CreateOrthographic(
            Game.camera.SCREEN_WIDTH / zoom,
            Game.camera.SCREEN_HEIGHT / zoom, 
            -200f, 
            200f,
            out projectionMatrix
        );
        return projectionMatrix;

        /*
        projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(
            OpenTK.Mathematics.MathHelper.DegreesToRadians(FOV),
            SCREEN_WIDTH / SCREEN_HEIGHT, 
            0.1f, 
            1000f
        );
        return projectionMatrix;
        */
    }

    
    public WorldManager()
    {
        _render = _renderType == RenderType.Solid ? RenderSolid : RenderWireframe;
        DepthPrepassFBO = new FBO(Game.Width, Game.Height);
        /*
        // 1000 blocks
        for (int i = 0; i < 1000; i++)
        {
            InfoBlockData blockData = new InfoBlockData
            {
                Position = (i * 25, 0, 0),
                Size = (20, 20, 20),
                // light green
                Color = (0.5f, 1, 0.5f, 0.5f),
            };
            
            Info.AddBlock(blockData);
        }
        Info.UpdateBlocks();
        */
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
                chunk.CreateChunk.Invoke();
            }
        }
        //ChunkManager.Update();
        CheckFrustum();
    }

    public override void Render()
    {
        _render.Invoke();
    }

    public void RenderSolid()
    {    
        Camera camera = Game.camera;

        int renderCount = 0;
        Info.VertexCount = 0;

        // Render depthPrepass
        GL.ColorMask(false, false, false, false);
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);
        GL.Enable(EnableCap.CullFace);

        DepthPrepassFBO.Bind();

        GL.ClearColor(0.5f, 0.5f, 0.5f, 1.0f);
        GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

        DepthPrePassShader.Bind();

        Matrix4 view = GetViewMatrix();
        Matrix4 projection = GetProjectionMatrix();

        int modelLocation = GL.GetUniformLocation(DepthPrePassShader.ID, "model");
        int viewLocation = GL.GetUniformLocation(DepthPrePassShader.ID, "view");
        int projectionLocation = GL.GetUniformLocation(DepthPrePassShader.ID, "projection");

        GL.UniformMatrix4(viewLocation, true, ref view);
        GL.UniformMatrix4(projectionLocation, true, ref projection);

        foreach (var (_, chunk) in ChunkManager.ActiveChunks)
        {   
            if (chunk.IsDisabled)
                continue;

            Matrix4 m = Matrix4.CreateTranslation(chunk.GetWorldPosition());
            GL.UniformMatrix4(modelLocation, true, ref m);
            chunk.Render.Invoke();
        }

        DepthPrePassShader.Unbind();

        DepthPrepassFBO.Unbind();



        // Render terrain
        GL.ColorMask(true, true, true, true);
        GL.Disable(EnableCap.DepthTest);

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);

        pullingShader.Bind();

        Matrix4 model = Matrix4.Identity;
        view = camera.viewMatrix;
        projection = camera.projectionMatrix;
        Matrix4 lightView = GetViewMatrix();
        Matrix4 lightProjection = GetProjectionMatrix();

        int modelLocationA = GL.GetUniformLocation(pullingShader.ID, "model");
        int viewLocationA = GL.GetUniformLocation(pullingShader.ID, "view");
        int projectionLocationA = GL.GetUniformLocation(pullingShader.ID, "projection");
        //int camPosLocationA = GL.GetUniformLocation(pullingShader.ID, "camPos");
        int lighViewLocationA = GL.GetUniformLocation(pullingShader.ID, "lightView");
        int lightProjectionLocationA = GL.GetUniformLocation(pullingShader.ID, "lightProjection");
        
        GL.UniformMatrix4(viewLocationA, true, ref view);
        GL.UniformMatrix4(projectionLocationA, true, ref projection);
        //GL.Uniform3(camPosLocationA, camera.Position); 
        GL.UniformMatrix4(lighViewLocationA, true, ref lightView);
        GL.UniformMatrix4(lightProjectionLocationA, true, ref lightProjection);
        GL.Uniform1(GL.GetUniformLocation(pullingShader.ID, "textureArray"), 0);
        GL.Uniform1(GL.GetUniformLocation(pullingShader.ID, "shadowMap"), 1); 

        _textures.Bind();
        DepthPrepassFBO.BindDepthTexture(TextureUnit.Texture1);

        foreach (var (_, chunk) in ChunkManager.ActiveChunks)
        {   
            if (chunk.IsDisabled)
                continue;

            renderCount++;
            Info.VertexCount += chunk.VertexCount;
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
        DepthPrepassFBO.UnbindTexture(TextureUnit.Texture1);

        ChunkManager.RenderChunks();

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
        if (!camera.IsMoving())
            return;

        camera.CalculateFrustumPlanes();
        
        foreach (var (_, chunk) in ChunkManager.ActiveChunks)
        {
            chunk.IsDisabled = !chunk.HasBlocks;// || !camera.FrustumIntersects(chunk.boundingBox);// || chunk.IsIndependent(); // testing
        }
    }

    public void RenderWireframe()
    {
        Camera camera = Game.camera;
        
        int renderCount = 0;
        Info.VertexCount = 0;

        GL.ColorMask(false, false, false, false);
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);
        GL.Enable(EnableCap.CullFace);

        DepthPrePassShader.Bind();

        Matrix4 view = camera.viewMatrix;
        Matrix4 projection = camera.projectionMatrix;

        int modelLocation = GL.GetUniformLocation(DepthPrePassShader.ID, "model");
        int viewLocation = GL.GetUniformLocation(DepthPrePassShader.ID, "view");
        int projectionLocation = GL.GetUniformLocation(DepthPrePassShader.ID, "projection");

        GL.UniformMatrix4(viewLocation, true, ref view);
        GL.UniformMatrix4(projectionLocation, true, ref projection);

        foreach (var (_, chunk) in ChunkManager.ActiveChunks)
        {   
            if (chunk.IsDisabled)
                continue;

            Matrix4 model = Matrix4.CreateTranslation(chunk.GetWorldPosition());
            GL.UniformMatrix4(modelLocation, true, ref model);
            chunk.Render.Invoke();
        }

        DepthPrePassShader.Unbind();

        GL.ColorMask(true, true, true, true);
        GL.Disable(EnableCap.DepthTest);
        
        _wireframeShader.Bind();
        
        view = camera.viewMatrix;
        projection = camera.projectionMatrix;

        modelLocation = GL.GetUniformLocation(_wireframeShader.ID, "model");
        viewLocation = GL.GetUniformLocation(_wireframeShader.ID, "view");
        projectionLocation = GL.GetUniformLocation(_wireframeShader.ID, "projection");
        
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

        Vector2i startPosition = (-1, -1);
        
        HashSet<Vector3i> chunkPositions = new HashSet<Vector3i>();
        
        for (int x = 1; x < render*2+1; x+=2)
        {
            for (int i = 0; i < x; i++)
            {
                startPosition.X++;
                for (int y = -1; y < 4; y++)
                {
                    chunkPositions.Add(new Vector3i(startPosition.X, y, startPosition.Y) * 32 + _currentPlayerChunk);
                }
            }   

            for (int i = 0; i < x; i++)
            {
                startPosition.Y++;
                for (int y = -1; y < 4; y++)
                {
                    chunkPositions.Add(new Vector3i(startPosition.X, y, startPosition.Y) * 32 + _currentPlayerChunk);
                }
            }   

            for (int i = 0; i < x; i++)
            {
                startPosition.X--;
                for (int y = -1; y < 4; y++)
                {
                    chunkPositions.Add(new Vector3i(startPosition.X, y, startPosition.Y) * 32 + _currentPlayerChunk);
                }
            }   

            for (int i = 0; i < x; i++)
            {
                startPosition.Y--;
                for (int y = -1; y < 4; y++)
                {
                    chunkPositions.Add(new Vector3i(startPosition.X, y, startPosition.Y) * 32 + _currentPlayerChunk);
                }
            }   

            startPosition -= (1, 1);
        }
        
        return chunkPositions;
    }

    public void CheckRenderDistance()
    {
        _currentPlayerChunk = VoxelData.BlockToChunkPosition(Mathf.FloorToInt(PlayerData.Position));
        _currentPlayerChunk.Y = 0;

        if (_currentPlayerChunk == _lastPlayerPosition) 
            return;

        Console.WriteLine($"Render Distance: {_currentPlayerChunk}");

        var chunks = SetChunks();
        int render = World.renderDistance;
        
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

        _lastPlayerPosition = _currentPlayerChunk;
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
        while (ChunkManager.GenerateChunkQueue.TryDequeue(out var position))
        {
            if (ChunkManager.ActiveChunks.ContainsKey(position) || ChunkManager.IgnoreList.Contains(position)) 
                return;

            Chunk chunkData = new Chunk(_renderType, VoxelData.ChunkToRelativePosition(position));
            bool loaded = ChunkLoader.IsChunkStored(chunkData);
            chunkData.Save = !loaded;
            if (ChunkManager.TryAddActiveChunk(chunkData))
            {
                chunkData.Stage = ChunkStage.Instance;
                ThreadPool.QueueAction(() => GenerateChunk(chunkData, loaded), TaskPriority.Low);
            }
        }
    }

    private void RegenerateChunks()
    {
        while (ChunkManager.RegenerateMeshQueue.TryDequeue(out var chunkData))
        {
            Vector3i position = chunkData.GetWorldPosition();
            if (!ChunkManager.ActiveChunks.ContainsKey(position))// || ChunkManager.IgnoreList.Contains(position)) 
                return;

            ThreadPool.QueueAction(() => GenerateMeshChunk(chunkData), TaskPriority.Urgent);
        }
    }

    private void PopulateChunks()
    {
        while (ChunkManager.PopulateChunkQueue.TryDequeue(out var chunkData))
        {
            Vector3i position = chunkData.GetWorldPosition();
            if (!ChunkManager.ActiveChunks.ContainsKey(position) || ChunkManager.IgnoreList.Contains(position)) 
                return;

            ThreadPool.QueueAction(() => PopulateChunk(chunkData), TaskPriority.Normal);
        }
    }

    private void GenerateMeshChunks()
    {
        while (ChunkManager.GenerateMeshQueue.TryDequeue(out var chunkData))
        {
            Vector3i position = chunkData.GetWorldPosition();
            if (!ChunkManager.ActiveChunks.ContainsKey(position) || ChunkManager.IgnoreList.Contains(position)) 
                return;

            ThreadPool.QueueAction(() => GenerateMeshChunk(chunkData), TaskPriority.High);
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
                chunkData.LoadChunk();
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
            //chunkData.SaveChunk();
            chunkData.Save = false;
        });
    }

    private static async Task GenerateMeshChunk(Chunk chunkData)
    {
        await Task.Run(() =>
        {
            ChunkGenerator.GenerateOcclusion(chunkData);
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
    }
}