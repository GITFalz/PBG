using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class UIController
{
    public static Matrix4 OrthographicProjection;
    
    private List<StaticButton> staticButtons = new List<StaticButton>();
    private List<DynamicButton> dynamicButtons = new List<DynamicButton>();
    private List<StaticText> staticTexts = new List<StaticText>();
    private List<StaticPanel> staticPanels = new List<StaticPanel>();
    
    private static ShaderProgram _uiShader = new ShaderProgram("UI/UI.vert", "UI/UI.frag");
    private static TextureArray _uItexture = new TextureArray("UI_Atlas.png", 64, 64);
        
    // Load Text
    private static ShaderProgram _textShader = new ShaderProgram("Text/Text.vert", "Text/Text.frag");
    private static Texture _textTexture = new Texture("text.png");
    
    private UiMesh _uiMesh = new UiMesh();
    private List<TextMesh> _textMeshes = new List<TextMesh>();

    private StaticPanel? _parentPanel = null;
    
    public void AddStaticButton(StaticButton button)
    {
        if (button.PositionType == PositionType.Relative && _parentPanel != null)
            _parentPanel.AddElement(button);
        
        button.SetMesh(_uiMesh);
        staticButtons.Add(button);
    }
    
    public void AddStaticText(StaticText text)
    {
        _textMeshes.Add(text.Mesh);
        staticTexts.Add(text);
    }
    
    public void AddStaticPanel(StaticPanel panel)
    {
        panel.SetMesh(_uiMesh);
        staticPanels.Add(panel);
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
        if (index < staticPanels.Count && index >= 0)
            return staticPanels[index];
        return null;
    }
    
    public void SetParentPanel(StaticPanel panel)
    {
        _parentPanel = panel;
    }
    
    public void Generate()
    {
        foreach (var text in staticTexts)
        {
            text.Generate();
        }
        
        foreach (var textMesh in _textMeshes)
        {
            textMesh.GenerateBuffers();
        }
        
        GenerateUi();
    }

    public void GenerateUi()
    {
        foreach (var button in staticButtons)
        {
            if (button.PositionType != PositionType.Relative)
                button.Generate();
        }
        
        foreach (var panel in staticPanels)
        {
            panel.Generate();
        }
        
        _uiMesh.GenerateBuffers();
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
        _uiMesh.Clear();
    }

    public void Clear()
    {
        staticPanels.Clear();
        staticButtons.Clear();
        staticTexts.Clear();
        _uiMesh.Clear();
        _textMeshes.Clear();
    }

    public void TestButtons()
    {
        foreach (var button in staticButtons)
        {
            button.ButtonTest();
        }
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