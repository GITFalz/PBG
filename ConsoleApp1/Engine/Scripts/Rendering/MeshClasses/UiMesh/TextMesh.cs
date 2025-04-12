using System.Drawing;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class TextMesh
{
    public List<int> chars = [];
    public List<Matrix4> TransformationMatrices = [];  
    public List<Vector2> Sizes = [];
    public List<Vector4i> Data = [];

    private VAO _vao = new VAO();
    private SSBO<Matrix4> _transformationSsbo = new(new List<Matrix4>());
    private SSBO<Vector2> _sizeSsbo = new(new List<Vector2>());
    private SSBO<Vector4i> _dataSsbo = new(new List<Vector4i>());
    private TBO<int> _textTbo = new([]);

    private List<UIText> _textElements = new List<UIText>();

    public int ElementCount = 0;
    public int VisibleElementCount = 0;

    public void SetCharacters(List<int> characters, int offset)
    {
        for (int i = 0; i < characters.Count; i++)
        {
            int index = offset + i;
            if (index >= chars.Count)
                chars.Add(characters[i]);
            else
                chars[index] = characters[i];
        }
    }

    public void SetVisibility(bool visible, int index)
    {
        if (index >= Data.Count)
            return;

        VisibleElementCount += visible ? 1 : -1;
        UpdateData();
    }

    public void AddTextElement(UIText element, ref int uiIndex, int offset)
    {
        uiIndex = ElementCount;

        Sizes.Add((element.newScale.X, element.newScale.Y));
        Data.Add((element.MaxCharCount, offset, ElementCount, 0));
        _textElements.Add(element);

        TransformationMatrices.Add(element.Transformation);

        ElementCount++;
        VisibleElementCount++;
    }

    public void RemoveTextElement(UIText element)
    {
        var index = element.ElementIndex;

        Sizes.RemoveAt(index);
        Data.RemoveAt(index);
        _textElements.RemoveAt(index);
        TransformationMatrices.RemoveAt(index);

        ElementCount--;
        VisibleElementCount--;
    }

    public void UpdateData()
    {
        int offsetIndex = 0;

        // Assuming the Data list is the same size as _visibility
        for (int i = 0; i < _textElements.Count; i++)
        {
            if (_textElements[i].Visible)
            {
                Vector4i data = Data[offsetIndex];
                data.Z = i;
                Data[offsetIndex] = data;
                offsetIndex++;
            }
        }

        // Update the Data list
        _dataSsbo.Update(Data, 2);
    }   

    public void UpdateElementTransformation(UIText element)
    {
        TransformationMatrices[element.ElementIndex] = element.Transformation;
    }

    public void GenerateBuffers()
    {
        _transformationSsbo = new(TransformationMatrices);
        _sizeSsbo = new(Sizes);
        _dataSsbo = new(Data);
        _textTbo = new(chars);
    }

    public void Render()
    {
        _vao.Bind();
        _transformationSsbo.Bind(0);
        _sizeSsbo.Bind(1);
        _dataSsbo.Bind(2);
        _textTbo.Bind(TextureUnit.Texture1);

        GL.DrawArrays(PrimitiveType.Triangles, 0, VisibleElementCount * 6);
        
        _transformationSsbo.Unbind();
        _sizeSsbo.Unbind();
        _dataSsbo.Unbind();
        _textTbo.Unbind();
        _vao.Unbind();
    }

    public void UpdateMatrices()
    {
        _transformationSsbo.Update(TransformationMatrices, 0);
        _sizeSsbo.Update(Sizes, 1);
        _dataSsbo.Update(Data, 2);
    }

    public void UpdateText()
    {
        _textTbo.Update(chars);
    }

    public void Clear()
    {
        TransformationMatrices.Clear();
        Sizes.Clear();
        Data.Clear();
        chars.Clear();
        ElementCount = 0;
        VisibleElementCount = 0;
    }
}