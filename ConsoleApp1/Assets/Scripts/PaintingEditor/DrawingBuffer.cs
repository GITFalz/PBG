using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class DrawingBuffer
{
    public static int Width;
    public static int Height;

    private static FBO _fbo = new FBO(100, 100);
    private static ShaderProgram _paintingShader = new ShaderProgram("Painting/Painting.vert", "Painting/Painting.frag");
    private static ShaderProgram _textureShader = new ShaderProgram("Painting/Texture.vert", "Painting/Texture.frag");
    private static VAO _vao = new VAO();
    private static VAO _textureVao = new VAO();

    private static Matrix4 _projectionMatrix = Matrix4.Identity;

    public static bool IsDrawing = false;

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


    private static Vector2 _drawingCanvasPosition = new Vector2(0, 0);
    private static Vector2 _drawingCanvasOffset = new Vector2(0, 0);
    private static Vector2 _drawingCanvasScale = new Vector2(1, 1);

    private static float _drawingCanvasSize = 2f;

    public static DrawingMode DrawingMode = DrawingMode.None;

    public DrawingBuffer(int width, int height)
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
    
    public static void Render()
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
        Matrix4 projection = _projectionMatrix;
        Vector2 mousePos = Input.GetMousePosition() * _drawingCanvasSize - _drawingCanvasOffset;

        int modelLocation = GL.GetUniformLocation(_paintingShader.ID, "model");
        int projectionLocation = GL.GetUniformLocation(_paintingShader.ID, "projection");
        int sizeLocation = GL.GetUniformLocation(_paintingShader.ID, "size");
        int pointLocation = GL.GetUniformLocation(_paintingShader.ID, "point");

        GL.UniformMatrix4(modelLocation, false, ref model);
        GL.UniformMatrix4(projectionLocation, true, ref projection);
        GL.Uniform2(sizeLocation, new Vector2(Width, Height));
        GL.Uniform2(pointLocation, mousePos);

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

        Matrix4 model = Matrix4.CreateTranslation(x, y, 0);
        Matrix4 projection = Matrix4.CreateOrthographicOffCenter(0, width, height, 0, -1, 1);

        int modelLocation = GL.GetUniformLocation(_textureShader.ID, "model");
        int projectionLocation = GL.GetUniformLocation(_textureShader.ID, "projection");
        int sizeLocation = GL.GetUniformLocation(_textureShader.ID, "size");

        GL.UniformMatrix4(modelLocation, true, ref model);
        GL.UniformMatrix4(projectionLocation, true, ref projection);
        GL.Uniform2(sizeLocation, _drawingCanvasScale);

        _fbo.BindTexture();
        _textureVao.Bind();

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        _textureVao.Unbind();
        _fbo.UnbindTexture();

        _textureShader.Unbind();

        GL.Disable(EnableCap.Blend);

        GL.Viewport(0, 0, Game.Width, Game.Height);
    }
}

public enum DrawingMode
{
    None,
    Move,
    Brush
}