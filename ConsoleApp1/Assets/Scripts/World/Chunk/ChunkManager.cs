using System.Collections.Concurrent;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public static class ChunkManager
{
    /// <summary>
    /// All the chunks in the world
    /// </summary>
    public static ConcurrentDictionary<Vector3i, Chunk> ActiveChunks = [];
    public static ConcurrentQueue<Chunk> GenerateChunkQueue = [];
    public static ConcurrentQueue<Chunk> PopulateChunkQueue = [];
    public static ConcurrentQueue<Chunk> GenerateMeshQueue = [];
    public static ConcurrentQueue<Chunk> RegenerateMeshQueue = [];
    public static ConcurrentQueue<Chunk> CreateQueue = [];
    public static ConcurrentBag<Vector3i> IgnoreList = [];

    private static readonly object _chunkLock = new object();

    // Chunk merging values
    public static float ChunkInactiveTime = 2; // Amount in seconds to wait before unloading chunks (unload meaning not in range of the player)
    public static float ChunkActivityTime = 5;
    private static double _activityTimer = 0;

    public static float ChunkMergeTime = 3; // Amount in seconds to wait before merging chunks
    private static double _mergingTimer = 0;

    private static float SSBOResizeTime = 60; // Amount in seconds to wait before resizing the SSBO
    private static double _ssboResizeTimer = 0;

    private static List<Chunk> _chunksToMerge = [];

    private static ShaderProgram pullingShader = new ShaderProgram("World/BatchPulling.vert", "World/BatchPulling.frag");
    private static VAO _chunkVao = new VAO();
    private static SSBO<Vector2i> _chunkSSBO = new();
    private static SSBO<ModelUniform> _chunkUniformSSBO = new();
    private static List<DrawArraysIndirectCommand> _chunkIndirectCommands = [];
    private static List<Chunk> _independentChunks = [];

    public static bool TryAddActiveChunk(Chunk chunk)
    {
        if (!ActiveChunks.TryAdd(chunk.GetWorldPosition(), chunk))
            return false;

        ChunkNeighbourCheck(chunk);
        return true;
    }

    public static bool RemoveChunk(Vector3i position, out Chunk chunk)
    {
        chunk = Chunk.Empty;
        if (ActiveChunks.TryRemove(position, out var c))
        {
            chunk = c;
            return true;
        }
        return false;
    }

    public static void ReloadChunks()
    {
        RegenerateMeshQueue.Clear();

        foreach (var chunk in ActiveChunks.Values)
        {
            if (chunk.Stage != ChunkStage.Rendered)
                continue;

            chunk.Reload();
            RegenerateMeshQueue.Enqueue(chunk);
        }
    }

    public static void DisplayChunkBorders()
    {
        Info.ClearBlocks();
        foreach (var chunk in ActiveChunks.Values)
        {
            Vector3 position = chunk.GetWorldPosition();
            InfoBlockData blockData = new InfoBlockData(
                position,
                (32, 32, 32),
                (0, 1, 0, 0.1f)
            );
            Info.AddBlock(blockData);
        }
        Info.UpdateBlocks();
    }

    public static void DisplayChunkBordersNotAllNeighbours()
    {
        Info.ClearBlocks();
        foreach (var chunk in ActiveChunks.Values)
        {
            if (chunk.HasAllNeighbourChunks())
                continue;

            Vector3 position = chunk.GetWorldPosition();
            InfoBlockData blockData = new InfoBlockData(
                position,
                (32, 32, 32),
                (0, 1, 0, 0.1f)
            );
            Info.AddBlock(blockData);
        }
        Info.UpdateBlocks();
    }

    public static void ChunkNeighbourChecks()
    {
        foreach (var chunk in ActiveChunks.Values)
        {
            ChunkNeighbourCheck(chunk);
        }
    }

    public static void ChunkNeighbourCheck(Chunk chunk)
    {
        lock (_chunkLock)
        {
            foreach (var pos in chunk.GetSideChunkPositions())
            {
                if (ActiveChunks.TryGetValue(pos, out var sideChunk))
                {
                    chunk.AddChunk(sideChunk);
                }
            }
        }
    }

    public static void Update()
    {   
        foreach (var chunk in ActiveChunks.Values)
        {
            chunk.Update();
        }

        if (_activityTimer > ChunkActivityTime)
        {
            // Check activity of chunks
            foreach (var chunk in ActiveChunks.Values)
            {
                chunk.ActivityCheck();

                if (chunk.WasInactive() && chunk.IsIndependent())
                {
                    _chunksToMerge.Add(chunk);
                }
                if (chunk.IsActive() && chunk.WasIndependent())
                {
                    _chunksToMerge.Remove(chunk);
                    _independentChunks.Remove(chunk);
                }
            }

            _activityTimer = 0;
        }

        _activityTimer += GameTime.DeltaTime;

        return;

        if (_ssboResizeTimer >= SSBOResizeTime)
        {


            _ssboResizeTimer = 0;
        }

        _ssboResizeTimer += GameTime.DeltaTime;

        if (_mergingTimer >= ChunkMergeTime)
        {
            // Merge chunks
            if (_chunksToMerge.Count > 0)
            {
                for (int i = 0; i < _chunksToMerge.Count; i++)
                {
                    Chunk chunk = _chunksToMerge[i];
                    if (chunk.IsIndependent() && !_independentChunks.Contains(chunk))
                    {
                        _independentChunks.Add(chunk);
                    }
                }

                // Populate SSBO
                int totalSize = _independentChunks.Count;
                int totalDataSize = 0;
                for (int i = 0; i < totalSize; i++)
                {
                    totalDataSize += _independentChunks[i].VertexSSBO.DataCount;
                }

                _chunkSSBO = new SSBO<Vector2i>(new Vector2i[totalDataSize]);
                List<ModelUniform> matrices = [];

                int offset = 0;
                List<Chunk> copy = [.. _independentChunks];
                while (totalSize > 0)
                {
                    int size = Mathf.Min(5, copy.Count);

                    if (size == 5)
                    {
                        Chunk chunk1 = copy[0];
                        Chunk chunk2 = copy[1];
                        Chunk chunk3 = copy[2];
                        Chunk chunk4 = copy[3];
                        Chunk chunk5 = copy[4];

                        int offset1 = chunk1.VertexSSBO.DataCount + offset;
                        int offset2 = chunk2.VertexSSBO.DataCount + offset;
                        int offset3 = chunk3.VertexSSBO.DataCount + offset;
                        int offset4 = chunk4.VertexSSBO.DataCount + offset;
                        int offset5 = chunk5.VertexSSBO.DataCount + offset;

                        matrices.Add(new ModelUniform { model = chunk1.GetModel(), offset = offset1 });
                        matrices.Add(new ModelUniform { model = chunk2.GetModel(), offset = offset2 });
                        matrices.Add(new ModelUniform { model = chunk3.GetModel(), offset = offset3 });
                        matrices.Add(new ModelUniform { model = chunk4.GetModel(), offset = offset4 });
                        matrices.Add(new ModelUniform { model = chunk5.GetModel(), offset = offset5 });

                        DataMerger.Merge(_chunkSSBO, chunk1.VertexSSBO, chunk2.VertexSSBO, chunk3.VertexSSBO, chunk4.VertexSSBO, chunk5.VertexSSBO, offset1, offset2, offset3, offset4, offset5);

                        copy.RemoveRange(0, 5);
                        offset += offset1 + offset2 + offset3 + offset4 + offset5;
                    }
                    else if (size == 4)
                    {
                        Chunk chunk1 = copy[0];
                        Chunk chunk2 = copy[1];
                        Chunk chunk3 = copy[2];
                        Chunk chunk4 = copy[3];

                        int offset1 = chunk1.VertexSSBO.DataCount + offset;
                        int offset2 = chunk2.VertexSSBO.DataCount + offset;
                        int offset3 = chunk3.VertexSSBO.DataCount + offset;
                        int offset4 = chunk4.VertexSSBO.DataCount + offset;

                        matrices.Add(new ModelUniform { model = chunk1.GetModel(), offset = offset1 });
                        matrices.Add(new ModelUniform { model = chunk2.GetModel(), offset = offset2 });
                        matrices.Add(new ModelUniform { model = chunk3.GetModel(), offset = offset3 });
                        matrices.Add(new ModelUniform { model = chunk4.GetModel(), offset = offset4 });

                        DataMerger.Merge(_chunkSSBO, chunk1.VertexSSBO, chunk2.VertexSSBO, chunk3.VertexSSBO, chunk4.VertexSSBO, offset1, offset2, offset3, offset4);

                        copy.RemoveRange(0, 4);
                        offset += offset1 + offset2 + offset3 + offset4;
                    }
                    else if (size == 5)
                    {
                        Chunk chunk1 = copy[0];
                        Chunk chunk2 = copy[1];
                        Chunk chunk3 = copy[2];

                        int offset1 = chunk1.VertexSSBO.DataCount + offset;
                        int offset2 = chunk2.VertexSSBO.DataCount + offset;
                        int offset3 = chunk3.VertexSSBO.DataCount + offset;

                        matrices.Add(new ModelUniform { model = chunk1.GetModel(), offset = offset1 });
                        matrices.Add(new ModelUniform { model = chunk2.GetModel(), offset = offset2 });
                        matrices.Add(new ModelUniform { model = chunk3.GetModel(), offset = offset3 });

                        DataMerger.Merge(_chunkSSBO, chunk1.VertexSSBO, chunk2.VertexSSBO, chunk3.VertexSSBO, offset1, offset2, offset3);

                        copy.RemoveRange(0, 3);
                        offset += offset1 + offset2 + offset3;
                    }
                    else if (size == 5)
                    {
                        Chunk chunk1 = copy[0];
                        Chunk chunk2 = copy[1];

                        int offset1 = chunk1.VertexSSBO.DataCount + offset;
                        int offset2 = chunk2.VertexSSBO.DataCount + offset;

                        matrices.Add(new ModelUniform { model = chunk1.GetModel(), offset = offset1 });
                        matrices.Add(new ModelUniform { model = chunk2.GetModel(), offset = offset2 });

                        DataMerger.Merge(_chunkSSBO, chunk1.VertexSSBO, chunk2.VertexSSBO, offset1, offset2);

                        copy.RemoveRange(0, 2);
                        offset += offset1 + offset2;
                    }
                    else if (size == 5)
                    {
                        Chunk chunk1 = copy[0];

                        int offset1 = chunk1.VertexSSBO.DataCount + offset;

                        matrices.Add(new ModelUniform { model = chunk1.GetModel(), offset = offset1 });

                        DataMerger.Merge(_chunkSSBO, chunk1.VertexSSBO, offset1);

                        copy.RemoveRange(0, 1);
                        offset += offset1;
                    }

                    totalSize -= size;
                }

                _chunkUniformSSBO = new SSBO<ModelUniform>(matrices);
                Console.WriteLine($"Merged {_independentChunks.Count} chunks into one chunk. New size: {_chunkSSBO.DataCount}");
            }

            _chunksToMerge.Clear();
            _mergingTimer = 0;
        }

        _mergingTimer += GameTime.DeltaTime;
    }

    public static void RenderChunks()
    {
        return; 
        if (_chunkSSBO.DataCount == 0)
            return;

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);

        pullingShader.Bind();
        WorldManager._textures.Bind();

        Matrix4 model = Matrix4.Identity;
        Matrix4 projection = Game.camera.projectionMatrix;
        Matrix4 view = Game.camera.viewMatrix;

        int modelLocation = GL.GetUniformLocation(pullingShader.ID, "model");
        int projectionLocation = GL.GetUniformLocation(pullingShader.ID, "projection");
        int viewLocation = GL.GetUniformLocation(pullingShader.ID, "view");

        GL.UniformMatrix4(modelLocation, true, ref model);
        GL.UniformMatrix4(projectionLocation, true, ref projection);
        GL.UniformMatrix4(viewLocation, true, ref view);

        _chunkVao.Bind();
        _chunkSSBO.Bind(0);
        _chunkUniformSSBO.Bind(1);

        GL.DrawArrays(PrimitiveType.Triangles, 0, _chunkSSBO.DataCount * 6);

        _chunkSSBO.Unbind();
        _chunkUniformSSBO.Unbind();
        _chunkVao.Unbind();

        WorldManager._textures.Unbind();
        pullingShader.Unbind();
    }

    

    public static void Clear()
    {
        ActiveChunks.Clear();
        GenerateChunkQueue.Clear();
        PopulateChunkQueue.Clear();
        GenerateMeshQueue.Clear();
        RegenerateMeshQueue.Clear();
        CreateQueue.Clear();
        IgnoreList.Clear();
    }
}

public struct ModelUniform
{
    public Matrix4 model;
    public int offset;
}