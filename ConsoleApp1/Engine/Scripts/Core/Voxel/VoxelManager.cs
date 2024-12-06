using ConsoleApp1.Engine.Scripts.Core.Math;
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

    public void GenerateBlock(Vector3 origin)
    {
        GenerateFrontFace(origin);
        GenerateRightFace(origin);
        GenerateTopFace(origin);
        GenerateLeftFace(origin);
        GenerateBottomFace(origin);
        GenerateBackFace(origin);
    }
    
    public void GenerateFrontFace(Vector3 origin)
    {
        Vector3 a = origin + Vector3Utils.p000;
        Vector3 b = origin + Vector3Utils.p010;
        Vector3 c = origin + Vector3Utils.p110;
        Vector3 d = origin + Vector3Utils.p100;
        
       GenerateBaseFace(a, b, c, d);
    }
    
    public void GenerateBackFace(Vector3 origin)
    {
        Vector3 a = origin + Vector3Utils.p101;
        Vector3 b = origin + Vector3Utils.p111;
        Vector3 c = origin + Vector3Utils.p011;
        Vector3 d = origin + Vector3Utils.p001;
        
        GenerateBaseFace(a, b, c, d);
    }
    
    public void GenerateTopFace(Vector3 origin)
    {
        Vector3 a = origin + Vector3Utils.p010;
        Vector3 b = origin + Vector3Utils.p011;
        Vector3 c = origin + Vector3Utils.p111;
        Vector3 d = origin + Vector3Utils.p110;
        
        GenerateBaseFace(a, b, c, d);
    }
    
    public void GenerateBottomFace(Vector3 origin)
    {
        Vector3 a = origin + Vector3Utils.p101;
        Vector3 b = origin + Vector3Utils.p100;
        Vector3 c = origin + Vector3Utils.p000;
        Vector3 d = origin + Vector3Utils.p001;
        
        GenerateBaseFace(a, b, c, d);
    }
    
    public void GenerateLeftFace(Vector3 origin)
    {
        Vector3 a = origin + Vector3Utils.p001;
        Vector3 b = origin + Vector3Utils.p011;
        Vector3 c = origin + Vector3Utils.p010;
        Vector3 d = origin + Vector3Utils.p000;
        
        GenerateBaseFace(a, b, c, d);
    }
    
    public void GenerateRightFace(Vector3 origin)
    {
        Vector3 a = origin + Vector3Utils.p100;
        Vector3 b = origin + Vector3Utils.p110;
        Vector3 c = origin + Vector3Utils.p111;
        Vector3 d = origin + Vector3Utils.p101;
        
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
        
        uvs2D.Add(new Vector2(0, 0));
        uvs2D.Add(new Vector2(1, 0));
        uvs2D.Add(new Vector2(1, 1));
        uvs2D.Add(new Vector2(0, 1));
        
        texIndex.Add(1);
        texIndex.Add(1);
        texIndex.Add(1);
        texIndex.Add(1);
    }
}