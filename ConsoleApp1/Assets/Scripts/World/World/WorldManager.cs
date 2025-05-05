using System.Collections.Concurrent;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Veldrid;

public class WorldManager : ScriptingNode
{
    public static ShaderProgram _wireframeShader = new ShaderProgram("World/Wireframe.vert", "World/Wireframe.frag");
    public static ShaderProgram newTestShader = new ShaderProgram("World/World.vert", "World/World.frag");   

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

    public static Dictionary<Vector3i, Chunk> OpaqueChunks = new Dictionary<Vector3i, Chunk>();
    public static Dictionary<Vector3i, Chunk> TransparentChunks = new Dictionary<Vector3i, Chunk>();

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

        CWorldOutputNode outputNode = CWorldMultithreadNodeManager.CWorldOutputNode; 
        CWorldSampleNode sampleNode = new CWorldSampleNode()
        {
            Scale = (0.01f, 0.01f),
        };
        outputNode.InputNode = sampleNode;

        CWorldMultithreadNodeManager.AddNode(sampleNode);

        CWorldMultithreadNodeManager.Copy(ThreadPool.ThreadCount);
    }
    
    void Awake()
    {
        Console.WriteLine("World Manager");
        _lastPlayerPosition = (int.MaxValue, int.MaxValue, int.MaxValue);
        ChunkManager.Clear();
    }
    
    void Update()
    {
        if (Input.IsKeyAndControlPressed(Keys.J) && GetChunk(VoxelData.BlockToChunkPosition(Game.Camera.Position), out var cc))
            cc._ibo.ReadData();

        if (Input.IsKeyAndControlPressed(Keys.K) && GetChunk(VoxelData.BlockToChunkPosition(Game.Camera.Position), out cc))
            cc.VertexVBO.ReadData();

        if (Input.IsKeyAndControlPressed(Keys.R))
            //ChunkManager.ReloadChunks();

        if (Input.IsKeyAndControlPressed(Keys.B))
        {
            BufferBase.PrintBufferCount();
            Console.WriteLine("There are: " + Chunk.Chunks.Count + " chunks.");
            ChunkPool.Print();
            ChunkManager.Print();
            ThreadPool.Print();
        }

        GenerateMeshChunks();
        RegenerateChunks();
        PopulateChunks();
        GenerateChunks();
        CheckRenderDistance();
        CheckFrustum();
    }

    void Render()
    {
        _render.Invoke();
    }

    public void RenderSolid()
    {    
        Camera camera = Game.Camera;

        Matrix4 lightView = Matrix4.Identity;
        Matrix4 lightProjection = Matrix4.Identity;

        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);
        GL.Enable(EnableCap.CullFace);

        /*
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
        */

        Matrix4 model = Matrix4.Identity; 
        Matrix4 projection = camera.ProjectionMatrix;
        Matrix4 view = camera.ViewMatrix;

        newTestShader.Bind();

        int modelLocation = newTestShader.GetLocation("model");
        int viewLocation = newTestShader.GetLocation("view");
        int projectionLocation = newTestShader.GetLocation("projection");
        int textureArrayLocation = newTestShader.GetLocation("textureArray");

        GL.UniformMatrix4(viewLocation, false, ref view);
        GL.UniformMatrix4(projectionLocation, false, ref projection);
        GL.Uniform1(textureArrayLocation, 0);

        Shader.Error("Setting uniforms: ");

        WorldShader.Textures.Bind(TextureUnit.Texture0);

        GL.DepthMask(true);
        GL.Disable(EnableCap.Blend);

        foreach (var (key, chunk) in OpaqueChunks)
        {   
            model = Matrix4.CreateTranslation(key);
            GL.UniformMatrix4(modelLocation, false, ref model);
            chunk.RenderChunk(); 
        }

        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.DepthMask(false);

        foreach (var (key, chunk) in TransparentChunks)
        {   
            model = Matrix4.CreateTranslation(key);
            GL.UniformMatrix4(modelLocation, false, ref model);
            chunk.RenderChunkTransparent(); 
        }

        WorldShader.Textures.Unbind();

        newTestShader.Unbind();

        Shader.Error("After Render End: ");

        model = Matrix4.Identity;
        //WorldShader.UniformModel(ref model);
        
        //WorldShader.Unbind();

        //DepthPrepassFBO.UnbindTexture(TextureUnit.Texture1);

    }

    public void CheckFrustum()
    {
        Camera camera = Game.Camera;
        camera.CalculateFrustumPlanes();
        
        int renderCount = 0;
        Info.VertexCount = 0;
        
        foreach (var (_, chunkEntry) in ChunkManager.ActiveChunks)
        {
            var chunk = chunkEntry.Chunk;

            bool frustum = camera.FrustumIntersects(chunk.boundingBox);
            bool baseDisabled = chunkEntry.Stage != ChunkStage.Rendered || !frustum || chunk.BlockRendering;

            bool isDisabled = chunk.IsDisabled;
            chunk.IsDisabled = baseDisabled || !chunk.HasBlocks;

            if (!isDisabled && chunk.IsDisabled)
            {
                OpaqueChunks.Remove(chunk.GetWorldPosition());
            }
            else if (isDisabled && !chunk.IsDisabled)
            {
                OpaqueChunks.TryAdd(chunk.GetWorldPosition(), chunk);
            }

            bool isTransparentDisabled = chunk.IsTransparentDisabled;
            chunk.IsTransparentDisabled = baseDisabled || !chunk.HasTransparentBlocks;

            if (!isTransparentDisabled && chunk.IsTransparentDisabled)
            {
                TransparentChunks.Remove(chunk.GetWorldPosition());
            }
            else if (isTransparentDisabled && !chunk.IsTransparentDisabled)
            {
                TransparentChunks.TryAdd(chunk.GetWorldPosition(), chunk);
            }

            if (chunk.IsDisabled && chunk.IsTransparentDisabled)
                continue;

            renderCount++;
            Info.VertexCount += chunk.VertexCount;
        }

        if (renderCount != _oldRenderedChunks)
        {
            Info.SetGlobalChunkInfo(renderCount, Info.VertexCount);
            _oldRenderedChunks = renderCount;
        }
    }

    public void RenderWireframe()
    {
        /*
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
            Info.VertexCount += (int)chunk.VertexCount;
            Matrix4 model = Matrix4.CreateTranslation(chunk.GetWorldPosition());
            GL.UniformMatrix4(modelLocation, true, ref model);
            chunk.Render.Invoke();
        }
        
        _wireframeShader.Unbind();
        */
    }

    void Exit()
    {
        Delete();
    }

    public void CheckRenderDistance()
    {
        _currentPlayerChunk = VoxelData.ChunkToRelativePosition(VoxelData.BlockToChunkPosition(Mathf.FloorToInt(PlayerData.Position)));
        _currentPlayerChunk.Y = 0;

        if (_currentPlayerChunk == _lastPlayerPosition) 
            return;

        // Chunks in world position
        var chunks = SetChunks();
        int render = World.renderDistance;

        //ChunkManager.GenerateChunkQueue = [];
        
        HashSet<Vector3i> chunksToRemove = [];
        foreach (var chunk in ChunkManager.ActiveChunks)
        {
            chunksToRemove.Add(chunk.Key);
        }

        HashSet<Vector3i> chunksToAdd = [];
        foreach (var chunk in chunks)
        {
            chunksToAdd.Add(chunk);
            chunksToRemove.Remove(chunk);
        }

        foreach (var chunk in chunksToRemove)
        {
            if (ChunkManager.RemoveChunk(chunk, out var oldChunk))
            {
                OpaqueChunks.Remove(oldChunk.GetWorldPosition());
                TransparentChunks.Remove(oldChunk.GetWorldPosition());
            }
        }

        foreach (var chunk in chunksToAdd)
        {
            if (ChunkManager.TryAddActiveChunk(chunk, out var newChunk) && newChunk.Stage == ChunkStage.Empty)
            {
                newChunk.Stage = ChunkStage.Instance;
                ChunkManager.GenerateChunkQueue.Enqueue(newChunk);
            }
        }

        ChunkManager.UpdateChunkNeighbours();

        _lastPlayerPosition = _currentPlayerChunk;
    }

    public static int GetBlock(Vector3i blockPosition, out Block block, out ChunkStage stage)
    {
        block = Block.Air;
        stage = ChunkStage.Empty;

        Vector3i chunkPosition = VoxelData.BlockToChunkPosition(blockPosition);

        if (!ChunkManager.ActiveChunks.TryGetValue(chunkPosition, out var chunk)) 
            return -1;
        
        block = chunk.blockStorage.GetBlock(VoxelData.BlockToRelativePosition(blockPosition));
        stage = chunk.Stage;
        return block.IsAir() ? 1 : 0;
    }

    public static bool GetBlock(Vector3i blockPosition, out Block block)
    {
        block = Block.Air;
        Vector3i chunkPosition = VoxelData.BlockToChunkPosition(blockPosition);

        if (!ChunkManager.ActiveChunks.TryGetValue(chunkPosition, out var chunk)) 
            return false;
        
        block = chunk[VoxelData.BlockToRelativePosition(blockPosition)];
        return true;
    }

    public static int GetBlockState(Vector3i blockPosition, out Block block)
    {
        block = Block.Air;
        Vector3i chunkPosition = VoxelData.BlockToChunkPosition(blockPosition);

        if (!ChunkManager.ActiveChunks.TryGetValue(chunkPosition, out var chunk)) 
            return -1;
        
        block = chunk[VoxelData.BlockToRelativePosition(blockPosition)];
        return block.IsAir() ? 1 : 0;
    }

    public static bool GetBlock(Vector3i blockPosition)
    {
        GetBlockState(blockPosition, out var block);
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
                    int result = GetBlockState(position, out var block);
                    if (result == 0)
                        blocks.Add((position, block));
                }
            }
        }

        return blocks.Count > 0;
    }

    public static bool GetChunk(Vector3i chunkPosition, out Chunk chunkData)
    {
        chunkData = Chunk.Empty;

        if (!ChunkManager.ActiveChunks.TryGetValue(chunkPosition, out var chunk))
            return false;

        chunkData = chunk;
        return true;
    }


    public static bool SetBlock(Vector3i blockPosition, Block block, out Chunk chunkData)
    {
        chunkData = Chunk.Empty;

        Vector3i chunkPosition = VoxelData.BlockToChunkPosition(blockPosition);

        if (!ChunkManager.ActiveChunks.TryGetValue(chunkPosition, out var chunk))
            return false;

        chunk.blockStorage.SetBlock(VoxelData.BlockToRelativePosition(blockPosition), block);
        ChunkManager.ReloadChunk(chunk);
        return true;
    }

    public static bool GetRemoveBlock(Vector3i blockPosition, Block block, out Chunk chunkData, out Block swapped)
    {
        chunkData = Chunk.Empty;
        swapped = Block.Air;

        Vector3i chunkPosition = VoxelData.BlockToChunkPosition(blockPosition);

        if (!ChunkManager.ActiveChunks.TryGetValue(chunkPosition, out var chunk))
            return false;
        
        swapped = chunk.blockStorage.GetRemoveBlock(VoxelData.BlockToRelativePosition(blockPosition), block);
        ChunkManager.ReloadChunk(chunk);
        return true;
    }


    private void GenerateChunks()
    {
        int processedThisFrame = 0;
        int maxPerFrame = ThreadPool.ThreadCount;

        while (processedThisFrame < maxPerFrame && ChunkManager.GenerateChunkQueue.TryDequeue(out var chunk))
        {
            bool loaded = ChunkLoader.IsChunkStored(chunk);
            chunk.Save = !loaded;

            if ((int)chunk.Stage >= (int)ChunkStage.Generated)
                continue;

            ChunkGenerationProcess chunkGenerationProcess = new ChunkGenerationProcess(chunk, loaded);
            ThreadPool.QueueAction(chunkGenerationProcess, TaskPriority.Low);

            processedThisFrame++;
        }
    }

    private void RegenerateChunks()
    {
        while (ChunkManager.RegenerateMeshQueue.TryDequeue(out var chunk))
        {
            Vector3i position = chunk.GetWorldPosition();
            if (!ChunkManager.ActiveChunks.ContainsKey(position))// || ChunkManager.IgnoreList.Contains(position)) 
                return;

            ThreadPool.QueueAction(() => GenerateMeshChunk(chunk), TaskPriority.Urgent);
        }
    }

    private void PopulateChunks()
    {
        while (ChunkManager.WaitingToPopulateQueue.TryDequeue(out var chunk))
        {
            ChunkManager.PopulateChunk(chunk);

            foreach (var c in chunk.GetNeighbourChunks())
            {
                ChunkManager.PopulateChunk(c);
            }
        }

        while (ChunkManager.PopulateChunkQueue.TryDequeue(out var chunk))
        {
            Vector3i position = chunk.GetWorldPosition();
            if (!ChunkManager.ActiveChunks.ContainsKey(position) || ChunkManager.IgnoreList.Contains(position)) 
            {
                Console.WriteLine($"Deleting chunf from populate");
                chunk.Delete();
                return;
            }

            if (chunk.Stage >= ChunkStage.Populated)
                continue;

            ThreadPool.QueueAction(() => PopulateChunk(chunk), TaskPriority.Normal);
        }
    }

    private void GenerateMeshChunks()
    {
        while (ChunkManager.GenerateMeshQueue.TryDequeue(out var chunk))
        {
            Vector3i position = chunk.GetWorldPosition();
            if (!ChunkManager.ActiveChunks.ContainsKey(position)) 
                return;

            if (chunk.Stage >= ChunkStage.Rendered)
                continue;

            ThreadPool.QueueAction(() => GenerateMeshChunk(chunk), TaskPriority.High);
        }
    }

    private static void PopulateChunk(Chunk chunk)
    {
        if (ChunkGenerator.PopulateChunk(ref chunk) == -1)
            return;

        chunk.Stage = ChunkStage.Populated;
        ChunkManager.GenerateMeshQueue.Enqueue(chunk);
        //chunkData.SaveChunk();
        chunk.Save = false;
        ChunkManager.RemoveBeingPopulated(chunk);
    }

    private static void GenerateMeshChunk(Chunk chunkData)
    {
        if (ChunkGenerator.GenerateOcclusion(chunkData) == -1)
            return;

        if (ChunkGenerator.GenerateMesh(chunkData) == -1)
            return;

        ChunkManager.CreateQueue.Enqueue(chunkData);
    }
    
    public static bool IsBlockChcks(Vector3i[] positions)
    {
        Block block;

        foreach (var position in positions)
        {
            int result = GetBlockState(position, out block);

            if (result == 0 || result == 2)
                return true;
        }
        
        return false;
    }
    
    public static bool IsBlockChecks(Vector3i position)
    {
        Block block;
        
        int result = GetBlockState(position, out block);

        if (result == 0 || result == 2)
            return true;
            
        return false;
    }
    
    public static void Delete()
    {
        ChunkManager.Unload();
        CWorldMultithreadNodeManager.Clear();
        Chunk.Chunks = [];

        OpaqueChunks = [];
        TransparentChunks = [];

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
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