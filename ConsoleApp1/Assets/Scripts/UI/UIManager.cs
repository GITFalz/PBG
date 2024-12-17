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
    
    private UI_Text text;
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
    public TextMesh textMesh = new TextMesh();
    
    
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
        text = new UI_Text(textMesh, 0);
        
        text.SetText("1000", 1f);
        
        text.SetOffset(new Vector4(0, 0, 0, 0));
        text.SetAnchorAlignment(UiAnchorAlignment.TopLeft);
        text.SetAnchorReference(UiAnchor.Absolute);
        
        text2 = new UI_Text(textMesh, 4);
        
        text2.SetText("Hello world", 1f);
        
        text2.SetOffset(new Vector4(0, 0, 0, 0));
        text2.SetAnchorAlignment(UiAnchorAlignment.BottomRight);
        text2.SetAnchorReference(UiAnchor.Absolute);
        
        
        // UI
        UI_Panel panel = new UI_Panel(uiMesh);
        
        panel.SetSize(new Vector2(100, 100));
        panel.SetOffset(new Vector4(0, 0, 200, 0));
        panel.SetAnchorAlignment(UiAnchorAlignment.MiddleCenter);
        panel.SetAnchorReference(UiAnchor.Absolute);
        
        
        // Add to list
        _ui.AddElement(text);
        //_ui.AddElement(text2);
        _ui.AddElement(panel);
        
        _meshes.Add(textMesh);
        _meshes.Add(uiMesh);
        
        GenerateMeshes();
        
        RenderText();
        RenderUI();
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
        int textCharsLoc = GL.GetUniformLocation(_textShader.ID, "chars");
        
        GL.UniformMatrix4(textProjectionLoc, true, ref OrthographicProjection);
        GL.Uniform1(textCharsLoc, textMesh.chars.Length, textMesh.chars);
        
        textMesh.RenderMesh();
        
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
        Console.WriteLine("Generating meshes : " + _meshes.Count);
        
        foreach (var mesh in _meshes)
        {
            mesh.GenerateBuffers();
        }
    }

    public void UpdateFps(int fps)
    {
        string t = fps.ToString();

        int miss = 4 - t.Length;
            
        for (int i = 0; i < miss; i++)
        {
            t += " ";
        }
            
        text.SetText(t, 1);
    }
}