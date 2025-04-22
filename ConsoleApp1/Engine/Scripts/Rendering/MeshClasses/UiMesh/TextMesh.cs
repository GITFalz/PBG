using OpenTK.Mathematics;

public class TextMesh
{
    public VAO TextVAO = new();
    public SSBO<Vector4i> CharacterSSBO = new();
    public SSBO<LineStruct> LineSSBO = new();

    public List<Vector4i> Characters = [];
    public List<LineStruct> Lines = [];

    public List<TextElementData> Elements = new();

    public int ElementCount = 0;
    public int VisibleElementCount = 0;

    public void AddElement(UIText element, ref int uiIndex)
    {
        TextElementData data = new TextElementData()
        {
            Element = element,
            CharCount = element.MaxCharCount
        };

        if (Elements.Contains(data))
            return;
         uiIndex = ElementCount;
        Elements.Add(data);

        UpdateFirstIndex(data);

        for (int i = 0; i < element.MaxCharCount; i++)
            Characters.Add(new Vector4i(-1, ElementCount, i, 0)); // { character index, element index, line index, none }

        ElementCount++;
        VisibleElementCount++;
    }

    public void UpdateCharCount(UIText element)
    {
        if (GetElementData(element, out var data))
        {
            int oldCharCount = data.CharCount;
            int newCharCount = element.MaxCharCount;

            if (oldCharCount == newCharCount)
                return; // No change in character count

            int firstIndex = data.FirstIndex;

            if (newCharCount > oldCharCount)
            {
                for (int i = oldCharCount; i < newCharCount; i++)
                {
                    Characters.Insert(firstIndex + i, new Vector4i(-1, data.FirstIndex, i, 0));
                }
            }
            else if (newCharCount < oldCharCount)
            {
                for (int i = oldCharCount - 1; i >= newCharCount; i--)
                {
                    Characters.RemoveAt(firstIndex + i);
                }
            }

            int index = Elements.IndexOf(data);
            data.CharCount = element.MaxCharCount;
            Elements[index] = data;
            UpdateFirstIndex(index+1); // Update the first index of the next element if it exists
        }
    }

    public void RemoveElement(UIText element)
    {
        if (GetElementData(element, out var data))
        {
            int index = Elements.IndexOf(data); // Get the index of the element in the list
            Elements.Remove(data);
            UpdateFirstIndex(index); // Use the same index because the previous element has been removed
            
            ElementCount--;
            VisibleElementCount--;
        }
    }
    
    public void UpdateFirstIndex(int index)
    {
        if (index < Elements.Count && index >= 0)
        {
            var element = Elements[index];
            UpdateFirstIndex(element);
        }
    }

    public void UpdateFirstIndex(UIText element)
    {
        if (GetElementData(element, out var data))
            UpdateFirstIndex(data);
    }

    public void UpdateFirstIndex(TextElementData element)
    {
        int index = Elements.IndexOf(element);
        if (index == 0)
        {
            element.FirstIndex = 0;
            Elements[index] = element;
        }
        else
        {
            var previousElement = Elements[index - 1];
            element.FirstIndex = previousElement.FirstIndex + previousElement.CharCount;
            Elements[index] = element;
        }

        if (index+1 < Elements.Count)
        {
            var nextElement = Elements[index+1];
            UpdateFirstIndex(nextElement); // Recursive call to update the next element's first index
        }
    }

    private bool GetElementData(UIText element, out TextElementData data)
    {
        foreach (var dataElement in Elements)
        {
            if (dataElement.Element == element)
            {
                data = dataElement;
                return true;
            }
        }

        data = TextElementData.Empty;
        return false;
    }

    /// <summary>
    /// Only use if the number of characters hasn't changed.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="characters"></param>
    public void SetCharacters(UIText element, List<int> characters)
    {
        if (GetElementData(element, out var data))
        {
            for (int i = 0; i < characters.Count && i < element.MaxCharCount; i++)
            {
                Characters[data.FirstIndex + i] = new Vector4i(characters[i], data.FirstIndex, i, 0);
            }
        }
    }

    public void GenerateBuffers()
    {
        CharacterSSBO.Renew(Characters);
        LineSSBO.Renew(Lines);
    }

    public void Update()
    {
        UpdateText();
        UpdateLines();
    }

    public void UpdateText()
    {
        CharacterSSBO.Update(Characters, 0);
    }

    public void UpdateLines()
    {
        LineSSBO.Update(Lines, 0);
    }
}

public struct LineStruct
{
    public Vector2 Size;
    public Matrix4 Transformation;
    public Vector4 Data; // includes: spacing
    public Vector2i Index; // includes: first char index
};

public struct TextElementData
{
    public static readonly TextElementData Empty = new TextElementData()
    {
        Element = UIText.Empty,
        FirstIndex = 0,
        CharCount = 0
    };

    public UIText Element;
    public int FirstIndex; // Index of the first character
    public int CharCount; // Number of characters in the text

    public readonly bool Equals(TextElementData other) 
    {
        return ReferenceEquals(Element, other.Element);
    }

    public override readonly bool Equals(object? obj) 
    {
        return obj is TextElementData other && Equals(other);
    }

    public override readonly int GetHashCode() 
    {
        return Element.GetHashCode();
    }
}