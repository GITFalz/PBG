using OpenTK.Mathematics;

public class MaskData
{
    public SSBO<UIMaskStruct> UIMaskSSBO = new();
    public List<UIMaskStruct> Masks = [];
    public List<UIPanel> Elements = [];
    public Dictionary<UIPanel, int> MaskElements = [];

    public int MaskCount = 0;

    private bool _generateBuffers = false;
    private bool _updateData = false;

    public void AddElement(UIMask element)
    {
        UIMaskStruct uiData = new UIMaskStruct()
        {
            Size = element.Scale,
            Index = (MaskCount, 0),
            Transformation = element.Transformation
        };

        Masks.Add(uiData);
        Elements.Add(element);
        MaskElements.Add(element, MaskCount);

        MaskCount++;

        _generateBuffers = true;
    }

    public bool GetElementData(UIMask element, out int index)
    {
        return MaskElements.TryGetValue(element, out index);
    }

    public void RemoveElement(UIMask element)
    {
        if (MaskElements.TryGetValue(element, out int index))
        {
            int lastIndex = Elements.Count - 1;

            if (index != lastIndex)
            {
                var lastElement = Elements[lastIndex];
                var lastCharacter = Masks[lastIndex];

                Elements[index] = lastElement;
                Masks[index] = lastCharacter;

                MaskElements[lastElement] = index;
            }

            Elements.RemoveAt(lastIndex);
            Masks.RemoveAt(lastIndex);
            MaskElements.Remove(element);

            MaskCount--;

            _generateBuffers = true;
        }
    }


    public void UpdateElementTransformation(UIMask element)
    {
        Internal_UpdateElementTransform(element);
        _updateData = true;
    }

    private void Internal_UpdateElementTransform(UIMask element)
    {
        if (!GetElementData(element, out int index))
            return;

        UIMaskStruct data = Masks[index];
        data.Transformation = element.Transformation;
        Masks[index] = data;
    }


    public void UpdateElementScale(UIMask element)
    {
        Internal_UpdateElementScale(element);
        _updateData = true;
    }

    private void Internal_UpdateElementScale(UIMask element)
    {
        if (!GetElementData(element, out int index))
            return;

        UIMaskStruct data = Masks[index];
        data.Size = element.Scale;
        Masks[index] = data;
    }

    public void GenerateBuffers()
    {
        UIMaskSSBO.Renew(Masks);
    }

    public void Update()
    {
        if (_generateBuffers)
        {
            GenerateBuffers();
            _generateBuffers = false;
        }
        if (_updateData)
        {
            UIMaskSSBO.Update(Masks, 0);
            _updateData = false;
        }
    }

    public void Clear()
    {
        Masks.Clear();
        Elements.Clear();
        MaskElements.Clear();
        MaskCount = 0;
    }

    public void Delete()
    {
        UIMaskSSBO.DeleteBuffer();
        Clear();
    }
}

public struct UIMaskStruct
{
    public Vector2 Size;
    public Vector2i Index;
    public Matrix4 Transformation;
}