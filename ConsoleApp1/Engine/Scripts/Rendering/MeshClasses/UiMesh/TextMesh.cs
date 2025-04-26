using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class TextMesh
{
    public VAO TextVAO = new();
    public SSBO<Matrix4> TransformationSSBO = new();
    public SSBO<CharacterData> CharacterSSBO = new();

    public List<Matrix4> TransformationMatrices = new();
    public List<UIText> Elements = new();
    public Dictionary<UIText, int> ElementIndices = new();


    public List<CharacterData> Characters = new();
    public List<TextElementData> TextElements = new();
    public Dictionary<TextElementData, int> TextDataIndices = new();

    public int ElementCount = 0;

    public int CharacterCount => TextDataIndices.Count;
    public int VisibleElementCount = 0;

    private bool _generateBuffers = false;
    private bool _updateData = false;
    private bool _updateVisibility = false;

    private int _mask = 0x40000000;

    public void AddElement(UIText element)
    {
        TransformationMatrices.Add(element.Transformation);
        Elements.Add(element);
        ElementIndices.Add(element, ElementCount);

        element.SetCharacterElementIndex(ElementCount);

        AddCharacters(element.CharacterDataList);

        ElementCount++;

        _generateBuffers = true;
    }

    private void AddCharacters(List<TextElementData> characters)
    {
        foreach (var character in characters)
        {
            AddCharacter(character);
        }

        _updateVisibility = true;
    }

    private void AddCharacter(TextElementData character)
    {
        character.SetActualIndex(VisibleElementCount);

        TextDataIndices.Add(character, CharacterCount);
        Characters.Add(character.Character);
        TextElements.Add(character);

        VisibleElementCount++;

        _generateBuffers = true;
    }

    public bool GetElementData(UIText element, out int index)
    {
        return ElementIndices.TryGetValue(element, out index);
    }

    public void RemoveElement(UIText element)
    {
        if (ElementIndices.TryGetValue(element, out int index))
        {
            RemoveCharacters(element.CharacterDataList);

            int lastIndex = Elements.Count - 1;

            if (index != lastIndex)
            {
                var lastElement = Elements[lastIndex];
                var lastCharacter = TransformationMatrices[lastIndex];

                Elements[index] = lastElement;
                TransformationMatrices[index] = lastCharacter;

                ElementIndices[lastElement] = index;

                lastElement.SetCharacterElementIndex(index); 
                lastElement.UpdateCharacters();
            }

            Elements.RemoveAt(lastIndex);
            TransformationMatrices.RemoveAt(lastIndex);
            ElementIndices.Remove(element);

            ElementCount--;

            _generateBuffers = true;
            _updateVisibility = true;
        }
    }

    public void RemoveCharacters(List<TextElementData> characters)
    {
        foreach (var character in characters)
        {
            RemoveCharacter(character);
        }
    }

    public void RemoveCharacter(TextElementData character)
    {
        if (TextDataIndices.TryGetValue(character, out int index))
        {
            int lastIndex = TextElements.Count - 1;

            if (index != lastIndex)
            {
                var lastElement = TextElements[lastIndex];
                var lastCharacter = Characters[lastIndex];

                TextElements[index] = lastElement;
                Characters[index] = lastCharacter;

                TextDataIndices[lastElement] = index;

                lastElement.SetActualIndex(index);
            }

            TextElements.RemoveAt(lastIndex);
            Characters.RemoveAt(lastIndex);
            TextDataIndices.Remove(character);

            VisibleElementCount--;

            _generateBuffers = true;
        }
    }

    public void UpdateCharacters(UIText text, List<TextElementData> characters)
    {
        foreach (var character in characters)
        {
            UpdateCharacter(text, character);
        }
    }

    public void UpdateCharacter(UIText text, TextElementData character)
    {
        if (TextDataIndices.TryGetValue(character, out int textIndex))
        {
            Characters[textIndex] = character.Character;
            _updateVisibility = true;
        }
    }

    public void SetVisibility()
    {
        _updateVisibility = true;
    }

    public void UpdateElementTransformation(UIText element)
    {
        Internal_UpdateElementTransform(element);
        _updateData = true;
    }

    private void Internal_UpdateElementTransform(UIText element)
    {
        if (!GetElementData(element, out int index))
            return;

        TransformationMatrices[index] = element.Transformation;
    }

    public void UpdateData()
    {
        VisibleElementCount = 0;

        for (int i = 0; i < Characters.Count; i++)
        {
            if (Characters[i].IsVisible())
            {
                CharacterData character = Characters[VisibleElementCount];
                character.SetActualIndex(i);
                Characters[VisibleElementCount] = character;
                VisibleElementCount++;
            }
        }
    }

    public void GenerateBuffers()
    {
        TransformationSSBO.Renew(TransformationMatrices);
        CharacterSSBO.Renew(Characters);
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
            TransformationSSBO.Update(TransformationMatrices, 0);
            CharacterSSBO.Update(Characters, 0);
            _updateData = false;
        }
    }

    public void Render()
    {
        TextVAO.Bind();
        CharacterSSBO.Bind(0);
        TransformationSSBO.Bind(1);

        GL.DrawArrays(PrimitiveType.Triangles, 0, VisibleElementCount * 6);

        CharacterSSBO.Unbind();
        TransformationSSBO.Unbind();
        TextVAO.Unbind();
    }

    public void Clear()
    {
        Elements.Clear();
        TransformationMatrices.Clear();
        Characters.Clear();
        TextElements.Clear();
        TextDataIndices.Clear();

        ElementCount = 0;
        VisibleElementCount = 0;
    }

    public void Delete()
    {
        TextVAO.DeleteBuffer();
        TransformationSSBO.DeleteBuffer();
        CharacterSSBO.DeleteBuffer();

        Clear();
    }
}

public class TextElementData
{
    public CharacterData Character = new CharacterData();

    public TextElementData(CharacterData character)
    {
        Character = character;
    }

    public void SetCharacterIndex(int index)
    {
        Character.SetCharacterIndex(index); 
    }

    public void SetActualIndex(int index)
    {
        Character.SetActualIndex(index);
    }

    public void SetElementIndex(int index)
    {
        Character.SetElementIndex(index);
    }

    public void SetVisibility(bool visible)
    {
        Character.SetVisibility(visible);
    }
}

public struct CharacterData
{
    public Vector4 PositionSize;
    public Vector4i Index; // { index, *isVisible (2nd last bit) / actual index*, none, none }

    public void SetCharacterIndex(int index)
    {
        Index.X = index;
    }

    public bool IsVisible()
    {
        return (Index.Y & 0x40000000) != 0;
    }

    public void SetVisibility(bool visible)
    {
        if (visible)
            Index.Y |= 0x40000000;
        else
            Index.Y &= 0x0FFFFFFF;
    }

    public int GetActualIndex()
    {
        return Index.Y & 0x0FFFFFFF;
    }

    public void SetElementIndex(int index)
    {
        Index.Z = index;
    }

    public bool SetActualIndex(int index)
    {
        if (GetActualIndex() == index)
            return false;

        Index.Y = (Index.Y & 0x40000000) | index;
        return true;
    }

    public override string ToString()
    {
        return $"\n---Character---\nPosition: ({PositionSize.X}, {PositionSize.Y})\nSize: ({PositionSize.Z}, {PositionSize.W})\nCharacter: {Index.X}\nVisible: {IsVisible()}\nActualIndex: {GetActualIndex()}";
    }
}