using OpenTK.Graphics.OpenGL4;

public class UIData
{
    public static ShaderProgram UiShader = new ShaderProgram("UI/UI.vert", "UI/UI.frag");
    public static TextureArray UiTexture = new TextureArray("ProjectManagerUI.png", 64, 64);
    public static ShaderProgram TextShader = new ShaderProgram("Text/Text copy.vert", "Text/Text copy.frag");
    public static Texture TextTexture = new Texture("text.png");

    public static int modelLoc = -1;
    public static int projectionLoc = -1;
    public static int cycleLoc = -1;

    public static int textModelLoc = -1;
    public static int textProjectionLoc = -1;
    public static int charsLoc = -1;

    public static int maskModelLoc = -1;
    public static int maskProjectionLoc = -1;

    public static int maskTextModelLoc = -1;
    public static int maskTextProjectionLoc = -1;
    public static int maskCharsLoc = -1;

    static UIData()
    {
        // Normal Shader
        UiShader.Bind();

        modelLoc = GL.GetUniformLocation(UiShader.ID, "model");
        projectionLoc = GL.GetUniformLocation(UiShader.ID, "projection");
        cycleLoc = GL.GetUniformLocation(UiShader.ID, "cycle");

        UiShader.Unbind();

        TextShader.Bind();

        textModelLoc = GL.GetUniformLocation(TextShader.ID, "model");
        textProjectionLoc = GL.GetUniformLocation(TextShader.ID, "projection");
        charsLoc = GL.GetUniformLocation(TextShader.ID, "charBuffer");

        TextShader.Unbind();

        // Mask Shader
        UiShader.Bind();

        maskModelLoc = GL.GetUniformLocation(UiShader.ID, "model");
        maskProjectionLoc = GL.GetUniformLocation(UiShader.ID, "projection");

        UiShader.Unbind();

        TextShader.Bind();

        maskTextModelLoc = GL.GetUniformLocation(TextShader.ID, "model");
        maskTextProjectionLoc = GL.GetUniformLocation(TextShader.ID, "projection");
        maskCharsLoc = GL.GetUniformLocation(TextShader.ID, "charBuffer");

        TextShader.Unbind();
    }
}