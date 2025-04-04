using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class ColorPicker
{
    public static int Width;
    public static int Height;

    private static ShaderProgram _pickerShader = new ShaderProgram("Painting/Rectangle.vert", "Painting/Picker.frag");
    private static ShaderProgram _pickerBarShader = new ShaderProgram("Painting/Rectangle.vert", "Painting/PickerBar.frag");
    private static VAO _colorPickerVao = new VAO();

    public static Vector2 ColorPickerPosition
    {
        get => _colorPickerPosition;
        set
        {
            _colorPickerPosition = value;
        }
    }
    

    public static float ColorPickerSize
    {
        get => 1 / _colorPickerSize;
        set
        {
            _colorPickerSize = 1 / value;
            _colorPickerScale = new Vector2(Width, Height) / _colorPickerSize;
        }
    }

    // Red
    public static Vector3 Color = new Vector3(1, 0, 0);
    public static float Saturation = 0f;
    public static float Brightness = 0f;

    private static Vector2 _colorPickerPosition = new Vector2(0, 0);
    private static Vector2 _colorPickerScale = new Vector2(1, 1);

    private static float _colorPickerSize = 2f;

    public ColorPicker(int width, int height)
    {
        Width = width;
        Height = height;

        ColorPickerPosition = new Vector2(0, 0);
        ColorPickerSize = 1f;
    }

    public static void RenderTexture(Vector2i offset, int width, int height)
    {
        GL.Viewport(offset.X, offset.Y, width, height);

        float minSize = Mathf.Min(_colorPickerScale.X, _colorPickerScale.Y - 20);

        _pickerBarShader.Bind();

        Matrix4 model = Matrix4.CreateTranslation(0, _colorPickerScale.Y - 20, 0.06f);
        Matrix4 projection = Matrix4.CreateOrthographicOffCenter(0, width, height, 0, -1, 1);

        int modelLocation = GL.GetUniformLocation(_pickerBarShader.ID, "model");
        int projectionLocation = GL.GetUniformLocation(_pickerBarShader.ID, "projection");
        int sizeLocation = GL.GetUniformLocation(_pickerBarShader.ID, "size");

        GL.UniformMatrix4(modelLocation, true, ref model);
        GL.UniformMatrix4(projectionLocation, true, ref projection);
        GL.Uniform2(sizeLocation, (_colorPickerScale.X, 20));

        _colorPickerVao.Bind();

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        _colorPickerVao.Unbind();

        _pickerBarShader.Unbind();


        _pickerShader.Bind();

        model = Matrix4.CreateTranslation(0, 0, 0.06f);
        projection = Matrix4.CreateOrthographicOffCenter(0, width, height, 0, -1, 1);

        modelLocation = GL.GetUniformLocation(_pickerShader.ID, "model");
        projectionLocation = GL.GetUniformLocation(_pickerShader.ID, "projection");
        sizeLocation = GL.GetUniformLocation(_pickerShader.ID, "size");
        int colorLocation = GL.GetUniformLocation(_pickerShader.ID, "color");

        GL.UniformMatrix4(modelLocation, true, ref model);
        GL.UniformMatrix4(projectionLocation, true, ref projection);
        GL.Uniform2(sizeLocation, (minSize, minSize));
        GL.Uniform3(colorLocation, Color.X, Color.Y, Color.Z);

        _colorPickerVao.Bind();

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        _colorPickerVao.Unbind();

        _pickerShader.Unbind();

        GL.Disable(EnableCap.Blend);

        GL.Viewport(0, 0, Game.Width, Game.Height);
    }
}