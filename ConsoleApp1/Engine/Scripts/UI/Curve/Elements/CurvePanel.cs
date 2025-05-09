    using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class CurvePanel
{
    private static ShaderProgram _curvePanelShader = new ShaderProgram("Painting/Rectangle.vert", "Painting/Curve.frag");

    private static VAO _curvePanelVao = new VAO();

    private static int _modelLocation = -1;
    private static int _projectionLocation = -1;
    private static int _sizeLocation = -1;
    private static int _pointCountLocation = -1;


    private SSBO<Vector2> _pointSSBO = new(new Vector2[]{(0, 0), (0.5f, 0.5f), (1,0.2f)});
    private List<Vector2> _points = [(0, 0), (0.5f, 0.5f), (1, 0.2f)];

    public Vector2 Position {
        get => _position;
        set
        {
            _position = value;
            ModelMatrix = Matrix4.CreateTranslation(value.X, value.Y, 0);
        }
    }
    private Vector2 _position;
    public Vector2 Size = new Vector2(300, 300);

    public Matrix4 ModelMatrix = Matrix4.Identity;
    public Matrix4 ProjectionMatrix;

    static CurvePanel()
    {
        _modelLocation = _curvePanelShader.GetLocation("model");
        _projectionLocation = _curvePanelShader.GetLocation("projection");
        _sizeLocation = _curvePanelShader.GetLocation("size");
        _pointCountLocation = _curvePanelShader.GetLocation("pointCount");
    }

    public CurvePanel()
    {
        Position = new Vector2(100, 100);
        ProjectionMatrix = UIController.OrthographicProjection;
    }

    public void Render()
    {
        Render(ProjectionMatrix);
    }

    public void Render(Matrix4 orthographicProjectionMatrix)
    {
        _curvePanelShader.Bind();

        Matrix4 model = ModelMatrix;
        Matrix4 projection = orthographicProjectionMatrix;

        GL.UniformMatrix4(_modelLocation, true, ref model);
        GL.UniformMatrix4(_projectionLocation, true, ref projection);
        GL.Uniform2(_sizeLocation, Size);
        GL.Uniform1(_pointCountLocation, _points.Count);
        
        _curvePanelVao.Bind();
        _pointSSBO.Bind(0);

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        Shader.Error("Error while drawing curve panel: ");

        _pointSSBO.Unbind();
        _curvePanelVao.Unbind();

        _curvePanelShader.Unbind();
    }
}