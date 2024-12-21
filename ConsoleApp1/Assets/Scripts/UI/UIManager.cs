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


    public StaticText text;
    
    
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
        /*
        StaticPanel topLeftPanel = new StaticPanel();

        topLeftPanel.SetMesh(uiMesh);
        topLeftPanel.SetAnchorType(AnchorType.TopLeft);
        topLeftPanel.Generate();
        
        StaticPanel topCenterPanel = new StaticPanel();
        
        topCenterPanel.SetMesh(uiMesh);
        topCenterPanel.SetAnchorType(AnchorType.TopCenter);
        topCenterPanel.Generate();
        
        StaticPanel topRightPanel = new StaticPanel();
        
        topRightPanel.SetMesh(uiMesh);
        topRightPanel.SetAnchorType(AnchorType.TopRight);
        topRightPanel.Generate();
        
        StaticPanel middleLeftPanel = new StaticPanel();
        
        middleLeftPanel.SetMesh(uiMesh);
        middleLeftPanel.SetAnchorType(AnchorType.MiddleLeft);
        middleLeftPanel.Generate();
        
        StaticPanel middleCenterPanel = new StaticPanel();
        
        middleCenterPanel.SetMesh(uiMesh);
        middleCenterPanel.SetAnchorType(AnchorType.MiddleCenter);
        middleCenterPanel.Generate();
        
        StaticPanel middleRightPanel = new StaticPanel();
        
        middleRightPanel.SetMesh(uiMesh);
        middleRightPanel.SetAnchorType(AnchorType.MiddleRight);
        middleRightPanel.Generate();
        
        StaticPanel bottomLeftPanel = new StaticPanel();
        
        bottomLeftPanel.SetMesh(uiMesh);
        bottomLeftPanel.SetAnchorType(AnchorType.BottomLeft);
        bottomLeftPanel.Generate();
        
        StaticPanel bottomCenterPanel = new StaticPanel();
        
        bottomCenterPanel.SetMesh(uiMesh);
        bottomCenterPanel.SetAnchorType(AnchorType.BottomCenter);
        bottomCenterPanel.Generate();
        
        StaticPanel bottomRightPanel = new StaticPanel();
        
        bottomRightPanel.SetMesh(uiMesh);
        bottomRightPanel.SetAnchorType(AnchorType.BottomRight);
        bottomRightPanel.Generate();
        */
        
        
        text = new StaticText("10000");
        
        text.SetMesh(textMesh);
        text.SetAnchorType(AnchorType.TopLeft);
        text.Generate();
        
        
        uiMesh.GenerateBuffers();
        textMesh.GenerateBuffers();
    }

    public void OnResize()
    {
        foreach (var mesh in _meshes)
        {
            mesh.Clear();
        }
        
        GenerateMeshes();
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
        
        //Console.WriteLine("Vertices : " + textMesh.transformedVertices[0] + " " + textMesh.transformedVertices[1] + " " + textMesh.transformedVertices[2] + " " + textMesh.transformedVertices[3]);
        //Console.WriteLine("Indices : " + textMesh.Indices[0] + " " + textMesh.Indices[1] + " " + textMesh.Indices[2] + " " + textMesh.Indices[3] + " " + textMesh.Indices[4] + " " + textMesh.Indices[5]);
        //Console.WriteLine("Uvs : " + textMesh.Uvs[0] + " " + textMesh.Uvs[1] + " " + textMesh.Uvs[2] + " " + textMesh.Uvs[3]);
        
        _textShader.Unbind();
        _textTexture.Unbind();
    }

    public void OnUpdateFrame(KeyboardState keyboard, MouseState mouse, FrameEventArgs args)
    {
        //_ui.Update(mouse);
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
            
        text.SetText(t);
        text.Generate();
        text.UpdateText();
    }
}