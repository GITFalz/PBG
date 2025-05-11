using OpenTK.Mathematics;

public abstract class UINoiseNodePrefab : UIPrefab
{
    public static Vector4 SAMPLE_NODE_COLOR     = new Vector4(0.290f, 0.565f, 0.886f, 1f); // #4A90E2
    public static Vector4 VORONOI_NODE_COLOR    = new Vector4(0.314f, 0.890f, 0.761f, 1f); // #50E3C2
    public static Vector4 DOUBLE_INPUT_COLOR    = new Vector4(0.961f, 0.647f, 0.137f, 1f); // #F5A623
    public static Vector4 MIN_MAX_INPUT_COLOR   = new Vector4(0.973f, 0.905f, 0.110f, 1f); // #F8E71C
    public static Vector4 RANGE_NODE_COLOR      = new Vector4(0.494f, 0.827f, 0.129f, 1f); // #7ED321
    public static Vector4 COMBINE_NODE_COLOR    = new Vector4(0.565f, 0.075f, 0.996f, 1f); // #9013FE
    public static Vector4 INIT_MASK_NODE_COLOR  = new Vector4(0.816f, 0.008f, 0.106f, 1f); // #D0021B
    public static Vector4 CURVE_NODE_COLOR = new Vector4(0.855f, 0.388f, 0.725f, 1f); // #DA63B9

    public UINoiseNodePrefab(string name, UIController controller, Vector4 offset) : base(name, controller, offset) {}
}