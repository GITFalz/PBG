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
using ConsoleApp1.Assets.Scripts.Rendering;
using ConsoleApp1.Engine.Scripts.Core.Data;
using ConsoleApp1.Engine.Scripts.Core.MathLibrary;

namespace ConsoleApp1.Assets.Scripts.World.Chunk;

public class Chunk
{
    public const int WIDTH = 32;
    public const int HEIGHT = 32;
    public const int DEPTH = 32;
    
    public static void GenerateChunk(ref ChunkData chunkData, Vector3i position)
    {
        Block[] blocks = new Block[WIDTH * HEIGHT * DEPTH];
        
        for (var x = 0; x < WIDTH; x++)
        {
            for (var z = 0; z < DEPTH; z++)
            {
                int height = Mathf.FloorToInt(Mathf.Lerp(10, 22, NoiseLib.Noise(((float)x + position.X + 0.001f) / 20f, ((float)z + position.Y + 0.001f) / 20f)));

                for (int y = 0; y < height; y++)
                {
                    blocks[x + z * 32 + y * 1024] = new Block(0, 0);
                }
            }
        }
        
        chunkData = new ChunkData(position);
        chunkData.SetBlocks(blocks);
        chunkData.meshData = new MeshData();
        
        GenerateOcclusion(blocks);
        GenerateMesh(chunkData);
    }

    public static void GenerateOcclusion(Block[] blocks, int width = WIDTH, int lod = 0)
    {
        int index = 0;
        for (int y = 0; y < width; y++)
        {
            for (int z = 0; z < width; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (blocks[index] != null)
                    {
                        byte occlusion = 0;
                            
                        for (int i = 0; i < 6; i++)
                        {
                            if (VoxelData.InBounds(x, y, z, i, width) && blocks[index + VoxelData.IndexOffsetLod[lod, i]] != null)
                                occlusion |= VoxelData.ShiftPosition[i];
                        }
                            
                        blocks[index].occlusion = occlusion;
                    }
                        
                    index++;
                }
            }
        }
    }
    
    public static void GenerateMesh(ChunkData chunkData)
    {
        int index = 0;
        
        for (int y = 0; y < 32; y++)
        {
            for (int z = 0; z < 32; z++)
            {
                for (int x = 0; x < 32; x++)
                {
                    Block block = chunkData.blocks[index];
                    
                    if (block != null)
                    {
                        int[] ids;
                        try
                        {
                            ids = BlockManager.GetBlock(block.blockData).GetUVs();
                        }
                        catch (NullReferenceException e)
                        {
                            return;
                        }
                        
                        for (int side = 0; side < 6; side++)
                        {
                            if (((block.check >> side) & 1) == 0 && ((block.occlusion >> side) & 1) == 0)
                            {
                                block.check |= (byte)(1 << side);
                                
                                for (int tris = 0; tris < 6; tris++)
                                {
                                    chunkData.meshData.tris.Add(VoxelData.TrisIndexTable[tris] + (uint)chunkData.meshData.verts.Count);
                                }
                                
                                int i = index;
                                int loop = VoxelData.FirstLoopBase[side](y, z);
                                int height = 1;
                                int width = 1;
                                while (loop > 0)
                                {
                                    i += VoxelData.FirstOffsetBase[side];
                                    if (chunkData.blocks[i] == null)
                                        break;

                                    if (((chunkData.blocks[i].check >> side) & 1) != 0 ||
                                        ((chunkData.blocks[i].occlusion >> side) & 1) != 0 ||
                                        chunkData.blocks[i].blockData != block.blockData)
                                        break;

                                    chunkData.blocks[i].check |= (byte)(1 << side);

                                    height++;
                                    loop--;
                                }

                                i = index;
                                loop = VoxelData.SecondLoopBase[side](x, z);
                                
                                bool quit = false;
                                
                                while (loop > 0)
                                {
                                    i += VoxelData.SecondOffsetBase[side];
                                    int up = i;
                                    
                                    for (int j = 0; j < height; j++)
                                    {
                                        if (chunkData.blocks[up] == null)
                                        {
                                            quit = true;
                                            break;
                                        }

                                        if (((chunkData.blocks[up].check >> side) & 1) != 0 ||
                                            ((chunkData.blocks[up].occlusion >> side) & 1) != 0 ||
                                            chunkData.blocks[up].blockData != block.blockData)
                                        {
                                            quit = true;
                                            break;
                                        }
                                        
                                        up += VoxelData.FirstOffsetBase[side];
                                    }
                                    
                                    if (quit) break;
                                    
                                    up = i;
                                    
                                    for (int j = 0; j < height; j++) {
                                        chunkData.blocks[up].check |= (byte)(1 << side);
                                        up += VoxelData.FirstOffsetBase[side];
                                    }

                                    width++;
                                    loop--;
                                }

                                Vector3[] positions = VoxelData.PositionOffset[side](width, height);
                                Vector3 position = new Vector3(x, y, z) + chunkData.position;

                                int id = ids[side];
                                
                                chunkData.meshData.uvs.Add(new Vector2(width, height));
                                chunkData.meshData.uvs.Add(new Vector2(width, 0));
                                chunkData.meshData.uvs.Add(new Vector2(0, 0));
                                chunkData.meshData.uvs.Add(new Vector2(0, height));
                                
                                chunkData.meshData.tCoords.Add(id);
                                chunkData.meshData.tCoords.Add(id);
                                chunkData.meshData.tCoords.Add(id);
                                chunkData.meshData.tCoords.Add(id);
                                
                                chunkData.meshData.verts.Add(position + positions[0]);
                                chunkData.meshData.verts.Add(position + positions[1]);
                                chunkData.meshData.verts.Add(position + positions[2]);
                                chunkData.meshData.verts.Add(position + positions[3]);
                            }
                        }
                    }
                    
                    index++;
                }
            }
        }
    }
}