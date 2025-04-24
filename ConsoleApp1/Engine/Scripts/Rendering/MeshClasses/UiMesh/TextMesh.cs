using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class TextMesh
{
    public VAO TextVAO = new();
    public SSBO<Matrix4> TransformationSSBO = new();
    public SSBO<CharacterData> CharacterSSBO = new();

    public List<UIText> UIElements = new();
    
    public List<Matrix4> TransformationMatrices = new();
    public List<CharacterData> Characters = new();
    public List<TextElementData> TextElements = new();
    public Dictionary<TextElementData, int> Elements = new();

    public int ElementCount = 0;
    public int VisibleElementCount = 0;

    private bool _generateBuffers = false;
    private bool _updateData = false;
    private bool _updateVisibility = false;

    public void AddElement(UIText element, ref int uiIndex)
    {
        uiIndex = ElementCount;

        TransformationMatrices.Add(element.Transformation);
        UIElements.Add(element);

        ElementCount++;
    }

    public void AddCharacters(List<TextElementData> characters)
    {
        foreach (var character in characters)
        {
            AddCharacter(character);
        }

        _updateVisibility = true;
    }

    public void AddCharacter(TextElementData character)
    {
        Elements.Add(character, Elements.Count);
        Characters.Add(character.Character);
        TextElements.Add(character);

        VisibleElementCount++;

        _generateBuffers = true;
    }

    public void RemoveElement(UIText element)
    {
        RemoveCharacters(element.CharacteData);

        UIElements.RemoveAt(element.ElementIndex);
        TransformationMatrices.RemoveAt(element.ElementIndex);

        for (int i = element.ElementIndex; i < UIElements.Count; i++)
        {
            UIElements[i].ElementIndex--;
        }

        ElementCount--;

        _generateBuffers = true;
        _updateVisibility = true;
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
        if (Elements.TryGetValue(character, out int index))
        {
            int lastIndex = TextElements.Count - 1;

            if (index != lastIndex)
            {
                var lastElement = TextElements[lastIndex];
                var lastCharacter = Characters[lastIndex];

                TextElements[index] = lastElement;
                Characters[index] = lastCharacter;

                Elements[lastElement] = index;
            }

            TextElements.RemoveAt(lastIndex);
            Characters.RemoveAt(lastIndex);
            Elements.Remove(character);

            VisibleElementCount--;

            _generateBuffers = true;
        }
    }

    public void UpdateCharacters(List<TextElementData> characters)
    {
        foreach (var character in characters)
        {
            UpdateCharacter(character);
        }
    }

    public void UpdateCharacter(TextElementData character)
    {
        if (Elements.TryGetValue(character, out int index))
        {
            Characters[index] = character.Character;
            _updateVisibility = true;
        }
    }

    public void SetVisibility(List<TextElementData> characters, bool visible)
    {
        foreach (var character in characters)
        {
            SetVisibility(character, visible);
        }
    }

    public void SetVisibility(TextElementData character, bool visible)
    {
        if (Elements.TryGetValue(character, out int index))
        {
            character.Character.SetVisible(visible);
            Characters[index] = character.Character;
            _updateVisibility = true;
        }
    }

    public void UpdateElementTransformation(UIText element)
    {
        if (element.ElementIndex < 0 || element.ElementIndex >= TransformationMatrices.Count)
            return;

        TransformationMatrices[element.ElementIndex] = element.Transformation;
        _updateData = true;
    }

    public void UpdateData()
    {
        VisibleElementCount = 0;

        for (int i = 0; i < TextElements.Count; i++)
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
}

public class TextElementData
{
    public CharacterData Character = new CharacterData();
}

public struct CharacterData
{
    public Vector4 PositionSize;
    public Vector4i Index; // { index, *isVisible (2nd last bit) / actual index*, none, none }

    public bool IsVisible()
    {
        return (Index.Y & 0x40000000) != 0;
    }

    public void SetVisible(bool visible)
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