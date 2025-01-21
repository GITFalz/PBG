using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class OldUIController
{
    public static HashSet<StaticInputField> InputFields = new HashSet<StaticInputField>();
    public static StaticInputField? activeInputField = null;
    private static bool _isInit = false;
    
    
    
    public static Matrix4 OrthographicProjection = UIController.OrthographicProjection;
    
    public List<UiPanel> staticElements = new List<UiPanel>();
    
    // Load UI
    public static ShaderProgram _uiShader;// = new ShaderProgram("UI/UI.vert", "UI/UI.frag");
    public static TextureArray _uItexture;// = new TextureArray("UI_Atlas.png", 64, 64);
        
    // Load Text
    private static ShaderProgram _textShader;// = new ShaderProgram("NewText/Text.vert", "NewText/Text.frag");
    private static Texture _textTexture;// = new Texture("text.png");
    
    private OldUiMesh _uiMesh = new OldUiMesh();
    private OldUiMesh _maskMesh = new OldUiMesh();
    private OldUiMesh _maskedUiMesh = new OldUiMesh();
    private List<OldTextMesh> _textMeshes = new List<OldTextMesh>();

    private StaticPanel? _parentPanel = null;
    
    public StaticPanel panel;
    
    public void AddStaticElement(UiPanel element)
    {
        AddStaticElement(element, _uiMesh);
    }
    
    private void AddStaticElement(UiPanel element, OldUiMesh uiMesh)
    {
        element.SetMesh(uiMesh);
        staticElements.Add(element);
        
        if (element is StaticInputField inputField)
        {
            _textMeshes.Add(inputField.TextMesh);
            if (!InputFieldExists(inputField.Name))
                InputFields.Add(inputField);
        }
        else if (element is StaticText text)
        {
            _textMeshes.Add(text.Mesh);
        }
        else if (element is StaticPanel panel)
        {
            foreach (var child in panel.ChildElements)
            {
                AddStaticElement(child, uiMesh);
            }
        }
        else if (element is StaticScrollView scrollView)
        {
            scrollView.SetMesh(_maskedUiMesh, _maskMesh);
            foreach (var child in scrollView.ChildElements)
            {
                AddStaticElement(child, _maskedUiMesh);
            }
        }
    }
    
    public static bool InputFieldExists(string name)
    {
        foreach (var inputField in InputFields)
        {
            if (inputField.Name == name)
                return true;
        }

        return false;
    }
    
    public static StaticInputField? GetStaticInputField(string name)
    {
        foreach (var inputField in InputFields)
        {
            if (inputField.Name == name)
                return inputField;
        }

        return null;
    }
    
    public static void AssignInputField(string name)
    {
        Console.WriteLine("Assigning: " + name);
        
        StaticInputField? inputField = GetStaticInputField(name);
        if (inputField == null)
            return;
        
        activeInputField = inputField;
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
            else if (element is StaticPanel panel && panel.PositionType != PositionType.Relative)
            {
                foreach (var child in panel.ChildElements)
                {
                    uiElements.Add(child);
                }
            }
        }

        return uiElements;
    }

    public T? GetElement<T>(string name) where T : UiElement
    {
        foreach (var element in staticElements)
        {
            if (element is T matchedElement && matchedElement.Name == name)
            {
                return matchedElement;
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

    public void SetElementScreenOffset(Vector4 offset)
    {
        foreach (var element in staticElements)
        {
            element.ScreenOffset = offset;
        }
    }

    public void GenerateUi()
    {   
        _maskMesh.Clear();
        _maskMesh.GenerateBuffers();
            
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
    
    public void TestButtons(Vector2 offset)
    {
        foreach (var element in staticElements)
        {
            element.Test(offset);
        }
    }
    
    public void LoadUi(string fileName)
    {
        Console.WriteLine(Path.Combine("Load: " + Game.uiPath, $"{fileName}.ui"));

        string[] lines;
        try
        {
            lines = File.ReadAllLines(Path.Combine(Game.uiPath, $"{fileName}.ui"));
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error loading file {fileName}.ui with exception: " + e);
            return;
        }
        
        if (lines.Length == 0 || lines[0].Split(":")[1].Trim() != fileName)
            return;
        
        UiLoader.Load(this, lines);
    }
    
    public void SaveUi(string fileName)
    {
        List<string> lines = new List<string>() { "File: " + fileName, ""};
        
        foreach (var element in GetUiElements())
        {
            lines.AddRange(element.ToLines(0));
        }
        
        try
        {
            File.WriteAllLines(Path.Combine(Game.uiPath, $"{fileName}.ui"), lines);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error saving file {fileName}.ui with exception: " + e);
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
        {
            activeInputField.AddCharacter(c);
            activeInputField.OnTextChange?.Invoke();
        }
    }
    
    public UiElement? IsMouseOverIgnore(List<UiElement> alreadySelected)
    {
        foreach (var element in staticElements)
        {
            if (element.IsMouseOver() && !alreadySelected.Contains(element))
            {
                return element;
            }
        }

        return null;
    }
    
    public UiElement? IsMouseOverIgnore(List<UiElement> alreadySelected, Vector2 offset)
    {
        foreach (var element in staticElements)
        {
            if (element.IsMouseOver(offset) && !alreadySelected.Contains(element) && !IsElementChildInList(element, alreadySelected))
            {
                return element;
            }
        }

        return null;
    }
    
    public bool IsElementChildInList(UiElement element, List<UiElement> selected)
    {
        foreach (var parent in selected)
        {
            if (parent.HasChild(element))
                return true;
        }

        return false;
    }
    
    public UiElement? IsMouseOver()
    {
        foreach (var element in staticElements)
        {
            if (element.IsMouseOver())
                return element;
        }

        return null;
    }
    
    public UiElement? IsMouseOver(Vector2 offset)
    {
        foreach (var element in staticElements)
        {
            if (element.IsMouseOver(offset))
                return element;
        }

        return null;
    }

    public void Render()
    {
        /*
        GL.Disable(EnableCap.DepthTest);
        GL.DepthMask(false);
        GL.Enable(EnableCap.Blend);
        GL.Disable(EnableCap.CullFace);
        GL.FrontFace(FrontFaceDirection.Ccw);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        
        GL.Enable(EnableCap.StencilTest);
        GL.Clear(ClearBufferMask.StencilBufferBit);  // Clear previous stencil values
        GL.StencilFunc(StencilFunction.Always, 1, 0xFF);  // Always write 1 to stencil
        GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace); 
        
        _uiShader.Bind();
        _uItexture.Bind();
        
        _maskMesh.RenderMesh();

        Matrix4 model = Matrix4.Identity;
        
        int modelLoc = GL.GetUniformLocation(_uiShader.ID, "model");
        int projectionLoc = GL.GetUniformLocation(_uiShader.ID, "projection");

        GL.UniformMatrix4(modelLoc, true, ref model);
        GL.UniformMatrix4(projectionLoc, true, ref OrthographicProjection);
        
        GL.StencilFunc(StencilFunction.Equal, 1, 0xFF);
        GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);

        //Render masked Ui (not implemented)

        GL.Disable(EnableCap.StencilTest);
        
        GL.DepthMask(true);
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Lequal);
        
        
        //Render unmasked Ui
        _uiMesh.RenderMesh();

        GL.Disable(EnableCap.StencilTest);
        GL.Clear(ClearBufferMask.StencilBufferBit);
        
        _uiShader.Unbind();
        _uItexture.Unbind();
        
        _textShader.Bind();
        _textTexture.Bind();

        model = Matrix4.Identity;

        int textModelLoc = GL.GetUniformLocation(_textShader.ID, "model");
        int textProjectionLoc = GL.GetUniformLocation(_textShader.ID, "projection");
        int charsLoc = GL.GetUniformLocation(_textShader.ID, "charBuffer");
        
        GL.UniformMatrix4(textModelLoc, true, ref model);
        GL.UniformMatrix4(textProjectionLoc, true, ref OrthographicProjection);
        GL.Uniform1(charsLoc, 1);
        
        foreach (var textMesh in _textMeshes)
        {
            textMesh.RenderMesh();
        }
        
        _textShader.Unbind();
        _textTexture.Unbind();
        */
    }
}