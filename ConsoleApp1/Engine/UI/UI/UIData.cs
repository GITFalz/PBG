using OpenTK.Graphics.OpenGL4;

public class UIData
{
    public static ShaderProgram UiShader = new ShaderProgram("UI/UI.vert", "UI/UI.frag");
    public static TextureArray UiTexture = new TextureArray("ProjectManagerUI.png", 64, 64);
    public static ShaderProgram TextShader = new ShaderProgram("Text/Text.vert", "Text/Text.frag");
    public static Texture TextTexture = new Texture("text.png");

    public static int modelLoc = -1;
    public static int projectionLoc = -1;
    public static int cycleLoc = -1;
    public static int textureLoc = -1;
    public static int charsLoc = -1;
    public static int textureArrayLoc = -1;

    public static int textModelLoc = -1;
    public static int textProjectionLoc = -1;
    public static int textTextureLoc = -1;

    public static int maskModelLoc = -1;
    public static int maskProjectionLoc = -1;

    public static int maskTextModelLoc = -1;
    public static int maskTextProjectionLoc = -1;
    public static int maskTextTextureLoc = -1;
    public static int maskCharsLoc = -1;

    static UIData()
    {
        // Normal Shader
        UiShader.Bind();

        modelLoc = UiShader.GetLocation("model");
        projectionLoc = UiShader.GetLocation("projection");
        cycleLoc = UiShader.GetLocation("cycle");
        textureLoc = UiShader.GetLocation("texture0");
        charsLoc = UiShader.GetLocation("charBuffer");
        textureArrayLoc = UiShader.GetLocation("textureArray");

        UiShader.Unbind();

        TextShader.Bind();

        textModelLoc = GL.GetUniformLocation(TextShader.ID, "model");
        textProjectionLoc = GL.GetUniformLocation(TextShader.ID, "projection");
        textTextureLoc = GL.GetUniformLocation(TextShader.ID, "texture0");

        TextShader.Unbind();

        // Mask Shader
        UiShader.Bind();

        maskModelLoc = GL.GetUniformLocation(UiShader.ID, "model");
        maskProjectionLoc = GL.GetUniformLocation(UiShader.ID, "projection");

        UiShader.Unbind();

        TextShader.Bind();

        maskTextModelLoc = GL.GetUniformLocation(TextShader.ID, "model");
        maskTextProjectionLoc = GL.GetUniformLocation(TextShader.ID, "projection");
        maskTextTextureLoc = GL.GetUniformLocation(TextShader.ID, "texture0");
        maskCharsLoc = GL.GetUniformLocation(TextShader.ID, "charBuffer");

        TextShader.Unbind();
    }
}