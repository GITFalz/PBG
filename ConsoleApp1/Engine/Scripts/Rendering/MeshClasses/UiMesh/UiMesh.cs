using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class UIMesh
{
    public List<Matrix4> TransformationMatrices = []; 

    // New SSBO variables
    private List<Vector4> _ssboFloatData = new List<Vector4>(); // 2 vector4 per vertex, 2 floats for size, 2 floats for slice and 3 floats for color, leaves float unused
    private List<Vector4i> _ssboIntData = new List<Vector4i>();

    
    private VAO _vao = new VAO();
    private SSBO _transformationSsbo = new SSBO(new List<Matrix4>());
    private SSBO _floatDataSsbo = new SSBO(new List<Vector4>());
    private SSBO _intDataSsbo = new SSBO(new List<Vector4i>());
    public int ElementCount = 0;
    public int VisibleElementCount = 0;

    public List<UIPanel> Elements = new List<UIPanel>();

    public void SetVisibility(bool visible, int index)
    {
        if (index >= _ssboIntData.Count)
            return;

        VisibleElementCount += visible ? 1 : -1;
        UpdateData();
    }

    public void AddElement(UIPanel element, ref int uiIndex)
    {
        uiIndex = ElementCount;

        Vector4 sizeSlice = (element.newScale.X, element.newScale.Y, element.Slice.X, element.Slice.Y);
        Vector4 color = (element.Color.X, element.Color.Y, element.Color.Z, 1f);
        _ssboFloatData.AddRange(sizeSlice, color);
        _ssboIntData.Add((element.TextureIndex, ElementCount, 0, 0));
        TransformationMatrices.Add(element.Transformation);

        ElementCount++;
        VisibleElementCount++;
    }

    public void RemoveElement(UIPanel element)
    {
        var index = element.ElementIndex;

        _ssboFloatData.RemoveRange(index*2, 2);
        _ssboIntData.RemoveAt(index);
        TransformationMatrices.RemoveAt(index);

        ElementCount--;
        VisibleElementCount--;
    }

    public void UpdateData()
    {
        int offsetIndex = 0;

        // Assuming the Data list is the same size as _visibility
        for (int i = 0; i < Elements.Count; i++)
        {
            if (Elements[i].Visible)
            {
                Vector4i data = _ssboIntData[offsetIndex];
                data.Y = i;
                _ssboIntData[offsetIndex] = data;
                offsetIndex++;
            }
        }

        // Update the Data list
        _intDataSsbo.Update(_ssboIntData, 2);
    }   

    public void UpdateElementTransformation(UIElement element)
    {
        TransformationMatrices[element.ElementIndex] = element.Transformation;
    }

    public void UpdateElementScale(UIPanel element)
    {
        Vector4 sizeSlice = (element.newScale.X, element.newScale.Y, element.Slice.X, element.Slice.Y);
        _ssboFloatData[element.ElementIndex * 2] = sizeSlice;
        _floatDataSsbo.Update(_ssboFloatData, 1);
    }

    public void UpdateElementTexture(UIPanel element)
    {
        Vector4i data = _ssboIntData[element.ElementIndex];
        data.X = element.TextureIndex;
        _ssboIntData[element.ElementIndex] = data;
        _intDataSsbo.Update(_ssboIntData, 2);
    }

    public void GenerateBuffers()
    {
        _transformationSsbo = new SSBO(TransformationMatrices);
        _floatDataSsbo = new SSBO(_ssboFloatData);
        _intDataSsbo = new SSBO(_ssboIntData);
    }

    public void Render()
    {
        _vao.Bind();
        _transformationSsbo.Bind(0);
        _floatDataSsbo.Bind(1);
        _intDataSsbo.Bind(2);

        GL.DrawArrays(PrimitiveType.Triangles, 0, VisibleElementCount * 6);

        _transformationSsbo.Unbind();
        _floatDataSsbo.Unbind();
        _intDataSsbo.Unbind();
        _vao.Unbind();
    }

    public void UpdateMatrices()
    {
        _transformationSsbo.Update(TransformationMatrices, 0);
        _floatDataSsbo.Update(_ssboFloatData, 1);
        _intDataSsbo.Update(_ssboIntData, 2);
    }

    public void Clear()
    {
        TransformationMatrices.Clear();
        _ssboFloatData.Clear();
        _ssboIntData.Clear();
        ElementCount = 0;
        VisibleElementCount = 0;
    }
}