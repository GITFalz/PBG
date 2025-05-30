using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;

public abstract class UINoiseNodePrefab : UIPrefab
{
    public static Vector4 SAMPLE_NODE_COLOR = new Vector4(0.290f, 0.565f, 0.886f, 1f); // #4A90E2
    public static Vector4 DIRECT_SAMPLE_NODE_COLOR = new Vector4(0.204f, 0.486f, 0.847f, 1f); // #347CD8
    public static Vector4 VORONOI_NODE_COLOR = new Vector4(0.314f, 0.890f, 0.761f, 1f); // #50E3C2
    public static Vector4 DOUBLE_INPUT_COLOR = new Vector4(0.961f, 0.647f, 0.137f, 1f); // #F5A623
    public static Vector4 BASE_INPUT_COLOR = new Vector4(0.400f, 0.600f, 0.800f, 1f); // #6699CC
    public static Vector4 MIN_MAX_INPUT_COLOR = new Vector4(0.973f, 0.905f, 0.110f, 1f); // #F8E71C
    public static Vector4 RANGE_NODE_COLOR = new Vector4(0.494f, 0.827f, 0.129f, 1f); // #7ED321
    public static Vector4 COMBINE_NODE_COLOR = new Vector4(0.565f, 0.075f, 0.996f, 1f); // #9013FE
    public static Vector4 INIT_MASK_NODE_COLOR = new Vector4(0.816f, 0.008f, 0.106f, 1f); // #D0021B
    public static Vector4 CURVE_NODE_COLOR = new Vector4(0.855f, 0.388f, 0.725f, 1f); // #DA63B9
    public static Vector4 SELECTION_COLOR = new Vector4(0.529f, 0.808f, 0.980f, 1.0f); // #87CEEB

    public Action AddedMoveAction = () => { };
    private Vector2 _oldMouseButtonPosition = new Vector2(0, 0);

    public UINoiseNodePrefab(string name, UIController controller, Vector4 offset) : base(name, controller, offset) { }

    public void SetOldMousePosition() => _oldMouseButtonPosition = Input.GetMousePosition();
    public virtual void MoveNode()
    {
        if (Input.GetMouseDelta() == Vector2.Zero)
            return;

        Vector2 mousePosition = Input.GetMousePosition();
        Vector2 delta = (mousePosition - _oldMouseButtonPosition) * (1 / Collection.UIController.Scale);

        Collection.SetOffset(Collection.Offset + new Vector4(delta.X, delta.Y, 0, 0));
        Collection.Align();
        Collection.UpdateTransformation();
        SetOldMousePosition();

        AddedMoveAction.Invoke();
    }

    public abstract bool GetConnectorNode(Dictionary<UINoiseNodePrefab, ConnectorNode> noiseNodes, List<InputGateConnector> inputGates, List<OutputGateConnector> outputGates, [NotNullWhen(true)] out ConnectorNode? connectorNode);
}