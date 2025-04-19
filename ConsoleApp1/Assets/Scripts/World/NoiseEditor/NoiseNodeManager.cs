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

    public static UIController NodeController = UIController.Empty;

    public static bool HoveringOverNode = false;

    public static string FileName = "noise";

    public static void AddNode(UINoiseNodePrefab nodePrefab)
    {
        AddNode(nodePrefab, out _);
    }

    public static bool AddNode(UINoiseNodePrefab nodePrefab, out ConnectorNode connectorNode)
    {
        connectorNode = ConnectorNode.Empty;
        if (NoiseNodes.ContainsKey(nodePrefab))
            return false;

        if (nodePrefab is UIDisplayNodePrefab displayNodePrefab)
        {
            // Only one, no delete
            if (DisplayNode != null)
                return false;

            var node = new DisplayConnectorNode(displayNodePrefab);
            connectorNode = node;
            NoiseNodes.Add(nodePrefab, node);
            InputGateConnectors.Add(node.InputGateConnector);
            displayNodePrefab.AddedMoveAction = UpdateLines;
            DisplayNode = node;
        }
        else if (nodePrefab is UISampleNodePrefab sampleNodePrefab)
        {
            var node = new SampleConnectorNode(sampleNodePrefab);
            connectorNode = node;
            NoiseNodes.Add(nodePrefab, node);
            OutputGateConnectors.Add(node.OutputGateConnector);
            sampleNodePrefab.AddedMoveAction = UpdateLines;
        }
        else if (nodePrefab is UIMinMaxInputNodePrefab minMaxInputNode)
        {
            var node = new MinMaxInputOperationConnectorNode(minMaxInputNode, minMaxInputNode.Type);
            connectorNode = node;
            NoiseNodes.Add(nodePrefab, node);
            InputGateConnectors.Add(node.InputGateConnector);
            OutputGateConnectors.Add(node.OutputGateConnector);
            minMaxInputNode.AddedMoveAction = UpdateLines;
        }
        else if (nodePrefab is UIDoubleInputNodePrefab doubleInputNode)
        {
            var node = new DoubleInputOperationConnectorNode(doubleInputNode, doubleInputNode.Type);
            connectorNode = node;
            NoiseNodes.Add(nodePrefab, node);
            InputGateConnectors.Add(node.InputGateConnector1);
            InputGateConnectors.Add(node.InputGateConnector2);
            OutputGateConnectors.Add(node.OutputGateConnector);
            doubleInputNode.AddedMoveAction = UpdateLines;
        }

        return true;
    }

    public static void RemoveNode(ConnectorNode node, bool generateLines = true)
    {
        RemoveNode(generateLines, node.GetNodePrefabs());
    }

    public static void RemoveNode(bool generateLines = true, params UINoiseNodePrefab[] nodePrefab)
    {
        foreach (var prefab in nodePrefab)
        {
            RemoveNode(prefab);
            prefab.Destroy();
        }

        if (generateLines)
            GenerateLines();
    }

    public static void RemoveNode(UINoiseNodePrefab nodePrefab)
    {
        if (!NoiseNodes.ContainsKey(nodePrefab))
            return;

        var node = NoiseNodes[nodePrefab];

        foreach (var input in node.GetInputGateConnectors())
        {
            InputGateConnectors.Remove(input); 
        }
        foreach (var output in node.GetOutputGateConnectors())
        {
            OutputGateConnectors.Remove(output);
        }

        ConnectorNode.Disconnect(node);
        NoiseNodes.Remove(nodePrefab);
    }

    public static void GenerateLines()
    {
        Console.WriteLine("Generating lines");
        if (OutputGateConnectors.Count == 0 || InputGateConnectors.Count == 0)
            return;

        _connectorLineVertices.Clear();

        int index = 0;
        foreach (var (input, output) in GetConnections())
        {
            output.Index = index; 
            _connectorLineVertices.Add(Vector3.TransformPosition(output.Position, NodeController.ModelMatrix));
            _connectorLineVertices.Add(Vector3.TransformPosition(input.Position, NodeController.ModelMatrix));

            index += 2;
        }

        Console.WriteLine($"Line count: {_connectorLineVertices.Count}");
        _connectorLineVBO.Renew(_connectorLineVertices);
        _connectorLineVAO.LinkToVAO(0, 3, _connectorLineVBO);

        Compile();
    }

    private static List<(InputGateConnector, OutputGateConnector)> GetConnections()
    {
        List<(InputGateConnector, OutputGateConnector)> connections = [];

        foreach (var output in OutputGateConnectors)
        {
            if (output.IsConnected && output.InputGateConnector != null)
            {
                connections.Add((output.InputGateConnector, output));
            }
        }

        return connections;
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

    public static void SaveNodes()
    {
        List<string> lines = [];

        List<ConnectorNode> nodes = [];
        var connections = GetConnections();

        int index = 0;
        int inputIndex = 0;
        int outputIndex = 0;

        foreach (var (prefab, node) in NoiseNodes)
        {
            string name = $"node{index}";
            node.VariableName = name;
            nodes.Add(node);
            index++;
        }

        foreach (var input in InputGateConnectors)
        {
            string name = $"input{inputIndex}";
            input.Name = name;
            inputIndex++;
        }

        foreach (var output in OutputGateConnectors)
        {
            string name = $"output{outputIndex}";
            output.Name = name;
            outputIndex++;
        }

        lines.Add($"Nodes: {index}");
        foreach (var node in nodes)
        {
            lines.AddRange(node.ToStringList());
        }
        lines.Add($"Connections: {connections.Count}");
        foreach (var (input, output) in connections)
        {
            lines.Add($"{output.Name} - {input.Name}");
        }

        File.WriteAllLines(Path.Combine(Game.worldNoiseNodeNodeEditorPath, FileName + ".cWorldNode"), lines);
    }

    public static void LoadNodes()
    {
        Clear();

        if (!File.Exists(Path.Combine(Game.worldNoiseNodeNodeEditorPath, FileName + ".cWorldNode")))
            return;

        string[] lines = File.ReadAllLines(Path.Combine(Game.worldNoiseNodeNodeEditorPath, FileName + ".cWorldNode"));

        Dictionary<string, OutputGateConnector> outputGateConnectors = [];
        Dictionary<string, InputGateConnector> inputGateConnectors = [];

        int nodeCount = int.Parse(lines[0].Split(' ')[1]);
        int connectionsCount = int.Parse(lines[nodeCount + 1].Split(' ')[1]);

        for (int i = 0; i < nodeCount; i++)
        {
            int index = i + 1;
            string[] values = lines[index].Split(' ');
            string nodeType = values[1];

            if (nodeType == "Display")
            {   
                string inputName = values[3];

                string name = values[5];

                Vector4 offset = Parse.Vec4(values[6]);

                UIDisplayNodePrefab displayNodePrefab = new UIDisplayNodePrefab(name, NodeController, offset);

                if (AddNode(displayNodePrefab, out ConnectorNode node) && node is DisplayConnectorNode displayNode)
                {
                    displayNode.InputGateConnector.Name = inputName;
                }

                NodeController.AddElements(displayNodePrefab);
            }
            else if (nodeType == "DoubleInputOperation")
            {   
                string value1 = values[3];
                string value2 = values[4];
                int type = int.Parse(values[5]);

                string inputName1 = values[7];
                string inputName2 = values[8];

                string outputName = values[10];

                string name = values[12];
                Vector4 offset = Parse.Vec4(values[13]);

                UIDoubleInputNodePrefab doubleInputNodePrefab = new UIDoubleInputNodePrefab(name, NodeController, offset, (DoubleInputOperationType)type);

                if (AddNode(doubleInputNodePrefab, out ConnectorNode node) && node is DoubleInputOperationConnectorNode doubleInputNode)
                {
                    doubleInputNode.InputGateConnector1.Name = inputName1;
                    doubleInputNode.InputGateConnector2.Name = inputName2;
                    doubleInputNode.OutputGateConnector.Name = outputName;

                    doubleInputNode.Value1 = float.Parse(value1);
                    doubleInputNode.Value2 = float.Parse(value2);
                }
                
                NodeController.AddElements(doubleInputNodePrefab);
            }
            else if (nodeType == "MinMaxInputOperation")
            {
                string min = values[3];
                string max = values[4];
                int type = int.Parse(values[5]);

                string inputName = values[7];

                string outputName = values[9];

                string name = values[11];
                Vector4 offset = Parse.Vec4(values[12]);

                UIMinMaxInputNodePrefab minMaxInputNodePrefab = new UIMinMaxInputNodePrefab(name, NodeController, offset, (MinMaxInputOperationType)type);

                if (AddNode(minMaxInputNodePrefab, out ConnectorNode node) && node is MinMaxInputOperationConnectorNode minMaxInputNode)
                {
                    minMaxInputNode.InputGateConnector.Name = inputName;
                    minMaxInputNode.OutputGateConnector.Name = outputName;

                    minMaxInputNode.Min = float.Parse(min);
                    minMaxInputNode.Max = float.Parse(max);
                }

                NodeController.AddElements(minMaxInputNodePrefab);
            }
            else if (nodeType == "Sample")
            {
                string scale = values[3];
                string noiseOffset = values[4];

                string outputName = values[6];

                string name = values[8];
                Vector4 offset = Parse.Vec4(values[9]);

                UISampleNodePrefab sampleNodePrefab = new UISampleNodePrefab(name, NodeController, offset);

                if (AddNode(sampleNodePrefab, out ConnectorNode node) && node is SampleConnectorNode sampleNode)
                {
                    sampleNode.OutputGateConnector.Name = outputName;

                    sampleNode.Scale = float.Parse(scale);
                    sampleNode.Offset = Parse.Vec2(noiseOffset);
                }

                NodeController.AddElements(sampleNodePrefab);
            }
        }

        foreach (var output in OutputGateConnectors)
        {
            outputGateConnectors.Add(output.Name, output);
        }

        foreach (var input in InputGateConnectors)
        {
            inputGateConnectors.Add(input.Name, input);
        }

        for (int i = 0; i < connectionsCount; i++)
        {
            int index = nodeCount + 2 + i;
            string[] values = lines[index].Split(' ');
            string outputName = values[0];
            string inputName = values[2];

            if (outputGateConnectors.ContainsKey(outputName) && inputGateConnectors.ContainsKey(inputName))
            {
                var input = inputGateConnectors[inputName];
                var output = outputGateConnectors[outputName];

                ConnectorNode.Connect(input, output);
            }
        }

        NodeController.Test();

        GenerateLines();
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
                Vector3 position1 = output.Position;
                Vector3 position2 = output.InputGateConnector.Position;

                Vector3 translatedPosition1 = Vector3.TransformPosition(position1, NodeController.ModelMatrix);
                Vector3 translatedPosition2 = Vector3.TransformPosition(position2, NodeController.ModelMatrix);

                _connectorLineVertices[output.Index] = translatedPosition1;
                _connectorLineVertices[output.Index + 1] = translatedPosition2;
            }
        }

        _connectorLineVBO.Update(_connectorLineVertices);
    }

    public static void RenderLine(Matrix4 orthographicProjection)
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
        GL.UniformMatrix4(projectionLocation, true, ref orthographicProjection);

        GL.Uniform3(colorLocation, ref color);

        _connectorLineVAO.Bind();

        GL.DrawArrays(PrimitiveType.Lines, 0, _connectorLineVertices.Count);

        _connectorLineVAO.Unbind();

        ConnectorLineShaderProgram.Unbind();
    }

    public static void Clear()
    {
        foreach (var node in NoiseNodes.Values)
        {
            RemoveNode(node, false);
        }

        NoiseNodes.Clear();
        ConnectedNodeList.Clear();
        OutputGateConnectors.Clear();
        InputGateConnectors.Clear();

        DisplayNode = null;
    }
}