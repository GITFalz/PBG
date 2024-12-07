using OpenTK.Mathematics;

public class UI_Text : UI_Base
{
    public string text;
    public char[] textArray;
    
    public UI_Text(string text, UiAnchorAlignment alignment, Vector4 offset, Vector2 size)
    {
        name = "UI_Text";
        
        this.text = text;
        textArray = text.ToCharArray();
        
        this.baseOffset = offset;
        this.baseSize = size;
        
        anchorAlignment = alignment;
    }

    public override void RenderText()
    {
        
    }
}