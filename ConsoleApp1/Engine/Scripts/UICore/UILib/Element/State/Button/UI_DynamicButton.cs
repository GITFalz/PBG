using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class UI_DynamicButton : UI_Button
{
    public Vector2 textureSize = new Vector2(64, 64);
    public UI_DynamicText Text;
    
    public UiMesh mesh;
    public bool holding = false;
    private int _index = 0;
    
    public UI_DynamicButton(UiMesh mesh, DynamicTextMesh textMesh)
    {
        name = "UI_Button";
        this.mesh = mesh;

        Text = new UI_DynamicText();
        Text.SetMesh(textMesh);
        GenerateBaseText("Button");
    }
    
    public UI_DynamicButton(UiMesh mesh, DynamicTextMesh textMesh, string text)
    {
        name = "UI_Button";
        this.mesh = mesh;

        Text = new UI_DynamicText();
        Text.SetMesh(textMesh);
        GenerateBaseText(text);
    }

    private void GenerateBaseText(string text)
    {
        Text.SetText(text, 1.5f);
        
        Text.SetOffset(new Vector4(0, 0, 0, 0));
        Text.SetAnchorAlignment(UiAnchorAlignment.MiddleCenter);
        Text.SetAnchorReference(UiAnchor.Relative);
        
        Text.SetParent(this);
    }
    
    // Setters
    public override void SetSize(Vector2 s)
    {
        base.SetSize(s);
        Text.SetSize(s);
    }
    
    public void SetPosition(Vector3 pos)
    {
        position = pos;
    }
    
    public override void SetTextAlignment(UiAnchorAlignment alignment)
    {
        Text.SetTextAlignment(alignment);
    }
    
    // Render
    public override void RenderUI()
    {
        Align();
        UI.Generate9Slice(position, textureSize.X, textureSize.Y, size.X, size.Y, 10f, new Vector4(10f, 10f, 10f, 10f), mesh);
    }
    
    public override void RenderText()
    {
        Text.RenderText();
    }

    public void UpdatePosition(Vector3 position)
    {
        this.position = position;
        mesh.MoveUiElement(1, position);
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

    private readonly Dictionary<TextType, TextMesh> _textMeshes = new Dictionary<TextType, TextMesh>()
    {
        { TextType.Static, new StaticTextMesh() },
        { TextType.Dynamic, new DynamicTextMesh() }
    };
    
    private readonly Dictionary<TextType, UI_Text> _textTypes = new Dictionary<TextType, UI_Text>()
    {
        { TextType.Static, new UI_StaticText() },
        { TextType.Dynamic, new UI_DynamicText() }
    };
}