public class UIData
{
    public static ShaderProgram UiShader = new ShaderProgram("NewUI/UI.vert", "NewUI/UI.frag");
    public static TextureArray UiTexture = new TextureArray("UI_Atlas.png", 64, 64);
    public static ShaderProgram TextShader = new ShaderProgram("NewText/Text.vert", "NewText/Text.frag");
    public static Texture TextTexture = new Texture("text.png");
}