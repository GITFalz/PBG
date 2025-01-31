﻿using OpenTK.Mathematics;

public class StaticInputField : StaticClickable
{
    public string Text;
    public float FontSize;
    
    public char[] Characters;
    public int CharCount;
    
    public Vector2 TextSize;
    
    public TextType TextType = TextType.Alphabetic;
    
    public OldTextMesh TextMesh;
    
    public SerializableEvent? OnTextChange = null;

    private int _uiIndex = 0;
    private bool _generated = false;
    
    public void SetText(string text, float fontSize)
    {
        FontSize = fontSize;
        text = Format(text);
        SetText(text);
    }
    
    public void SetText(string text)
    {
        Text = text;
        
        Characters = Text.ToCharArray();
        CharCount = Characters.Length;
        
        TextSize = new Vector2(CharCount * (20 * FontSize), 20 * FontSize);
        //Scale = new Vector3(TextSize.X, TextSize.Y, 0);
    }

    
    public StaticInputField(string name, string text, float fontSize)
    {
        Name = name;
        
        SetText(text, fontSize);
        
        AnchorType = AnchorType.MiddleCenter;
        PositionType = PositionType.Absolute;
        
        TextMesh = new OldTextMesh();  
    }
    
    public override void Generate()
    {
        Align();
        Create(Origin);
    }
    
    public override void Generate(Vector3 offset)
    {
        Align();
        Create(Origin + offset);
    }

    public override void Create(Vector3 position)
    {
        MeshQuad meshQuad = MeshHelper.GenerateTextQuad(TextSize.X, TextSize.Y, 0, CharCount, 0);
        TextMesh.SetQuad(position + new Vector3(0, 0, 1f), meshQuad);
        foreach (var character in Characters)
        {
            TextMesh.chars.Add(TextShaderHelper.GetChar(character));
        }
    }

    public override void Reset()
    {
        _generated = false;
        _uiIndex = 0;
    }

    public void UpdateText()
    {
        TextMesh.UpdateMesh();
    }
    
    public void AddCharacter(char character)
    {
        if (!TextShaderHelper.CharExists(character)) 
            return;
        SetText(Format(Text + character));
        Generate();
        UpdateText();
    }
    
    public void RemoveCharacter()
    {
        if (Text.Length <= 0) return;
        SetText(Text[..^1]);
        Generate();
        UpdateText();
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
        
        lines.Add(gapString + "Static Input Field");
        lines.Add(gapString + "{");
        lines.Add(gapString + "    Name: " + Name);
        lines.Add(gapString + "    Text: " + Text);
        lines.Add(gapString + "    FontSize: " + FontSize);
        lines.Add(gapString + "    Position: " + Position);
        lines.Add(gapString + "    Scale: " + Scale);
        lines.Add(gapString + "    Offset: " + Offset);
        lines.Add(gapString + "    AnchorType: " + (int)AnchorType);
        lines.Add(gapString + "    PositionType: " + (int)PositionType);
        lines.Add(gapString + "    TextType: " + (int)TextType);
        lines.Add(gapString + "    TextureIndex: " + TextureIndex);
        lines.Add(gapString + "    OnClick: " + GetMethodString(OnClick));
        lines.Add(gapString + "    OnHover: " + GetMethodString(OnHover));
        lines.Add(gapString + "    OnHold: " + GetMethodString(OnHold));
        lines.Add(gapString + "    OnRelease: " + GetMethodString(OnRelease));
        lines.Add(gapString + "    OnTextChange: " + GetMethodString(OnTextChange));
        lines.Add(gapString + "}");
        
        return lines;
    }
}