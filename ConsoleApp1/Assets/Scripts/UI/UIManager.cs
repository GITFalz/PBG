using System.Diagnostics;
using ConsoleApp1.Assets.Scripts.Inputs;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class UIManager : Updateable
{
    // UI
    private ShaderProgram _uiShader;
    private TextureArray _uItexture;
    
    
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
    
    int frameCount = 0;
    float elapsedTime = 0;
    Stopwatch stopwatch = new Stopwatch();
    
    
    public UiMesh uiMesh = new UiMesh();
    public TextMesh textMesh = new TextMesh();
    
    public TextMesh xTextMesh = new TextMesh();
    public TextMesh yTextMesh = new TextMesh();
    public TextMesh zTextMesh = new TextMesh();
    
    public TextMesh buttonTextMesh = new TextMesh();
    
    public TextMesh symmetryTextMesh = new TextMesh();
    
    //Model
    


    public StaticText text;

    public StaticText xText;
    public StaticText yText;
    public StaticText zText;
    
    public StaticText symmetryText;
    
    public List<StaticButton> buttons = new List<StaticButton>();
    
    public StaticButton buttonX = new StaticButton();
    public StaticButton buttonY = new StaticButton();
    public StaticButton buttonZ = new StaticButton();
    
    public StaticButton buttonVert1X;
    public StaticButton buttonVert1Y;
    public StaticButton buttonVert1Z;
    
    public StaticButton buttonVert2X;
    public StaticButton buttonVert2Y;
    public StaticButton buttonVert2Z;
    
    public StaticButton buttonVert3X;
    public StaticButton buttonVert3Y;
    public StaticButton buttonVert3Z;
    
    public StaticButton buttonVert4X;
    public StaticButton buttonVert4Y;
    public StaticButton buttonVert4Z;
    
    public StaticButton buttonVert5X;
    public StaticButton buttonVert5Y;
    public StaticButton buttonVert5Z;
    
    public StaticButton buttonVert6X;
    public StaticButton buttonVert6Y;
    public StaticButton buttonVert6Z;
    
    public StaticButton buttonVert7X;
    public StaticButton buttonVert7Y;
    public StaticButton buttonVert7Z;
    
    public StaticButton buttonVert8X;
    public StaticButton buttonVert8Y;
    public StaticButton buttonVert8Z;
    
    
    public StaticPanel panel = new StaticPanel();
    public StaticPanel subPanel = new StaticPanel();
    
    
    public static float x = 0;
    public static float y = 0;
    public static float z = 0;
    
    public static string? symmetry = null;
    
    public static Vector3 vert1 = new Vector3(0, 0, 0);
    public static Vector3 vert2 = new Vector3(0, 0, 0);
    public static Vector3 vert3 = new Vector3(0, 0, 0);
    public static Vector3 vert4 = new Vector3(0, 0, 0);
    public static Vector3 vert5 = new Vector3(0, 0, 0);
    public static Vector3 vert6 = new Vector3(0, 0, 0);
    public static Vector3 vert7 = new Vector3(0, 0, 0);
    public static Vector3 vert8 = new Vector3(0, 0, 0);
    
    
    private Vector2 _oldMousePosition;
    
    
    private float angle = 0;
    
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
    
    public override void Render()
    {
        
    }
    

    public override void Update()
    {
        
    }
    
    private void InputField(KeyboardState keyboard, MouseState mouse, FrameEventArgs args)
    {
        if (keyboard.IsAnyKeyDown)
        {
            List<Character> pressedCharacters = Input.GetPressedKeys(keyboard);

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
    
    public StaticButton CreateButton(AnchorType anchorType, PositionType positionType, Vector3 scale, Vector4 offset, UiMesh mesh)
    {
        StaticButton button = new StaticButton();
        
        button.SetAnchorType(anchorType);
        button.SetPositionType(positionType);
        button.SetScale(scale);
        button.SetOffset(offset);
        button.SetMesh(mesh);
        
        return button;
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