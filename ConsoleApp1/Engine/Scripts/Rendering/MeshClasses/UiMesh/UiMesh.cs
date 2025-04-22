using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class UIMesh
{
    public static UIMesh Empty = new UIMesh();

    public List<int> chars = [];
    public List<UIStruct> UIData = new List<UIStruct>();

    
    private VAO _vao = new VAO();
    private TBO<int> _textTbo = new([]);
    private SSBO<UIStruct> _uiDataSSBO = new(new List<UIStruct>());
    
    public int ElementCount = 0;
    public int MaskCount = 0;
    public int VisibleElementCount = 0;

    private int _mask = 0x40000000; // 0x40000000 = 0100 0000 0000 0000 0000 0000 0000 0000

    private bool _updateData = false;
    private bool _updateVisibility = false;

    public List<UIRender> Elements = [];

    public void AddElement(UIPanel element, ref int uiIndex)
    {
        uiIndex = ElementCount;
        
        int mask = 0x40000000; // 0x40000000 = 0100 0000 0000 0000 0000 0000 0000 0000

        UIStruct uiData = new UIStruct()
        {
            SizeSlice = element.SizeSlice,
            Color = element.Color,
            TextureIndex = (element.TextureIndex, 0, ElementCount, element.Masked ? (element.MaskIndex | mask) : 0),
            Transformation = element.Transformation
        };

        UIData.Add(uiData);
        Elements.Add(element);

        ElementCount++;
        VisibleElementCount++;
    }

    public void AddElement(UIText element, ref int uiIndex, int offset)
    {
        uiIndex = ElementCount;

        UIStruct uiData = new UIStruct()
        {
            SizeSlice = element.SizeSlice,
            Color = element.Color,
            TextureIndex = (element.MaxCharCount, offset + 1, ElementCount, element.Masked ? (element.MaskIndex | _mask) : 0), // +1 because of the way i detect if it is ui or text
            Transformation = element.Transformation
        };

        UIData.Add(uiData);
        Elements.Add(element);

        ElementCount++;
        VisibleElementCount++;
    }

    public void UpdateMaskedIndex(int index, bool masked, int maskIndex)
    {
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
        int offsetIndex = 0;
        VisibleElementCount = 0;

        // Assuming the Data list is the same size as _visibility
        for (int i = 0; i < Elements.Count; i++)
        {
            UIRender element = Elements[i];
            if (element.Visible)
            {
                UIStruct data = UIData[offsetIndex];
                data.TextureIndex.Z = i;
                UIData[offsetIndex] = data;
                offsetIndex++;
                VisibleElementCount++; 
            }
        }
    }   


    public void UpdateElementTransformation(UIElement element)
    {
        Internal_UpdateElementTransform(element);
        _updateData = true;
    }

    private void Internal_UpdateElementTransform(UIElement element)
    {
        UIStruct data = UIData[element.ElementIndex];
        data.Transformation = element.Transformation;
        UIData[element.ElementIndex] = data;
    }


    public void UpdateElementScale(UIRender element)
    {
        Internal_UpdateElementScale(element);
        _updateData = true;
    }

    private void Internal_UpdateElementScale(UIRender element)
    {
        UIStruct data = UIData[element.ElementIndex];
        data.SizeSlice = element.SizeSlice;
        UIData[element.ElementIndex] = data;
    }


    public void UpdateElementTexture(UIRender element)
    {
        Internal_UpdateElementTexture(element);
        _updateData = true;
    }

    private void Internal_UpdateElementTexture(UIRender element)
    {
        UIStruct data = UIData[element.ElementIndex];
        data.TextureIndex.X = element.TextureIndex;
        UIData[element.ElementIndex] = data;
    }
    

    // Text
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

    public void UpdateText()
    {
        _textTbo.Update(chars);
    }

    public void GenerateBuffers()
    {
        _uiDataSSBO.Renew(UIData);
        _textTbo.Renew(chars);
    }

    public void Update()
    {
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
        _textTbo.Bind(TextureUnit.Texture1);

        GL.DrawArrays(PrimitiveType.Triangles, 0, VisibleElementCount * 6);
        
        _textTbo.Unbind();
        _uiDataSSBO.Unbind();
        _vao.Unbind();
    }

    public void Clear()
    {
        UIData.Clear();
        ElementCount = 0;
        VisibleElementCount = 0;
        MaskCount = 0;
        Elements.Clear();
        chars.Clear();
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