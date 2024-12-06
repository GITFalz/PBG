using ConsoleApp1.Assets.Scripts.World.Blocks;
using ConsoleApp1.Engine.Scripts.Core.Graphics;
using ConsoleApp1.Engine.Scripts.Core.Rendering;
using ConsoleApp1.Engine.Scripts.Core.Voxel;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleApp1.Engine.Scripts.Core.Data;
using ConsoleApp1.Engine.Scripts.Core.MathLibrary;

namespace ConsoleApp1.Assets.Scripts.World.Chunk;

public class Chunk
{
    public const int WIDTH = 32;
    public const int HEIGHT = 32;
    public const int DEPTH = 32;
    
    public VoxelManager voxelManager;

    public VAO chunkVao;
    public VBO vertVBO;
    public VBO uvVBO;
    public VBO textureIndexVBO;
    public IBO chunkIbo;
    
    public void GenerateChunk(Vector3 position)
    {
        voxelManager = new VoxelManager();
        
        for (var x = 0; x < WIDTH; x++)
        {
            for (var z = 0; z < DEPTH; z++)
            {
                int height = Mathf.FloorToInt(Mathf.Lerp(10, 22, NoiseLib.Noise(((float)x + position.X + 0.001f) / 20f, ((float)z + position.Y + 0.001f) / 20f)));

                for (int y = 0; y < height; y++)
                {
                    Vector3 blockPos = new Vector3(x + position.X, y + position.Y, z + position.Z);
                    
                    if (y + 1 == height)
                        voxelManager.GenerateBlock(blockPos, 0);
                    if (y + 3 >= height)
                        voxelManager.GenerateBlock(blockPos, 1);
                    else
                        voxelManager.GenerateBlock(blockPos, 2);
                }
            }
        }
    }

    public void RenderChunk()
    {
        chunkVao = new VAO();
        
        vertVBO = new VBO(voxelManager.vertices);
        uvVBO = new VBO(voxelManager.uvs2D);
        textureIndexVBO = new VBO(voxelManager.texIndex);
        
        chunkVao.LinkToVAO(0, 3, vertVBO);
        chunkVao.LinkToVAO(1, 2, uvVBO);
        chunkVao.LinkToVAO(2, 1, textureIndexVBO);
        
        chunkIbo = new IBO(voxelManager.indices);
    }
}