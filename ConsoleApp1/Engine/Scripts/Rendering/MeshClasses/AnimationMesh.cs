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
    public List<Vector4> BoneIndices = new List<Vector4>();
    public List<Matrix4> BoneMatrices = new List<Matrix4>();  
    private VAO _vao = new VAO();
    private IBO _ibo = new IBO([]);
    private VBO _vertVbo = new VBO([(0, 0, 0)]);
    private VBO _uvVbo = new VBO([(0, 0)]);
    private VBO _textureVbo = new VBO([0]);
    private VBO _normalVbo = new VBO([(0, 0, 0)]);
    private VBO _boneVbo = new VBO([(0, 0, 0, 0)]);
    private SSBO _boneSSBO = new SSBO([]);
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
        
        _vertVbo = new VBO(Vertices);
        _uvVbo = new VBO(Uvs);
        _textureVbo = new VBO(TextureIndices);
        _normalVbo = new VBO(Normals);
        _boneVbo = new VBO(BoneIndices);
        _boneSSBO = new SSBO(BoneMatrices);
        
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