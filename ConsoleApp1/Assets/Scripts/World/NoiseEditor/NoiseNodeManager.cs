using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public static class NoiseNodeManager
{
    public static Dictionary<UINoiseNodePrefab, ConnectorNode> NoiseNodes = [];
    public static List<ConnectorNode> ConnectedNodeList = [];
    public static List<OutputGateConnector> OutputGateConnectors = [];
    public static List<InputGateConnector> InputGateConnectors = [];

    public static DisplayConnectorNode? DisplayNode = null;

    public static ShaderProgram ConnectorLineShaderProgram = new ShaderProgram("Noise/ConnectorLine.vert", "Noise/ConnectorLine.frag");
    private static VAO _connectorLineVAO = new VAO();
    private static VBO<Vector3> _connectorLineVBO = new([]);
    private static List<Vector3> _connectorLineVertices = [];

    public static void AddNode(UINoiseNodePrefab nodePrefab)
    {
        if (NoiseNodes.ContainsKey(nodePrefab))
            return;

        if (nodePrefab is UISampleNodePrefab sampleNodePrefab)
        {
            var node = new SampleConnectorNode(sampleNodePrefab);
            NoiseNodes.Add(nodePrefab, node);
            OutputGateConnectors.Add(node.OutputGateConnector);
            sampleNodePrefab.AddedMoveAction = UpdateLines;
        }
        else if (nodePrefab is UIDisplayNodePrefab displayNodePrefab)
        {
            var node = new DisplayConnectorNode(displayNodePrefab);
            NoiseNodes.Add(nodePrefab, node);
            InputGateConnectors.Add(node.InputGateConnector);
            displayNodePrefab.AddedMoveAction = UpdateLines;
            DisplayNode = node;
        }
        else if (nodePrefab is UISingleInputNodePrefab singleInputNode)
        {
            var node = new SingleInputOperationConnectorNode(singleInputNode, singleInputNode.Type);
            NoiseNodes.Add(nodePrefab, node);
            InputGateConnectors.Add(node.InputGateConnector);
            OutputGateConnectors.Add(node.OutputGateConnector);
            singleInputNode.AddedMoveAction = UpdateLines;
        }
    }

    public static void GenerateLines()
    {
        if (OutputGateConnectors.Count == 0 || InputGateConnectors.Count == 0)
            return;

        _connectorLineVertices.Clear();

        int index = 0;
        foreach (var output in OutputGateConnectors)
        {
            if (output.IsConnected && output.InputGateConnector != null)
            {
                output.Index = index; 
                _connectorLineVertices.Add(output.Position);
                _connectorLineVertices.Add(output.InputGateConnector.Position);
                index += 2;
            }
        }

        _connectorLineVBO.Renew(_connectorLineVertices);
        _connectorLineVAO.LinkToVAO(0, 3, _connectorLineVBO);

        Compile();
    }

    public static void Compile()
    {
        if (DisplayNode == null)
            return;

        ConnectedNodeList = [DisplayNode];
        GetConnectedNodes(DisplayNode, ConnectedNodeList);
        for (int i = 0; i < ConnectedNodeList.Count; i++)
        {
            ConnectorNode node = ConnectedNodeList[i];
            node.VariableName = "variable" + i;
        }
        NoiseGlslNodeManager.Compile(ConnectedNodeList);
    }

    public static void GetConnectedNodes(ConnectorNode node, List<ConnectorNode> nodes)
    {
        foreach (var n in node.GetInputNodes())
        {
            if (nodes.Contains(n))
                nodes.Remove(n);

            nodes.Insert(0, n);
            GetConnectedNodes(n, nodes);
        }
    }

    public static void UpdateLines()
    {
        if (OutputGateConnectors.Count == 0 || InputGateConnectors.Count == 0)
            return;

        foreach (var output in OutputGateConnectors)
        {
            if (output.Index != -1 && output.IsConnected && output.InputGateConnector != null)
            {
                _connectorLineVertices[output.Index] = output.Position;
                _connectorLineVertices[output.Index + 1] = output.InputGateConnector.Position;
            }
        }

        _connectorLineVBO.Update(_connectorLineVertices);
    }

    public static void RenderLine()
    {
        if (OutputGateConnectors.Count == 0 || InputGateConnectors.Count == 0)
            return;

        ConnectorLineShaderProgram.Bind();

        Matrix4 model = Matrix4.Identity;

        Vector3 color = new Vector3(1, 0, 0);

        int modelLocation = ConnectorLineShaderProgram.GetLocation("model");
        int projectionLocation = ConnectorLineShaderProgram.GetLocation("projection");

        int colorLocation = ConnectorLineShaderProgram.GetLocation("color");

        GL.UniformMatrix4(modelLocation, true, ref model);
        GL.UniformMatrix4(projectionLocation, true, ref UIController.OrthographicProjection);

        GL.Uniform3(colorLocation, ref color);

        _connectorLineVAO.Bind();

        GL.DrawArrays(PrimitiveType.Lines, 0, _connectorLineVertices.Count);

        _connectorLineVAO.Unbind();

        ConnectorLineShaderProgram.Unbind();
    }
}