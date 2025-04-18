using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class UIMesh
{
    public List<UIStruct> UIData = new List<UIStruct>();

    
    private VAO _vao = new VAO();
    private SSBO<UIStruct> _uiDataSSBO = new(new List<UIStruct>());
    public int ElementCount = 0;
    public int VisibleElementCount = 0;

    private bool _updateVisibility = true;

    public List<UIPanel> Elements = new List<UIPanel>();

    public void SetVisibility()
    {
        _updateVisibility = true;
    }

    public void AddElement(UIPanel element, ref int uiIndex)
    {
        uiIndex = ElementCount;

        UIStruct uiData = new UIStruct()
        {
            SizeSlice = (element.newScale.X, element.newScale.Y, element.Slice.X, element.Slice.Y),
            Color = (element.Color.X, element.Color.Y, element.Color.Z, 1f),
            TextureIndex = (element.TextureIndex, ElementCount, 1, 0),
            Transformation = element.Transformation
        };

        UIData.Add(uiData);
        Elements.Add(element);

        ElementCount++;
        VisibleElementCount++;
    }

    public void RemoveElement(UIPanel element)
    {
        var index = element.ElementIndex;

        UIData.RemoveAt(index);
        Elements.RemoveAt(index);

        ElementCount--;
        VisibleElementCount--;
    }

    public void UpdateData()
    {
        int offsetIndex = 0;
        VisibleElementCount = 0;

        // Assuming the Data list is the same size as _visibility
        for (int i = 0; i < Elements.Count; i++)
        {
            if (Elements[i].Visible)
            {
                UIStruct data = UIData[offsetIndex];
                data.TextureIndex.Y = i;
                UIData[offsetIndex] = data;
                offsetIndex++;
                VisibleElementCount++;
            }
        }
    }   

    public void UpdateVisibility()
    {
        if (!_updateVisibility) 
            return;

        UpdateData();

        _updateVisibility = false;
        _uiDataSSBO.Update(UIData, 2);
    }


    public void UpdateElement(UIPanel element)
    {
        Internal_UpdateElementTransform(element);
        Internal_UpdateElementScale(element);
        Internal_UpdateElementTexture(element);
        _uiDataSSBO.Update(UIData, 0);
    }


    public void UpdateElementTransformation(UIElement element)
    {
        Internal_UpdateElementTransform(element);
        _uiDataSSBO.Update(UIData, 0);
    }

    private void Internal_UpdateElementTransform(UIElement element)
    {
        UIStruct data = UIData[element.ElementIndex];
        data.Transformation = element.Transformation;
        UIData[element.ElementIndex] = data;
    }


    public void UpdateElementScale(UIPanel element)
    {
        Internal_UpdateElementScale(element);
        _uiDataSSBO.Update(UIData, 0);
    }

    private void Internal_UpdateElementScale(UIPanel element)
    {
        UIStruct data = UIData[element.ElementIndex];
        data.SizeSlice = (element.newScale.X, element.newScale.Y, element.Slice.X, element.Slice.Y);
        UIData[element.ElementIndex] = data;
    }


    public void UpdateElementTexture(UIPanel element)
    {
        Internal_UpdateElementTexture(element);
        _uiDataSSBO.Update(UIData, 0);
    }

    private void Internal_UpdateElementTexture(UIPanel element)
    {
        UIStruct data = UIData[element.ElementIndex];
        data.TextureIndex.X = element.TextureIndex;
        UIData[element.ElementIndex] = data;
    }


    public void GenerateBuffers()
    {
        _uiDataSSBO = new(UIData);
    }

    public void Render()
    {
        _vao.Bind();
        _uiDataSSBO.Bind(0);

        GL.DrawArrays(PrimitiveType.Triangles, 0, VisibleElementCount * 6);

        _uiDataSSBO.Unbind();
        _vao.Unbind();
    }

    public void Clear()
    {
        UIData.Clear();
        ElementCount = 0;
        VisibleElementCount = 0;
    }
}

public struct UIStruct
{
    public Vector4 SizeSlice;
    public Vector4 Color;
    public Vector4i TextureIndex;
    public Matrix4 Transformation;
}