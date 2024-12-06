using ConsoleApp1.Assets.Scripts.World.Blocks;
using OpenTK.Mathematics;

namespace ConsoleApp1.Engine.Scripts.Core.Voxel;

public class VoxelManager
{
    public List<Vector3> vertices { get; set; }
    public List<Vector3> uvs { get; set; }
    public List<uint> indices { get; set; }
    public List<Vector2> uvs2D { get; set; }
    public List<int> texIndex { get; set; }
    
    public VoxelManager()
    {
        vertices = new List<Vector3>();
        uvs = new List<Vector3>();
        indices = new List<uint>();
        uvs2D = new List<Vector2>();
        texIndex = new List<int>();
    }

    public void GenerateBlock(Vector3 origin, int blockId)
    {
        BlockData blockData = BlockManager.GetBlockData(blockId);
        if (blockData.blockType == BlockType.AIR) return;
        
        GenerateFrontFace(origin);
        GenerateRightFace(origin);
        GenerateTopFace(origin);
        GenerateLeftFace(origin);
        GenerateBottomFace(origin);
        GenerateBackFace(origin);
        
        for (int i = 0; i < 6; i++)
        {
            SetFaceIndex(_textureIndexes[blockData.blockType][i]);
        }
    }
    
    public void GenerateFrontFace(Vector3 origin)
    {
        Vector3 a = origin + VoxelData.voxelVerts[0][0];
        Vector3 b = origin + VoxelData.voxelVerts[0][1];
        Vector3 c = origin + VoxelData.voxelVerts[0][2];
        Vector3 d = origin + VoxelData.voxelVerts[0][3];
        
       GenerateBaseFace(a, b, c, d);
    }
    
    public void GenerateBackFace(Vector3 origin)
    {
        Vector3 a = origin + VoxelData.voxelVerts[5][0];
        Vector3 b = origin + VoxelData.voxelVerts[5][1];
        Vector3 c = origin + VoxelData.voxelVerts[5][2];
        Vector3 d = origin + VoxelData.voxelVerts[5][3];
        
        GenerateBaseFace(a, b, c, d);
    }
    
    public void GenerateTopFace(Vector3 origin)
    {
        Vector3 a = origin + VoxelData.voxelVerts[2][0];
        Vector3 b = origin + VoxelData.voxelVerts[2][1];
        Vector3 c = origin + VoxelData.voxelVerts[2][2];
        Vector3 d = origin + VoxelData.voxelVerts[2][3];
        
        GenerateBaseFace(a, b, c, d);
    }
    
    public void GenerateBottomFace(Vector3 origin)
    {
        Vector3 a = origin + VoxelData.voxelVerts[4][0];
        Vector3 b = origin + VoxelData.voxelVerts[4][1];
        Vector3 c = origin + VoxelData.voxelVerts[4][2];
        Vector3 d = origin + VoxelData.voxelVerts[4][3];
        
        GenerateBaseFace(a, b, c, d);
    }
    
    public void GenerateLeftFace(Vector3 origin)
    {
        Vector3 a = origin + VoxelData.voxelVerts[3][0];
        Vector3 b = origin + VoxelData.voxelVerts[3][1];
        Vector3 c = origin + VoxelData.voxelVerts[3][2];
        Vector3 d = origin + VoxelData.voxelVerts[3][3];
        
        GenerateBaseFace(a, b, c, d);
    }
    
    public void GenerateRightFace(Vector3 origin)
    {
        Vector3 a = origin + VoxelData.voxelVerts[1][0];
        Vector3 b = origin + VoxelData.voxelVerts[1][1];
        Vector3 c = origin + VoxelData.voxelVerts[1][2];
        Vector3 d = origin + VoxelData.voxelVerts[1][3];
        
        GenerateBaseFace(a, b, c, d);
    }

    public void GenerateBaseFace(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        indices.Add((uint)vertices.Count);
        indices.Add((uint)vertices.Count + 1);
        indices.Add((uint)vertices.Count + 2);
        indices.Add((uint)vertices.Count + 2);
        indices.Add((uint)vertices.Count + 3);
        indices.Add((uint)vertices.Count);
        
        vertices.Add(a);
        vertices.Add(b);
        vertices.Add(c);
        vertices.Add(d);
        
        uvs2D.Add(new Vector2(1, 1));
        uvs2D.Add(new Vector2(1, 0));
        uvs2D.Add(new Vector2(0, 0));
        uvs2D.Add(new Vector2(0, 1));
    }
    
    public void SetFaceIndex(int textureIndex)
    {
        texIndex.Add(textureIndex);
        texIndex.Add(textureIndex);
        texIndex.Add(textureIndex);
        texIndex.Add(textureIndex);
    }
    
    private readonly Dictionary<BlockType, int[]> _textureIndexes = new Dictionary<BlockType, int[]>()
    {
        {BlockType.GRASS, new int[] { 0, 0, 1, 0, 2, 0 }},
        {BlockType.DIRT, new int[] { 2, 2, 2, 2, 2, 2 }},
        {BlockType.STONE, new int[] { 3, 3, 3, 3, 3, 3 }},
    };
}