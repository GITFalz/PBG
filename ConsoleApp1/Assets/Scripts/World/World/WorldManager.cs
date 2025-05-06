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

        CWorldOutputNode outputNode = CWorldMultithreadNodeManager.CWorldOutputNode; 
        CWorldSampleNode sampleNode = new CWorldSampleNode()
        {
            Scale = (0.01f, 0.01f),
        };
        outputNode.InputNode = sampleNode;

        CWorldMultithreadNodeManager.AddNode(sampleNode);
        CWorldMultithreadNodeManager.Copy(ThreadPool.ThreadCount);

        ChunkManager.GenerateNearbyPositions();
    }
    
    void Awake()
    {
        Console.WriteLine("World Manager");
        _lastPlayerPosition = (int.MaxValue, int.MaxValue, int.MaxValue);
        ChunkManager.Clear();
        ChunkManager.CheckChunks();
        ChunkManager.UpdateChunkNeighbours();
    }
    
    void Update()
    {
        if (Input.IsKeyAndControlPressed(Keys.B))
        {
            BufferBase.PrintBufferCount();
            Console.WriteLine("There are: " + Chunk.Chunks.Count + " chunks.");
            Console.WriteLine("There are: " + ChunkManager.OpaqueChunks.Count + " opaque chunks.");
            Console.WriteLine("There are: " + ChunkManager.TransparentChunks.Count + " transparent chunks.");
            ChunkPool.Print();
            ChunkManager.Print();
            ThreadPool.Print();
        }

        if (Input.IsKeyAndControlPressed(Keys.R))
        {
            ChunkManager.Reload();
        }

        if (Input.IsKeyAndControlPressed(Keys.C))
        {
            Info.ClearBlocks();
            foreach (var (_, chunk) in ChunkManager.ActiveChunks)
            {
                Info.AddBlock(new InfoBlockData() {
                    Position = chunk.Chunk.GetWorldPosition() + (11, 11, 11),
                    Size = (10, 10, 10),
                    Color = (1, 0, 0, 0.5f)
                });
            }
            Info.UpdateBlocks();
        }

        ChunkManager.HandleRenderDistance();
        ChunkManager.Update();
        ChunkManager.CheckFrustum();
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

        foreach (var (key, chunk) in ChunkManager.OpaqueChunks)
        {   
            model = Matrix4.CreateTranslation(key);
            GL.UniformMatrix4(modelLocation, false, ref model);
            chunk.RenderChunk(); 
        }

        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.DepthMask(false);

        foreach (var (key, chunk) in ChunkManager.TransparentChunks)
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

    public static int GetBlock(Vector3i blockPosition, out Block block, out ChunkStage stage)
    {
        block = Block.Air;
        stage = ChunkStage.Empty;

        Vector3i chunkPosition = VoxelData.ChunkToRelativePosition(VoxelData.BlockToChunkPosition(blockPosition));

        if (!ChunkManager.GetChunk(chunkPosition, out ChunkEntry? entry)) 
            return -1;
        
        block = entry.Chunk.blockStorage.GetBlock(VoxelData.BlockToRelativePosition(blockPosition));
        stage = entry.Stage;
        return block.IsAir() ? 1 : 0;
    }

    public static bool GetBlock(Vector3i blockPosition, out Block block)
    {
        block = Block.Air;
        Vector3i chunkPosition = VoxelData.ChunkToRelativePosition(VoxelData.BlockToChunkPosition(blockPosition));

        if (!ChunkManager.GetChunk(chunkPosition, out Chunk? chunk)) 
            return false;
        
        block = chunk[VoxelData.BlockToRelativePosition(blockPosition)];
        return true;
    }

    public static int GetBlockState(Vector3i blockPosition, out Block block)
    {
        block = Block.Air;
        Vector3i chunkPosition = VoxelData.ChunkToRelativePosition(VoxelData.BlockToChunkPosition(blockPosition));

        if (!ChunkManager.GetChunk(chunkPosition, out Chunk? chunk)) 
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

        if (!ChunkManager.GetChunk(chunkPosition, out Chunk? chunk))
            return false;

        chunkData = chunk;
        return true;
    }


    public static bool SetBlock(Vector3i blockPosition, Block block, out Chunk chunkData)
    {
        chunkData = Chunk.Empty;

        Vector3i chunkPosition = VoxelData.ChunkToRelativePosition(VoxelData.BlockToChunkPosition(blockPosition));

        if (!ChunkManager.GetChunk(chunkPosition, out ChunkEntry? entry))
            return false;

        entry.Chunk.blockStorage.SetBlock(VoxelData.BlockToRelativePosition(blockPosition), block);
        entry.SetStage(ChunkStage.ToBeRendered);
        if (VoxelData.BlockOnBorder(blockPosition, out var offsets))
        {
            foreach (var offset in offsets)
            {
                if (ChunkManager.GetChunk(chunkPosition + offset, out ChunkEntry? newEntry))
                {
                    newEntry.SetStage(ChunkStage.ToBeRendered);
                }
            }
        }
        chunkData = entry.Chunk;
        return true;
    }

    public static bool GetRemoveBlock(Vector3i blockPosition, Block block, out Chunk chunkData, out Block swapped)
    {
        chunkData = Chunk.Empty;
        swapped = Block.Air;

        Vector3i chunkPosition = VoxelData.ChunkToRelativePosition(VoxelData.BlockToChunkPosition(blockPosition));

        if (!ChunkManager.GetChunk(chunkPosition, out ChunkEntry? entry))
            return false;
        
        swapped = entry.Chunk.blockStorage.GetRemoveBlock(VoxelData.BlockToRelativePosition(blockPosition), block);
        entry.SetStage(ChunkStage.ToBeRendered);
        if (VoxelData.BlockOnBorder(blockPosition, out var offsets))
        {
            foreach (var offset in offsets)
            {
                if (ChunkManager.GetChunk(chunkPosition + offset, out ChunkEntry? newEntry))
                {
                    newEntry.SetStage(ChunkStage.ToBeRendered);
                }
            }
        }
        chunkData = entry.Chunk;
        return true;
    }
    
    public static void Delete()
    {
        ChunkManager.Unload();
        CWorldMultithreadNodeManager.Clear();
        Chunk.Chunks = [];

        ChunkManager.OpaqueChunks = [];
        ChunkManager.TransparentChunks = [];

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