using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class UIController
{
    public UIMesh uIMesh = new();
    public TextMesh textMesh = new();

    public UIMesh maskMesh = new();
    public UIMesh maskeduIMesh = new();
    public TextMesh maskedTextMesh = new();
    
    public static ShaderProgram _uiShader = UIData.UiShader;
    public static TextureArray _uItexture = UIData.UiTexture;
    public static ShaderProgram _textShader = UIData.TextShader;
    public static Texture _textTexture = UIData.TextTexture;

    public static Matrix4 OrthographicProjection = Matrix4.Identity;
    public List<UIElement> Elements = [];
    public List<UIElement> AbsoluteElements = [];
    public List<UIButton> Buttons = [];
    public static List<UIInputField> InputFields = [];

    public static UIInputField? activeInputField = null;

    public bool render = true;
    public static int TextOffset = 0;

    public UIController() {}

    public UIController(params UIElement[] elements)
    {
        foreach (var element in elements)
        {
            AddElement(element);
        }
    }

    public void AddElement(UIElement element, MeshType type = MeshType.UnMasked, bool test = false)
    {
        if (element.PositionType == PositionType.Absolute)
            AbsoluteElements.Add(element);

        if (element is UIPanel panel)
        {
            if (panel is UIButton button)
                Buttons.Add(button);
        }
        else if (element is UIText text)
        {
            if (text is UIInputField inputField)
            {
                Elements.Add(inputField.Button);
                Buttons.Add(inputField.Button);
                InputFields.Add(inputField);
            }
        }
        else if (element is UICollection collection)
        {
            foreach (var e in collection.Elements)
            {
                AddElement(e, type, test);
            }
        }

        element.UIController = this;
        Elements.Add(element);
    }

    private static readonly Dictionary<MeshType, Func<UIController, UIMesh>> uiMeshType = new Dictionary<MeshType, Func<UIController, UIMesh>>()
    {
        { MeshType.UnMasked, controller => controller.uIMesh },
        { MeshType.Masked, controller => controller.maskeduIMesh }
    };

    private static readonly Dictionary<MeshType, Func<UIController, TextMesh>> textMeshType = new Dictionary<MeshType, Func<UIController, TextMesh>>()
    {
        { MeshType.UnMasked, controller => controller.textMesh },
        { MeshType.Masked, controller => controller.maskedTextMesh }
    };

    public void RemoveElement(UIElement element)
    {

    }

    public UIElement? GetElement<T>(string name) where T : UIElement
    {
        foreach (var element in Elements)
        {
            if (element.Name == name && element is T)
                return element;
        }

        return null;
    }

    public void Test()
    {
        foreach (var element in Elements)
        {
            element.Test();
        }
    }

    public void Test(Vector2 offset)
    {
        foreach (var element in Elements)
        {
            element.Test(offset);
        }
    }

    public UIElement? IsMouseOver()
    {
        foreach (var element in Elements)
        {
            if (element.IsMouseOver())
            {
                return element;
            }
        }

        return null;
    }

    public UIElement? IsMouseOver(Vector2 offset)
    {
        foreach (var element in Elements)
        {
            if (element.IsMouseOver(offset))
            {
                return element;
            }
        }

        return null;
    }

    public UIElement? IsMouseOverIgnore(List<UIElement> alreadySelected)
    {
        foreach (var element in Elements)
        {
            if (element.IsMouseOver() && !alreadySelected.Contains(element))
            {
                return element;
            }
        }

        return null;
    }
    
    public UIElement? IsMouseOverIgnore(List<UIElement> alreadySelected, Vector2 offset)
    {
        foreach (var element in Elements)
        {
            if (element.IsMouseOver(offset) && !alreadySelected.Contains(element))
            {
                return element;
            }
        }

        return null;
    }

    public string GetNextElementName()
    {
        int i = 0;
        string name = "Element" + i;
        while (GetElement<UIElement>(name) != null)
        {
            i++;
            name = "Element" + i;
        }
        return name;
    }

    public static UIInputField? GetStaticInputField(string name)
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
        
        UIInputField? inputField = GetStaticInputField(name);
        if (inputField == null)
            return;
        
        activeInputField = inputField;
    }

    public static void RemoveInputField()
    {
        activeInputField = null;
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

    public bool ElementSharePosition(UIElement element)
    {
        foreach (var e in Elements)
        {
            if (e == element)
                continue;

            if (e.Offset == element.Offset && e.AnchorType == element.AnchorType)
                return true;
        }

        return false;
    }

    public void Generate()
    {
        TextOffset = 0;

        foreach (var element in AbsoluteElements)
        {
            element.Align();    
        }

        foreach (var element in Elements)
        {
            element.Generate();    
        }
    }

    public void Buffers()
    {
        uIMesh.GenerateBuffers();
        textMesh.GenerateBuffers();
        maskMesh.GenerateBuffers();
        maskeduIMesh.GenerateBuffers(); 
        maskedTextMesh.GenerateBuffers();
    }

    public void GenerateBuffers()
    {
        Generate();
        Buffers();
    }

    public void UpdateMatrices()
    {
        uIMesh.UpdateMatrices();
        textMesh.UpdateMatrices();
        maskMesh.UpdateMatrices();
        maskeduIMesh.UpdateMatrices();
        maskedTextMesh.UpdateMatrices();
    }

    public void UpdateTextures()
    {
        uIMesh.UpdateTexture();
        maskeduIMesh.UpdateTexture();
    }

    public List<string> ToLines()
    {
        List<string> lines = new List<string>();

        return lines;
    }

    public void Clear()
    {
        uIMesh.Clear();
        textMesh.Clear();

        maskMesh.Clear();
        maskeduIMesh.Clear();
        maskedTextMesh.Clear();
        
        Elements.Clear();
        AbsoluteElements.Clear();
        Buttons.Clear();
        InputFields.Clear();
    }

    public void OnResize()
    {
        foreach (var element in AbsoluteElements)
        {
            element.Align();
        }

        foreach (var element in Elements)
        {
            element.UpdateScale();
            element.UpdateTransformation();
        }

        UpdateMatrices();
        uIMesh.UpdateVertices();
    }

    public void Update()
    {
        Test();
    }

    public void Render()
    {
        if (!render)
            return;

        GL.Disable(EnableCap.DepthTest);
        GL.DepthMask(false);
        GL.Enable(EnableCap.Blend);
        GL.Disable(EnableCap.CullFace);
        GL.FrontFace(FrontFaceDirection.Ccw);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        Matrix4 model = Matrix4.Identity;

        _uiShader.Bind();
        _uItexture.Bind();

        int modelLoc = GL.GetUniformLocation(_uiShader.ID, "model");
        int projectionLoc = GL.GetUniformLocation(_uiShader.ID, "projection");

        GL.UniformMatrix4(modelLoc, true, ref model);
        GL.UniformMatrix4(projectionLoc, true, ref OrthographicProjection);

        uIMesh.Render();
        
        _uiShader.Unbind();
        _uItexture.Unbind();



        _textShader.Bind();
        _textTexture.Bind();

        int textModelLoc = GL.GetUniformLocation(_textShader.ID, "model");
        int textProjectionLoc = GL.GetUniformLocation(_textShader.ID, "projection");
        int charsLoc = GL.GetUniformLocation(_textShader.ID, "charBuffer");
        
        GL.UniformMatrix4(textModelLoc, true, ref model);
        GL.UniformMatrix4(textProjectionLoc, true, ref OrthographicProjection);
        GL.Uniform1(charsLoc, 1);
        
        textMesh.Render();
        
        _textShader.Unbind();
        _textTexture.Unbind();
        

        GL.Enable(EnableCap.StencilTest);
        GL.Clear(ClearBufferMask.StencilBufferBit);
        GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
        GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace); 

        _uiShader.Bind();
        _uItexture.Bind();

        model = Matrix4.Identity;
        
        modelLoc = GL.GetUniformLocation(_uiShader.ID, "model");
        projectionLoc = GL.GetUniformLocation(_uiShader.ID, "projection");

        GL.UniformMatrix4(modelLoc, true, ref model);
        GL.UniformMatrix4(projectionLoc, true, ref OrthographicProjection);

        maskMesh.Render();
        
        GL.StencilFunc(StencilFunction.Equal, 1, 0xFF);
        GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);

        maskeduIMesh.Render();

        _uiShader.Unbind();
        _uItexture.Unbind();

        _textShader.Bind();
        _textTexture.Bind();

        model = Matrix4.Identity;

        textModelLoc = GL.GetUniformLocation(_textShader.ID, "model");
        textProjectionLoc = GL.GetUniformLocation(_textShader.ID, "projection");
        charsLoc = GL.GetUniformLocation(_textShader.ID, "charBuffer");
        
        GL.UniformMatrix4(textModelLoc, true, ref model);
        GL.UniformMatrix4(textProjectionLoc, true, ref OrthographicProjection);
        GL.Uniform1(charsLoc, 1);
        
        maskedTextMesh.Render();
        
        _textShader.Unbind();
        _textTexture.Unbind();

        GL.Disable(EnableCap.StencilTest);
        GL.DepthMask(true);
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Lequal);
    }
}

public enum MeshType
{
    UnMasked,
    Masked
}