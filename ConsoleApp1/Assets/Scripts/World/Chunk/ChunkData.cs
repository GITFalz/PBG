    using System.Diagnostics;
    using ConsoleApp1.Assets.Scripts.World.Blocks;
    using OpenTK.Graphics.OpenGL4;
    using OpenTK.Mathematics;

    namespace ConsoleApp1.Assets.Scripts.World.Chunk;

    public class ChunkData
    {
        public Vector3i position;
        public Block[]? blocks;
        public MeshData meshData;
        public ChunkData[] sideChunks;
        public Bounds bounds;
        
        public VAO chunkVao;
        public VBO vertVBO;
        public VBO uvVBO;
        public VBO textureIndexVBO;
        public IBO chunkIbo;

        public ChunkData(Vector3i position)
        {
            this.position = position;
            
            Vector3 min = new Vector3(position.X, position.Y, position.Z);
            Vector3 max = min + new Vector3(Chunk.WIDTH, Chunk.HEIGHT, Chunk.DEPTH);
            
            bounds = new Bounds(min, max);
        }

        public void SetBlocks(Block[] newBlocks)
        {
            blocks ??= new Block[newBlocks.Length];
                    
            for (int i = 0; i < newBlocks.Length; i++)
            {
                blocks[i] = newBlocks[i];
            }
        }

        public void Clear()
        {
            blocks = null;
            meshData.Clear();
            sideChunks = null;
        }
        
        public bool isEmpty()
        {
            foreach (var b in blocks)
            {
                if (b != null)
                    return false;
            }

            return true;
        }
        
        public static ChunkData operator +(ChunkData a, ChunkData b)
        {
            for (int i = 0; i < b.blocks.Length; i++)
            {
                if (b.blocks[i] != null && a.blocks[i] == null)
                    a.blocks[i] = b.blocks[i];
            }

            return a;
        }

        public void Delete()
        {
            chunkVao.Delete();
            vertVBO.Delete();
            uvVBO.Delete();
            textureIndexVBO.Delete();
            chunkIbo.Delete();
            
            meshData.Clear();
            blocks = null;
        }
        
        public void CreateChunk()
        {
            chunkVao = new VAO();
            
            vertVBO = new VBO(meshData.verts);
            uvVBO = new VBO(meshData.uvs);
            textureIndexVBO = new VBO(meshData.tCoords);
            
            chunkVao.LinkToVAO(0, 3, vertVBO);
            chunkVao.LinkToVAO(1, 2, uvVBO);
            chunkVao.LinkToVAO(2, 1, textureIndexVBO);
            
            chunkIbo = new IBO(meshData.tris);
        }
        
        public void RenderChunk()
        {
            chunkVao.Bind();
            chunkIbo.Bind();
            
            GL.DrawElements(PrimitiveType.Triangles, meshData.tris.Count, DrawElementsType.UnsignedInt, 0);
            
            chunkIbo.Unbind();
            chunkVao.Unbind();
        }
    }

    public struct Bounds
    {
        public Vector3 Min;
        public Vector3 Max;

        public Bounds(Vector3 min, Vector3 max)
        {
            Min = min;
            Max = max;
        }
    }