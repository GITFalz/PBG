using OpenTK.Mathematics;

public abstract class ConnectorNode
{
    public static ConnectorNode Empty = new EmptyConnectorNode();
    public static GateConnector? SelectedGateConnector = null;


    public string VariableName = "variable0";

    public abstract string GetLine();
    public abstract List<ConnectorNode> GetConnectedNodes(); 
    public abstract List<ConnectorNode> GetInputNodes();
    public abstract List<ConnectorNode> GetOutputNodes();
    public abstract List<InputGateConnector> GetInputGateConnectors();
    public abstract List<OutputGateConnector> GetOutputGateConnectors();
    public abstract UINoiseNodePrefab[] GetNodePrefabs();
    public abstract UIController GetUIController();
    public abstract string ToStringList();
    public abstract void SetValueReferences(List<float> values, ref int index);

    public void SetSlideValue(ref float value, UIInputField inputField, float speed, int index)
    {
        float delta = Input.GetMouseDelta().X * speed * GameTime.DeltaTime;
        value += delta;
        inputField.SetText(value.ToString("0.00")).GenerateChars().UpdateText();
        NoiseGlslNodeManager.UpdateValue(index, value);
    }

    public void SetValue(ref float value, UIInputField inputField, float replacement, int index)
    {
        value = inputField.ParseFloat(replacement);
        NoiseGlslNodeManager.UpdateValue(index, value);
    }

    public void OutputConnectionTest(OutputGateConnector output)
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
            if (input.Node == output.Node || output.InputGateConnectors.Contains(input))
            {
                SelectedGateConnector = null;
                return;
            }

            Connect(input, output);
            SelectedGateConnector = null;
            NoiseNodeManager.GenerateLines();
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

        input.Connect(output);
        output.Connect(input);
    }

    public static void Disconnect(ConnectorNode node)
    {
        foreach (var input in node.GetInputGateConnectors())
        {
            Disconnect(input);
        }

        foreach (var output in node.GetOutputGateConnectors())
        {
            Disconnect(output);
        }
    }

    public static void Disconnect(InputGateConnector input, OutputGateConnector output)
    {
        output.Disconnect(input);
        input.Disconnect();
    }

    public static void Disconnect(InputGateConnector input)
    {
        input.OutputGateConnector?.Disconnect(input);
        input.Disconnect();
    }

    public static void Disconnect(OutputGateConnector output)
    {
        foreach (var input in output.InputGateConnectors)
        {
            input.Disconnect();
        }
        output.Disconnect();
    }

    public static bool Connected(InputGateConnector input, OutputGateConnector output)
    {
        return input.IsConnected && output.IsConnected && input.OutputGateConnector == output && output.InputGateConnectors.Contains(input);
    }

    public static string NoSpace(float value)
    {
        return NoSpace(value.ToString());
    }

    public static string NoSpace(int value)
    {
        return NoSpace(value.ToString());
    }

    public static string NoSpace(Vector4 value) 
    {
        return NoSpace(value.ToString());
    }

    public static string NoSpace(Vector3 value)
    {
        return NoSpace(value.ToString());
    }

    public static string NoSpace(Vector2 value)
    {
        return NoSpace(value.ToString());
    }

    public static string NoSpace(string str)
    {
        return str.Replace(" ", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty); // Remove all spaces and new lines
    }
}