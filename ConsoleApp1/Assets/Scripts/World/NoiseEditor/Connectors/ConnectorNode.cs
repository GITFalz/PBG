public abstract class ConnectorNode
{
    public static ConnectorNode Empty = new EmptyConnectorNode();
    public static GateConnector? SelectedGateConnector = null;


    public string VariableName = "variable0";

    public abstract string GetLine();
    public abstract List<ConnectorNode> GetConnectedNodes(); 
    public abstract List<ConnectorNode> GetInputNodes();
    public abstract List<ConnectorNode> GetOutputNodes();

    public static void Compile()
    {
        NoiseGlslNodeManager.Compile();
    }

    public void OutputConnectionTest(OutputGateConnector output)
    {
        if (output.IsConnected)
        {
            Disconnect(output);
            NoiseNodeManager.GenerateLines();
            return;
        }
        else
        {
            if (SelectedGateConnector == null)
            {
                SelectedGateConnector = output;
                return;
            }

            if (SelectedGateConnector is OutputGateConnector)
            {
                SelectedGateConnector = null;
                NoiseNodeManager.GenerateLines();
                return;
            }

            if (SelectedGateConnector is InputGateConnector input)
            {
                if (input.Node == output.Node)
                {
                    SelectedGateConnector = null;
                    return;
                }

                Connect(input, output);
                SelectedGateConnector = null;
                NoiseNodeManager.GenerateLines();
            }
        }
    }

    public void InputConnectionTest(InputGateConnector input)
    {
        if (input.IsConnected)
        {
            Disconnect(input);
            NoiseNodeManager.GenerateLines();
            return;
        }
        else
        {
            if (SelectedGateConnector == null)
            {
                SelectedGateConnector = input;
                return;
            }

            if (SelectedGateConnector is InputGateConnector)
            {
                SelectedGateConnector = null;
                NoiseNodeManager.GenerateLines();
                return;
            }

            if (SelectedGateConnector is OutputGateConnector output)
            {
                if (input.Node == output.Node)
                {
                    SelectedGateConnector = null;
                    return;
                }
                
                Connect(input, output);
                SelectedGateConnector = null;
                NoiseNodeManager.GenerateLines();
            }
        }
    }


    public static void Connect(InputGateConnector input, OutputGateConnector output)
    {
        // Make sure to disconnect first
        Disconnect(input);
        Disconnect(output);

        input.Connect(output);
        output.Connect(input);
    }

    public static void Disconnect(InputGateConnector input, OutputGateConnector output)
    {
        output.Disconnect();
        input.Disconnect();
    }

    public static void Disconnect(InputGateConnector input)
    {
        input.OutputGateConnector?.Disconnect();
        input.Disconnect();
    }

    public static void Disconnect(OutputGateConnector output)
    {
        output.InputGateConnector?.Disconnect();
        output.Disconnect();
    }

    public static bool Connected(InputGateConnector input, OutputGateConnector output)
    {
        return input.IsConnected && output.IsConnected && input.OutputGateConnector == output && output.InputGateConnector == input;
    }
}