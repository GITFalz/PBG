using OpenTK.Graphics.OpenGL4;

public class UIData
{
    public static ShaderProgram UiShader = new ShaderProgram("UI/UI.vert", "UI/UI.frag");
    public static TextureArray UiTexture = new TextureArray("ProjectManagerUI.png", 64, 64);

    public static int modelLoc = -1;
    public static int projectionLoc = -1;
    public static int textureArrayLoc = -1;

    
    public static ShaderProgram TextShader = new ShaderProgram("Text/Text.vert", "Text/Text.frag");
    public static Texture TextTexture = new Texture("Text2.png");

    public static int textModelLoc = -1;
    public static int textProjectionLoc = -1;
    public static int textTextureLoc = -1;

    static UIData()
    {
        // Normal Shader
        UiShader.Bind();

        modelLoc = UiShader.GetLocation("model");
        projectionLoc = UiShader.GetLocation("projection");
        textureArrayLoc = UiShader.GetLocation("textureArray");

        UiShader.Unbind();

        // Text Shader
        TextShader.Bind();

        textModelLoc = GL.GetUniformLocation(TextShader.ID, "model");
        textProjectionLoc = GL.GetUniformLocation(TextShader.ID, "projection");
        textTextureLoc = GL.GetUniformLocation(TextShader.ID, "texture0");

        TextShader.Unbind();
    }
}