using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class AnimationMesh
{
    public List<Vector3> Vertices = new List<Vector3>();
    public List<Vector2> Uvs = new List<Vector2>();
    public List<uint> Indices = new List<uint>();
    public List<int> TextureIndices = new List<int>();
    public List<Vector3> Normals = new List<Vector3>();
    public List<Vector4i> BoneIndices = new List<Vector4i>();
    public List<Matrix4> BoneMatrices = new List<Matrix4>();  
    private VAO _vao = new VAO();
    private IBO _ibo = new IBO([]);
    private VBO<Vector3> _vertVbo = new([(0, 0, 0)]);
    private VBO<Vector2> _uvVbo = new([(0, 0)]);
    private VBO<int> _textureVbo = new([0]);
    private VBO<Vector3> _normalVbo = new([(0, 0, 0)]);
    private VBO<Vector4i> _boneVbo = new([(0, 0, 0, 0)]);
    private SSBO<Matrix4> _boneSSBO = new(new List<Matrix4>{});
    public Bone RootBone;
    public int BoneCount;

    public AnimationMesh(Bone rootBone, int boneCount)
    {
        RootBone = rootBone;
        BoneCount = boneCount;
    }

    public void GenerateBuffers()
    {
        GenerateIndices();
        GenerateBasicBoneMatrices();
        
        _vertVbo = new(Vertices);
        _uvVbo = new(Uvs);
        _textureVbo = new(TextureIndices);
        _normalVbo = new(Normals);
        _boneVbo = new(BoneIndices);
        _boneSSBO = new(BoneMatrices);
        
        _vao.LinkToVAO(0, 3, _vertVbo);
        _vao.LinkToVAO(1, 2, _uvVbo);
        _vao.LinkToVAO(2, 1, _textureVbo);
        _vao.LinkToVAO(3, 3, _normalVbo);
        _vao.LinkToVAO(4, 4, _boneVbo);
        
        _ibo = new IBO(Indices);
    }

    public void GenerateIndices()
    {
        Indices.Clear();
        for (uint i = 0; i < Vertices.Count; i++)
        {
            Indices.Add(i);
        }
    }

    public void GenerateBasicBoneMatrices()
    {
        BoneMatrices.Clear();
        for (int i = 0; i < BoneCount; i++)
        {
            BoneMatrices.Add(Matrix4.Identity);
        }
    }

    public void RenderMesh()
    {
        _vao.Bind();
        _ibo.Bind();
        _boneSSBO.Bind(0);
        
        GL.DrawElements(PrimitiveType.Triangles, Indices.Count, DrawElementsType.UnsignedInt, 0);
        
        _vao.Unbind();
        _ibo.Unbind();
        _boneSSBO.Unbind();
    }

    public void UpdateBoneMatrices()
    {
        _boneSSBO.Update(BoneMatrices, 0);
    }
}