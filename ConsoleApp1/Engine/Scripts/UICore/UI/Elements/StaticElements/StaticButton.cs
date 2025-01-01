using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class StaticButton : StaticElement
{
    public UiMesh Mesh;
    public TextMesh TextMesh;
    
    public StaticText? Text;

    public Action OnHover;
    public Action OnClick;
    public Action OnHold;
    public Action OnRelease;
    
    private bool _clicked = false;
    
    public int TextureIndex = 0;
    
    public StaticButton(StaticText text)
    {
        Name = "Static Button";
        
        Text = text;
        
        AnchorType = AnchorType.MiddleCenter;
        PositionType = PositionType.Absolute;
    }
    
    public StaticButton()
    {
        Name = "Static Button";
        
        AnchorType = AnchorType.MiddleCenter;
        PositionType = PositionType.Absolute;
    }
    
    public void SetMesh(UiMesh mesh)
    {
        Mesh = mesh;
    }
    
    public void SetMesh(UiMesh mesh, TextMesh textMesh)
    {
        Mesh = mesh;
        TextMesh = textMesh;
        
        Text?.SetMesh(TextMesh);
    }
    
    public override void Generate()
    {
        Align();
        if (TextureIndex >= 0)
        {
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
        
            Mesh.AddPanel(panel);
        }
        
        Text?.Generate();
    }
    
    public void UpdateText()
    {
        Text?.UpdateText();
    }
    
    public bool IsMouseOver()
    {
        Vector2 pos = Input.GetMousePosition();
        return pos.X >= Origin.X && pos.X <= Origin.X + Scale.X && pos.Y >= Origin.Y && pos.Y <= Origin.Y + Scale.Y;
    }

    public void ButtonTest()
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
}