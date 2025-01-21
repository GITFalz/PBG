using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class UIController : Updateable
{
    public UIMesh uIMesh = new();
    public TextMesh textMesh = new();
    public static ShaderProgram _uiShader = new ShaderProgram("NewUI/UI.vert", "NewUI/UI.frag");
    public static TextureArray _uItexture = new TextureArray("UI_Atlas.png", 64, 64);
    public static ShaderProgram _textShader = new ShaderProgram("NewUI/Text.vert", "NewUI/Text.frag");
    public static Texture _textTexture = new Texture("text.png");

    public static Matrix4 OrthographicProjection = Matrix4.Identity;


    public List<UIElement> UIElements = [];
    public List<UIElement> TextElements = [];

    public void AddElement(UIElement element)
    {
        if (element is UIPanel panel)
        {
            panel.uIMesh = uIMesh;
            UIElements.Add(panel);
            foreach (var child in panel.Children)
            {
                AddElement(child);
            }
        }

        if (element is UIText textElement)
        {
            textElement.textMesh = textMesh;
            TextElements.Add(textElement);
        }
    }

    public void GenerateBuffers()
    {
        int offset = 0;

        foreach (var element in UIElements)
        {
            element.Generate();
        }

        foreach (var element in TextElements)
        {
            element.Generate(ref offset);
        }

        uIMesh.GenerateBuffers();
        textMesh.GenerateBuffers();
    }

    public void UpdateMatrices()
    {
        uIMesh.UpdateMatrices();
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Render()
    {
        GL.Disable(EnableCap.DepthTest);
        GL.DepthMask(false);
        GL.Enable(EnableCap.Blend);
        GL.Disable(EnableCap.CullFace);
        GL.FrontFace(FrontFaceDirection.Ccw);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        
        _uiShader.Bind();
        _uItexture.Bind();

        Matrix4 model = Matrix4.Identity;
        
        int modelLoc = GL.GetUniformLocation(_uiShader.ID, "model");
        int projectionLoc = GL.GetUniformLocation(_uiShader.ID, "projection");

        GL.UniformMatrix4(modelLoc, true, ref model);
        GL.UniformMatrix4(projectionLoc, true, ref OrthographicProjection);
        
        GL.DepthMask(true);
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Lequal);
        
        
        //Render unmasked Ui
        uIMesh.Render();

        //Console.WriteLine(uIMesh.TransformationMatrices[0]);

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
        
        textMesh.Render();
        
        _textShader.Unbind();
        _textTexture.Unbind();
    }
}