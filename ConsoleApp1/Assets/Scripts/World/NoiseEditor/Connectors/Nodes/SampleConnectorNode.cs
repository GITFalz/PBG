using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

public class SampleConnectorNode : ConnectorNode
{
    public string Name => SampleNodePrefab.Name;
    public UISampleNodePrefab SampleNodePrefab;

    public OutputGateConnector OutputGateConnector;

    public float Scale = 1.0f;
    public Vector2 Offset = (0.0f, 0.0f);

    private int _scaleIndex = -1;
    private int _offsetXIndex = -1;
    private int _offsetYIndex = -1;

    public SampleConnectorNode(UISampleNodePrefab sampleNodePrefab)
    {
        SampleNodePrefab = sampleNodePrefab;
        OutputGateConnector = new OutputGateConnector(SampleNodePrefab.OutputButton, this);
        SampleNodePrefab.OutputButton.SetOnClick(() => OutputConnectionTest(OutputGateConnector));

        SampleNodePrefab.ScaleInputField.SetOnTextChange(() => SetValue(ref Scale, SampleNodePrefab.ScaleInputField, 1.0f, _scaleIndex));
        SampleNodePrefab.OffsetXInputField.SetOnTextChange(() => SetValue(ref Offset.X, SampleNodePrefab.OffsetXInputField, 0.0f, _offsetXIndex));
        SampleNodePrefab.OffsetYInputField.SetOnTextChange(() => SetValue(ref Offset.Y, SampleNodePrefab.OffsetYInputField, 0.0f, _offsetYIndex));

        SampleNodePrefab.ScaleTextField.SetOnClick(() => Game.SetCursorState(CursorState.Grabbed));
        SampleNodePrefab.OffsetXTextField.SetOnClick(() => Game.SetCursorState(CursorState.Grabbed));
        SampleNodePrefab.OffsetYTextField.SetOnClick(() => Game.SetCursorState(CursorState.Grabbed));

        SampleNodePrefab.ScaleTextField.SetOnHold(() => SetSlideValue(ref Scale, SampleNodePrefab.ScaleInputField, 5f, _scaleIndex)); 
        SampleNodePrefab.OffsetXTextField.SetOnHold(() => SetSlideValue(ref Offset.X, SampleNodePrefab.OffsetXInputField, 5f, _offsetXIndex));
        SampleNodePrefab.OffsetYTextField.SetOnHold(() => SetSlideValue(ref Offset.Y, SampleNodePrefab.OffsetYInputField, 5f, _offsetYIndex));

        SampleNodePrefab.ScaleTextField.SetOnRelease(() => Game.SetCursorState(CursorState.Normal));
        SampleNodePrefab.OffsetXTextField.SetOnRelease(() => Game.SetCursorState(CursorState.Normal));
        SampleNodePrefab.OffsetYTextField.SetOnRelease(() => Game.SetCursorState(CursorState.Normal));

        sampleNodePrefab.Collection.SetOnClick(() => NoiseEditor.SelectedNode = this);
    } 

    public override string GetLine()
    {
        string scaleValue = _scaleIndex != -1 ? $"values[{_scaleIndex}]" : Scale.ToString();
        string offsetXValue = _offsetXIndex != -1 ? $"values[{_offsetXIndex}]" : Offset.X.ToString();
        string offsetYValue = _offsetYIndex != -1 ? $"values[{_offsetYIndex}]" : Offset.Y.ToString();

        string scale = $"vec2({VariableName}Scale, {VariableName}Scale)";
        string offset = $"vec2({offsetXValue}, {offsetYValue})";

        return $"    float {VariableName}Scale = {scaleValue}; float {VariableName} = SampleNoise({scale}, {offset});";
    }

    public override List<ConnectorNode> GetConnectedNodes()
    {
        List<ConnectorNode> connectedNodes = [];
        if (OutputGateConnector.IsConnected && OutputGateConnector.InputGateConnector != null)
            connectedNodes.Add(OutputGateConnector.InputGateConnector.Node);
        return connectedNodes;
    }

    public override List<ConnectorNode> GetInputNodes() 
    { 
        return [];
    }

    public override List<ConnectorNode> GetOutputNodes() 
    { 
        List<ConnectorNode> outputNodes = [];
        if (OutputGateConnector.IsConnected && OutputGateConnector.InputGateConnector != null)
            outputNodes.Add(OutputGateConnector.InputGateConnector.Node);
        return outputNodes;
    }

    public override List<InputGateConnector> GetInputGateConnectors()
    {
        return [];
    }
    public override List<OutputGateConnector> GetOutputGateConnectors()
    {
        return [OutputGateConnector];
    }

    public override UINoiseNodePrefab[] GetNodePrefabs()
    {
        return [SampleNodePrefab];
    }

    public override UIController GetUIController()
    {
        return SampleNodePrefab.Collection.UIController;
    }

    public override string ToStringList() 
    {
        return 
            $"NodeType: Sample " +
            $"Values: {NoSpace(Scale)} {NoSpace(Offset)} " +
            $"Outputs: {NoSpace(OutputGateConnector.Name)} " +
            $"Prefab: {NoSpace(Name)} {NoSpace(SampleNodePrefab.Collection.Offset)}";
    }

    public override void SetValueReferences(List<float> values, ref int index)
    {
        _scaleIndex = index;
        values.Add(Scale);
        index++;

        _offsetXIndex = index;
        values.Add(Offset.X);
        index++;

        _offsetYIndex = index;
        values.Add(Offset.Y);
        index++;
    }
}