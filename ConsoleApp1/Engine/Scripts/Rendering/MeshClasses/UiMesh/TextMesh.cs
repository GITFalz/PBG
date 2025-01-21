using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class TextMesh
{
    public List<Vector3> Vertices = [];
    public List<Vector2> Uvs = [];
    public List<uint> Indices = [];
    public List<int> TransformationIndex = [];
    public List<Matrix4> TransformationMatrices = [];  
    public List<Vector2i> TextUvs = [];
    public List<int> chars = [];
    private VAO _vao = new VAO();
    private IBO _ibo = new IBO([]);
    private VBO _vertVbo = new VBO(new List<Vector3>());
    private VBO _uvVbo = new VBO(new List<Vector2>());
    private VBO _transformationVbo = new VBO(new List<int>());
    public VBO _textUvVbo = new VBO(new List<Vector2i>());
    private SSBO _transformationSsbo = new SSBO([]);
    public TBO _textTbo = new TBO([]);
    public int ElementCount = 0;

    public void AddTextElement(UIText element, ref int uiIndex)
    {
        uiIndex = ElementCount;
        var textQuad = element.textQuad;

        Vertices.AddRange(textQuad.Vertices);
        Uvs.AddRange(textQuad.Uvs);
        TextUvs.AddRange(textQuad.TextSize);
        chars.AddRange(element.Chars);

        TransformationIndex.AddRange([ElementCount, ElementCount, ElementCount, ElementCount]);
        TransformationMatrices.Add(element.Transformation);

        ElementCount++;
    }

    public void GenerateBuffers()
    {
        GenerateIndices();

        _vertVbo = new VBO(Vertices);
        _uvVbo = new VBO(Uvs);
        _transformationVbo = new VBO(TransformationIndex);
        _textUvVbo = new VBO(TextUvs);
        _transformationSsbo = new SSBO(TransformationMatrices);
        _textTbo = new TBO(chars);
        
        _vao.LinkToVAO(0, 3, _vertVbo);
        _vao.LinkToVAO(1, 2, _uvVbo);
        _vao.LinkToVAO(4, 1, _transformationVbo);
        _vao.LinkToVAO(5, 2, _textUvVbo);
        
        _ibo = new IBO(Indices);
    }

    public void Render()
    {
        _vao.Bind();
        _ibo.Bind();
        _transformationSsbo.Bind(0);
        _textTbo.Bind(TextureUnit.Texture1);

        GL.DrawElements(PrimitiveType.Triangles, Indices.Count, DrawElementsType.UnsignedInt, 0);

        _vao.Unbind();
        _ibo.Unbind();
        _transformationSsbo.Unbind();
        _textTbo.Unbind();
    }

    public void UpdateMatrices()
    {
        _transformationSsbo.Update(TransformationMatrices, 0);
    }

    public void Clear()
    {
        Vertices.Clear();
        Uvs.Clear();
        Indices.Clear();
        TransformationIndex.Clear();
        TransformationMatrices.Clear();
        ElementCount = 0;
    }

    public void GenerateIndices()
    {
        Indices.Clear();

        for (uint i = 0; i < ElementCount; i++)
        {
            uint index = i * 4;
            Indices.AddRange([index, index + 1, index + 2, index + 2, index + 3, index]);
        }
    }
}