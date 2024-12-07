using ConsoleApp1.Assets.Scripts.Inputs;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class UIController
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
    
    
    // UI Elements
    public List<UI_Base> uiElements = new List<UI_Base>();
    
    
    
    public void Load()
    {
        // Load UI
        _uiShader = new ShaderProgram("UI/UI.vert", "UI/UI.frag");
        _uItexture = new TextureArray("UI_Atlas.png", 64, 64);
        
        _uiMeshData = new MeshData();
        
        UI_Panel panel = new UI_Panel(new Vector2(64, 64), UiAnchorAlignment.MiddleCenter, new Vector4(10, 10, 10, 10), new Vector2(500, 500));
        panel.RenderUI(_uiMeshData);
        
        _uIvao = new VAO();
        
        _uIvbo = new VBO(_uiMeshData.verts);
        _uItextureVbo = new VBO(_uiMeshData.uvs);
        _uIIndexVbo = new VBO(_uiMeshData.tCoords);
        
        _uIvao.LinkToVAO(0, 3, _uIvbo);
        _uIvao.LinkToVAO(1, 2, _uItextureVbo);
        _uIvao.LinkToVAO(2, 1, _uIIndexVbo);
        
        _uIibo = new IBO(_uiMeshData.tris);
        
        
        // Load Text
        _textShader = new ShaderProgram("Text/Text.vert", "Text/Text.frag");

        _textTexture = new Texture("text.png");
        _textMeshData = new MeshData();
        
        _textVao = new VAO();

        _textVbo = new VBO(_textMeshData.verts);
        _textTextureVbo = new VBO(_textMeshData.uvs);

        _textVao.LinkToVAO(0, 3, _textVbo);
        _textVao.LinkToVAO(1, 2, _textTextureVbo);

        _textIbo = new IBO(_textMeshData.tris);
    }

    public void Start()
    {
        
    }
    
    public void Render()
    {
        foreach (var uiElement in uiElements)
        {
            uiElement.RenderUI(_uiMeshData);
        }
        
        foreach (var uiElement in uiElements)
        {
            uiElement.RenderText();
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
        UpdateUi();
        UpdateText();
    }

    public void OnUpdateFrame(KeyboardState keyboard, MouseState mouse, FrameEventArgs args)
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