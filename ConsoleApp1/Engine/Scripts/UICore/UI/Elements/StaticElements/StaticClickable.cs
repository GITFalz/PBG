using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public abstract class StaticClickable : StaticElement
{
    public UiMesh UiMesh;

    public SerializableEvent? OnHover;
    public SerializableEvent? OnClick;
    public SerializableEvent? OnHold;
    public SerializableEvent? OnRelease;
    
    private bool _clicked = false;
    
    public int TextureIndex = 0;
    
    private int _uiIndex = 0;
    private bool _generated = false;
    
    public override void SetMesh(UiMesh uiMesh)
    {
        UiMesh = uiMesh;
    }
    
    public override void Generate()
    {
        Align();
        Panel panel = new Panel();

        Vector3 position = Origin;
        
        panel.Vertices.Add(new Vector3(0, 0, 0) + position);
        panel.Vertices.Add(new Vector3(0, Scale.Y, 0) + position);
        panel.Vertices.Add(new Vector3(Scale.X, Scale.Y, 0) + position);
        panel.Vertices.Add(new Vector3(Scale.X, 0, 0) + position);
        
        panel.Uvs.Add(new Vector2(0, 0));
        panel.Uvs.Add(new Vector2(0, 1));
        panel.Uvs.Add(new Vector2(1, 1));
        panel.Uvs.Add(new Vector2(1, 0));
        
        panel.TextUvs.Add(TextureIndex);
        panel.TextUvs.Add(TextureIndex);
        panel.TextUvs.Add(TextureIndex);
        panel.TextUvs.Add(TextureIndex);
        
        panel.UiSizes.Add(new Vector2(Scale.X, Scale.Y));
        panel.UiSizes.Add(new Vector2(Scale.X, Scale.Y));
        panel.UiSizes.Add(new Vector2(Scale.X, Scale.Y));
        panel.UiSizes.Add(new Vector2(Scale.X, Scale.Y));
        
        if (!_generated)
        {
            UiMesh.AddPanel(panel, out _uiIndex);
            _generated = true;
        }
        else
            UiMesh.UpdatePanel(panel, _uiIndex);
    }
    
    public override void Reset()
    {
        _generated = false;
        _uiIndex = 0;
    }
    
    public bool IsMouseOver()
    {
        Vector2 pos = Input.GetMousePosition();
        return pos.X >= Origin.X && pos.X <= Origin.X + Scale.X && pos.Y >= Origin.Y && pos.Y <= Origin.Y + Scale.Y;
    }

    public override void Test()
    {
        bool mouseOver = IsMouseOver();
        
        if (mouseOver)
        {
            OnHover?.Invoke();
            
            if ( Input.IsMousePressed(MouseButton.Left) && !_clicked)
            {
                OnClick?.Invoke();
                _clicked = true;
            }
        }
        
        if (_clicked)
        {
            Game.SetCursorState(CursorState.Grabbed);
            OnHold?.Invoke();
        }
            
        if (Input.IsMouseReleased(MouseButton.Left) && _clicked)
        {
            Game.SetCursorState(CursorState.Normal);
            OnRelease?.Invoke();
            _clicked = false;
        }
    }

    public override void ToFile(string path, int gap = 1)
    {
        File.WriteAllLines(path, ToLines(gap));
    }

    public override string ToString()
    {
        return
            "Position: " + Position + "\n" +
            "Scale: " + Scale + "\n" +
            "Offset: " + Offset + "\n" +
            "AnchorType: " + AnchorType + "\n" +
            "PositionType: " + PositionType + "\n" +
            "TextureIndex: " + TextureIndex + "\n";
    }
}