public class UIData
{
    public static ShaderProgram UiShader = new ShaderProgram("UI/UI.vert", "UI/UI.frag");
    public static TextureArray UiTexture = new TextureArray("ProjectManagerUI.png", 64, 64);
    public static ShaderProgram TextShader = new ShaderProgram("Text/Text.vert", "Text/Text.frag");
    public static Texture TextTexture = new Texture("text.png");
}