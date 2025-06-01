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

    public static Action updateFileNameAction = () => { };

    public static void AddNode(UINoiseNodePrefab nodePrefab)
    {
        AddNode(nodePrefab, out _);
    }

    public static bool AddNode(UINoiseNodePrefab nodePrefab, out ConnectorNode connectorNode)
    {
        connectorNode = ConnectorNode.Empty;
        if (NoiseNodes.ContainsKey(nodePrefab))
            return false;

        if (nodePrefab is UIDisplayNodePrefab displayNodePrefab && DisplayNode != null)
            return false;

        if (!nodePrefab.GetConnectorNode(NoiseNodes, InputGateConnectors, OutputGateConnectors, out var node))
            return false;

        if (node is DisplayConnectorNode displayConnectorNode)
            DisplayNode = displayConnectorNode;

        nodePrefab.AddedMoveAction = UpdateLines;
        connectorNode = node;
        return true;
    }

    public static bool AddNode(ConnectorNode node)
    {
        if (node is DisplayConnectorNode displayConnectorNode)
        {
            // Only one, no delete
            if (DisplayNode != null)
                return false;

            DisplayNode = displayConnectorNode;
        }
        var prefabs = node.GetNodePrefabs();
        foreach (var prefab in prefabs)
        {
            if (NoiseNodes.ContainsKey(prefab))
                continue;

            NoiseNodes.Add(prefab, node);
        }
        InputGateConnectors.AddRange(node.GetInputGateConnectors());
        OutputGateConnectors.AddRange(node.GetOutputGateConnectors());
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

        node.Disconnect();
        NoiseNodes.Remove(nodePrefab);
    }

    public static void GenerateLines()
    {
        _connectorLineVertices = [];

        int index = 0;
        foreach (var (input, output) in GetConnections())
        { 
            output.SetIndex(input, index);
            _connectorLineVertices.Add(Vector3.TransformPosition(output.Position, NodeController.ModelMatrix));
            _connectorLineVertices.Add(Vector3.TransformPosition(input.Position, NodeController.ModelMatrix));

            index++;
        }

        _connectorLineVAO.Bind();

        _connectorLineVBO.Renew(_connectorLineVertices);
        _connectorLineVAO.LinkToVAO(0, 3, VertexAttribPointerType.Float, 0, 0, _connectorLineVBO); 

        _connectorLineVAO.Unbind();

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
        int nodeCount = ConnectedNodeList.Count;
        for (int i = 0; i < nodeCount; i++)
        {
            ConnectorNode node = ConnectedNodeList[i];
            var outputConnectors = node.GetOutputGateConnectors();
            if (outputConnectors.Count == 0)
                continue;
            
            for (int j = 0; j < outputConnectors.Count; j++)
            {
                OutputGateConnector output = outputConnectors[j];
                output.VariableName = "variable" + i + "_" + j;
            }
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
                CWorldSampleNode cWorldSampleNode = new CWorldSampleNode(sampleNode.Type)
                {
                    Name = node.GetOutputGateConnectors()[0].VariableName,
                    Scale = (sampleNode.Scale, sampleNode.Scale),
                    Offset = sampleNode.Offset,
                };
                nodeMap.Add(node, cWorldSampleNode);
                nodeManager.AddNode(cWorldSampleNode);
            }
            else if (node is DirectSampleConnectorNode directSampleNode)
            {
                CWorldDirectSampleNode cWorldSampleNode = new CWorldDirectSampleNode()
                {
                    Name = node.GetOutputGateConnectors()[0].VariableName,
                    Position = directSampleNode.Position, 
                    Scale = (directSampleNode.Scale, directSampleNode.Scale),
                    Offset = directSampleNode.Offset,
                };
                nodeMap.Add(node, cWorldSampleNode);
                nodeManager.AddNode(cWorldSampleNode);
            }
            else if (node is VoronoiConnectorNode voronoiNode)
            {
                CWorldVoronoiNode cWorldVoronoiNode = new CWorldVoronoiNode(voronoiNode.Type, voronoiNode.Flag)
                {
                    Name = node.GetOutputGateConnectors()[0].VariableName,
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
                    Name = node.GetOutputGateConnectors()[0].VariableName,
                    Min = minMaxNode.Min,
                    Max = minMaxNode.Max,
                };
                nodeMap.Add(node, cWorldMinMaxNode);
                nodeManager.AddNode(cWorldMinMaxNode);
            }
            else if (node is DoubleInputConnectorNode doubleInputNode)
            {
                CWorldDoubleInputNode cWorldDoubleInputNode = new CWorldDoubleInputNode(doubleInputNode.Type)
                {
                    Name = node.GetOutputGateConnectors()[0].VariableName,
                    Value1 = doubleInputNode.Value1,
                    Value2 = doubleInputNode.Value2,
                };
                nodeMap.Add(node, cWorldDoubleInputNode);
                nodeManager.AddNode(cWorldDoubleInputNode);
            }
            else if (node is BaseInputConnectorNode baseInputNode)
            {
                CWorldBaseInputNode cWorldBaseInputNode = new CWorldBaseInputNode(baseInputNode.Type)
                {
                    Name = node.GetOutputGateConnectors()[0].VariableName,
                    Value = 0,
                };
                nodeMap.Add(node, cWorldBaseInputNode);
                nodeManager.AddNode(cWorldBaseInputNode);
            }
            else if (node is CombineConnectorNode combineNode)
            {
                CWorldCombineNode cWorldCombineNode = new CWorldCombineNode()
                {
                    Name = node.GetOutputGateConnectors()[0].VariableName,
                    First = combineNode.Value1,
                    Second = combineNode.Value2,
                };
                nodeMap.Add(node, cWorldCombineNode);
                nodeManager.AddNode(cWorldCombineNode);
            }
            else if (node is RangeConnectorNode rangeNode)
            {
                CWorldRangeNode cWorldRangeNode = new CWorldRangeNode()
                {
                    Name = node.GetOutputGateConnectors()[0].VariableName,
                    Start = rangeNode.Start,
                    Height = rangeNode.Height,
                    Flipped = rangeNode.Flipped,
                };
                nodeMap.Add(node, cWorldRangeNode);
                nodeManager.AddNode(cWorldRangeNode);
            }
            else if (node is InitMaskThresholdConnectorNode initMaskNode)
            {
                CWorldThresholdInitMaskNode cWorldInitMaskNode = new CWorldThresholdInitMaskNode()
                {
                    Name = node.GetOutputGateConnectors()[0].VariableName,
                    Threshold = initMaskNode.Threshold,
                };
                nodeMap.Add(node, cWorldInitMaskNode);
                nodeManager.AddNode(cWorldInitMaskNode);
            }
            else if (node is InitMaskMinMaxConnectorNode minMaxInitMaskNode)
            {
                CWorldMinMaxInitMaskNode cWorldInitMaskNode = new CWorldMinMaxInitMaskNode()
                {
                    Name = node.GetOutputGateConnectors()[0].VariableName,
                    Min = minMaxInitMaskNode.Min,
                    Max = minMaxInitMaskNode.Max,
                };
                nodeMap.Add(node, cWorldInitMaskNode);
                nodeManager.AddNode(cWorldInitMaskNode);
            }
            else if (node is CurveConnectorNode curveNode)
            {
                CWorldCurveNode cWorldCurveNode = new CWorldCurveNode()
                {
                    Name = node.GetOutputGateConnectors()[0].VariableName,
                    Min = curveNode.Min,
                    Max = curveNode.Max,
                    Spline = curveNode.CurveWindow.Points.ToArray(),
                };
                nodeMap.Add(node, cWorldCurveNode);
                nodeManager.AddNode(cWorldCurveNode);
            }
        }

        foreach (var (node, cWorldNode) in nodeMap)
        {
            if (node is DisplayConnectorNode displayNode)
            {
                if (displayNode.InputGateConnector.GetConnectedNode(out var connectedNode) && nodeMap.TryGetValue(connectedNode, out var inputNode))
                {
                    nodeManager.CWorldOutputNode.InputNode = (CWorldGetterNode)inputNode;
                    nodeManager.CWorldOutputNode.InputNodeIndex = displayNode.InputGateConnector.GetOutputIndex();
                }
            }
            else if (node is MinMaxInputOperationConnectorNode minMaxNode)
            {
                if (minMaxNode.InputGateConnector.GetConnectedNode(out var connectedNode) && nodeMap.TryGetValue(connectedNode, out var inputNode))
                {
                    ((CWorldMinMaxNode)cWorldNode).InputNode = (CWorldGetterNode)inputNode;
                    ((CWorldMinMaxNode)cWorldNode).InputNodeIndex = minMaxNode.InputGateConnector.GetOutputIndex();
                }
            }
            else if (node is VoronoiConnectorNode voronoiNode)
            {
                if (voronoiNode.InputGateConnector1.GetConnectedNode(out var connectedNode) && nodeMap.TryGetValue(connectedNode, out var inputNode1))
                {
                    ((CWorldVoronoiNode)cWorldNode).InputNode1 = (CWorldGetterNode)inputNode1;
                    ((CWorldVoronoiNode)cWorldNode).InputNode1Index = voronoiNode.InputGateConnector1.GetOutputIndex();
                }
                if (voronoiNode.InputGateConnector2.GetConnectedNode(out connectedNode) && nodeMap.TryGetValue(connectedNode, out var inputNode2))
                {
                    ((CWorldVoronoiNode)cWorldNode).InputNode2 = (CWorldGetterNode)inputNode2;
                    ((CWorldVoronoiNode)cWorldNode).InputNode2Index = voronoiNode.InputGateConnector2.GetOutputIndex();
                }
            }
            else if (node is SampleConnectorNode sampleNode)
            {
                if (sampleNode.InputGateConnector1.GetConnectedNode(out var connectedNode) && nodeMap.TryGetValue(connectedNode, out var inputNode1))
                {
                    ((CWorldSampleNode)cWorldNode).InputNode1 = (CWorldGetterNode)inputNode1;
                    ((CWorldSampleNode)cWorldNode).InputNode1Index = sampleNode.InputGateConnector1.GetOutputIndex();
                }
                if (sampleNode.InputGateConnector2.GetConnectedNode(out connectedNode) && nodeMap.TryGetValue(connectedNode, out var inputNode2))
                {
                    ((CWorldSampleNode)cWorldNode).InputNode2 = (CWorldGetterNode)inputNode2;
                    ((CWorldSampleNode)cWorldNode).InputNode2Index = sampleNode.InputGateConnector2.GetOutputIndex();
                }
            }
            else if (node is DirectSampleConnectorNode directSampleNode)
            {
                if (directSampleNode.InputPosXConnector.GetConnectedNode(out var connectedNode) && nodeMap.TryGetValue(connectedNode, out var posXNode))
                {
                    ((CWorldDirectSampleNode)cWorldNode).PosXNode = (CWorldGetterNode)posXNode;
                    ((CWorldDirectSampleNode)cWorldNode).PosXNodeIndex = directSampleNode.InputPosXConnector.GetOutputIndex();
                }
                if (directSampleNode.InputPosYConnector.GetConnectedNode(out connectedNode) && nodeMap.TryGetValue(connectedNode, out var posYNode))
                {
                    ((CWorldDirectSampleNode)cWorldNode).PosYNode = (CWorldGetterNode)posYNode;
                    ((CWorldDirectSampleNode)cWorldNode).PosYNodeIndex = directSampleNode.InputPosYConnector.GetOutputIndex();
                }
                if (directSampleNode.InputGateConnector1.GetConnectedNode(out connectedNode) && nodeMap.TryGetValue(connectedNode, out var inputNode1))
                {
                    ((CWorldSampleNode)cWorldNode).InputNode1 = (CWorldGetterNode)inputNode1;
                    ((CWorldSampleNode)cWorldNode).InputNode1Index = directSampleNode.InputGateConnector1.GetOutputIndex();
                }
                if (directSampleNode.InputGateConnector2.GetConnectedNode(out connectedNode) && nodeMap.TryGetValue(connectedNode, out var inputNode2))
                {
                    ((CWorldSampleNode)cWorldNode).InputNode2 = (CWorldGetterNode)inputNode2;
                    ((CWorldSampleNode)cWorldNode).InputNode2Index = directSampleNode.InputGateConnector2.GetOutputIndex();
                }
            }
            else if (node is DoubleInputConnectorNode doubleInputNode)
            {
                if (doubleInputNode.InputGateConnector1.GetConnectedNode(out var connectedNode) && nodeMap.TryGetValue(connectedNode, out var inputNode1))
                {
                    ((CWorldDoubleInputNode)cWorldNode).InputNode1 = (CWorldGetterNode)inputNode1;
                    ((CWorldDoubleInputNode)cWorldNode).InputNode1Index = doubleInputNode.InputGateConnector1.GetOutputIndex();
                }
                if (doubleInputNode.InputGateConnector2.GetConnectedNode(out connectedNode) && nodeMap.TryGetValue(connectedNode, out var inputNode2))
                {
                    ((CWorldDoubleInputNode)cWorldNode).InputNode2 = (CWorldGetterNode)inputNode2;
                    ((CWorldDoubleInputNode)cWorldNode).InputNode2Index = doubleInputNode.InputGateConnector2.GetOutputIndex();
                }
            }
            else if (node is BaseInputConnectorNode baseInputNode)
            {
                if (baseInputNode.InputGateConnector.GetConnectedNode(out var connectedNode) && nodeMap.TryGetValue(connectedNode, out var inputNode))
                {
                    ((CWorldBaseInputNode)cWorldNode).InputNode = (CWorldGetterNode)inputNode;
                    ((CWorldBaseInputNode)cWorldNode).InputNodeIndex = baseInputNode.InputGateConnector.GetOutputIndex();
                }
            }
            else if (node is CombineConnectorNode combineNode)
            {
                if (combineNode.InputGateConnector1.GetConnectedNode(out var connectedNode) && nodeMap.TryGetValue(connectedNode, out var inputNode1))
                {
                    ((CWorldCombineNode)cWorldNode).FirstNode = (CWorldGetterNode)inputNode1;
                    ((CWorldCombineNode)cWorldNode).FirstValueIndex = combineNode.InputGateConnector1.GetOutputIndex();
                }
                if (combineNode.InputGateConnector2.GetConnectedNode(out connectedNode) && nodeMap.TryGetValue(connectedNode, out var inputNode2))
                {
                    ((CWorldCombineNode)cWorldNode).SecondNode = (CWorldGetterNode)inputNode2;
                    ((CWorldCombineNode)cWorldNode).SecondValueIndex = combineNode.InputGateConnector2.GetOutputIndex();
                }
            }
            else if (node is RangeConnectorNode rangeNode)
            {
                if (rangeNode.StartGateConnector.GetConnectedNode(out var connectedNode) && nodeMap.TryGetValue(connectedNode, out var startNode))
                {
                    ((CWorldRangeNode)cWorldNode).StartNode = (CWorldGetterNode)startNode;
                    ((CWorldRangeNode)cWorldNode).StartValueIndex = rangeNode.StartGateConnector.GetOutputIndex();
                }
                if (rangeNode.HeightGateConnector.GetConnectedNode(out connectedNode) && nodeMap.TryGetValue(connectedNode, out var heightNode))
                {
                    ((CWorldRangeNode)cWorldNode).HeightNode = (CWorldGetterNode)heightNode;
                    ((CWorldRangeNode)cWorldNode).HeightValueIndex = rangeNode.HeightGateConnector.GetOutputIndex();
                }
                ((CWorldRangeNode)cWorldNode).Flipped = rangeNode.Flipped;
            }
            else if (node is InitMaskThresholdConnectorNode initMaskNode)
            {
                if (initMaskNode.ChildGateConnector.GetConnectedNode(out var connectedNode) && nodeMap.TryGetValue(connectedNode, out var childNode))
                {
                    ((CWorldThresholdInitMaskNode)cWorldNode).ChildNode = (CWorldGetterNode)childNode;
                    ((CWorldThresholdInitMaskNode)cWorldNode).ChildValueIndex = initMaskNode.ChildGateConnector.GetOutputIndex();
                }
                if (initMaskNode.MaskGateConnector.GetConnectedNode(out connectedNode) && nodeMap.TryGetValue(connectedNode, out var maskNode))
                {
                    ((CWorldThresholdInitMaskNode)cWorldNode).MaskNode = (CWorldGetterNode)maskNode;
                    ((CWorldThresholdInitMaskNode)cWorldNode).MaskValueIndex = initMaskNode.MaskGateConnector.GetOutputIndex();
                }
            }
            else if (node is InitMaskMinMaxConnectorNode minMaxInitMaskNode)
            {
                if (minMaxInitMaskNode.ChildGateConnector.GetConnectedNode(out var connectedNode) && nodeMap.TryGetValue(connectedNode, out var childNode))
                {
                    ((CWorldMinMaxInitMaskNode)cWorldNode).ChildNode = (CWorldGetterNode)childNode;
                    ((CWorldMinMaxInitMaskNode)cWorldNode).ChildValueIndex = minMaxInitMaskNode.ChildGateConnector.GetOutputIndex();
                }
                if (minMaxInitMaskNode.MaskGateConnector.GetConnectedNode(out connectedNode) && nodeMap.TryGetValue(connectedNode, out var maskNode))
                {
                    ((CWorldMinMaxInitMaskNode)cWorldNode).MaskNode = (CWorldGetterNode)maskNode;
                    ((CWorldMinMaxInitMaskNode)cWorldNode).MaskValueIndex = minMaxInitMaskNode.MaskGateConnector.GetOutputIndex();
                }
            }
            else if (node is CurveConnectorNode curveNode)
            {
                if (curveNode.InputGateConnector.GetConnectedNode(out var connectedNode) && nodeMap.TryGetValue(connectedNode, out var inputNode))
                {
                    ((CWorldCurveNode)cWorldNode).InputNode = (CWorldGetterNode)inputNode;
                    ((CWorldCurveNode)cWorldNode).InputNodeIndex = curveNode.InputGateConnector.GetOutputIndex();
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

        var file = Path.Combine(Game.worldNoiseNodeNodeEditorPath, fileName + ".cWorldNode");

        if (!File.Exists(file))
        {
            var element = NoiseEditor.FileList.AddButton(fileName, out var button, out var deleteButton);
            button.SetOnClick(() => {
                SaveNodes(CurrentFileName); LoadNodes(fileName);
            });
            deleteButton.SetOnClick(() => {
                DeleteFile(fileName, element);
            });
            element.UIController.AddElements(element);
        }

        File.WriteAllLines(file, lines);

        CurrentFileName = fileName;
    }

    public static void DeleteFile(string fileName, UIElement button)
    {
        string file = Path.Combine(Game.worldNoiseNodeNodeEditorPath, fileName + ".cWorldNode");

        if (File.Exists(file) && NoiseEditor.FileList.DeleteButton(button))
        {
            File.Delete(file);
            button.Delete();
            NoiseEditor.FileList.ScrollView.ResetInit();
            NoiseEditor.FileList.ScrollView.QueueAlign();
            NoiseEditor.FileList.ScrollView.QueueUpdateTransformation();

            if (CurrentFileName == fileName)
            {
                ClearNoiseNodes();

                var files = Directory.GetFiles(Game.worldNoiseNodeNodeEditorPath, "*.cWorldNode");

                if (files.Length == 0)
                {
                    CurrentFileName = "noise";
                    FileName = "noise";
                    updateFileNameAction?.Invoke();
                    SaveNodes(CurrentFileName);
                }
                else
                {
                    string newFileName = Path.GetFileNameWithoutExtension(files[0]);
                    LoadNodes(newFileName);
                }
                
            }
        }
    }

    public static void DeleteAll()
    {
        NoiseEditor.FileList.ScrollView.DeleteSubElements();
    }

    public static void LoadNodes()
    {
        LoadNodes(FileName);
    }

    public static void LoadNodes(string fileName)
    {
        Clear();

        if (!File.Exists(Path.Combine(Game.worldNoiseNodeNodeEditorPath, fileName + ".cWorldNode")))
            return;

        if (FileName != fileName)
        {
            FileName = fileName;
            updateFileNameAction?.Invoke();
        }

        CurrentFileName = fileName;
        NoiseEditor.NodeNameField.SetText(fileName, 1.2f).UpdateCharacters();

        string[] lines = File.ReadAllLines(Path.Combine(Game.worldNoiseNodeNodeEditorPath, fileName + ".cWorldNode"));

        Dictionary<string, OutputGateConnector> outputGateConnectors = [];
        Dictionary<string, InputGateConnector> inputGateConnectors = [];

        if (!NodeParser.Parse(NodeController, lines, out int connectionIndex))
            return;

        var values = lines[connectionIndex].Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
        if (values.Length < 2)
            return;

        int connectionsCount = Int.Parse(values[1]);

        foreach (var node in NodeParser.ConnectorNodes)
        {
            AddNode(node);
        }

        int none = 0;
        foreach (var output in OutputGateConnectors)
        {
            if (output.Name == "none")
            {
                output.Name = "none" + none;
                none++;
            }
                
            outputGateConnectors.Add(output.Name.Trim(), output);
        }

        none = 0;
        foreach (var input in InputGateConnectors)
        {
            if (input.Name == "none")
            {
                input.Name = "none" + none;
                none++;
            }
            
            inputGateConnectors.Add(input.Name.Trim(), input);
        }

        for (int i = 0; i < connectionsCount; i++)
        {
            int index = connectionIndex + i + 1;
            values = lines[index].Split(' ');
            string outputName = values[0].Trim();
            string inputName = values[2].Trim();

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

    public static void ClearNoiseNodes()
    {
        Dictionary<UINoiseNodePrefab, ConnectorNode> copy = new(NoiseNodes);
        foreach (var node in copy.Values)
        {
            if (node is DisplayConnectorNode) // Don't remove display node
                continue;
            
            RemoveNode(node, false);
        }

        NoiseNodes = [];
        ConnectedNodeList = [];
        OutputGateConnectors = [];
        InputGateConnectors = [];

        GenerateLines();
        Compile();
    }

    public static void Clear()
    {
        Dictionary<UINoiseNodePrefab, ConnectorNode> copy = new(NoiseNodes);
        foreach (var node in copy.Values)
        {
            RemoveNode(node, false);
        }

        NoiseNodes = [];
        ConnectedNodeList = [];
        OutputGateConnectors = [];
        InputGateConnectors = [];
        _connectorLineVertices = [];

        DisplayNode = null;
    }
}