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
    
    UI_Text text;
    
    
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
        TextMesh textMesh = new TextMesh();
        
        RenderUI();
        
        
        // Text creation
        text = new UI_Text(textMesh);

        // Text 2
        text.SetText("1000", 1f);
        
        text.SetOffset(new Vector4(0, 0, 0, 0));
        text.SetSize(new Vector2(80, 80));
        text.SetAnchorAlignment(UiAnchorAlignment.TopLeft);
        text.SetAnchorReference(UiAnchor.Absolute);
        
        text.SetMem(0);
        
        
        // Add to list
        _ui.AddElement(text);
        
        _meshes.Add(textMesh);
        
        GenerateMeshes();
        
        RenderText();
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
        //GL.Enable(EnableCap.Blend);
        //GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        
        UpdateUi();
        
        if (_meshes[0] is not TextMesh textMesh) return;
        
        UpdateText(textMesh);
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
    
    private void UpdateUi()
    {
        _uiShader.Bind();
        _uItexture.Bind();
        
        int projectionLoc = GL.GetUniformLocation(_uiShader.ID, "projection");
        GL.UniformMatrix4(projectionLoc, true, ref OrthographicProjection);
        
        //_meshes[0].RenderMesh();
        
        _uiShader.Unbind();
        _uItexture.Unbind();
    }

    private void UpdateText(TextMesh textMesh)
    {
        _textShader.Bind();
        _textTexture.Bind();

        int textProjectionLoc = GL.GetUniformLocation(_textShader.ID, "projection");
        int textSizeLoc = GL.GetUniformLocation(_textShader.ID, "size");
        int textCharsLoc = GL.GetUniformLocation(_textShader.ID, "chars");
        
        GL.UniformMatrix4(textProjectionLoc, true, ref OrthographicProjection);
        GL.Uniform2(textSizeLoc, new Vector2(textMesh.chars.Length, 1));
        GL.Uniform1(textCharsLoc, textMesh.chars.Length, textMesh.chars);

        textMesh.RenderMesh();
        
        _textShader.Unbind();
        _textTexture.Unbind();
    }

    public void UpdateFps()
    {
        FpsCalculation();
        UpdateText((TextMesh)_meshes[0]);
    }
    
    private void FpsCalculation()
    {
        frameCount++;
        elapsedTime += stopwatch.ElapsedMilliseconds / 100.0f;
        stopwatch.Restart();
        
        if (elapsedTime >= 1.0f)
        {
            int fps = Mathf.FloorToInt(frameCount / elapsedTime);
            frameCount = 0;
            elapsedTime -= 1f;
            
            string t = fps.ToString();
            text.SetText(t, 1);
        }
    }
}