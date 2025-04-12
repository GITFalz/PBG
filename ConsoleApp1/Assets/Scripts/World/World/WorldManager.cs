using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Veldrid;

public class WorldManager : ScriptingNode
{
    public static ShaderProgram _wireframeShader = new ShaderProgram("World/Wireframe.vert", "World/Wireframe.frag");

    private static RenderType _renderType = RenderType.Solid;
    private Action _render = () => { };

    private int _oldRenderedChunks = 0;

    public ShaderProgram DepthPrePassShader = new ShaderProgram("World/Pulling.vert");
    public WorldShader WorldShader = new WorldShader();

    public FBO DepthPrepassFBO;
    public Matrix4 viewMatrix;
    public Matrix4 projectionMatrix;

    private Vector3i _currentPlayerChunk = Vector3i.Zero;
    private Vector3i _lastPlayerPosition = (int.MaxValue, int.MaxValue, int.MaxValue);

    public static bool DoAmbientOcclusion = true;
    public static bool DoRealtimeShadows = false;

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
            Game.Camera.SCREEN_WIDTH / zoom,
            Game.Camera.SCREEN_HEIGHT / zoom, 
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
        if (Input.IsKeyPressed(Keys.U))
            ChunkManager.Clear();

        if (Input.IsKeyAndControlPressed(Keys.R))
            ChunkManager.ReloadChunks();

        if (Input.IsKeyAndControlPressed(Keys.B))
            BufferBase.PrintBufferCount();
        
        if (Input.IsKeyAndControlPressed(Keys.I))
        {
            Vector3i chunkPosition = VoxelData.BlockToChunkPosition(Game.Camera.GetCameraMode() == CameraMode.Follow ? PlayerData.Position : Game.Camera.Position);

            if (ChunkManager.ActiveChunks.TryGetValue(chunkPosition, out var chunk1))
            {
                Info.AddBlock(new InfoBlockData(
                    chunkPosition + (4, 4, 4),
                    (24, 24, 24),
                    chunk1.Stage == ChunkStage.Rendered ? (0, 1, 0, 0.3f) : (chunk1.Stage == ChunkStage.Populated ? (0, 0, 1, 0.3f) : (1, 0, 0, 0.3f))
                ));

                foreach (var c in chunk1.GetNeighbourChunks())
                {
                    Info.AddBlock(new InfoBlockData(
                    c.GetWorldPosition() + (8, 8, 8),
                    (16, 16, 16),
                    c.Stage == ChunkStage.Rendered ? (0, 1, 0, 0.3f) : (c.Stage == ChunkStage.Populated ? (0, 0, 1, 0.3f) : (1, 0, 0, 0.3f))
                    ));

                }
                Info.UpdateBlocks();
            }
        }

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

    public void Reload()
    {

    }

    public override void Render()
    {
        _render.Invoke();
    }

    public void RenderSolid()
    {    
        Camera camera = Game.Camera;
        
        int renderCount = 0;
        Info.VertexCount = 0;

        Matrix4 lightView = Matrix4.Identity;
        Matrix4 lightProjection = Matrix4.Identity;

        GL.Disable(EnableCap.StencilTest);
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);
        GL.Enable(EnableCap.CullFace);

        if (DoRealtimeShadows)
        {
            lightView = GetViewMatrix();
            lightProjection = GetProjectionMatrix();

            // Render depthPrepass
            GL.ColorMask(false, false, false, false);

            DepthPrepassFBO.Bind();

            GL.ClearColor(0.5f, 0.5f, 0.5f, 1.0f);
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

            DepthPrePassShader.Bind();

            Matrix4 v = GetViewMatrix();
            Matrix4 p = GetProjectionMatrix();

            int modelLocation = GL.GetUniformLocation(DepthPrePassShader.ID, "model");
            int viewLocation = GL.GetUniformLocation(DepthPrePassShader.ID, "view");
            int projectionLocation = GL.GetUniformLocation(DepthPrePassShader.ID, "projection");

            GL.UniformMatrix4(viewLocation, true, ref v);
            GL.UniformMatrix4(projectionLocation, true, ref p);

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
        }

        Matrix4 model = Matrix4.Identity; 
        WorldShader.Bind();

        WorldShader.UniformGeneral();

        //DepthPrepassFBO.BindDepthTexture(TextureUnit.Texture1);

        foreach (var (_, chunk) in ChunkManager.ActiveChunks)
        {   
            if (chunk.IsDisabled)
                continue;

            renderCount++;
            Info.VertexCount += chunk.VertexCount;
            model = Matrix4.CreateTranslation(chunk.GetWorldPosition());
            WorldShader.UniformModel(ref model);
            chunk.Render.Invoke();
        }

        Shader.Error("After Render End0");

        model = Matrix4.Identity;
        WorldShader.UniformModel(ref model);

        Shader.Error("After Render End1");
        
        WorldShader.Unbind();

        //DepthPrepassFBO.UnbindTexture(TextureUnit.Texture1);

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
        Camera camera = Game.Camera;


        camera.CalculateFrustumPlanes();
        
        foreach (var (_, chunk) in ChunkManager.ActiveChunks)
        {
            chunk.IsDisabled = !chunk.HasBlocks || !camera.FrustumIntersects(chunk.boundingBox) || chunk.Stage != ChunkStage.Rendered;// || chunk.IsIndependent(); // testing
        }
    }

    public void RenderWireframe()
    {
        Camera camera = Game.Camera;
        
        int renderCount = 0;
        Info.VertexCount = 0;

        GL.ColorMask(false, false, false, false);
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);
        GL.Enable(EnableCap.CullFace);

        DepthPrePassShader.Bind();

        Matrix4 view = camera.ViewMatrix;
        Matrix4 projection = camera.ProjectionMatrix;

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
        
        view = camera.ViewMatrix;
        projection = camera.ProjectionMatrix;

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

        // Chunks in world position
        var chunks = SetChunks();
        int render = World.renderDistance;
        
        HashSet<Vector3i> chunksToRemove = new HashSet<Vector3i>();
        
        foreach (var chunk in ChunkManager.ActiveChunks)
        {
            chunksToRemove.Add(chunk.Key);
        }

        List<Vector3i> chunksToAdd = new List<Vector3i>();
        
        foreach (var chunk in chunks)
        {
            chunksToRemove.Remove(chunk);
            chunksToAdd.Add(chunk);
        }

        foreach (var chunk in chunksToRemove)
        {
            ChunkManager.RemoveChunk(chunk, out var oldChunk);
        }

        foreach (var chunk in chunksToAdd)
        {
            if (ChunkManager.TryAddActiveChunk(chunk, out var newChunk))
            {
                newChunk.Stage = ChunkStage.Instance;
                ChunkManager.GenerateChunkQueue.Enqueue(newChunk);
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
        while (ChunkManager.GenerateChunkQueue.TryDequeue(out var chunk))
        {
            bool loaded = ChunkLoader.IsChunkStored(chunk);
            chunk.Save = !loaded;
            ThreadPool.QueueAction(() => GenerateChunk(chunk, loaded), TaskPriority.Low);
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
            if (!ChunkManager.ActiveChunks.ContainsKey(position)) 
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
                if (ChunkGenerator.GenerateChunk(ref chunkData, chunkData.GetWorldPosition()) == -1)
                    return;
                chunkData.Stage = ChunkStage.Generated;

                if (chunkData.AllNeighbourChunkStageSuperiorOrEqual(ChunkStage.Generated))
                {
                    ChunkManager.PopulateChunkQueue.Enqueue(chunkData);
                    chunkData.Save = true;
                }

                foreach (var chunk in chunkData.GetNeighbourChunks())
                {
                    //Info.ClearBlocks();
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
            if (ChunkGenerator.PopulateChunk(ref chunkData) == -1)
                return;
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
            if (ChunkGenerator.GenerateOcclusion(chunkData) == -1)
                return;
            if (ChunkGenerator.GenerateMesh(chunkData) == -1)
                return;

            chunkData.Stage = ChunkStage.Rendered;
            Console.WriteLine($"Chunk {chunkData.GetWorldPosition()} generated");
            ChunkManager.CreateQueue.Enqueue(chunkData);
        });
    }
    
    public static bool IsBlockChcks(Vector3i[] positions)
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

public static class ThreadBlocker
{
    public static bool isBlocked = false;

    public static void Block()
    {
        isBlocked = true;
        while (isBlocked)
        {
            Thread.Sleep(1);
        }
    }

    public static void Unblock()
    {
        isBlocked = false;
    }
}