using ConsoleApp1.Assets.Scripts.Inputs;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class UIManager
{
    // UI
    private VAO _uIvao;
    private VBO _uIvbo;
    private VBO _uItextureVbo;
    private VBO _uIIndexVbo;
    private IBO _uIibo;
    
    private ShaderProgram _uiShader;
    private TextureArray _uItexture;
    
    private MeshData _uiMeshData;
    
    public Matrix4 OrthographicProjection;
    
    
    // Text
    private VAO _textVao;
    private VBO _textVbo;
    private VBO _textTextureVbo;
    private IBO _textIbo;
    
    private ShaderProgram _textShader;
    private Texture _textTexture;
    
    private MeshData _textMeshData;
    
    
    // Input
    private bool _isTyping = false;
    private Character _currentCharacter;
    private HashSet<Character> _pressedCharacters = new HashSet<Character>();
    private Queue<Character> _toBeAdded = new Queue<Character>();

    private List<Character> _inputField = new List<Character>();

    
    // Controller
    private UIController _ui = new UIController();
    
    
    
    public void Load()
    {
        // Load UI
        _uiShader = new ShaderProgram("UI/UI.vert", "UI/UI.frag");
        _uItexture = new TextureArray("UI_Atlas.png", 64, 64);
        
        _uiMeshData = new MeshData();
        _uIvao = new VAO();
        
        
        // Load Text
        _textShader = new ShaderProgram("Text/Text.vert", "Text/Text.frag");
        _textTexture = new Texture("text.png");
        
        _textMeshData = new MeshData();
        _textVao = new VAO();
        
    }

    public void Start()
    {
        // UI creation
        UI_Panel panel1 = new UI_Panel();
        /**/UI_Panel panel1a = new UI_Panel();
        /**/UI_Panel panel1b = new UI_Panel();
        /**//**/UI_Panel panel1b1 = new UI_Panel();
        /**/UI_Panel panel1c = new UI_Panel();
        
        UI_Button button = new UI_Button();
        
        
        // Panel 1
        panel1.SetOffset(new Vector4(10, 10, 10, 10));
        panel1.SetSize(new Vector2(500, 500));
        panel1.SetAnchorAlignment(UiAnchorAlignment.RightScale);
        panel1.SetAnchorReference(UiAnchor.Absolute);
        
        panel1.SetMem(0);
        

        // Panel 1a
        panel1a.SetOffset(new Vector4(10, 10, 10, 10));
        panel1a.SetSize(new Vector2(500, 200));
        panel1a.SetAnchorAlignment(UiAnchorAlignment.TopScale);
        panel1a.SetAnchorReference(UiAnchor.Relative);
        
        panel1a.SetParent(panel1);
        
        panel1a.SetMem(1);
        
        
        // Panel 1b
        panel1b.SetOffset(new Vector4(10, 10, 220, 10));
        panel1b.SetSize(new Vector2(500, 200));
        panel1b.SetAnchorAlignment(UiAnchorAlignment.TopScale);
        panel1b.SetAnchorReference(UiAnchor.Relative);
        
        panel1b.SetParent(panel1);
        
        panel1b.SetMem(2);

        
        // Panel 1b1
        panel1b1.SetOffset(new Vector4(10, 10, -200, 10));
        panel1b1.SetSize(new Vector2(100, 100));
        panel1b1.SetAnchorAlignment(UiAnchorAlignment.RightScale);
        panel1b1.SetAnchorReference(UiAnchor.Relative);
        
        panel1b1.SetParent(panel1b);
        
        panel1b1.SetMem(3);
        
        
        // Panel 1c
        panel1c.SetOffset(new Vector4(10, 10, 430, 10));
        panel1c.SetSize(new Vector2(500, 200));
        panel1c.SetAnchorAlignment(UiAnchorAlignment.TopScale);
        panel1c.SetAnchorReference(UiAnchor.Relative);
        
        panel1c.SetParent(panel1);
        
        panel1c.SetMem(4);
        
        
        
        
        // Button
        button.SetOffset(new Vector4(10, 10, -410, 10));
        button.SetSize(new Vector2(150, 50));
        button.SetAnchorAlignment(UiAnchorAlignment.BottomLeft);
        button.SetAnchorReference(UiAnchor.Absolute);
        
        button.SetMem(5);
        
        button.SetTextAlignment(UiAnchorAlignment.MiddleCenter);
        
        button.OnClick = () => { Console.WriteLine("Button Clicked"); };
        
        
        
            
        // Add to list
        _ui.AddElement(panel1);
        _ui.AddElement(panel1a);
        _ui.AddElement(panel1b);
        _ui.AddElement(panel1b1);
        _ui.AddElement(panel1c);
        
        _ui.AddElement(button);
        _ui.AddElement(button.text);
        
        RenderUI();
        
        // Generate UI
        GenerateUI();
        
        
        // Text creation
        UI_Text text = new UI_Text();
        UI_Text text2 = new UI_Text();
        
        // Text
        text.SetText("Hi there  my name is Falz", 2);
        
        text.SetOffset(new Vector4(10, 10, 10, 10));
        text.SetSize(new Vector2(500, 500));
        text.SetAnchorAlignment(UiAnchorAlignment.LeftScale);
        text.SetAnchorReference(UiAnchor.Relative);
        
        text.SetParent(panel1a);
        
        text.SetMem(7);
        
        
        // Text 2
        text2.SetText("Boo", 1.5f);
        
        text2.SetOffset(new Vector4(10, 10, 220, 10));
        text2.SetSize(new Vector2(80, 80));
        text2.SetAnchorAlignment(UiAnchorAlignment.TopLeft);
        text2.SetAnchorReference(UiAnchor.Relative);
        
        text2.SetParent(panel1b1);
        
        text2.SetMem(8);
        
        
        // Add to list
        _ui.AddElement(text);
        _ui.AddElement(text2);
        
        RenderText();
        
        
        // Generate Text
        GenerateText();
    }

    public void OnResize()
    {
        _uiMeshData.Clear();
        
        RenderUI();
        GenerateUI();
        
        _textMeshData.Clear();

        RenderText();
        GenerateText();
    }
    
    public void RenderUI()
    {
        foreach (var uiElement in _ui.GetElements())
        {
            uiElement.RenderUI(_uiMeshData);
        }
    }
    
    public void RenderText()
    {
        foreach (var uiElement in _ui.GetElements())
        {
            uiElement.RenderText(_textMeshData);
        }
    }
    
    public void Unload()
    {
        _uIvao.Delete();
        _uIvbo.Delete();
        _uItextureVbo.Delete();
        _uIIndexVbo.Delete();
        _uIibo.Delete();
        
        _uiShader.Delete();
        _uItexture.Delete();
        
        _uiMeshData.Clear();
        
        
        _textVao.Delete();
        _textVbo.Delete();
        _textTextureVbo.Delete();
        _textIbo.Delete();
        
        _textShader.Delete();
        _textTexture.Delete();
        
        _textMeshData.Clear();
        
        _inputField.Clear();
    }
    
    public void OnRenderFrame(FrameEventArgs args)
    {
        for (int i = 0; i < _uiMeshData.verts.Count; i++)
        {
            _uiMeshData.verts[i] = _uiMeshData.verts[i] + new Vector3(0, 0.01f, 0);
        }
        
        UpdateUi();
        UpdateText();
    }

    public void OnUpdateFrame(KeyboardState keyboard, MouseState mouse, FrameEventArgs args)
    {
        
        /*
         * script that handles inputFields
        */
        //InputField(keyboard, mouse, args);

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

    private void GenerateUI()
    {
        _uIvbo = new VBO(_uiMeshData.verts);
        _uItextureVbo = new VBO(_uiMeshData.uvs);
        _uIIndexVbo = new VBO(_uiMeshData.tCoords);
        
        _uIvao.LinkToVAO(0, 3, _uIvbo);
        _uIvao.LinkToVAO(1, 2, _uItextureVbo);
        _uIvao.LinkToVAO(2, 1, _uIIndexVbo);
        
        _uIibo = new IBO(_uiMeshData.tris);
    }

    private void GenerateText()
    {
        _textVbo = new VBO(_textMeshData.verts);
        _textTextureVbo = new VBO(_textMeshData.uvs);

        _textVao.LinkToVAO(0, 3, _textVbo);
        _textVao.LinkToVAO(1, 2, _textTextureVbo);

        _textIbo = new IBO(_textMeshData.tris);
    }
    
    private void UpdateUi()
    {
        _uiShader.Bind();
        _uItexture.Bind();
        
        int projectionLoc = GL.GetUniformLocation(_uiShader.ID, "projection");
        GL.UniformMatrix4(projectionLoc, true, ref OrthographicProjection);
        
        _uIvao.Bind();
        _uIibo.Bind();

        _uItextureVbo.Bind();

        GL.DrawElements(PrimitiveType.Triangles, _uiMeshData.tris.Count, DrawElementsType.UnsignedInt, 0);
        
        _uIvao.Unbind();
        _uIibo.Unbind();

        _uItextureVbo.Unbind();
    }

    private void UpdateText()
    {
        //FpsCalculation();

        UpdateCharacters();

        _textShader.Bind();
        _textTexture.Bind();

        int textProjectionLoc = GL.GetUniformLocation(_textShader.ID, "projection");
        GL.UniformMatrix4(textProjectionLoc, true, ref OrthographicProjection);

        _textVao.Bind();
        _textIbo.Bind();

        _textTextureVbo.Bind();

        GL.DrawElements(PrimitiveType.Triangles, _textMeshData.tris.Count, DrawElementsType.UnsignedInt, 0);

        _textVao.Unbind();
        _textIbo.Unbind();

        _textTextureVbo.Unbind();

    }
    
    private void RemoveLastCharacter()
    {
        if (_inputField.Count == 0)
            return;
        
        Character character = _inputField[^1];
        
        if (character == Character.Space)
        {
            UI.RemoveSpace();
        }
        else
        {
            UI.RemoveLastQuad(_textMeshData);
            
            _textVbo.Bind();
            _textVbo.Update(_textMeshData.verts);
            
            _textTextureVbo.Bind();
            _textTextureVbo.Update(_textMeshData.uvs);
            
            _textIbo.Bind();
            _textIbo.Update(_textMeshData.tris);

            _textVbo.Unbind();
            _textTextureVbo.Unbind();
            _textIbo.Unbind();
        }
        
        _inputField.RemoveAt(_inputField.Count - 1);
    }
    
    private void UpdateCharacters()
    {
        if (_toBeAdded.Count == 0)
            return;
        
        Character character;

        while (true)
        {
            try
            {
                character = _toBeAdded.Dequeue();
            }
            catch (InvalidOperationException)
            {
                break;
            }
            
            switch (character)
            {
                case Character.None:
                    continue;
                case Character.Backspace:
                    RemoveLastCharacter();
                    continue;
                default:
                    _inputField.Add(character);
                    UI.GenerateCharacterAtLastPosition(1, character, _textMeshData);
                    break;
            }
        }
        
        _textVao = new VAO();

        _textVbo = new VBO(_textMeshData.verts);
        _textTextureVbo = new VBO(_textMeshData.uvs);

        _textVao.LinkToVAO(0, 3, _textVbo);
        _textVao.LinkToVAO(1, 2, _textTextureVbo);

        _textIbo = new IBO(_textMeshData.tris);
    }
}