using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class UI_Button : UI_Base
{
    public Vector2 textureSize = new Vector2(64, 64);
    public UI_Text text = new UI_Text();

    public Action OnClick;
    public Action<MouseState> OnHold;
    public Action OnRelease;

    public bool holding = false;
    
    public UI_Button()
    {
        name = "UI_Button";
        
        GenerateBaseText();
    }
    
    public UI_Button(Mesh mesh)
    {
        name = "UI_Button";
        this.mesh = mesh;

        GenerateBaseText();
    }
    
    public UI_Button(Mesh mesh, Mesh textMesh)
    {
        name = "UI_Button";
        this.mesh = mesh;

        GenerateBaseText();
        text.SetMesh(textMesh);
    }

    private void GenerateBaseText()
    {
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
    
    public override int GetNextMem()
    {
        return memPos + text.GetNextMem();
    }
    
    public override int GetMemSize()
    {
        return memSize + text.GetMemSize();
    }
    
    public void SetTextAlignment(UiAnchorAlignment alignment)
    {
        text.SetTextAlignment(alignment);
    }
    
    public void SetTextMesh(Mesh m)
    {
        text.SetMesh(m);
    }
    
    
    // Render
    public override void RenderUI()
    {
        Align();
        UI.Generate9Slice(position, textureSize.X, textureSize.Y, size.X, size.Y, 10f, new Vector4(10f, 10f, 10f, 10f), mesh);
    }
    
    public override void RenderText()
    {
        text.RenderText();
    }


    public bool IsPointInBounds(Vector2 pos)
    {
        return pos.X > position.X && pos.X < position.X + size.X && pos.Y > position.Y && pos.Y < position.Y + size.Y;
    }
    
    public bool IsButtonPressed(MouseState mouse)
    {
        if (!IsPointInBounds(mouse.Position) || !mouse.IsButtonPressed(MouseButton.Left)) return false;
        
        holding = true;
        return true;
    }

    public bool IsButtonHeld(MouseState mouse)
    {
        return IsPointInBounds(mouse.Position) && holding;
    }

    public bool IsButtonReleased(MouseState mouse)
    {
        holding = false;
        
        return IsPointInBounds(mouse.Position);
    }
}