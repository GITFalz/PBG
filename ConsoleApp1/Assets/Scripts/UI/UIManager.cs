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
    
    public TextMesh xTextMesh = new TextMesh();
    public TextMesh yTextMesh = new TextMesh();
    public TextMesh zTextMesh = new TextMesh();
    
    public TextMesh buttonTextMesh = new TextMesh();
    
    public TextMesh symmetryTextMesh = new TextMesh();


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
    
    
    public void Load()
    {
        // Load UI
        _uiShader = new ShaderProgram("UI/UI.vert", "UI/UI.frag");
        _uItexture = new TextureArray("UI_Atlas.png", 64, 64);
        
        // Load Text
        _textShader = new ShaderProgram("Text/Text.vert", "Text/Text.frag");
        _textTexture = new Texture("text.png");
        
        text = new StaticText("10000");
        
        text.SetMesh(textMesh);
        text.SetAnchorType(AnchorType.TopLeft);
        text.Generate();
        
        xText = new StaticText("X 000");
        yText = new StaticText("Y 000");
        zText = new StaticText("Z 000");
        
        xText.SetMesh(xTextMesh);
        xText.SetAnchorType(AnchorType.MiddleLeft);
        xText.Generate();
        
        yText.SetMesh(yTextMesh);
        yText.SetAnchorType(AnchorType.MiddleCenter);
        yText.Generate();
        
        zText.SetMesh(zTextMesh);
        zText.SetAnchorType(AnchorType.MiddleRight);
        zText.Generate();
        
        symmetryText = new StaticText("Symmetry : None");
        
        symmetryText.SetMesh(symmetryTextMesh);
        symmetryText.SetAnchorType(AnchorType.TopRight);
        symmetryText.Generate();
        
        
        panel.SetMesh(uiMesh);
        panel.SetAnchorType(AnchorType.TopLeft);
        panel.SetScale(new Vector3(200, 500, 0));
        
        buttonX.SetAnchorType(AnchorType.TopLeft);
        buttonX.SetPositionType(PositionType.Relative);
        buttonX.SetScale(new Vector3(180, 50, 0));
        buttonX.SetOffset((10, 10, 10, 10));
        buttonX.SetMesh(uiMesh);
        
        buttonX.OnHold += () =>
        {
            float mouseX = InputManager.GetMouseDelta().X; 
            
            x = (x + mouseX * GameTime.DeltaTime * 50) % 360;
            if (x < 0) x += 360;
        };
        
        buttonY.SetAnchorType(AnchorType.TopLeft);
        buttonY.SetPositionType(PositionType.Relative);
        buttonY.SetScale(new Vector3(180, 50, 0));
        buttonY.SetOffset((10, 10, 70, 10));
        buttonY.SetMesh(uiMesh);
        
        buttonY.OnHold += () =>
        {
            float mouseX = InputManager.GetMouseDelta().X; 
            
            y = (y + mouseX * GameTime.DeltaTime * 50) % 360;
            if (y < 0) y += 360;
        };
        
        buttonZ.SetAnchorType(AnchorType.TopLeft);
        buttonZ.SetPositionType(PositionType.Relative);
        buttonZ.SetScale(new Vector3(180, 50, 0));
        buttonZ.SetOffset((10, 10, 130, 10));
        buttonZ.SetMesh(uiMesh);
        
        buttonZ.OnHold += () =>
        {
            float mouseX = InputManager.GetMouseDelta().X; 
            
            z = (z + mouseX * GameTime.DeltaTime * 50) % 360;
            if (z < 0) z += 360;
        };
        
        buttonX.OnClick += () => { AnimationEditor.clickedRotate = true; };
        buttonY.OnClick += () => { AnimationEditor.clickedRotate = true; };
        buttonZ.OnClick += () => { AnimationEditor.clickedRotate = true; };
        
        
        buttonVert1X = CreateButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(10, 10, 200, 10), uiMesh);
        buttonVert1X.OnHold += () =>
        {
            float mouseX = InputManager.GetMouseDelta().X; 
            vert1.X += mouseX * GameTime.DeltaTime * 50;
        };
        
        buttonVert1Y = CreateButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(70, 10, 200, 10), uiMesh);
        buttonVert1Y.OnHold += () =>
        {
            float mouseX = InputManager.GetMouseDelta().X; 
            vert1.Y += mouseX * GameTime.DeltaTime * 50;
        };
        
        buttonVert1Z = CreateButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(130, 10, 200, 10), uiMesh);
        buttonVert1Z.OnHold += () =>
        {
            float mouseX = InputManager.GetMouseDelta().X; 
            vert1.Z += mouseX * GameTime.DeltaTime * 50;
        };
        
        
        buttonVert2X = CreateButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(10, 10, 240, 10), uiMesh);
        buttonVert2X.OnHold += () =>
        {
            float mouseX = InputManager.GetMouseDelta().X; 
            vert2.X += mouseX * GameTime.DeltaTime * 50;
        };
        
        buttonVert2Y = CreateButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(70, 10, 240, 10), uiMesh);
        buttonVert2Y.OnHold += () =>
        {
            float mouseX = InputManager.GetMouseDelta().X; 
            vert2.Y += mouseX * GameTime.DeltaTime * 50;
        };
        
        buttonVert2Z = CreateButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(130, 10, 240, 10), uiMesh);
        buttonVert2Z.OnHold += () =>
        {
            float mouseX = InputManager.GetMouseDelta().X; 
            vert2.Z += mouseX * GameTime.DeltaTime * 50;
        };
        
        buttonVert3X = CreateButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(10, 10, 280, 10), uiMesh);
        buttonVert3X.OnHold += () =>
        {
            float mouseX = InputManager.GetMouseDelta().X; 
            vert3.X += mouseX * GameTime.DeltaTime * 50;
        };
        
        buttonVert3Y = CreateButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(70, 10, 280, 10), uiMesh);
        buttonVert3Y.OnHold += () =>
        {
            float mouseX = InputManager.GetMouseDelta().X; 
            vert3.Y += mouseX * GameTime.DeltaTime * 50;
        };
        
        buttonVert3Z = CreateButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(130, 10, 280, 10), uiMesh);
        buttonVert3Z.OnHold += () =>
        {
            float mouseX = InputManager.GetMouseDelta().X; 
            vert3.Z += mouseX * GameTime.DeltaTime * 50;
        };
        
        buttonVert4X = CreateButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(10, 10, 320, 10), uiMesh);
        buttonVert4X.OnHold += () =>
        {
            float mouseX = InputManager.GetMouseDelta().X; 
            vert4.X += mouseX * GameTime.DeltaTime * 50;
        };
        
        buttonVert4Y = CreateButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(70, 10, 320, 10), uiMesh);
        buttonVert4Y.OnHold += () =>
        {
            float mouseX = InputManager.GetMouseDelta().X; 
            vert4.Y += mouseX * GameTime.DeltaTime * 50;
        };
        
        buttonVert4Z = CreateButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(130, 10, 320, 10), uiMesh);
        buttonVert4Z.OnHold += () =>
        {
            float mouseX = InputManager.GetMouseDelta().X; 
            vert4.Z += mouseX * GameTime.DeltaTime * 50;
        };
        
        buttonVert5X = CreateButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(10, 10, 360, 10), uiMesh);
        buttonVert5X.OnHold += () =>
        {
            float mouseX = InputManager.GetMouseDelta().X; 
            vert5.X += mouseX * GameTime.DeltaTime * 50;
        };
        
        buttonVert5Y = CreateButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(70, 10, 360, 10), uiMesh);
        buttonVert5Y.OnHold += () =>
        {
            float mouseX = InputManager.GetMouseDelta().X; 
            vert5.Y += mouseX * GameTime.DeltaTime * 50;
        };
        
        buttonVert5Z = CreateButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(130, 10, 360, 10), uiMesh);
        buttonVert5Z.OnHold += () =>
        {
            float mouseX = InputManager.GetMouseDelta().X; 
            vert5.Z += mouseX * GameTime.DeltaTime * 50;
        };
        
        buttonVert6X = CreateButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(10, 10, 400, 10), uiMesh);
        buttonVert6X.OnHold += () =>
        {
            float mouseX = InputManager.GetMouseDelta().X; 
            vert6.X += mouseX * GameTime.DeltaTime * 50;
        };
        
        buttonVert6Y = CreateButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(70, 10, 400, 10), uiMesh);
        buttonVert6Y.OnHold += () =>
        {
            float mouseX = InputManager.GetMouseDelta().X; 
            vert6.Y += mouseX * GameTime.DeltaTime * 50;
        };
        
        buttonVert6Z = CreateButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(130, 10, 400, 10), uiMesh);
        buttonVert6Z.OnHold += () =>
        {
            float mouseX = InputManager.GetMouseDelta().X; 
            vert6.Z += mouseX * GameTime.DeltaTime * 50;
        };
        
        buttonVert7X = CreateButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(10, 10, 440, 10), uiMesh);
        buttonVert7X.OnHold += () =>
        {
            float mouseX = InputManager.GetMouseDelta().X; 
            vert7.X += mouseX * GameTime.DeltaTime * 50;
        };
        
        buttonVert7Y = CreateButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(70, 10, 440, 10), uiMesh);
        buttonVert7Y.OnHold += () =>
        {
            float mouseX = InputManager.GetMouseDelta().X; 
            vert7.Y += mouseX * GameTime.DeltaTime * 50;
        };
        
        buttonVert7Z = CreateButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(130, 10, 440, 10), uiMesh);
        buttonVert7Z.OnHold += () =>
        {
            float mouseX = InputManager.GetMouseDelta().X; 
            vert7.Z += mouseX * GameTime.DeltaTime * 50;
        };
        
        buttonVert8X = CreateButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(10, 10, 480, 10), uiMesh);
        buttonVert8X.OnHold += () =>
        {
            float mouseX = InputManager.GetMouseDelta().X; 
            vert8.X += mouseX * GameTime.DeltaTime * 50;
        };
        
        buttonVert8Y = CreateButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(70, 10, 480, 10), uiMesh);
        buttonVert8Y.OnHold += () =>
        {
            float mouseX = InputManager.GetMouseDelta().X; 
            vert8.Y += mouseX * GameTime.DeltaTime * 50;
        };
        
        buttonVert8Z = CreateButton(AnchorType.TopLeft, PositionType.Relative, new Vector3(50, 30, 0), new Vector4(130, 10, 480, 10), uiMesh);
        buttonVert8Z.OnHold += () =>
        {
            float mouseX = InputManager.GetMouseDelta().X; 
            vert8.Z += mouseX * GameTime.DeltaTime * 50;
        };
        
        buttonVert1X.OnClick += () => { AnimationEditor.clickedMove = true; };
        buttonVert1Y.OnClick += () => { AnimationEditor.clickedMove = true; };
        buttonVert1Z.OnClick += () => { AnimationEditor.clickedMove = true; };
        
        buttonVert2X.OnClick += () => { AnimationEditor.clickedMove = true; };
        buttonVert2Y.OnClick += () => { AnimationEditor.clickedMove = true; };
        buttonVert2Z.OnClick += () => { AnimationEditor.clickedMove = true; };
        
        buttonVert3X.OnClick += () => { AnimationEditor.clickedMove = true; };
        buttonVert3Y.OnClick += () => { AnimationEditor.clickedMove = true; };
        buttonVert3Z.OnClick += () => { AnimationEditor.clickedMove = true; };
        
        buttonVert4X.OnClick += () => { AnimationEditor.clickedMove = true; };
        buttonVert4Y.OnClick += () => { AnimationEditor.clickedMove = true; };
        buttonVert4Z.OnClick += () => { AnimationEditor.clickedMove = true; };
        
        buttonVert5X.OnClick += () => { AnimationEditor.clickedMove = true; };
        buttonVert5Y.OnClick += () => { AnimationEditor.clickedMove = true; };
        buttonVert5Z.OnClick += () => { AnimationEditor.clickedMove = true; };
        
        buttonVert6X.OnClick += () => { AnimationEditor.clickedMove = true; };
        buttonVert6Y.OnClick += () => { AnimationEditor.clickedMove = true; };
        buttonVert6Z.OnClick += () => { AnimationEditor.clickedMove = true; };
        
        buttonVert7X.OnClick += () => { AnimationEditor.clickedMove = true; };
        buttonVert7Y.OnClick += () => { AnimationEditor.clickedMove = true; };
        buttonVert7Z.OnClick += () => { AnimationEditor.clickedMove = true; };
        
        buttonVert8X.OnClick += () => { AnimationEditor.clickedMove = true; };
        buttonVert8Y.OnClick += () => { AnimationEditor.clickedMove = true; };
        buttonVert8Z.OnClick += () => { AnimationEditor.clickedMove = true; };
        
        
        buttons.Add(buttonX);
        buttons.Add(buttonY);
        buttons.Add(buttonZ);
        
        buttons.Add(buttonVert1X);
        buttons.Add(buttonVert1Y);
        buttons.Add(buttonVert1Z);
        
        buttons.Add(buttonVert2X);
        buttons.Add(buttonVert2Y);
        buttons.Add(buttonVert2Z);
        
        buttons.Add(buttonVert3X);
        buttons.Add(buttonVert3Y);
        buttons.Add(buttonVert3Z);
        
        buttons.Add(buttonVert4X);
        buttons.Add(buttonVert4Y);
        buttons.Add(buttonVert4Z);
        
        buttons.Add(buttonVert5X);
        buttons.Add(buttonVert5Y);
        buttons.Add(buttonVert5Z);
        
        buttons.Add(buttonVert6X);
        buttons.Add(buttonVert6Y);
        buttons.Add(buttonVert6Z);
        
        buttons.Add(buttonVert7X);
        buttons.Add(buttonVert7Y);
        buttons.Add(buttonVert7Z);
        
        buttons.Add(buttonVert8X);
        buttons.Add(buttonVert8Y);
        buttons.Add(buttonVert8Z);
        
        panel.AddElement(buttonX);
        panel.AddElement(buttonY);
        panel.AddElement(buttonZ);
        
        foreach (var button in buttons)
        {
            panel.AddElement(button);
        }
        
        panel.Generate();
        
        uiMesh.GenerateBuffers();
        textMesh.GenerateBuffers();
        
        xTextMesh.GenerateBuffers();
        yTextMesh.GenerateBuffers();
        zTextMesh.GenerateBuffers();
        
        buttonTextMesh.GenerateBuffers();
        
        symmetryTextMesh.GenerateBuffers();
    }

    public override void Start()
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
    
    public override void Render()
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
        
        xTextMesh.RenderMesh();
        yTextMesh.RenderMesh();
        zTextMesh.RenderMesh();
        
        buttonTextMesh.RenderMesh();
        
        symmetryTextMesh.RenderMesh();
        
        //Console.WriteLine("Vertices : " + textMesh.transformedVertices[0] + " " + textMesh.transformedVertices[1] + " " + textMesh.transformedVertices[2] + " " + textMesh.transformedVertices[3]);
        //Console.WriteLine("Indices : " + textMesh.Indices[0] + " " + textMesh.Indices[1] + " " + textMesh.Indices[2] + " " + textMesh.Indices[3] + " " + textMesh.Indices[4] + " " + textMesh.Indices[5]);
        //Console.WriteLine("Uvs : " + textMesh.Uvs[0] + " " + textMesh.Uvs[1] + " " + textMesh.Uvs[2] + " " + textMesh.Uvs[3]);
        
        _textShader.Unbind();
        _textTexture.Unbind();
    }

    public override void Update()
    {
        xText.SetText("X " + Math.Round(x, 2));
        yText.SetText("Y " + Math.Round(y, 2));
        zText.SetText("Z " + Math.Round(z, 2));
        
        xText.Generate();
        yText.Generate();
        zText.Generate();
        
        xText.UpdateText();
        yText.UpdateText();
        zText.UpdateText();
        
        if (symmetry != null)
        {
            int length = symmetry.Length;
            for (int i = 0; i < 4 - length; i++)
            {
                symmetry += " ";
            }
            symmetryText.SetText("Symmetry : " + symmetry);
            symmetryText.Generate();
            symmetryText.UpdateText();
            symmetry = null;
        }
        
        buttonX.ButtonTest();
        buttonY.ButtonTest();
        buttonZ.ButtonTest();
        
        foreach (var button in buttons)
        {
            button.ButtonTest();
        }
        
        _oldMousePosition = InputManager.GetMousePosition();
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