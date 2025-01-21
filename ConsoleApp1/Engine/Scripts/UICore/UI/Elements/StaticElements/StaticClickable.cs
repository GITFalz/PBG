using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public abstract class StaticClickable : UiPanel
{
    public SerializableEvent? OnHover;
    public SerializableEvent? OnClick;
    public SerializableEvent? OnHold;
    public SerializableEvent? OnRelease;
    
    private bool _clicked = false;
    
    private int _uiIndex = 0;
    private bool _generated = false;
    
    public override void SetMesh(OldUiMesh uiMesh)
    {
        UiMesh = uiMesh;
    }
    
    public override void Generate(Vector3 offset)
    {
        Align();
        Create(Origin + offset);
    }
    
    public override void Generate()
    {
        Align();
        Create(Origin);
    }

    public override void Create(Vector3 position)
    {
        Panel panel = new Panel();
        
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

    public override void Test()
    {
        TestButtons(IsMouseOver());
    }
    
    public override void Test(Vector2 offset)
    {
        TestButtons(IsMouseOver(offset));
    }

    private void TestButtons(bool mouseOver)
    {
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
    
    public string GetMethodString(SerializableEvent? e)
    {
        return e == null ? "null" : (e.IsStatic ? "Static" : SceneName) + "." + e.TargetName + "." + e.MethodName + (e.FixedParameter == null ? "" : $"({e.FixedParameter})");
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