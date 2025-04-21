using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class UIController
{
    public static List<UIController> Controllers = [];
    public static UIController Empty = new();

    public UIMesh UIMesh = new();
    
    public static ShaderProgram _uiShader = UIData.UiShader;
    public static TextureArray _uItexture = UIData.UiTexture;
    public static ShaderProgram _textShader = UIData.TextShader;
    public static Texture _textTexture = UIData.TextTexture;

    public static Matrix4 OrthographicProjection = Matrix4.Identity;
    public List<UIElement> Elements = [];
    public List<UIElement> AbsoluteElements = [];
    public List<UIScrollView> ScrollViews = [];
    public List<UIButton> Buttons = [];
    public static List<UIInputField> InputFields = [];

    public static UIInputField? activeInputField = null;

    public bool render = true;
    public static int TextOffset = 0;

    public bool RegenerateBuffers = false;
    public bool UpdateVisibility = true;

    public List<UIElement> ElementsToAdd = [];  
    public List<UIElement> ElementsToRemove = [];

    public Matrix4 ModelMatrix = Matrix4.Identity;

    public Vector3 Position { get; private set; } = (0, 0, 0);
    public float Scale { get; private set; } = 1f;

    public Vector3 _localPosition = (0, 0, 0);

    public UIController()
    {
        Controllers.Add(this);
    }

    // Adds the element to the list of elements to be added, but does not add it to the UIController yet.
    // This is useful for adding elements in a single pass, rather than immediately adding them.
    // And it also needs to be done at the start of the frame otherwise the list could change it's size mid-loop.
    public void AddElements(params UIPrefab[] elements)
    {
        foreach (UIPrefab element in elements)
        {
            AddElements(element.GetMainElements());
        }
    }

    public void AddElements(params UIElement[][] elements)
    {
        foreach (UIElement[] element in elements)
        {
            AddElements(element);
        }
    }

    public void AddElements(params UIElement[] elements)
    {
        foreach (var element in elements)
        {
            AddElement(element);
        }
    }

    public void AddElement(UIElement element)
    {
        if (ElementsToAdd.Contains(element))
            return;
        
        ElementsToAdd.Add(element);
        RegenerateBuffers = true;
    }

    private void Internal_AddElement(UIElement element)
    {
        if (element.PositionType == PositionType.Absolute)
            AbsoluteElements.Add(element);

        element.CanUpdate = true;
        element.UIController = this;
        Elements.Add(element);

        if (element is UIPanel panel)
        {
            if (panel is UIButton button)
                Buttons.Add(button);
        }
        else if (element is UIText text)
        {
            if (text is UIInputField inputField)
            {
                InputFields.Add(inputField);
            }
        }
        else if (element is UICollection collection)
        {
            if (collection is UIScrollView scrollView)
            {
                ScrollViews.Add(scrollView);
            }

            foreach (var e in collection.Elements)
            {
                Internal_AddElement(e);
            }
        }
    }
    
    // Adds the element to the list of elements to be removed, but does not remove it from the UIController yet.
    // This is needed for removing elements in a single pass, rather than immediately removing them.
    // And it also needs to be done at the start of the frame otherwise the list could change it's size mid-loop.
    public void RemoveElements(params UIPrefab[] elements)
    {
        foreach (var element in elements)
        {
            RemoveElements(element.GetMainElements());
        }
    }

    public void RemoveElements(params UIElement[] elements)
    {
        foreach (var element in elements)
        {
            RemoveElement(element);
        }
    }

    public void RemoveElement(UIElement element)
    {
        if (ElementsToRemove.Contains(element))
            return;
        
        ElementsToRemove.Add(element);
        RegenerateBuffers = true;
    }
    
    private void Internal_RemoveElement(UIElement element)
    {
        if (element.PositionType == PositionType.Absolute)
            AbsoluteElements.Remove(element);

        if (element is UIPanel panel)
        {
            if (panel is UIButton button)
                Buttons.Remove(button);
        }
        else if (element is UIText text)
        {
            if (text is UIInputField inputField)
            {
                InputFields.Remove(inputField);
            }
        }
        else if (element is UICollection collection)
        {
            if (collection is UIScrollView scrollView)
            {
                ScrollViews.Remove(scrollView);
            }

            foreach (var e in collection.Elements)
            {
                Internal_RemoveElement(e);
                e.Clear();
            }
        }

        element.CanUpdate = false;
        Elements.Remove(element);
    }



    public void SetPosition(Vector3 position)
    {
        Position = position;
        ModelMatrix = Matrix4.CreateScale(new Vector3(Scale, Scale, 1f)) * Matrix4.CreateTranslation(Position);
        _localPosition = Vector3.TransformPosition((0, 0, 0), ModelMatrix);
    }

    public void SetScale(float scale)
    {
        Vector3 mousePosition = Input.GetMousePosition3();

        Vector3 offset = mousePosition - Position;
        Vector3 position = offset / Scale;

        Vector3 mPosition = position * scale;
        Vector3 mOffset = mPosition - mousePosition;
        Vector3 newPosition = mOffset * -1;
        Position = newPosition;

        Scale = scale;
        ModelMatrix = Matrix4.CreateScale(new Vector3(Scale, Scale, 1f)) * Matrix4.CreateTranslation(Position);
        _localPosition = Vector3.TransformPosition((0, 0, 0), ModelMatrix);
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
        Test(Vector2.Zero);
    }

    public void Test(Vector2 offset)
    {
        Vector2 newOffset = offset;
        GenerateBuffers();
        foreach (var element in Elements)
        {
            element.Test(newOffset);
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

    public static void AssignInputField(UIInputField inputField)
    {
        Console.WriteLine("Assigning: " + inputField.Name);
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
            activeInputField.OnTextChange?.Invoke();
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

        foreach (var scrollView in ScrollViews)
        {
            scrollView.GenerateMask(); 
        }
    }

    public void PrintMemory()
    {
        string memory = "UIController: " + Elements.Count + " elements\n";
        memory += "Buttons: " + Buttons.Count + "\n";
        memory += "InputFields: " + InputFields.Count + "\n";
        memory += "AbsoluteElements: " + AbsoluteElements.Count + "\n";
        memory += "ElementsToAdd: " + ElementsToAdd.Count + "\n";
        memory += "ElementsToRemove: " + ElementsToRemove.Count + "\n";
        memory += "newUIMesh: " + UIMesh.ElementCount + "\n";
        Console.WriteLine(memory);
    }

    public void Buffers()
    {
        UIMesh.GenerateBuffers();
    }

    public void GenerateBuffers()
    {
        if (RegenerateBuffers)
        {
            foreach (var element in ElementsToRemove)
            {
                Internal_RemoveElement(element);
            }

            foreach (var element in ElementsToAdd)
            {
                Internal_AddElement(element);
            }

            ElementsToAdd.Clear();
            ElementsToRemove.Clear();

            UIMesh.Clear();

            Generate();
            Buffers();

            RegenerateBuffers = false;
        }
            
        if (UpdateVisibility)
        {
            UIMesh.UpdateVisibility();

            UpdateVisibility = false;
        }
    }

    public List<string> ToLines()
    {
        List<string> lines = new List<string>();

        return lines;
    }

    public static void ClearAll()
    {
        foreach (var controller in Controllers)
        {
            controller.Clear();
        }
        Controllers.Clear();
    }

    public void Clear()
    {
        UIMesh.Clear();

        foreach (var element in Elements)
        {
            RemoveElement(element);
        }

        Elements.Clear();
    }

    public void Resize()
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
    }

    public void Update()
    {
        Test();
    }

    public void RenderDepthTest()
    {
        RenderDepthTest(OrthographicProjection);
    }

    public void RenderDepthTest(Matrix4 orthographicsProjectionMatrix)
    {
        GL.Enable(EnableCap.DepthTest);
        GL.DepthMask(true);

        Render(orthographicsProjectionMatrix);

        GL.DepthFunc(DepthFunction.Lequal);
        GL.DepthMask(true);
    }

    public void RenderNoDepthTest()
    {
        RenderNoDepthTest(OrthographicProjection);
    }

    public void RenderNoDepthTest(Matrix4 orthographicsProjectionMatrix)
    {
        GL.Disable(EnableCap.DepthTest);
        GL.DepthMask(false);

        Render(orthographicsProjectionMatrix);

        GL.DepthFunc(DepthFunction.Lequal);
        GL.DepthMask(true);
    }

    private void Render(Matrix4 orthographicsProjectionMatrix)
    {
        if (!render)
            return;

        GL.Enable(EnableCap.Blend);
        GL.Enable(EnableCap.CullFace);
        GL.FrontFace(FrontFaceDirection.Ccw);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        Matrix4 model = ModelMatrix;

        if (UIMesh.ElementCount > 0)
        {
            _textTexture.Bind(TextureUnit.Texture0);
            _uItexture.Bind(TextureUnit.Texture2);
            _uiShader.Bind();

            GL.UniformMatrix4(UIData.modelLoc, true, ref model);
            GL.UniformMatrix4(UIData.projectionLoc, true, ref orthographicsProjectionMatrix);
            GL.Uniform1(UIData.textTextureLoc, 0);
            GL.Uniform1(UIData.charsLoc, 1);
            GL.Uniform1(UIData.textureArrayLoc, 2);

            UIMesh.Render();
        
            //Shader.Error("Ui render error: ");

            _uiShader.Unbind();
            _uItexture.Unbind();
            _textTexture.Unbind();
        }

        GL.Disable(EnableCap.StencilTest);
        GL.DepthMask(true);
        GL.DepthFunc(DepthFunction.Lequal);
    }
}

public enum MeshType
{
    UnMasked,
    Masked
}