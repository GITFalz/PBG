using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class UIMesh
{
    public List<Vector3> Vertices = [];
    public List<Vector2> Uvs = [];
    public List<uint> Indices = [];
    public List<int> TextureIndices = [];
    public List<Vector2> UiSizes = [];
    public List<int> TransformationIndex = [];
    public List<Matrix4> TransformationMatrices = [];  
    private VAO _vao = new VAO();
    private IBO _ibo = new IBO([]);
    private VBO _vertVbo = new VBO(new List<Vector3>());
    private VBO _uvVbo = new VBO(new List<Vector2>());
    private VBO _textureVbo = new VBO(new List<int>());
    public VBO _uiSizeVbo = new VBO(new List<Vector2>());
    private VBO _transformationVbo = new VBO(new List<int>());
    private SSBO _transformationSsbo = new SSBO([]);
    public int ElementCount = 0;

    public void AddElement(UIElement element, ref int uiIndex)
    {
        uiIndex = ElementCount;
        var panel = element.panel;

        Vertices.AddRange(panel.Vertices);
        Uvs.AddRange(panel.Uvs);
        TextureIndices.AddRange(panel.TextUvs);
        UiSizes.AddRange(panel.UiSizes);

        TransformationIndex.AddRange([ElementCount, ElementCount, ElementCount, ElementCount]);
        TransformationMatrices.Add(element.Transformation);

        ElementCount++;
    }

    public void UpdateElementTransformation(UIElement element)
    {
        TransformationMatrices[element.ElementIndex] = element.Transformation;
    }

    public void UpdateElementTexture(UIElement element)
    {
        int index = element.ElementIndex * 4;
        for (int i = 0; i < 4; i++)
        {
            TextureIndices[index + i] = element.TextureIndex;
        }
    }

    public void GenerateBuffers()
    {
        GenerateIndices();

        _vertVbo = new VBO(Vertices);
        _uvVbo = new VBO(Uvs);
        _textureVbo = new VBO(TextureIndices);
        _uiSizeVbo = new VBO(UiSizes);
        _transformationVbo = new VBO(TransformationIndex);
        _transformationSsbo = new SSBO(TransformationMatrices);
        
        _vao.LinkToVAO(0, 3, _vertVbo);
        _vao.LinkToVAO(1, 2, _uvVbo);
        _vao.LinkToVAO(2, 1, _textureVbo);
        _vao.LinkToVAO(3, 2, _uiSizeVbo);
        _vao.LinkToVAO(4, 1, _transformationVbo);
        
        _ibo = new IBO(Indices);
    }

    public void Render()
    {
        _vao.Bind();
        _ibo.Bind();
        _transformationSsbo.Bind(0);

        GL.DrawElements(PrimitiveType.Triangles, Indices.Count, DrawElementsType.UnsignedInt, 0);

        _vao.Unbind();
        _ibo.Unbind();
        _transformationSsbo.Unbind();
    }

    public void UpdateMatrices()
    {
        _transformationSsbo.Update(TransformationMatrices, 0);
    }

    public void UpdateTexture()
    {
        _textureVbo.Update(TextureIndices);
    }

    public void Clear()
    {
        Vertices.Clear();
        Uvs.Clear();
        Indices.Clear();
        TextureIndices.Clear();
        UiSizes.Clear();
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