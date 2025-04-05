using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class DrawingPanel
{
    public static int Width;
    public static int Height;

    private static FBO _fbo = new FBO(100, 100);
    private static ShaderProgram _paintingShader = new ShaderProgram("Painting/Painting.vert", "Painting/Painting.frag");
    private static ShaderProgram _textureShader = new ShaderProgram("Painting/Rectangle.vert", "Painting/Texture.frag");
    private static ShaderProgram _brushCircleShader = new ShaderProgram("Painting/Rectangle.vert", "Painting/CircleOutline.frag");
    private static VAO _vao = new VAO();
    private static VAO _textureVao = new VAO();
    
    private static Matrix4 _projectionMatrix;
    private static Matrix4 _textureProjectionMatrix = Matrix4.Identity;

    public static Vector4 BrushColor = new Vector4(0, 0, 0, 1);

    public static bool IsDrawing = false;

    private static float _brushHalfSize = 50;
    private static float _brushSize = 100;
    public static float BrushSize 
    {
        get {
            return _brushSize;
        }
        set
        {
            _brushSize = value;
            _brushHalfSize = value / 2f;
        }
    }

    public static Vector2 DrawingCanvasPosition
    {
        get => _drawingCanvasPosition;
        set
        {
            _drawingCanvasPosition = value;
            _drawingCanvasOffset = _drawingCanvasPosition * _drawingCanvasSize;
        }
    }

    public static void SetDrawingCanvasPosition(float x, float y) { SetDrawingCanvasPosition((x, y));}
    public static void SetDrawingCanvasPosition(Vector2 position) { DrawingCanvasPosition = position; }

    public static float DrawingCanvasSize
    {
        get => 1 / _drawingCanvasSize;
        set
        {
            _drawingCanvasSize = 1 / value;
            _drawingCanvasScale = new Vector2(Width, Height) / _drawingCanvasSize;
            _drawingCanvasOffset = _drawingCanvasPosition * _drawingCanvasSize;
        }
    }

    public static void SetDrawingCanvasSize(float size) { DrawingCanvasSize = size; }

    public static void ZoomAt(Vector2 center, float zoomFactor)
    {
        DrawingCanvasSize += zoomFactor; 
    }

    public static void ZoomBrush(float zoomFactor)
    {
        BrushSize += zoomFactor;
        Console.WriteLine($"Brush Size: {BrushSize}");
    }


    private static Vector2 _drawingCanvasPosition = new Vector2(0, 0);
    private static Vector2 _drawingCanvasOffset = new Vector2(0, 0);
    private static Vector2 _drawingCanvasScale = new Vector2(1, 1);

    private static float _drawingCanvasSize = 2f;

    public static DrawingMode DrawingMode = DrawingMode.None;
    public static bool DisplayBrushCircle = false;
    public static void SetDrawingMode(DrawingMode mode) 
    { 
        DrawingMode = mode;
        switch (mode)
        {
            case DrawingMode.Eraser:
                DisplayBrushCircle = true;
                break;
            case DrawingMode.Brush:
                DisplayBrushCircle = true;
                break;
            case DrawingMode.Blur:
                DisplayBrushCircle = true;
                break;
            default:
                DisplayBrushCircle = false;
                break;
        }
    }

    public static float Falloff = 2f;

    public static float BrushStrength = 1f;

    public static int RenderSet = 0;

    public DrawingPanel(int width, int height)
    {
        Width = width;
        Height = height;

        _fbo.Delete();
        FBO.FBOs.Remove(_fbo);
        _fbo = new FBO(width, height);

        _projectionMatrix = Matrix4.CreateOrthographicOffCenter(0, width, height, 0, -1, 1);

        DrawingCanvasPosition = new Vector2(0, 0);
        DrawingCanvasSize = 1f;
    }
    
    public static void RenderFramebuffer()
    {   
        if (!IsDrawing || !Input.IsMouseDown(MouseButton.Left) || DrawingMode == DrawingMode.None || DrawingMode == DrawingMode.Move) 
            return;
            
        // Set the viewport to the FBO size
        GL.Viewport(0, 0, Width, Height);

        _fbo.Bind();

        GL.ClearColor(0.5f, 0.5f, 0.5f, 1.0f);
        GL.Clear(ClearBufferMask.DepthBufferBit);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        _paintingShader.Bind();
        
        Matrix4 model = Matrix4.Identity;
        Vector2 mousePos = Input.GetMousePosition() * _drawingCanvasSize - _drawingCanvasOffset;

        int modelLocation = GL.GetUniformLocation(_paintingShader.ID, "model");
        int projectionLocation = GL.GetUniformLocation(_paintingShader.ID, "projection");
        int sizeLocation = GL.GetUniformLocation(_paintingShader.ID, "size");
        int pointLocation = GL.GetUniformLocation(_paintingShader.ID, "point");
        int radiusLocation = GL.GetUniformLocation(_paintingShader.ID, "radius");  
        int colorLocation = GL.GetUniformLocation(_paintingShader.ID, "color");
        int modeLocation = GL.GetUniformLocation(_paintingShader.ID, "paintMode");
        int falloffLocation = GL.GetUniformLocation(_paintingShader.ID, "falloff");
        int brushStrengthLocation = GL.GetUniformLocation(_paintingShader.ID, "brushStrength");

        GL.UniformMatrix4(modelLocation, false, ref model);
        GL.UniformMatrix4(projectionLocation, true, ref _projectionMatrix);
        GL.Uniform2(sizeLocation, new Vector2(Width, Height));
        GL.Uniform2(pointLocation, mousePos);
        GL.Uniform1(radiusLocation, _drawingCanvasSize * _brushHalfSize);
        GL.Uniform4(colorLocation, BrushColor.X, BrushColor.Y, BrushColor.Z, BrushColor.W);
        GL.Uniform1(modeLocation, (int)DrawingMode);
        GL.Uniform1(falloffLocation, Falloff);
        GL.Uniform1(brushStrengthLocation, BrushStrength);

        _fbo.BindTexture();
        _vao.Bind();

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        _vao.Unbind();
        _fbo.UnbindTexture();

        if (Input.IsKeyPressed(Keys.U))
        {
            _fbo.SaveFramebufferToPNG(Width, Height, "TestFB.png");
        }

        _paintingShader.Unbind();
        _fbo.Unbind();

        GL.Disable(EnableCap.Blend);
    }

    public static void RenderTexture(Vector2i offset, int width, int height, int x, int y)
    {
        GL.Viewport(offset.X, offset.Y, width, height);

        _textureShader.Bind();

        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        Matrix4 model = Matrix4.CreateTranslation(x, y, 0.01f);
        _textureProjectionMatrix = Matrix4.CreateOrthographicOffCenter(0, width, height, 0, -1, 1);

        int modelLocation = GL.GetUniformLocation(_textureShader.ID, "model");
        int projectionLocation = GL.GetUniformLocation(_textureShader.ID, "projection");
        int sizeLocation = GL.GetUniformLocation(_textureShader.ID, "size");

        GL.UniformMatrix4(modelLocation, true, ref model);
        GL.UniformMatrix4(projectionLocation, true, ref _textureProjectionMatrix);
        GL.Uniform2(sizeLocation, _drawingCanvasScale);

        _fbo.BindTexture();
        _textureVao.Bind();

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        _textureVao.Unbind();
        _fbo.UnbindTexture();

        _textureShader.Unbind();

        GL.Viewport(0, 0, Game.Width, Game.Height);
    }

    public static void RenderBrushCircle(Vector2i offset, int width, int height)
    {
        GL.Viewport(offset.X, offset.Y, width, height);

        _brushCircleShader.Bind();

        Vector2 mousePos = Input.GetMousePosition();
        float invertedOffset = Game.Height - height - offset.Y;
        Vector2 pos = mousePos - (offset.X, invertedOffset) - (_brushHalfSize, _brushHalfSize);
        Vector2 brushPos = (mousePos.X, Game.Height - mousePos.Y);

        Matrix4 model = Matrix4.CreateTranslation(pos.X, pos.Y, 0.02f);
        _textureProjectionMatrix = Matrix4.CreateOrthographicOffCenter(0, width, height, 0, -1, 1);

        int modelLocation = GL.GetUniformLocation(_brushCircleShader.ID, "model");
        int projectionLocation = GL.GetUniformLocation(_brushCircleShader.ID, "projection");
        int sizeLocation = GL.GetUniformLocation(_brushCircleShader.ID, "size");
        int pointLocation = GL.GetUniformLocation(_brushCircleShader.ID, "point");
        int radiusLocation = GL.GetUniformLocation(_brushCircleShader.ID, "radius");
        int doFalloffLocation = GL.GetUniformLocation(_brushCircleShader.ID, "brushSet");
        int falloffLocation = GL.GetUniformLocation(_brushCircleShader.ID, "falloff");
        int brushStrengthLocation = GL.GetUniformLocation(_brushCircleShader.ID, "brushStrength");

        GL.UniformMatrix4(modelLocation, true, ref model);
        GL.UniformMatrix4(projectionLocation, true, ref _textureProjectionMatrix);
        GL.Uniform2(sizeLocation, new Vector2(_brushSize, _brushSize));
        GL.Uniform2(pointLocation, brushPos);
        GL.Uniform1(radiusLocation, _brushHalfSize);
        GL.Uniform1(doFalloffLocation, RenderSet);
        GL.Uniform1(falloffLocation, Falloff);
        GL.Uniform1(brushStrengthLocation, BrushStrength);

        _vao.Bind();

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6); 

        _vao.Unbind();

        _brushCircleShader.Unbind();

        RenderSet = 0;

        GL.Disable(EnableCap.Blend);

        GL.Viewport(0, 0, Game.Width, Game.Height);
    }
}

public enum DrawingMode
{
    None = -2,
    Move = -1,
    Eraser = 0,
    Brush = 1,
    Blur = 2,
}