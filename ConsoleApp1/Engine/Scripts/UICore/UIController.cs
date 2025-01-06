using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class UIController
{
    public static Matrix4 OrthographicProjection;
    
    private List<StaticElement> staticElements = new List<StaticElement>();
    
    private static ShaderProgram _uiShader = new ShaderProgram("UI/UI.vert", "UI/UI.frag");
    private static TextureArray _uItexture = new TextureArray("UI_Atlas.png", 64, 64);
        
    // Load Text
    private static ShaderProgram _textShader = new ShaderProgram("Text/Text.vert", "Text/Text.frag");
    private static Texture _textTexture = new Texture("text.png");
    
    private UiMesh _uiMesh = new UiMesh();
    private List<TextMesh> _textMeshes = new List<TextMesh>();

    private StaticPanel? _parentPanel = null;
    
    public static StaticInputField? activeInputField = null;
    
    public void AddStaticElement(StaticElement element)
    {
        element.SetMesh(_uiMesh);
        staticElements.Add(element);
        
        if (element is StaticInputField inputField)
        {
            _textMeshes.Add(inputField.TextMesh);
        }
        else if (element is StaticText text)
        {
            _textMeshes.Add(text.Mesh);
        }
        else if (element is StaticPanel panel)
        {
            foreach (var child in panel.ChildElements)
            {
                AddStaticElement(child);
            }
        }
    }
    
    public UiMesh GetUiMesh()
    {
        return _uiMesh;
    }
    
    public void SetStaticPanelTexureIndex(int index, int textureIndex)
    {
        StaticPanel? panel = GetStaticPanel(index);
        if (panel == null)
            return;
        
        panel.TextureIndex = textureIndex;
    }

    public StaticPanel? GetStaticPanel(int index)
    {
        if (index >= staticElements.Count || index < 0) 
            return null;
        if (staticElements[index] is StaticPanel panel)
            return panel;
        return null;
    }
    
    public void SetParentPanel(StaticPanel panel)
    {
        _parentPanel = panel;
    }
    
    public void Generate()
    {
        GenerateUi();
        GenerateBuffers();
    }

    public void Resize()
    {
        ClearUiMesh();
        foreach (var element in staticElements)
        {
            element.Align();
        }
        GenerateUi();
        _uiMesh.Init();
        Update();
    }
    
    public List<UiElement> GetUiElements()
    {
        List<UiElement> uiElements = new List<UiElement>();
        foreach (var element in staticElements)
        {
            if (element.ParentElement == null)
                uiElements.Add(element);
            else if (element is StaticPanel panel)
            {
                foreach (var child in panel.ChildElements)
                {
                    uiElements.Add(child);
                }
            }
        }

        return uiElements;
    }

    public StaticText? GetText(string name)
    {
        foreach (var element in staticElements)
        {
            if (element is StaticText text && text.Name == name)
            {
                return text;
            }
        }

        return null;
    }
    
    public StaticInputField? GetInputField(string name)
    {
        foreach (var element in staticElements)
        {
            if (element is StaticInputField inputField && inputField.Name == name)
            {
                return inputField;
            }
        }

        return null;
    }

    public void GenerateUi()
    {
        foreach (var element in staticElements)
        {
            if (element.PositionType != PositionType.Relative) 
                element.Generate();
        }
    }

    public void GenerateBuffers()
    {
        _uiMesh.GenerateBuffers();
        foreach (var textMesh in _textMeshes)
        {
            textMesh.GenerateBuffers();
        }
    }

    public void Update()
    {
        _uiMesh.UpdateMesh();
        foreach (var textMesh in _textMeshes)
        {
            textMesh.UpdateMesh();
        }
    }
    
    public void ClearUiMesh()
    {
        foreach (var element in staticElements)
        {
            element.Reset();
        }
        
        _uiMesh.Clear();
    }

    public void Clear()
    {
        staticElements.Clear();
        _uiMesh.Clear();
        _textMeshes.Clear();
    }

    public void Test()
    {
        TestButtons();
    }

    public void TestButtons()
    {
        foreach (var element in staticElements)
        {
            element.Test();
        }
    }

    public static void InputField(Keys key)
    {
        if (activeInputField == null || key == Keys.LeftShift || key == Keys.RightShift)
            return;
        
        if (key == Keys.Backspace)
        {
            activeInputField.RemoveCharacter();
            return;
        }
        
        if (!Char.GetChar(out char c, key, Input.AreKeysDown(Keys.LeftShift, Keys.RightShift), Input.AreKeysDown(Keys.LeftAlt)))
            return;
        
        if (TextShaderHelper.CharExists(c))
            activeInputField.AddCharacter(c);
    }

    public void Render()
    {
        GL.Disable(EnableCap.DepthTest);
        GL.DepthMask(false);
        GL.Enable(EnableCap.Blend);
        GL.Disable(EnableCap.CullFace);
        GL.FrontFace(FrontFaceDirection.Ccw);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        _uiShader.Bind();
        _uItexture.Bind();
        
        int projectionLoc = GL.GetUniformLocation(_uiShader.ID, "projection");
        GL.UniformMatrix4(projectionLoc, true, ref OrthographicProjection);
        
        _uiMesh.RenderMesh();
        
        _uiShader.Unbind();
        _uItexture.Unbind();
        
        _textShader.Bind();
        _textTexture.Bind();

        int textProjectionLoc = GL.GetUniformLocation(_textShader.ID, "projection");
        int charsLoc = GL.GetUniformLocation(_textShader.ID, "charBuffer");
        
        GL.UniformMatrix4(textProjectionLoc, true, ref OrthographicProjection);
        GL.Uniform1(charsLoc, 1);
        
        foreach (var textMesh in _textMeshes)
        {
            textMesh.RenderMesh();
        }
        
        _textShader.Unbind();
        _textTexture.Unbind();
        
        GL.DepthMask(true);
        GL.Enable(EnableCap.DepthTest);
        
    }
}