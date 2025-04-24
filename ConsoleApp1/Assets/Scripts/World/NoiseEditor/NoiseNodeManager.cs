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
    public static string CurrentFileName = "noise";

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
        else if (nodePrefab is UIVoronoiPrefab voronoiNodePrefab)
        {
            var node = new VoronoiConnectorNode(voronoiNodePrefab, voronoiNodePrefab.Type);
            connectorNode = node;
            NoiseNodes.Add(nodePrefab, node);
            OutputGateConnectors.Add(node.OutputGateConnector);
            voronoiNodePrefab.AddedMoveAction = UpdateLines;
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
        _connectorLineVertices.Clear();

        int index = 0;
        foreach (var (input, output) in GetConnections())
        { 
            output.SetIndex(input, index);
            _connectorLineVertices.Add(Vector3.TransformPosition(output.Position, NodeController.ModelMatrix));
            _connectorLineVertices.Add(Vector3.TransformPosition(input.Position, NodeController.ModelMatrix));

            index++;
        }

        _connectorLineVBO.Renew(_connectorLineVertices);
        _connectorLineVAO.LinkToVAO(0, 3, _connectorLineVBO);

        Compile();
    }

    public static void UpdateLines()
    {
        if (OutputGateConnectors.Count == 0 || InputGateConnectors.Count == 0)
            return;

        foreach (var output in OutputGateConnectors)
        {
            if (output.IsConnected)
            {
                for (int i = 0; i < output.InputGateConnectors.Count; i++)
                {
                    int index = output.Indices[i] * 2;
                    if (index < 0)
                        continue;
                    
                    var input = output.InputGateConnectors[i];
                    Vector3 position1 = output.Position;
                    Vector3 position2 = input.Position;

                    Vector3 translatedPosition1 = Vector3.TransformPosition(position1, NodeController.ModelMatrix);
                    Vector3 translatedPosition2 = Vector3.TransformPosition(position2, NodeController.ModelMatrix);

                    _connectorLineVertices[index] = translatedPosition1;
                    _connectorLineVertices[index + 1] = translatedPosition2;
                }
            }
        }

        _connectorLineVBO.Update(_connectorLineVertices);
    }

    private static List<(InputGateConnector, OutputGateConnector)> GetConnections()
    {
        List<(InputGateConnector, OutputGateConnector)> connections = [];

        foreach (var output in OutputGateConnectors)
        {
            if (output.IsConnected)
            {
                foreach (var input in output.InputGateConnectors)
                {
                    connections.Add((input, output));
                }
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

    public static void Compile(CWorldNodeManager nodeManager)
    {
        if (DisplayNode == null)
            return;

        nodeManager.Delete();

        ConnectedNodeList = [DisplayNode];
        GetConnectedNodes(DisplayNode, ConnectedNodeList);

        Dictionary<ConnectorNode, CWorldNode> nodeMap = [];
        for (int i = 0; i < ConnectedNodeList.Count; i++)
        {
            ConnectorNode node = ConnectedNodeList[i];
            if (node is DisplayConnectorNode)
            {
                nodeMap.Add(node, nodeManager.CWorldOutputNode);
            }
            else if (node is SampleConnectorNode sampleNode)
            {
                CWorldSampleNode cWorldSampleNode = new CWorldSampleNode()
                {
                    Name = node.VariableName,
                    Scale = (sampleNode.Scale, sampleNode.Scale),
                    Offset = sampleNode.Offset,
                };
                nodeMap.Add(node, cWorldSampleNode);
                nodeManager.AddNode(cWorldSampleNode);
            }
            else if (node is VoronoiConnectorNode voronoiNode)
            {
                CWorldVoronoiNode cWorldVoronoiNode = new CWorldVoronoiNode(voronoiNode.Type)
                {
                    Name = node.VariableName,
                    Scale = (voronoiNode.Scale, voronoiNode.Scale),
                    Offset = voronoiNode.Offset,
                };
                nodeMap.Add(node, cWorldVoronoiNode);
                nodeManager.AddNode(cWorldVoronoiNode);
            }
            else if (node is MinMaxInputOperationConnectorNode minMaxNode)
            {
                CWorldMinMaxNode cWorldMinMaxNode = new CWorldMinMaxNode(minMaxNode.Type)
                {
                    Name = node.VariableName,
                    Min = minMaxNode.Min,
                    Max = minMaxNode.Max,
                };
                nodeMap.Add(node, cWorldMinMaxNode);
                nodeManager.AddNode(cWorldMinMaxNode);
            }
            else if (node is DoubleInputOperationConnectorNode doubleInputNode)
            {
                CWorldDoubleInputNode cWorldDoubleInputNode = new CWorldDoubleInputNode(doubleInputNode.Type)
                {
                    Name = node.VariableName,
                    Value1 = doubleInputNode.Value1,
                    Value2 = doubleInputNode.Value2,
                };
                nodeMap.Add(node, cWorldDoubleInputNode);
                nodeManager.AddNode(cWorldDoubleInputNode);
            }
        }

        foreach (var (node, cWorldNode) in nodeMap)
        {
            if (node is DisplayConnectorNode displayNode)
            {
                if (displayNode.InputGateConnector.GetConnectedNode(out var connectedNode) && nodeMap.TryGetValue(connectedNode, out var inputNode))
                {
                    nodeManager.CWorldOutputNode.InputNode = (CWorldGetterNode)inputNode;
                }
            }
            else if (node is MinMaxInputOperationConnectorNode minMaxNode)
            {
                if (minMaxNode.InputGateConnector.GetConnectedNode(out var connectedNode) && nodeMap.TryGetValue(connectedNode, out var inputNode))
                {
                    ((CWorldMinMaxNode)cWorldNode).InputNode = (CWorldGetterNode)inputNode;
                }
            }
            else if (node is DoubleInputOperationConnectorNode doubleInputNode)
            {
                if (doubleInputNode.InputGateConnector1.GetConnectedNode(out var connectedNode) && nodeMap.TryGetValue(connectedNode, out var inputNode1))
                {
                    ((CWorldDoubleInputNode)cWorldNode).InputNode1 = (CWorldGetterNode)inputNode1;
                }
                if (doubleInputNode.InputGateConnector2.GetConnectedNode(out connectedNode) && nodeMap.TryGetValue(connectedNode, out var inputNode2))
                {
                    ((CWorldDoubleInputNode)cWorldNode).InputNode2 = (CWorldGetterNode)inputNode2;
                }
            }
        }
    }

    public static void SaveNodes()
    {
        SaveNodes(FileName);
    }

    public static void SaveNodes(string fileName)
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

        File.WriteAllLines(Path.Combine(Game.worldNoiseNodeNodeEditorPath, fileName + ".cWorldNode"), lines);

        CurrentFileName = fileName;
    }

    public static void LoadNodes()
    {
        Clear();

        if (!File.Exists(Path.Combine(Game.worldNoiseNodeNodeEditorPath, FileName + ".cWorldNode")))
            return;

        CurrentFileName = FileName;

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
            }
            else if (nodeType == "Voronoi")
            {
                string scale = values[3];
                string noiseOffset = values[4];
                int type = int.Parse(values[5]);

                string outputName = values[7];

                string name = values[9];
                Vector4 offset = Parse.Vec4(values[10]);

                UIVoronoiPrefab voronoiNodePrefab = new UIVoronoiPrefab(name, NodeController, offset, (VoronoiOperationType)type);

                if (AddNode(voronoiNodePrefab, out ConnectorNode node) && node is VoronoiConnectorNode voronoiNode)
                {
                    voronoiNode.OutputGateConnector.Name = outputName;

                    voronoiNode.Scale = float.Parse(scale);
                    voronoiNode.Offset = Parse.Vec2(noiseOffset);
                }
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
        Dictionary<UINoiseNodePrefab, ConnectorNode> copy = new(NoiseNodes);
        foreach (var node in copy.Values)
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