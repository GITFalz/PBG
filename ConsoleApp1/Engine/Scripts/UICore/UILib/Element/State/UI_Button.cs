using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class UI_Button : UI_Base
{
    public Vector2 textureSize = new Vector2(64, 64);
    public UI_Text text = new UI_Text();

    public Action OnClick;
    
    public UI_Button()
    {
        name = "UI_Button";
        
        text.SetText("Button", 2);
        
        text.SetOffset(new Vector4(0, 0, 0, 0));
        text.SetAnchorAlignment(UiAnchorAlignment.MiddleCenter);
        text.SetAnchorReference(UiAnchor.Relative);
        
        text.SetParent(this);
    }
    
    // Setters
    public override void SetSize(Vector2 s)
    {
        base.SetSize(s);
        text.SetSize(s);
    }
    
    public override void SetMem(int pos)
    {
        base.SetMem(pos);
        text.SetMem(pos + 1);
    }
    
    public void SetTextAlignment(UiAnchorAlignment alignment)
    {
        text.SetTextAlignment(alignment);
    }
    
    
    // Render
    public override void RenderUI(MeshData meshData)
    {
        Align();
        UI.Generate9Slice(position, textureSize.X, textureSize.Y, size.X, size.Y, 10f, new Vector4(10f, 10f, 10f, 10f), meshData);
    }
    
    public override void RenderText(MeshData meshData)
    {
        text.RenderText(meshData);
    }


    public bool IsPointInBounds(Vector2 pos)
    {
        return pos.X > position.X && pos.X < position.X + size.X && pos.Y > position.Y && pos.Y < position.Y + size.Y;
    }
    
    public bool IsButtonPressed(MouseState mouse)
    {
        return IsPointInBounds(mouse.Position) && mouse.IsButtonPressed(MouseButton.Left);
    }
}