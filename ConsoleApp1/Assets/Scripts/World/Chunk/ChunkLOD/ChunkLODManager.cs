using System.Collections.Concurrent;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public static class ChunkLODManager
{
    public static LODChunkGrid[] LODChunks = [];
    public static List<LODChunk> Chunks = [];
    public static List<LODChunk> OpaqueChunks = [];
    public static ConcurrentQueue<LODChunk> ChunksToCreateMesh = [];

    public static object lockObject = new object();

    public static void Initialize(int x, int y, int z, int startX, int startY, int startZ, int resolution)
    {
        int scale = (int)Mathf.Pow(2, resolution) * 32;
        LODChunks = new LODChunkGrid[x * y * z];
        int index = 0;
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                for (int k = 0; k < z; k++)
                {
                    Vector3i position = new Vector3i(startX + i, startY + j, startZ + k) * scale;
                    LODChunkGrid newGrid = new LODChunkGrid(position, scale, resolution - 1);
                    LODChunks[index] = newGrid;
                    index++;
                }
            }
        }
    }

    public static void CheckChunkResolution(Vector3i position)
    {
        for (int i = 0; i < LODChunks.Length; i++)
        {
            LODChunkGrid chunk = LODChunks[i];
            chunk.UpdateResolution(position);
        }

        for (int i = 0; i < LODChunks.Length; i++)
        {
            LODChunkGrid chunk = LODChunks[i];
            chunk.GenerateChunk();
        }
    }

    public static void CheckFrustum()
    {
        Camera camera = Game.Camera;

        Info.ChunkCount = 0;
        Info.VertexCount = 0;

        foreach (var chunkData in Chunks)
        {
            ChunkMesh chunk = chunkData.Mesh;

            bool frustum = camera.FrustumIntersects(chunkData.boundingBox);
            bool baseDisabled = !frustum || chunk.BlockRendering;

            bool isDisabled = chunk.IsDisabled;
            chunk.IsDisabled = baseDisabled || !chunk.HasBlocks;

            if (!isDisabled && chunk.IsDisabled)
            {
                OpaqueChunks.Remove(chunkData);
            }
            else if (isDisabled && !chunk.IsDisabled)
            {
                OpaqueChunks.Add(chunkData);
            }

            /*
            bool isTransparentDisabled = chunk.IsTransparentDisabled;
            chunk.IsTransparentDisabled = baseDisabled || !chunk.HasTransparentBlocks;

            if (!isTransparentDisabled && chunk.IsTransparentDisabled)
            {
                TransparentChunks.Remove(chunkData);
            }
            else if (isTransparentDisabled && !chunk.IsTransparentDisabled)
            {
                TransparentChunks.TryAdd(chunk.GetWorldPosition(), chunk);
            }

            if (chunk.IsDisabled && chunk.IsTransparentDisabled)
                continue;
                */
        }
    }

    public static void Update()
    {
        if (!ChunksToCreateMesh.IsEmpty)
        {
            while (ChunksToCreateMesh.TryDequeue(out LODChunk? chunk))
            {
                chunk.Mesh.CreateChunkSolid();
                Chunks.Add(chunk);
            }
        }

        CheckFrustum();
    }

    public static void RenderChunks()
    {
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);
        GL.Enable(EnableCap.CullFace);

        Matrix4 projection = Game.Camera.GetProjectionMatrix(300f, 10000f);
        Matrix4 view = Game.Camera.ViewMatrix;

        WorldManager.newTestShader.Bind();

        int modelLocation = WorldManager.newTestShader.GetLocation("model");
        int viewLocation = WorldManager.newTestShader.GetLocation("view");
        int projectionLocation = WorldManager.newTestShader.GetLocation("projection");
        int textureArrayLocation = WorldManager.newTestShader.GetLocation("textureArray");

        GL.UniformMatrix4(viewLocation, false, ref view);
        GL.UniformMatrix4(projectionLocation, false, ref projection);
        GL.Uniform1(textureArrayLocation, 0);

        WorldShader.Textures.Bind(TextureUnit.Texture0);

        GL.DepthMask(true);
        GL.Disable(EnableCap.Blend);

        for (int i = 0; i < OpaqueChunks.Count; i++)
        {
            LODChunk chunk = OpaqueChunks[i];
            if (chunk.Mesh.IsDisabled || !chunk.Mesh.HasBlocks)
                continue;

            chunk.RenderChunk(modelLocation);
        }

        /*
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.DepthMask(false);

        foreach (var (key, chunk) in ChunkManager.TransparentChunks)
        {   
            model = Matrix4.CreateTranslation(key);
            GL.UniformMatrix4(modelLocation, false, ref model);
            chunk.RenderChunkTransparent(); 
        }
        */

        WorldShader.Textures.Unbind();

        WorldManager.newTestShader.Unbind();
    }

    public static void Clear()
    {
        foreach (var chunk in LODChunks)
        {
            chunk.Clear();
        }
        LODChunks = [];
        Chunks = [];
        OpaqueChunks = [];
        ChunksToCreateMesh = [];
    }
}