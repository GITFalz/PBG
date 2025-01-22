using OpenTK.Mathematics;

public class UIInputField : UIText
{
    public UIInputField(string name, AnchorType anchorType, PositionType positionType, Vector3 pivot, Vector2 scale, Vector4 offset, float rotation, int textureIndex, TextMesh? text) : base(name, anchorType, positionType, pivot, scale, offset, rotation, textureIndex, text)
    {
        button = new UIButton(name + "Button", anchorType, positionType, pivot, scale, offset, rotation, textureIndex, null, UIState.InvisibleInteractable);
        button.OnClick = new SerializableEvent();
        button.OnClick.SetAction(() => UIController.AssignInputField(Name));
    }

    public UIButton button;
    public SerializableEvent? OnTextChange = null;

    public override void SetUIMesh(UIMesh uIMesh)
    {
        
        button.SetUIMesh(uIMesh);
    }

    private void UpdateButton()
    {
        button.Transformation = Transformation;
        button.Width = Width;
        button.Height = Height;
    }

    public override void Generate(ref int offset)
    {
        Align();
        UpdateButton();
        GenerateChars();
        GenerateQuad(ref offset);
    }

    public void UpdateText()
    {
        textMesh?.UpdateText();
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
        string gapString = "";
        for (int i = 0; i < gap; i++)
        {
            gapString += "    ";
        }
        
        lines.Add(gapString + "Inputfield");
        lines.Add(gapString + "{");
        lines.Add(gapString + "    Text: " + Text);
        lines.Add(gapString + "    FontSize: " + FontSize);
        lines.AddRange(GetBasicDisplayLines(gap));
        
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