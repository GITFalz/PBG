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
    public List<Vector2> Slices = [];
    public List<Matrix4> TransformationMatrices = [];  
    private VAO _vao = new VAO();
    private IBO _ibo = new IBO([]);
    private VBO _vertVbo = new VBO(new List<Vector3>());
    private VBO _uvVbo = new VBO(new List<Vector2>());
    private VBO _textureVbo = new VBO(new List<int>());
    public VBO _uiSizeVbo = new VBO(new List<Vector2>());
    private VBO _transformationVbo = new VBO(new List<int>());
    private VBO _sliceVbo = new VBO(new List<Vector2>());
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
        Slices.AddRange([element.Slice, element.Slice, element.Slice, element.Slice]);
        TransformationMatrices.Add(element.Transformation);

        ElementCount++;
    }

    public void UpdateElementTransformation(UIElement element)
    {
        TransformationMatrices[element.ElementIndex] = element.Transformation;
    }

    public void UpdateElementScale(UIElement element)
    {
        Panel panel = GetUIQuad(element);
        int index = element.ElementIndex * 4;
        for (int i = 0; i < 4; i++)
        {
            UiSizes[index + i] = (element.newScale.X, element.newScale.Y);
            Vertices[index + i] = panel.Vertices[i];
        }
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
        _sliceVbo = new VBO(Slices);
        _transformationSsbo = new SSBO(TransformationMatrices);
        
        _vao.LinkToVAO(0, 3, _vertVbo);
        _vao.LinkToVAO(1, 2, _uvVbo);
        _vao.LinkToVAO(2, 1, _textureVbo);
        _vao.LinkToVAO(3, 2, _uiSizeVbo);
        _vao.LinkToVAO(4, 1, _transformationVbo);
        _vao.LinkToVAO(5, 2, _sliceVbo);
        
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

    public void UpdateVertices()
    {
        _vertVbo.Update(Vertices);
        _uiSizeVbo.Update(UiSizes);
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


    public Panel GetUIQuad(UIElement element)
    {
        Panel panel = new Panel();

        Vector3 Pivot = element.Pivot;
        Vector2 newScale = element.newScale;

        float Depth = element.Depth;
        float Rotation = element.Rotation;

        int TextureIndex = element.TextureIndex;

        Vector3 position1 = Mathf.RotateAround((0,             0,           Depth),  Pivot, (0, 0, 1), Rotation);
        Vector3 position2 = Mathf.RotateAround((0,             newScale.Y,  Depth),  Pivot, (0, 0, 1), Rotation);
        Vector3 position3 = Mathf.RotateAround((newScale.X,    newScale.Y,  Depth),  Pivot, (0, 0, 1), Rotation);
        Vector3 position4 = Mathf.RotateAround((newScale.X,    0,           Depth),  Pivot, (0, 0, 1), Rotation);
        
        panel.Vertices.Add(position1);
        panel.Vertices.Add(position2);
        panel.Vertices.Add(position3);
        panel.Vertices.Add(position4);
        
        panel.Uvs.Add(new Vector2(0, 0));
        panel.Uvs.Add(new Vector2(0, 1));
        panel.Uvs.Add(new Vector2(1, 1));
        panel.Uvs.Add(new Vector2(1, 0));
        
        panel.TextUvs.Add(TextureIndex);
        panel.TextUvs.Add(TextureIndex);
        panel.TextUvs.Add(TextureIndex);
        panel.TextUvs.Add(TextureIndex);
        
        panel.UiSizes.Add(new Vector2(newScale.X, newScale.Y));
        panel.UiSizes.Add(new Vector2(newScale.X, newScale.Y));
        panel.UiSizes.Add(new Vector2(newScale.X, newScale.Y));
        panel.UiSizes.Add(new Vector2(newScale.X, newScale.Y));

        return panel;
    }
}