using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class UIController
{
    public static List<UIController> Controllers = [];
    public static UIController Empty = new();

    public UIMesh UIMesh = new();
    public TextMesh TextMesh = new();
    public MaskData MaskData = new();
    
    public static ShaderProgram _uiShader = UIData.UiShader;
    public static TextureArray _uItexture = UIData.UiTexture;
    public static ShaderProgram _textShader = UIData.TextShader;
    public static Texture _textTexture = UIData.TextTexture;

    public static Matrix4 OrthographicProjection = Matrix4.CreateOrthographicOffCenter(0, 1, 1, 0, -1, 1);
    public List<UIElement> Elements = [];
    public List<UIElement> AbsoluteElements = [];
    public List<UIScrollView> ScrollViews = [];
    public List<UIButton> Buttons = [];
    public static List<UIInputField> InputFields = [];

    public static UIInputField? activeInputField = null;

    public bool render = true;
    public static int TextOffset = 0;

    public bool RegenerateBuffers = false;

    public Queue<UIElement> ElementsToAdd = [];  
    public Queue<UIElement> ElementsToRemove = [];

    public Queue<UIElement> AddedElements = [];
    public Queue<UIElement> RemovedElements = [];

    private Queue<UIElement> ElementsToAlign = [];
    private Queue<UIElement> ElementsToTransform= [];

    public Matrix4 ModelMatrix = Matrix4.Identity;

    public Vector3 Position = (0, 0, 0);
    public float Scale = 1f;

    public Vector3 _localPosition = (0, 0, 0);

    /// <summary>
    /// Make sure to call this constructor inside of another constructor (no static fields), otherwise the ui will not render and will trow errors.
    /// </summary>
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

        ElementsToAdd.Enqueue(element);
        RegenerateBuffers = true;
    }

    private void Internal_AddElement(UIElement element)
    {
        if (element.PositionType == PositionType.Absolute)
            AbsoluteElements.Add(element);

        element.CanUpdate = true;
        Elements.Add(element);
        AddedElements.Enqueue(element);

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
        
        ElementsToRemove.Enqueue(element);
        RegenerateBuffers = true;
    }
    
    private void Internal_RemoveElement(UIElement element)
    {
        if (element.PositionType == PositionType.Absolute) 
            AbsoluteElements.Remove(element);

        if (element is UIPanel panel)
        {
            if (panel is UIButton button)
            {
                Buttons.Remove(button);
            }
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
            foreach (var e in collection.Elements)
            {
                Internal_RemoveElement(e);
                e.Clear();
            }
        }

        element.CanUpdate = false;
        Elements.Remove(element);
    }

    public void QueueAlign(UIElement element)
    {
        ElementsToAlign.Enqueue(element);
    }

    public void QueueElementTransformation(UIElement element)
    {
        ElementsToTransform.Enqueue(element);
    }   

    public void SetPosition(Vector2 position)
    {
        SetPosition(new Vector3(position.X, position.Y, Position.Z));
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
        ApplyBufferChanges();

        foreach (var element in Elements)
            element.Test(newOffset);

        FinalizeBuffers();

        PostTestUpdates();
        UpdateMeshes();
    }

    public void ApplyBufferChanges()
    {
        if (!RegenerateBuffers)
            return;

        while (ElementsToRemove.Count > 0)
            Internal_RemoveElement(ElementsToRemove.Dequeue());

        while (ElementsToAdd.Count > 0)
            Internal_AddElement(ElementsToAdd.Dequeue());
    }

    public void FinalizeBuffers()
    {
        if (!RegenerateBuffers)
            return;

        foreach (var element in AbsoluteElements)
            element.Align();

        foreach (var element in AddedElements)
            element.Generate();

        foreach (var scrollView in ScrollViews)
            scrollView.GenerateMask();

        RemovedElements.Clear();
        AddedElements.Clear();

        ElementsToAdd.Clear();
        ElementsToRemove.Clear();

        RegenerateBuffers = false;
    }

    /// <summary>
    /// This is where stuff happens that you want to have happen after the test but before the update.
    /// </summary>
    public void PostTestUpdates()
    {
        while (ElementsToAlign.Count > 0)
        {
            UIElement element = ElementsToAlign.Dequeue();
            element.Align();
        }

        while (ElementsToTransform.Count > 0)
        {
            UIElement element = ElementsToTransform.Dequeue();
            element.UpdateTransformation();
        }
    }

    public void UpdateMeshes()
    {
        MaskData.Update();
        UIMesh.Update();
        TextMesh.Update();
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
        if (activeInputField == null || key == Keys.LeftShift || key == Keys.RightShift || key == Keys.RightAlt || key == Keys.LeftAlt || key == Keys.LeftControl || key == Keys.RightControl)
            return;
        
        if (key == Keys.Backspace)
        {
            activeInputField.RemoveCharacter();
            activeInputField.OnTextChange?.Invoke();
            return;
        }
        
        if (!Char.GetChar(out char c, key, Input.AreKeysDown(Keys.LeftShift, Keys.RightShift), Input.AreKeysDown(Keys.RightAlt)))
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
        foreach (var element in AbsoluteElements)
        {
            RemoveElement(element);
        }

        foreach (var element in ElementsToRemove)
        {
            Internal_RemoveElement(element);
        }

        MaskData.Clear();
        UIMesh.Clear();
        TextMesh.Clear(); 

        Elements.Clear();
        AbsoluteElements.Clear();
        ScrollViews.Clear();
        Buttons.Clear();
        InputFields.Clear();
        ElementsToAdd.Clear();
        ElementsToRemove.Clear();
    }

    public void Delete()
    {
        Clear();
        UIMesh.Delete();
        MaskData.Delete();
        TextMesh.Delete();
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
            _uItexture.Bind(TextureUnit.Texture0);
            _uiShader.Bind();

            GL.UniformMatrix4(UIData.modelLoc, true, ref model);
            GL.UniformMatrix4(UIData.projectionLoc, true, ref orthographicsProjectionMatrix);
            GL.Uniform1(UIData.textureArrayLoc, 0);

            MaskData.UIMaskSSBO.Bind(1);

            UIMesh.Render();
        
            Shader.Error("Ui render error: ");

            MaskData.UIMaskSSBO.Unbind();
            _uItexture.Unbind();
            _uiShader.Unbind();
        }

        if (TextMesh.ElementCount > 0)
        {
            _textTexture.Bind(TextureUnit.Texture0);
            _textShader.Bind();
            MaskData.UIMaskSSBO.Bind(2);

            GL.UniformMatrix4(UIData.textModelLoc, true, ref model);
            GL.UniformMatrix4(UIData.textProjectionLoc, true, ref orthographicsProjectionMatrix);
            GL.Uniform1(UIData.textTextureLoc, 0);

            TextMesh.Render();

            Shader.Error("Text render error: ");

            MaskData.UIMaskSSBO.Unbind();
            _textShader.Unbind();
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