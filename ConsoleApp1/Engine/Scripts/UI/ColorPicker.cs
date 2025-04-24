using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class ColorPicker
{
    public int Width;
    public int Height;

    private static ShaderProgram _pickerShader = new ShaderProgram("Painting/Rectangle.vert", "Painting/Picker.frag");
    private static ShaderProgram _pickerBarShader = new ShaderProgram("Painting/Rectangle.vert", "Painting/PickerBar.frag");
    private VAO _colorPickerVao = new VAO();
    private int _barModelLocation = -1;
    private int _barProjectionLocation = -1;
    private int _barSizeLocation = -1;

    private int _pickerModelLocation = -1;  
    private int _pickerProjectionLocation = -1;
    private int _pickerSizeLocation = -1;
    private int _pickerColorLocation = -1;

    public Vector2i ColorPickerPosition = new Vector2i(94, 80);
    

    public float ColorPickerSize
    {
        get => 1 / _colorPickerSize;
        set
        {
            _colorPickerSize = 1 / value;
            _colorPickerScale = new Vector2(Width, Height) / _colorPickerSize;
        }
    }

    public Vector3 BaseColor = new Vector3(1, 0, 0);
    public Vector4 Color = new Vector4(1, 0, 0, 1f);
    public float Saturation = 0f;
    public float Brightness = 0f;

    private Vector2i _colorPickerPosition = new Vector2i(100, 100);
    private Vector2 _colorPickerScale = new Vector2(1, 1);

    private float _colorPickerSize = 2f;

    public UIController ColorPickerController;

    private UICollection _colorPanelCollection;
    private UIButton _colorButton;


    public ColorPicker(int width, int height, Vector2i position)
    {
        Width = width;
        Height = height;

        _colorPickerPosition = position + (6, 20);

        Vector2i newPosition = ((int)_colorPickerPosition.X, (int)-_colorPickerPosition.Y + (Game.Height - Height));
        ColorPickerPosition = newPosition;
        ColorPickerSize = 1f;

        _barModelLocation = _pickerBarShader.GetLocation("model");
        _barProjectionLocation = _pickerBarShader.GetLocation("projection");
        _barSizeLocation = _pickerBarShader.GetLocation("size");

        _pickerModelLocation = _pickerShader.GetLocation("model");
        _pickerProjectionLocation = _pickerShader.GetLocation("projection");
        _pickerSizeLocation = _pickerShader.GetLocation("size");
        _pickerColorLocation = _pickerShader.GetLocation("color");

        ColorPickerController = new();

        _colorPanelCollection = new("ColorPanelCollection", ColorPickerController, AnchorType.TopLeft, PositionType.Absolute, (0, 0, 0), (1, 1), (position.X, position.Y, 0, 0), 0);

        _colorButton = new UIButton("ColorEditorMoveButton", ColorPickerController, AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f, 1f), (0, 0, 0), (Width + 12, 14), (0, 0, 0, 0), 0, 10, (5, 0.025f), UIState.Interactable);
        UIImage ColorBG = new UIImage("ColorPanelBackGround", ColorPickerController, AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f, 1f), (0, 0, 0), (Width + 12, Height + 12), (0, 14, 0, 0), 0, 1, (10, 0.05f));
        UIButton ColorPickSlider = new UIButton("ColorPickSlider", ColorPickerController, AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f, 1f), (0, 0, 0), (12, 12), (0, 14, 0, 0), 0, 10, (5, 0.025f), UIState.Interactable);
        UIButton ColorBarSlider = new UIButton("ColorBarSlider", ColorPickerController, AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f, 1f), (0, 0, 0), (12, 12), (0, Height + 4, 0, 0), 0, 10, (5, 0.025f), UIState.Interactable);

        _colorButton.Depth = 10;
        ColorBG.Depth = 10;
        ColorPickSlider.Depth = 15;
        ColorBarSlider.Depth = 15;
        ColorBG.CanTest = true;

        _colorButton.SetOnHold(() => {
            Vector2 mouseDelta = Input.GetMousePosition();
            if (mouseDelta == Vector2.Zero) return;
            UpdateColorPickerPosition();
        });

        ColorBG.SetOnHold(() => 
        {
            float x = Mathf.Clamp(0, Width, Input.GetMousePosition().X - ColorPickerPosition.X);
            float y = Input.GetMousePosition().Y - (Game.Height - ColorPickerPosition.Y) + Height;

            if (y > Height - 20)
            {
                ColorBarSlider.Offset.X = x;
                ColorBarSlider.Align();
                ColorBarSlider.UpdateTransformation();

                float rX = (Input.GetMousePosition().X - ColorPickerPosition.X) / Width;
                float h = Mathf.Clamp(0, 360, rX * 360);
                
                Vector3 color = (1, 0, 0);

                if (h < 60) color = (1, h / 60, 0);
                else if (h < 120) color = (1 - (h - 60) / 60, 1, 0);
                else if (h < 180) color = (0, 1, (h - 120) / 60);
                else if (h < 240) color = (0, 1 - (h - 180) / 60, 1);
                else if (h < 300) color = ((h - 240) / 60, 0, 1);
                else color = (1, 0, 1 - (h - 300) / 60);

                BaseColor = color;
            }

            x = Input.GetMousePosition().X - ColorPickerPosition.X;
            y = (Input.GetMousePosition().Y - (Game.Height - ColorPickerPosition.Y) + Height);

            if (y <= Height - 20)
            {
                x = Mathf.Clamp(0, Height - 20, x);
                y = Mathf.Clamp(0, Height - 20, y);

                ColorPickSlider.Offset.X = x;
                ColorPickSlider.Offset.Y = y + 14;
                ColorPickSlider.Align();
                ColorPickSlider.UpdateTransformation();

                float rX = x / (Height - 20);
                float rY = y / (Height - 20);

                Saturation = rX;
                Brightness = 1 - rY;
            }

            Color = new Vector4(
                Mathf.Lerp(1, BaseColor.X, Saturation),
                Mathf.Lerp(1, BaseColor.Y, Saturation),
                Mathf.Lerp(1, BaseColor.Z, Saturation), 
                1f
            );

            Color *= Brightness;
            Color.W = 1f;
        });

        _colorPanelCollection.AddElements(_colorButton, ColorBG, ColorPickSlider, ColorBarSlider);

        ColorPickerController.AddElements(_colorPanelCollection);
    }

    public void UpdateColorPickerPosition()
    {
        _colorPickerPosition += Mathf.FloorToInt(Input.GetMouseDelta()); 
        Vector2i newPosition = ((int)_colorPickerPosition.X, (int)-_colorPickerPosition.Y + (Game.Height - Height));
        ColorPickerPosition = newPosition;

        _colorPanelCollection.SetOffset((_colorPickerPosition.X - 6, _colorPickerPosition.Y - 20, 0, 0));
        _colorPanelCollection.Align();
        _colorPanelCollection.UpdateTransformation();
    }

    public void Resize()
    {
        Vector2i newPosition = ((int)_colorPickerPosition.X, (int)-_colorPickerPosition.Y + (Game.Height - Height));
        ColorPickerPosition = newPosition;
    }

    public void Update()
    {
        ColorPickerController.Test();
    }

    public void RenderTexture()
    {
        GL.Clear(ClearBufferMask.DepthBufferBit);

        ColorPickerController.RenderDepthTest();

        GL.Viewport(ColorPickerPosition.X, ColorPickerPosition.Y, Width, Height);

        float minSize = Mathf.Min(_colorPickerScale.X, _colorPickerScale.Y - 20);

        _pickerBarShader.Bind();

        Matrix4 model = Matrix4.CreateTranslation(0, _colorPickerScale.Y - 20, 0.06f);
        Matrix4 projection = Matrix4.CreateOrthographicOffCenter(0, Width, Height, 0, -1, 1);

        GL.UniformMatrix4(_barModelLocation, true, ref model);
        GL.UniformMatrix4(_barProjectionLocation, true, ref projection);
        GL.Uniform2(_barSizeLocation, (_colorPickerScale.X, 20));

        _colorPickerVao.Bind();

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        _colorPickerVao.Unbind();

        _pickerBarShader.Unbind();


        _pickerShader.Bind();

        model = Matrix4.CreateTranslation(0, 0, 0.06f);

        GL.UniformMatrix4(_pickerModelLocation, true, ref model);
        GL.UniformMatrix4(_pickerProjectionLocation, true, ref projection);
        GL.Uniform2(_pickerSizeLocation, (minSize, minSize));
        GL.Uniform3(_pickerColorLocation, BaseColor.X, BaseColor.Y, BaseColor.Z);

        _colorPickerVao.Bind();

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        _colorPickerVao.Unbind();

        _pickerShader.Unbind();

        GL.Disable(EnableCap.Blend);

        GL.Viewport(0, 0, Game.Width, Game.Height);
    }

    public void Delete()
    {
        _colorPickerVao.DeleteBuffer();
        ColorPickerController.Delete();
    }
}