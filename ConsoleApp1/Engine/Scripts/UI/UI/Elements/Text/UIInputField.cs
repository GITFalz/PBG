using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class UIInputField : UIText
{
    public SerializableEvent? OnTextChange = null;
    public UIButton Button;

    public UIInputField(string name, AnchorType anchorType, PositionType positionType, Vector3 pivot, Vector2 scale, Vector4 offset, float rotation, int textureIndex, Vector2 slice, TextMesh? text) : base(name, anchorType, positionType, pivot, scale, offset, rotation, textureIndex, slice, text)
    {
        Button = new UIButton(name + "Button", anchorType, positionType, (0, 0, 0), pivot, scale, offset, rotation, textureIndex, slice, null, UIState.InvisibleInteractable);
        Button.OnClick = new SerializableEvent(() => UIController.AssignInputField(this));
    }

    public override void SetParent(UIElement parent)
    {
        base.SetParent(parent);
        Button.SetParent(parent);
    }
    public override void SetOffset(Vector4 offset)
    {
        base.SetOffset(offset);
        Button.SetOffset(offset);
    }

    public override void SetAnchorType(AnchorType anchorType)
    {
        base.SetAnchorType(anchorType);
        Button.SetAnchorType(anchorType);
    }

    public override void SetPositionType(PositionType positionType)
    {
        base.SetPositionType(positionType);
        Button.SetPositionType(positionType);
    }


    public override UIText SetText(string text)
    {
        UIText uiText = base.SetText(text);
        Button.Scale = Scale;
        Button.newScale = newScale;
        
        return uiText;
    }

    public override void Align()
    {
        base.Align();
        Button.Align();
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
        Console.WriteLine("Text: " + Text + " Character: " + character);
        if (!TextShaderHelper.CharExists(character)) return;
        string formatedText = Format(Text + character);
        Console.WriteLine("Formated Text: " + formatedText);
        SetText(formatedText).GenerateChars().UpdateText();
    }
    
    public void RemoveCharacter()
    {
        if (Text.Length <= 0) return;
        SetText(Text[..^1]).GenerateChars().UpdateText();
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
        lines.AddRange(Button.ToLines(gap + 1));
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