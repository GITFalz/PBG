using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

public class CurveConnectorNode : ConnectorNode
{
    public string Name => UICurveNodePrefab.Name;
    public UICurveNodePrefab UICurveNodePrefab;
    public CurveWindow CurveWindow;

    public InputGateConnector InputGateConnector;
    public OutputGateConnector OutputGateConnector;

    public float Min {
        get {
            return _min; 
        } set {
            _min = value;
            UICurveNodePrefab.MinInputField.SetText(NoSpace(_min));
        }
    }
    public float Max {
        get {
            return _max; 
        } set {
            _max = value;
            UICurveNodePrefab.MaxInputField.SetText(NoSpace(_max));
        }
    }

    private float _min = 0;
    private float _max = 1;
    private int _pointCount = 0;

    private int _minIndex = -1;
    private int _maxIndex = -1;
    private int _curveIndex = -1;

    public CurveConnectorNode(UICurveNodePrefab uICurveNodePrefab)
    {
        UICurveNodePrefab = uICurveNodePrefab;
        UICurveNodePrefab.CurveWindow.CurveNode = this;
        CurveWindow = uICurveNodePrefab.CurveWindow;

        InputGateConnector = new InputGateConnector(UICurveNodePrefab.InputButton, this);
        OutputGateConnector = new OutputGateConnector(UICurveNodePrefab.OutputButton, this);

        UICurveNodePrefab.InputButton.SetOnClick(() => InputConnectionTest(InputGateConnector));
        UICurveNodePrefab.OutputButton.SetOnClick(() => OutputConnectionTest(OutputGateConnector));

        UICurveNodePrefab.MinInputField.SetOnTextChange(() => SetValue(ref _min, UICurveNodePrefab.MinInputField, 0, _minIndex));
        UICurveNodePrefab.MaxInputField.SetOnTextChange(() => SetValue(ref _max, UICurveNodePrefab.MaxInputField, 1, _maxIndex));

        UICurveNodePrefab.MinTextField.SetOnClick(() => Game.SetCursorState(CursorState.Grabbed));
        UICurveNodePrefab.MaxTextField.SetOnClick(() => Game.SetCursorState(CursorState.Grabbed));

        UICurveNodePrefab.MinTextField.SetOnHold(() => SetSlideValue(ref _min, UICurveNodePrefab.MinInputField, 5f, _minIndex));
        UICurveNodePrefab.MaxTextField.SetOnHold(() => SetSlideValue(ref _max, UICurveNodePrefab.MaxInputField, 5f, _maxIndex));

        UICurveNodePrefab.MinTextField.SetOnRelease(() => Game.SetCursorState(CursorState.Normal));
        UICurveNodePrefab.MaxTextField.SetOnRelease(() => Game.SetCursorState(CursorState.Normal));
        
        UICurveNodePrefab.Collection.SetOnClick(() => NoiseEditor.SelectedNode = this);
    }

    public void UpdateCurve(CurveWindow curveWindow)
    {
        if (_curveIndex == -1)
            return;

        for (int i = 0; i < curveWindow.Points.Count; i++)
        {
            Vector2 point = curveWindow.Points[i];
            NoiseGlslNodeManager.UpdateValue(_curveIndex + 1 + i*2,   point.X);
            NoiseGlslNodeManager.UpdateValue(_curveIndex + 1 + i*2+1, point.Y);
        }
        _pointCount = curveWindow.Points.Count;
    }

    public override string GetLine()
    {
        string minValue = _minIndex != -1 ? $"values[{_minIndex}]" : NoSpace(Min);
        string maxValue = _maxIndex != -1 ? $"values[{_maxIndex}]" : NoSpace(Max);
        string startIndex = _curveIndex != -1 ? $"values[{_curveIndex}]" : "0";
        string count = _curveIndex != -1 ? $"{_pointCount}" : "0";

        string line = $"    float {VariableName} = mix({minValue}, {maxValue}, GetSplineVector(int(floor({startIndex})), {count}, ";
        if (InputGateConnector.IsConnected && InputGateConnector.OutputGateConnector != null)
        {
            line += $"{InputGateConnector.OutputGateConnector.Node.VariableName}));";
        }
        else
        {
            line += $"0.0));";
        }
        return line;
    }

    public override List<ConnectorNode> GetConnectedNodes()
    {
        List<ConnectorNode> connectedNodes = [];
        if (InputGateConnector.IsConnected && InputGateConnector.OutputGateConnector != null)
            connectedNodes.Add(InputGateConnector.OutputGateConnector.Node);
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
        if (InputGateConnector.IsConnected && InputGateConnector.OutputGateConnector != null)
            inputNodes.Add(InputGateConnector.OutputGateConnector.Node);
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
        return [InputGateConnector];
    }
    public override List<OutputGateConnector> GetOutputGateConnectors()
    {
        return [OutputGateConnector];
    }

    public override UINoiseNodePrefab[] GetNodePrefabs()
    {
        return [UICurveNodePrefab];
    }

    public override UIController GetUIController()
    {
        return UICurveNodePrefab.Collection.UIController;
    }

    public override string ToStringList()
    {
        return 
            $"NodeType: MinMaxInputOperation " +
            $"Values: {NoSpace(Min)} {NoSpace(Max)} " +
            $"Inputs: {NoSpace(InputGateConnector.Name)} " +
            $"Outputs: {NoSpace(OutputGateConnector.Name)} " +
            $"Prefab: {NoSpace(Name)} {NoSpace(UICurveNodePrefab.Collection.Offset)}";
    }

    public override void SetValueReferences(List<float> values, ref int index)
    {
        _minIndex = index;
        values.Add(Min);
        index++;

        _maxIndex = index;
        values.Add(Max);
        index++;

        _curveIndex = index;
        values.Add(index);
        index++;

        for (int i = 0; i < CurveWindow.Points.Count; i++)
        {
            Vector2 point = CurveWindow.Points[i];
            values.Add(point.X);
            index++;
            values.Add(point.Y);
            index++;
        }
        _pointCount = CurveWindow.Points.Count;
    }
}