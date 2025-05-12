using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class UIMesh
{
    public static UIMesh Empty = new UIMesh();

    public List<UIStruct> UIData = new List<UIStruct>();
    public List<UIPanel> Elements = new();
    public Dictionary<UIPanel, int> ElementIndices = new();
    
    private VAO _vao = new VAO();
    private SSBO<UIStruct> _uiDataSSBO = new(new List<UIStruct>());
    
    public int ElementCount = 0;
    public int MaskCount = 0;
    public int VisibleElementCount = 0;

    private int _mask = 0x40000000;

    private bool _generateBuffers = false;
    private bool _updateData = false;
    private bool _updateVisibility = false;

    public void AddElement(UIPanel element)
    {
        UIStruct uiData = new UIStruct()
        {
            SizeSlice = element.SizeSlice,
            Color = element.Color,
            TextureIndex = (element.TextureIndex, 0, ElementCount, element.Masked ? (element.MaskIndex | _mask) : 0),
            Transformation = element.Transformation
        };

        UIData.Add(uiData);
        Elements.Add(element);
        ElementIndices.Add(element, ElementCount);

        ElementCount++;
        VisibleElementCount++;

        _generateBuffers = true;
        _updateVisibility = true;
    }

    public bool GetElementData(UIPanel element, out int index)
    {
        return ElementIndices.TryGetValue(element, out index);
    }

    public void RemoveElement(UIPanel element)
    {
        if (ElementIndices.TryGetValue(element, out int index))
        {
            int lastIndex = Elements.Count - 1;

            if (index != lastIndex)
            {
                var lastElement = Elements[lastIndex];
                var lastCharacter = UIData[lastIndex];

                Elements[index] = lastElement;
                UIData[index] = lastCharacter;

                ElementIndices[lastElement] = index;
            }

            Elements.RemoveAt(lastIndex);
            UIData.RemoveAt(lastIndex);
            ElementIndices.Remove(element);

            ElementCount--;

            if (element.Visible)
            {
                VisibleElementCount--;
            }

            _generateBuffers = true;
            _updateVisibility = true;
        }
    }

    public void UpdateMaskedIndex(UIPanel element, bool masked, int maskIndex)
    {
        if (!GetElementData(element, out int index))
            return;

        UIStruct data = UIData[index];
        data.TextureIndex.W = masked ? (maskIndex | _mask) : 0;
        UIData[index] = data;
    }

    public void SetVisibility()
    {
        _updateVisibility = true;
    }

    public void UpdateData()
    {
        VisibleElementCount = 0;

        // Assuming the Data list is the same size as _visibility
        for (int i = 0; i < Elements.Count; i++)
        {
            if (Elements[i].Visible)
            {
                UIStruct data = UIData[VisibleElementCount];
                data.TextureIndex.Z = i;
                UIData[VisibleElementCount] = data;
                VisibleElementCount++; 
            }
        }
    }   


    public void UpdateElementTransformation(UIPanel element)
    {
        Internal_UpdateElementTransform(element);
        _updateData = true;
    }

    private void Internal_UpdateElementTransform(UIPanel element)
    {
        if (!GetElementData(element, out int index))
            return;

        UIStruct data = UIData[index];
        data.Transformation = element.Transformation;
        UIData[index] = data;
    }


    public void UpdateElementScale(UIPanel element)
    {
        Internal_UpdateElementScale(element);
        _updateData = true;
    }

    private void Internal_UpdateElementScale(UIPanel element)
    {
        if (!GetElementData(element, out int index))
            return;

        UIStruct data = UIData[index];
        data.SizeSlice = element.SizeSlice;
        UIData[index] = data;
    }


    public void UpdateElementTexture(UIPanel element)
    {
        Internal_UpdateElementTexture(element);
        _updateData = true;
    }

    private void Internal_UpdateElementTexture(UIPanel element)
    {
        if (!GetElementData(element, out int index))
            return;

        UIStruct data = UIData[index];
        data.TextureIndex.X = element.TextureIndex;
        UIData[index] = data;
    }

    public void GenerateBuffers()
    {
        _uiDataSSBO.Renew(UIData);
    }

    public void Update()
    {
        if (_generateBuffers)
        {
            GenerateBuffers();
            _generateBuffers = false;
        }
        if (_updateVisibility)
        {
            UpdateData();
            _updateVisibility = false;
            _updateData = true;
        }
        if (_updateData)
        {
            _uiDataSSBO.Update(UIData, 0);
            _updateData = false;
        }
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
        MaskCount = 0;
        ElementIndices.Clear();
    }

    public void Delete()
    {
        Clear();
        _vao.DeleteBuffer();
        _uiDataSSBO.DeleteBuffer();
    }
}

public struct UIStruct
{
    public Vector4 SizeSlice;
    public Vector4 Color;
    public Vector4i TextureIndex;
    public Matrix4 Transformation;
    

    public override string ToString()
    {
        return $"SizeSlice: {SizeSlice},\n Color: {Color},\n TextureIndex: {TextureIndex},\n Transformation:\n{Transformation}\n";
    }
}