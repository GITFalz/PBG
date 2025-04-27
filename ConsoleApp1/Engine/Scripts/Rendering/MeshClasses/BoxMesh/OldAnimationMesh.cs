using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class OldAnimationMesh
{
    public List<Vector3> Vertices = new List<Vector3>();
    public List<Vector2> Uvs = new List<Vector2>();
    public List<uint> Indices = new List<uint>();
    public List<int> TextureIndices = new List<int>();
    public List<Vector3> _transformedVerts = new List<Vector3>();

    private VAO _vao = new VAO();
    private IBO _ibo = new IBO(new List<uint>());
    private VBO<Vector3> _vertVbo = new(new List<Vector3>());
    private VBO<Vector2> _uvVbo = new(new List<Vector2>());
    private VBO<int> _textureVbo = new(new List<int>());
    
    public Vector3 WorldPosition = Vector3.Zero;
    public Quaternion LastRotation = Quaternion.Identity;
    
    public OldAnimationMesh()
    {
        _vao = new VAO();
        
        Vertices = new List<Vector3>();
        Uvs = new List<Vector2>();
        Indices = new List<uint>();
        TextureIndices = new List<int>();
        
        _transformedVerts = new List<Vector3>();
    }

    public void UpdateMesh()
    {
        _vertVbo.Update(_transformedVerts);
    }
    
    public void GenerateBuffers()
    {
        _transformedVerts.Clear();
        for (int i = 0; i < Vertices.Count; i++)
        {
            _transformedVerts.Add(Vertices[i]);
        }

        _vao.Bind();
        
        _vertVbo.Renew(_transformedVerts);
        _uvVbo.Renew(Uvs);
        _textureVbo.Renew(TextureIndices);
        
        _vao.LinkToVAO(0, 3, VertexAttribPointerType.Float, 0, 0, _vertVbo);
        _vao.LinkToVAO(1, 2, VertexAttribPointerType.Float, 0, 0, _uvVbo);
        _vao.IntLinkToVAO(2, 1, VertexAttribIntegerType.Int, 0, 0, _textureVbo);
        
        _ibo.Renew(Indices);

        _vao.Unbind();
    }

    public void RenderMesh()
    {
        _vao.Bind();
        _ibo.Bind();
        
        GL.DrawElements(PrimitiveType.Triangles, Indices.Count, DrawElementsType.UnsignedInt, 0);

        _ibo.Unbind();
        _vao.Unbind();
    }
}