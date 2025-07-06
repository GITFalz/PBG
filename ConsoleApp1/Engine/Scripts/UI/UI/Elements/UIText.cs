using OpenTK.Mathematics;

public class UIText : UIElement
{
    public static UIText Empty = new();

    public int MaxCharCount = 20;
    public string Text = "";
    public string finalText = "";
    public float FontSize = 1.5f;
    public char[] Characters = [];
    public int CharCount = 0;
    public List<int> Chars = [];

    public List<TextElementData> CharacterDataList = [];

    public TextType TextType = TextType.Alphabetic;
    public int TextOffset = 0;

    public TextMesh TextMesh;

    private int _mask = 0x40000000;

    private UIText() : base() { }
    public UIText
    (
        string name,
        UIController controller,
        AnchorType anchorType,
        PositionType positionType,
        Vector4 color,
        Vector3 pivot,
        Vector2 scale,
        Vector4 offset,
        float rotation) :
        base(name, controller, anchorType, positionType, pivot, scale, offset, rotation)
    {
        TextMesh = controller.TextMesh;
    }

    public override void SetVisibility(bool visible)
    {
        if (Visible == visible)
            return;

        base.SetVisibility(visible);
        foreach (var character in CharacterDataList)
        {
            character.SetVisibility(visible);
        }
        UpdateCharacters();
    }

    public UIText SetText(string text, float fontSize)
    {
        FontSize = fontSize;
        return SetText(text);
    }

    public UIText SetTextType(TextType textType)
    {
        TextType = textType;
        return this;
    }

    public UIText SetMaxCharCount(int maxCharCount)
    {
        MaxCharCount = maxCharCount;
        return this;
    }

    public UIText SetTextCharCount(string text, float fontSize)
    {
        MaxCharCount = text.Length;
        return SetText(text, fontSize);
    }

    public virtual UIText SetText(string text)
    {
        Text = ClampText(text, 0, MaxCharCount);
        finalText = AddSpaces(Text, MaxCharCount);
        CharCount = Text.Length;
        Characters = finalText.ToCharArray();
        Scale = new Vector2(MaxCharCount * (10 * FontSize), 10 * FontSize);

        return this;
    }

    public float ParseFloat(float replacement = 0f)
    {
        float value;
        string text = Text;
        if (text.Length == 0 || text.EndsWith("."))
            text += "0";
        if (Float.TryParse(text, out float scaleValue))
            value = scaleValue;
        else
            value = replacement;
        return value;
    }

    public int ParseInt(int replacement = 0)
    {
        int value;
        string text = Text;
        if (int.TryParse(text, out int scaleValue))
            value = scaleValue;
        else
            value = replacement;
        return value;
    }

    protected override void Internal_UpdateTransformation()
    {
        TextMesh.UpdateElementTransformation(this);
    }

    public override void Generate()
    {
        SetScale(Scale);
        GenerateChars();
        GenerateQuad();
    }

    public override void SetMaskIndex(int maskedIndex)
    {
        base.SetMaskIndex(maskedIndex);
        for (int i = 0; i < CharacterDataList.Count; i++)
        {
            CharacterData Character = CharacterDataList[i].Character;
            Character.Index.W = Masked ? (MaskIndex | _mask) : 0;
            CharacterDataList[i].Character = Character;
        }
        TextMesh.UpdateCharacters(this, CharacterDataList);
    }

    public override void Delete(bool baseOnly = false)
    {

        base.Delete();
        if (baseOnly)
            return;
        TextMesh.RemoveElement(this);
        CharacterDataList.Clear();
        Chars.Clear();
    }

    public void SetCharacterElementIndex(int index)
    {
        foreach (var character in CharacterDataList)
        {
            character.SetElementIndex(index);
        }
    }

    public void UpdateCharacters()
    {
        if (CharacterDataList.Count == MaxCharCount)
        {
            int index = 0;
            foreach (var character in Characters)
            {
                int charIndex = TextShaderHelper.GetChar(character);
                CharacterDataList[index].SetCharacterIndex(charIndex);
                index++;
            }
            TextMesh.UpdateCharacters(this, CharacterDataList);
        }
    }

    private UIText GenerateChars()
    {
        Chars.Clear();
        CharacterDataList.Clear();

        int index = 0;
        foreach (var character in Characters)
        {
            int charIndex = TextShaderHelper.GetChar(character);
            Chars.Add(charIndex);

            CharacterData Character = new CharacterData
            {
                PositionSize = (index * (10 * FontSize), 0, 10 * FontSize, 10 * FontSize),
                Index = (charIndex, Visible ? _mask : 0, 0, Masked ? (MaskIndex | _mask) : 0)
            };

            TextElementData textElementData = new TextElementData(Character);
            CharacterDataList.Add(textElementData);

            index++;
        }

        return this;
    }

    public void GenerateQuad()
    {
        TextMesh.AddElement(this);
    }


    public static string ClampText(string text, int min, int max)
    {
        if (text.Length < min)
        {
            return text.PadRight(min, ' ');
        }
        else if (text.Length > max)
        {
            return text[..max];
        }
        return text;
    }

    public static string AddSpaces(string text, int maxCount)
    {
        if (text.Length > maxCount)
        {
            return text[..maxCount];
        }
        else if (text.Length < maxCount)
        {
            return text.PadRight(maxCount, ' ');
        }
        return text;
    }

    public override List<string> ToLines(int gap)
    {
        List<string> lines = new List<string>();
        string gapString = new(' ', gap * 4);

        lines.Add(gapString + "Text");
        lines.Add(gapString + "{");
        lines.AddRange(GetBasicDisplayLines(gapString));
        lines.Add(gapString + "    Text: " + Text);
        lines.Add(gapString + "    FontSize: " + FontSize);
        lines.Add(gapString + "    MaxCharCount: " + MaxCharCount);
        lines.Add(gapString + "    TextType: " + (int)TextType);
        lines.Add(gapString + "}");

        return lines;
    }


    public override float GetYScale()
    {
        return Scale.Y;
    }

    public override float GetXScale()
    {
        return Scale.X;
    }
}