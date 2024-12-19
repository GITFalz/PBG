using System.Diagnostics;
using ConsoleApp1.Assets.Scripts.Inputs;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class UIManager
{
    // UI
    private ShaderProgram _uiShader;
    private TextureArray _uItexture;
    
    public Matrix4 OrthographicProjection;
    
    
    // Text
    private ShaderProgram _textShader;
    private Texture _textTexture;
    
    private UI_StaticText text;
    private UI_Text text2;
    
    
    // Input
    private bool _isTyping = false;
    private Character _currentCharacter;
    private HashSet<Character> _pressedCharacters = new HashSet<Character>();
    private Queue<Character> _toBeAdded = new Queue<Character>();

    private List<Character> _inputField = new List<Character>();
    
    private List<Mesh> _meshes = new List<Mesh>() { };

    
    // Controller
    private UIController _ui = new UIController();
    
    int frameCount = 0;
    float elapsedTime = 0;
    Stopwatch stopwatch = new Stopwatch();
    
    
    public UiMesh uiMesh = new UiMesh();
 
    public StaticTextMesh textMesh = new StaticTextMesh();
    public DynamicTextMesh dynamicTextMesh = new DynamicTextMesh();
    
    
    private Vector2 _buttonOldMousePosition;
    
    
    public void Load()
    {
        // Load UI
        _uiShader = new ShaderProgram("UI/UI.vert", "UI/UI.frag");
        _uItexture = new TextureArray("UI_Atlas.png", 64, 64);
        
        
        // Load Text
        _textShader = new ShaderProgram("Text/Text.vert", "Text/Text.frag");
        _textTexture = new Texture("text.png");
        
    }

    public void Start()
    {
        // Text
        text = new UI_StaticText(textMesh, "10000", 1.5f);
        
        text.SetOffset(new Vector4(0, 0, 0, 0));
        text.SetAnchorAlignment(UiAnchorAlignment.TopLeft);
        text.SetAnchorReference(UiAnchor.Absolute);
        
        
        // UI
        UI_Panel panel = new UI_Panel(uiMesh);
        
        panel.SetSize(new Vector2(300, 200));
        panel.SetOffset(new Vector4(0, 0, 0, 0));
        panel.SetAnchorAlignment(UiAnchorAlignment.BottomLeft);
        
        
        UI_DynamicButton button1 = new UI_DynamicButton(uiMesh, dynamicTextMesh, "O");
        
        button1.SetSize(new Vector2(20, 20));
        button1.SetOffset(new Vector4(10, 10, 10, 10));
        button1.SetAnchorAlignment(UiAnchorAlignment.BottomLeft);
        button1.SetAnchorReference(UiAnchor.Free);
        
        button1.OnClick = () =>
        {
            _buttonOldMousePosition = InputManager.GetMousePosition();
            Console.WriteLine("Clicked");
        };
        
        button1.OnHold = (mouse) =>
        {
            if ((InputManager.GetMousePosition() - _buttonOldMousePosition).Length < 0.1f)
                return;
            
            float y = Mathf.Clamp(Game.height - 180, Game.height - 50, mouse.Y - button1.size.Y);
            
            button1.position = new Vector3(10, y, 0.1f);
            Vector3 center = new Vector3(button1.position.X + button1.halfSize.X, button1.position.Y + button1.halfSize.Y, 0.1f);
            
            button1.UpdatePosition(center);
            button1.Text.UpdatePosition(center);
        };
        
        
        // Add to list
        _ui.AddElement(text);
        _ui.AddElement(panel);
        _ui.AddElement(button1);
        
        _meshes.Add(textMesh);
        _meshes.Add(dynamicTextMesh);
        _meshes.Add(uiMesh);
        
        RenderText();
        RenderUI();
        
        GenerateMeshes();
    }

    public void OnResize()
    {
        foreach (var mesh in _meshes)
        {
            mesh.Clear();
        }
        
        RenderUI();
        RenderText();
        
        GenerateMeshes();
    }
    
    public void RenderUI()
    {
        foreach (var uiElement in _ui.GetElements())
        {
            uiElement.RenderUI();
        }
    }
    
    public void RenderText()
    {
        foreach (var uiElement in _ui.GetElements())
        {
            uiElement.RenderText();
        }
    }
    
    public void Unload()
    {
        _uiShader.Delete();
        _uItexture.Delete();
        
        foreach (var mesh in _meshes)
        {
            mesh.Clear();
            mesh.Delete();
        }
        
        _textShader.Delete();
        _textTexture.Delete();
        
        _inputField.Clear();
    }
    
    public void OnRenderFrame(FrameEventArgs args)
    {
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        
        _uiShader.Bind();
        _uItexture.Bind();
        
        int projectionLoc = GL.GetUniformLocation(_uiShader.ID, "projection");
        GL.UniformMatrix4(projectionLoc, true, ref OrthographicProjection);
        
        uiMesh.RenderMesh();
        
        _uiShader.Unbind();
        _uItexture.Unbind();
        
        _textShader.Bind();
        _textTexture.Bind();

        int textProjectionLoc = GL.GetUniformLocation(_textShader.ID, "projection");
        int charsLoc = GL.GetUniformLocation(_textShader.ID, "charBuffer");
        
        GL.UniformMatrix4(textProjectionLoc, true, ref OrthographicProjection);
        GL.Uniform1(charsLoc, 1);
        
        textMesh.RenderMesh();
        dynamicTextMesh.RenderMesh();
        
        _textShader.Unbind();
        _textTexture.Unbind();
    }

    public void OnUpdateFrame(KeyboardState keyboard, MouseState mouse, FrameEventArgs args)
    {
        _ui.Update(mouse);
    }
    
    private void InputField(KeyboardState keyboard, MouseState mouse, FrameEventArgs args)
    {
        if (keyboard.IsAnyKeyDown)
        {
            List<Character> pressedCharacters = InputManager.GetPressedKeys(keyboard);

            foreach (var character in pressedCharacters)
                if (!_pressedCharacters.Contains(character))
                    _toBeAdded.Enqueue(character);
        
            _pressedCharacters.Clear();
            _pressedCharacters.UnionWith(pressedCharacters);
        }
        else if (_pressedCharacters.Count > 0)
        {
            _pressedCharacters.Clear();
        }
    }

    private void GenerateMeshes()
    {
        foreach (var mesh in _meshes)
        {
            mesh.GenerateBuffers();
        }
    }

    public void UpdateFps(int fps)
    {
        string t = fps.ToString();

        int miss = 5 - t.Length;
            
        for (int i = 0; i < miss; i++)
        {
            t += " ";
        }
            
        text.UpdateTextWithSameLength(t);
    }
}