using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public abstract class Mesh
{
    public List<Vector3> Vertices;
    public List<uint> Indices;
    public List<Vector2> Uvs;
    
    public List<Vector3> transformedVertices = new List<Vector3>();
    
    public VAO _vao;
    public VBO _vertVbo;
    public VBO _uvVbo;
    public IBO _ibo;
    
    public Vector3 Position;
    
    private bool _updateMesh = false;
    
    public virtual void GenerateBuffers()
    {
        for (int i = 0; i < Vertices.Count; i++)
        {
            transformedVertices.Add(Vertices[i] + Position);
        }
        
        _vertVbo = new VBO(transformedVertices);
        _uvVbo = new VBO(Uvs);
        
        _vao.LinkToVAO(0, 3, _vertVbo);
        _vao.LinkToVAO(1, 2, _uvVbo);
        
        _ibo = new IBO(Indices);
    }

    public void RenderMesh()
    {
        if (_updateMesh)
            UpdateMesh();
        
        _vao.Bind();
        _ibo.Bind();

        GL.DrawElements(PrimitiveType.Triangles, Indices.Count, DrawElementsType.UnsignedInt, 0);
        
        _vao.Unbind();
        _ibo.Unbind();
    }
    
    public virtual void UpdateMesh()
    {
        _vertVbo.Update(transformedVertices);
    }
    
    public virtual void AddQuad(Vector3 position, Quad quad)
    {
        int index = Vertices.Count;
        
        foreach (int i in quad.Indices)
        {
            Indices.Add((uint)(i + index));
        }
        
        foreach (Vector3 vertex in quad.Vertices)
        {
            Vertices.Add(position + vertex);
        }
        
        foreach (Vector2 uv in quad.Uvs)
        {
            Uvs.Add(uv);
        }
        
        //_updateMesh = true;
    }
    

    public void RecalculateBounds()
    {
        Bounds bounds = new Bounds();
        bounds.min = new Vector3(float.MaxValue);
        bounds.max = new Vector3(float.MinValue);

        foreach (Vector3 vertex in Vertices)
        {
            bounds.min = Vector3.ComponentMin(bounds.min, vertex);
            bounds.max = Vector3.ComponentMax(bounds.max, vertex);
        }
    }
    
    public void Clear()
    {
        Vertices.Clear();
        Uvs.Clear();
        Indices.Clear();
        
        _updateMesh = true;
    }
    
    public virtual void Delete()
    {
        _vao.Delete();
        _vertVbo.Delete();
        _uvVbo.Delete();
        _ibo.Delete();
    }
    
    public struct Bounds
    {
        public Vector3 min;
        public Vector3 max;
    }
}

public struct Quad
{
    public Vector3[] Vertices;
    public int[] Indices;
    public Vector2[] Uvs;
    public int[] TextureIndices;
    public Vector2i[] TextureUvs;

    public Quad(Vector3[] v, int[] t)
    {
        Vertices = v;
        TextureIndices = t;
        
        Uvs = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(1, 0)
        };
        
        Indices = new int[]
        {
            0, 1, 2,
            2, 3, 0
        };
    }
    
    public Quad(Vector3[] v, int[] t, Vector2i[] uvs)
    {
        Vertices = v;
        TextureIndices = t;
        TextureUvs = uvs;
        
        Uvs = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        };
        
        Indices = new int[]
        {
            0, 1, 2,
            2, 3, 0
        };
    }
}