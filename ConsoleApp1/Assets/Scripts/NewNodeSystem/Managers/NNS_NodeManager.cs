using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class NNS_NodeManager
{
    public static List<NNS_NodeBase> Nodes = [];
    public static List<NNS_NodeInput> Inputs = [];
    public static List<NNS_NodeOutput> Outputs = [];

    public static NNS_OutputNode OutputNode = null!;

    public static ShaderProgram ConnectorLineShaderProgram = new ShaderProgram("Noise/ConnectorLine.vert", "Noise/ConnectorLine.frag");
    private static VAO _connectorLineVAO = new VAO();
    private static VBO<Vector3> _connectorLineVBO = new([]);
    private static List<Vector3> _connectorLineVertices = [];

    public static UIController NodeController = null!;

    public NNS_NodeManager()
    {
        NodeController = new();
        OutputNode = new NNS_OutputNode(new Vector2(0, 0));
    }

    public static void AddNode(NNS_NodeBase node)
    {
        if (node is NNS_OutputNode)
            return;

        Nodes.Add(node);
        for (int i = 0; i < node.Inputs.Count; i++)
        {
            Inputs.Add(node.Inputs[i]);
        }
        for (int i = 0; i < node.Outputs.Count; i++)
        {
            Outputs.Add(node.Outputs[i]);
        }
    }

    public static void RemoveNode(NNS_NodeBase node)
    {
        if (node is NNS_OutputNode)
            return;

        Nodes.Remove(node);
        for (int i = 0; i < node.Inputs.Count; i++)
        {
            Inputs.Remove(node.Inputs[i]);
        }
        for (int i = 0; i < node.Outputs.Count; i++)
        {
            Outputs.Remove(node.Outputs[i]);
        }
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
        if (Outputs.Count == 0 || Inputs.Count == 0)
            return;

        foreach (var output in Outputs)
        {
            if (output.IsConnected)
            {
                for (int i = 0; i < output.Inputs.Count; i++)
                {
                    int index = output.Indices[i] * 2;
                    if (index < 0)
                        continue;
                    
                    var input = output.Inputs[i];
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
    
    public static void RenderLine(Matrix4 orthographicProjection)
    {
        if (Outputs.Count == 0 || Inputs.Count == 0)
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

    public static void Compile()
    {
        List<NNS_NodeBase> ConnectedNodeList = [OutputNode];
        GetConnectedNodes(OutputNode, ConnectedNodeList);
        int nodeCount = ConnectedNodeList.Count;
        for (int i = 0; i < nodeCount; i++)
        {
            NNS_NodeBase node = ConnectedNodeList[i];
            if (node.Outputs.Count == 0)
                continue;

            for (int j = 0; j < node.Outputs.Count; j++)
            {
                NNS_NodeOutput output = node.Outputs[j];
                output.VariableName = "variable" + i + "_" + j;
            }
        }

        NNS_GLSLManager.Compile(ConnectedNodeList);
    }

    public static void GetConnectedNodes(NNS_NodeBase node, List<NNS_NodeBase> nodes)
    {
        foreach (var (input, n) in node.GetInputNodes())
        {
            if (!input.IsConnected)
                continue;

            nodes.Remove(n);
            nodes.Insert(0, n);
            GetConnectedNodes(n, nodes);
        }
    }
    
    public static List<(NNS_NodeInput, NNS_NodeOutput)> GetConnections()
    {
        List<(NNS_NodeInput, NNS_NodeOutput)> connections = [];
        foreach (var output in Outputs)
        {
            if (!output.IsConnected)
                continue;

            foreach (var input in output.Inputs)
            {
                connections.Add((input, output));
            }
        }
        return connections;
    }
}