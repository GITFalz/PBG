using OpenTK.Mathematics;

public static class ModelSettings
{
    public static ShaderProgram VertexShader = new ShaderProgram("Model/ModelVertex.vert", "Model/ModelVertex.frag");
    public static ShaderProgram EdgeShader = new ShaderProgram("Model/ModelEdge.vert", "Model/ModelEdge.frag");

    public static Camera? Camera;

    public static RenderType RenderType = RenderType.Vertex;

    // Ui values
    public static float MeshAlpha = 1.0f;
    public static bool WireframeVisible = true;
    public static bool BackfaceCulling = true;
    public static bool Snapping = false;
    public static bool GridAligned = false;
    public static float SnappingFactor = 1;
    public static int SnappingFactorIndex = 0;
    public static Vector3 SnappingOffset = new Vector3(0, 0, 0);

    public static Vector3i Mirror = (0, 0, 0);
    public static Vector3i Axis = (1, 1, 1);
    public static Vector3[] Mirrors => Mirroring[Mirror];
    public static bool[] Swaps => Swapping[Mirror];
    public static readonly Dictionary<Vector3i, Vector3[]> Mirroring = new Dictionary<Vector3i, Vector3[]>()
    {
        { (0, 0, 0), [(1, 1, 1)] },
        { (1, 0, 0), [(1, 1, 1), (-1, 1, 1)] },
        { (0, 1, 0), [(1, 1, 1), (1, -1, 1)] },
        { (0, 0, 1), [(1, 1, 1), (1, 1, -1)] },
        { (1, 1, 0), [(1, 1, 1), (-1, 1, 1), (1, -1, 1), (-1, -1, 1)] },
        { (1, 0, 1), [(1, 1, 1), (-1, 1, 1), (1, 1, -1), (-1, 1, -1)] },
        { (0, 1, 1), [(1, 1, 1), (1, -1, 1), (1, 1, -1), (1, -1, -1)] },
        { (1, 1, 1), [(1, 1, 1), (-1, 1, 1), (1, -1, 1), (-1, -1, 1), (1, 1, -1), (-1, 1, -1), (1, -1, -1), (-1, -1, -1)] }
    };

    public static readonly Dictionary<Vector3i, bool[]> Swapping = new Dictionary<Vector3i, bool[]>()
    {
        { (0, 0, 0), [false] },
        { (1, 0, 0), [false, true] },
        { (0, 1, 0), [false, true] },
        { (0, 0, 1), [false, true] },
        { (1, 1, 0), [false, true, true, false] },
        { (1, 0, 1), [false, true, true, false] },
        { (0, 1, 1), [false, true, true, false] },
        { (1, 1, 1), [false, true, true, false, true, false, false, true] }
    };
}