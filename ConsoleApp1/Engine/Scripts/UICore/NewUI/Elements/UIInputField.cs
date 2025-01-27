using OpenTK.Mathematics;

public class UIInputField : UIText
{
    public SerializableEvent? OnTextChange = null;
    public UIButton Button;

    public UIInputField(string name, AnchorType anchorType, PositionType positionType, Vector3 pivot, Vector2 scale, Vector4 offset, float rotation, int textureIndex, TextMesh? text) : base(name, anchorType, positionType, pivot, scale, offset, rotation, textureIndex, text)
    {
        Button = new UIButton(name + "Button", anchorType, positionType, pivot, scale, offset, rotation, textureIndex, null, UIState.InvisibleInteractable);
        Button.OnClick = new SerializableEvent(() => UIController.AssignInputField(Name));
    }

    public override void Generate(ref int offset)
    {
        Align();
        GenerateChars();
        GenerateQuad(ref offset);
    }

    public void AddCharacter(char character)
    {
        Console.WriteLine("InputField: " + character);
        if (!TextShaderHelper.CharExists(character)) 
            return;
        SetText(Format(Text + character));
        GenerateChars();
        UpdateText();
    }
    
    public void RemoveCharacter()
    {
        if (Text.Length <= 0) return;
        SetText(SetLastCharToSpace(Text));
        GenerateChars();
        UpdateText();
    }

    public static string SetLastCharToSpace(string Text)
    {
        for (int i = Text.Length - 1; i >= 0; i--)
        {
            if (Text[i] != ' ')
            {
                Text = Text.Remove(i, 1).Insert(i, " ");
                break;
            }
        }
        return Text;
    }
    
    public string Format(string text)
    {
        if (TextType == TextType.Numeric)
        {
            return new string(text.Where(char.IsDigit).ToArray());
        }
        else if (TextType == TextType.Decimal)
        {
            int dotCount = 0;
            return new string(text.Where(c => {
                if (c != '.') 
                    return char.IsDigit(c);
                dotCount++;
                return dotCount <= 1;
            }).ToArray());
        }
        else if (TextType == TextType.Alphabetic)
        {
            return new string(text.Where(char.IsLetter).ToArray());
        }
        else if (TextType == TextType.Alphanumeric)
        {
            return new string(text.Where(char.IsLetterOrDigit).ToArray());
        }
        else
        {
            return text;
        }
    }

    public override List<string> ToLines(int gap)
    {
        List<string> lines = new List<string>();
        string gapString = new(' ', gap * 4);
        
        lines.Add(gapString + "Inputfield");
        lines.Add(gapString + "{");
        lines.AddRange(GetBasicDisplayLines(gapString));
        lines.Add(gapString + "    Text: " + Text);
        lines.Add(gapString + "    FontSize: " + FontSize);
        lines.Add(gapString + "}");
        
        return lines;
    }
}

public enum TextType
{
    Numeric,
    Decimal,
    Alphabetic,
    Alphanumeric,
    All
}