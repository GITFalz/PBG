using OpenTK.Windowing.Common;

public class CombineConnectorNode : ConnectorNode
{
    public string Name => CombineNodePrefab.Name;
    public UICombineNodePrefab CombineNodePrefab;

    public InputGateConnector InputGateConnector1;
    public InputGateConnector InputGateConnector2;

    public OutputGateConnector OutputGateConnector;

    public float Value1 {
        get {
            return _value1; 
        } set {
            _value1 = value;
            CombineNodePrefab.Value1InputField.SetText(NoSpace(_value1));
        }
    }
    public float Value2 {
        get {
            return _value2; 
        } set {
            _value2 = value;
            CombineNodePrefab.Value2InputField.SetText(NoSpace(_value2));
        }
    }

    private float _value1 = 1.0f;
    private float _value2 = 1.0f;

    private int _value1Index = -1;
    private int _value2Index = -1;

    public CombineConnectorNode(UICombineNodePrefab combineNodePrefab)
    {
        CombineNodePrefab = combineNodePrefab;
        InputGateConnector1 = new InputGateConnector(CombineNodePrefab.InputButton1, this);
        InputGateConnector2 = new InputGateConnector(CombineNodePrefab.InputButton2, this);
        OutputGateConnector = new OutputGateConnector(CombineNodePrefab.OutputButton, this);

        CombineNodePrefab.InputButton1.SetOnClick(() => InputConnectionTest(InputGateConnector1));
        CombineNodePrefab.InputButton2.SetOnClick(() => InputConnectionTest(InputGateConnector2));
        CombineNodePrefab.OutputButton.SetOnClick(() => OutputConnectionTest(OutputGateConnector));

        CombineNodePrefab.Value1InputField.SetOnTextChange(() => SetValue(ref _value1, CombineNodePrefab.Value1InputField, 1.0f, _value1Index));
        CombineNodePrefab.Value2InputField.SetOnTextChange(() => SetValue(ref _value2, CombineNodePrefab.Value2InputField, 1.0f, _value2Index));

        CombineNodePrefab.Value1TextField.SetOnClick(() => Game.SetCursorState(CursorState.Grabbed));
        CombineNodePrefab.Value2TextField.SetOnClick(() => Game.SetCursorState(CursorState.Grabbed));

        CombineNodePrefab.Value1TextField.SetOnHold(() => SetSlideValue(ref _value1, CombineNodePrefab.Value1InputField, 5f, _value1Index));
        CombineNodePrefab.Value2TextField.SetOnHold(() => SetSlideValue(ref _value2, CombineNodePrefab.Value2InputField, 5f, _value2Index));

        CombineNodePrefab.Value1TextField.SetOnRelease(() => Game.SetCursorState(CursorState.Normal));
        CombineNodePrefab.Value2TextField.SetOnRelease(() => Game.SetCursorState(CursorState.Normal));

        CombineNodePrefab.Collection.SetOnClick(() => NoiseEditor.SelectedNode = this);
    }

    public override string GetLine()
    {
        string line = $"    float {VariableName} = ";

        string value1 = InputGateConnector1.IsConnected && InputGateConnector1.OutputGateConnector != null
            ? InputGateConnector1.OutputGateConnector.Node.VariableName
            : ( _value1Index != -1 ? $"values[{_value1Index}]" : NoSpace(Value1));

        line += $"{value1};";
        return line;
    }

    public override List<ConnectorNode> GetConnectedNodes()
    {
        List<ConnectorNode> connectedNodes = [];
        if (InputGateConnector1.IsConnected && InputGateConnector1.OutputGateConnector != null)
            connectedNodes.Add(InputGateConnector1.OutputGateConnector.Node);

        if (InputGateConnector2.IsConnected && InputGateConnector2.OutputGateConnector != null)
            connectedNodes.Add(InputGateConnector2.OutputGateConnector.Node);

        if (OutputGateConnector.IsConnected)
        {
            foreach (var input in OutputGateConnector.InputGateConnectors)
            {
                connectedNodes.Add(input.Node);
            }
        }
        return connectedNodes;
    }

    public override List<ConnectorNode> GetInputNodes() 
    { 
        List<ConnectorNode> inputNodes = [];
        if (InputGateConnector1.IsConnected && InputGateConnector1.OutputGateConnector != null)
            inputNodes.Add(InputGateConnector1.OutputGateConnector.Node);

        if (InputGateConnector2.IsConnected && InputGateConnector2.OutputGateConnector != null)
            inputNodes.Add(InputGateConnector2.OutputGateConnector.Node);
        return inputNodes; 
    }

    public override List<ConnectorNode> GetOutputNodes() 
    { 
        List<ConnectorNode> outputNodes = [];
        if (OutputGateConnector.IsConnected)
        {
            foreach (var input in OutputGateConnector.InputGateConnectors)
            {
                outputNodes.Add(input.Node);
            }
        }
        return outputNodes;
    }

    public override List<InputGateConnector> GetInputGateConnectors()
    {
        return [InputGateConnector1, InputGateConnector2];
    }

    public override List<OutputGateConnector> GetOutputGateConnectors()
    {
        return [OutputGateConnector];
    }

    public override UINoiseNodePrefab[] GetNodePrefabs()
    {
        return [CombineNodePrefab];
    }

    public override UIController GetUIController()
    {
        return CombineNodePrefab.Collection.UIController;
    }

    public override string ToStringList()
    {
        return 
            $"NodeType: Combine " +
            $"Values: {NoSpace(Value1)} {NoSpace(Value2)} " + 
            $"Inputs: {NoSpace(InputGateConnector1.Name)} {NoSpace(InputGateConnector2.Name)} " +
            $"Outputs: {NoSpace(OutputGateConnector.Name)} " +
            $"Prefab: {NoSpace(Name)} {NoSpace(CombineNodePrefab.Collection.Offset)}";
    }

    public override void SetValueReferences(List<float> values, ref int index)
    {
        if (!InputGateConnector1.IsConnected)
        {
            _value1Index = index;
            values.Add(Value1);
            index++;
        }
        else
        {
            _value1Index = -1;
        }

        if (!InputGateConnector2.IsConnected)
        {
            _value2Index = index;
            values.Add(Value2);
            index++;
        }
        else
        {
            _value2Index = -1;
        }
    }
}