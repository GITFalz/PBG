using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class UIController : Component
{
    public UIMesh uIMesh = new();
    public TextMesh textMesh = new();
    public static ShaderProgram _uiShader = UIData.UiShader;
    public static TextureArray _uItexture = UIData.UiTexture;
    public static ShaderProgram _textShader = UIData.TextShader;
    public static Texture _textTexture = UIData.TextTexture;

    public static Matrix4 OrthographicProjection = Matrix4.Identity;

    public List<UIPanel> UIElements = [];
    public List<UIText> TextElements = [];
    public List<UIButton> Buttons = [];
    public List<UIElement> Elements = [];
    public static List<UIInputField> InputFields = [];

    public static UIInputField? activeInputField = null;

    public bool render = true;

    public void AddElement(UIElement element, bool test = false)
    {
        Elements.Add(element);

        if (test)
            element.test = true;

        if (element is UIPanel panel)
        {
            panel.SetUIMesh(uIMesh);
            UIElements.Add(panel);
            foreach (var child in panel.Children)
            {
                AddElement(child);
            }
        }

        if (element is UIButton button)
        {
            button.SetUIMesh(uIMesh);
            Buttons.Add(button);
        }

        if (element is UIText textElement)
        {
            if (textElement is UIInputField inputField)
            {
                InputFields.Add(inputField);
                AddElement(inputField.Button);
            }

            textElement.SetTextMesh(textMesh);
            TextElements.Add(textElement);
        }
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
        int offset = 0;

        foreach (var element in UIElements)
        {
            element.Generate();
        }

        foreach (var element in Buttons)
        {
            element.Generate();
        }

        foreach (var element in TextElements)
        {
            element.Generate(ref offset);
        }
    }

    public void Buffers()
    {
        uIMesh.GenerateBuffers();
        textMesh.GenerateBuffers();
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
    }

    public void UpdateTextures()
    {
        uIMesh.UpdateTexture();
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
        UIElements.Clear();
        TextElements.Clear();
        Buttons.Clear();
        Elements.Clear();
    }

    public override void OnResize()
    {
        foreach (var element in Elements)
        {
            element.Align();
            element.UpdateScale();
        }

        UpdateMatrices();
        uIMesh.UpdateVertices();
    }

    public override void Update()
    {
        Test();
        base.Update();
    }

    public override void Render()
    {
        if (!render)
            return;

        GL.DepthMask(true);
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.Disable(EnableCap.CullFace);
        GL.FrontFace(FrontFaceDirection.Ccw);

        _uiShader.Bind();
        _uItexture.Bind();

        Matrix4 model = Matrix4.Identity;
        
        int modelLoc = GL.GetUniformLocation(_uiShader.ID, "model");
        int projectionLoc = GL.GetUniformLocation(_uiShader.ID, "projection");

        GL.UniformMatrix4(modelLoc, true, ref model);
        GL.UniformMatrix4(projectionLoc, true, ref OrthographicProjection);

        uIMesh.Render();
        
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
        
        textMesh.Render();
        
        _textShader.Unbind();
        _textTexture.Unbind();

        GL.Enable(EnableCap.DepthTest);
    }
}