using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class UIInputField : UIText
{
    public SerializableEvent? OnTextChange = null;

    public UIInputField(
        string name,
        UIController controller,
        AnchorType anchorType,
        PositionType positionType,
        Vector4 color,
        Vector3 pivot,
        Vector2 scale,
        Vector4 offset,
        float rotation,
        int textureIndex,
        Vector2 slice
    ) : base(name, controller, anchorType, positionType, color, pivot, scale, offset, rotation)
    {
        SetOnClick(() => UIController.AssignInputField(this));
    }

    public override void SetOffset(Vector4 offset)
    {
        base.SetOffset(offset);
    }

    public override void SetAnchorType(AnchorType anchorType)
    {
        base.SetAnchorType(anchorType);
    }

    public override void SetPositionType(PositionType positionType)
    {
        base.SetPositionType(positionType);
    }

    public UIInputField SetOnTextChange(Action onTextChange)
    {
        OnTextChange = new SerializableEvent(onTextChange);
        CanTest = true;
        return this;
    }

    public UIInputField SetOnTextChange(SerializableEvent onTextChange)
    {
        OnTextChange = onTextChange;
        CanTest = true;
        return this;
    }


    public override UIText SetText(string text)
    {
        UIText uiText = base.SetText(text);
        return uiText;
    }

    public override void Align()
    {
        base.Align();
    }

    public override bool Test(Vector2 offset = default)
    {
        if (UIController.activeInputField == this && Input.IsMousePressed(MouseButton.Left) && !IsMouseOver())
        {
            UIController.RemoveInputField();
        }
        return base.Test();
    }

    public void AddCharacter(char character)
    {
        //Console.WriteLine("Text: " + Text + " Character: " + character);
        if (!TextShaderHelper.CharExists(character)) return;
        string formatedText = Format(Text + character);
        //Console.WriteLine("Formated Text: " + formatedText);
        SetText(formatedText).UpdateCharacters();
    }

    public void RemoveCharacter()
    {
        if (Text.Length <= 0) return;
        SetText(Text[..^1]).UpdateCharacters();
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
            string final = "";
            int dotCount = 0;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '-' && i == 0)
                {
                    final += c;
                }
                else if (c == '.' && dotCount == 0)
                {
                    final += c;
                    dotCount++;
                }
                else if (char.IsDigit(c))
                {
                    final += c;
                }
            }
            if (final.Length > 0 && final[0] == '.')
            {
                final = "0" + final;
            }
            return final;
        }
        else if (TextType == TextType.Alphabetic)
        {
            return new string(text.Where(c => char.IsLetter(c) || char.IsWhiteSpace(c)).ToArray());
        }
        else if (TextType == TextType.Alphanumeric)
        {
            return new string(text.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)).ToArray());
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
        lines.Add(gapString + "    MaxCharCount: " + MaxCharCount);
        lines.Add(gapString + "    TextType: " + (int)TextType);
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