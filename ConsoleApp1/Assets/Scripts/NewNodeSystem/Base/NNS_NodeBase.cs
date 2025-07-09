using OpenTK.Mathematics;

public abstract class NNS_NodeBase
{
    public static Vector4 SAMPLE_NODE_COLOR = new Vector4(0.290f, 0.565f, 0.886f, 1f); // #4A90E2
    public static Vector4 DIRECT_SAMPLE_NODE_COLOR = new Vector4(0.204f, 0.486f, 0.847f, 1f); // #347CD8
    public static Vector4 VORONOI_NODE_COLOR = new Vector4(0.314f, 0.890f, 0.761f, 1f); // #50E3C2
    public static Vector4 DOUBLE_INPUT_COLOR = new Vector4(0.961f, 0.647f, 0.137f, 1f); // #F5A623
    public static Vector4 BASE_INPUT_COLOR = new Vector4(0.400f, 0.600f, 0.800f, 1f); // #6699CC
    public static Vector4 MIN_MAX_INPUT_COLOR = new Vector4(0.973f, 0.905f, 0.110f, 1f); // #F8E71C
    public static Vector4 RANGE_NODE_COLOR = new Vector4(0.494f, 0.827f, 0.129f, 1f); // #7ED321
    public static Vector4 COMBINE_NODE_COLOR = new Vector4(0.565f, 0.075f, 0.996f, 1f); // #9013FE
    public static Vector4 INIT_MASK_NODE_COLOR = new Vector4(0.816f, 0.008f, 0.106f, 1f); // #D0021B
    public static Vector4 CURVE_NODE_COLOR = new Vector4(0.855f, 0.388f, 0.725f, 1f); // #DA63B9
    public static Vector4 CONTEXT_NODE_COLOR = new Vector4(0.376f, 0.376f, 0.376f, 1f); // #606060
    public static Vector4 SELECTION_COLOR = new Vector4(0.529f, 0.808f, 0.980f, 1.0f); // #87CEEB

    /* Node info */
    public string Name = "Node";
    public Vector4 Color;

    public Vector2 Position = Vector2.Zero; // Position in the UI, used for initialization and moving the node
    private Vector2 _oldMouseButtonPosition = Vector2.Zero; // Used to track the mouse position when moving the node


    /* Node logic */
    public List<NNS_NodeInput> Inputs = [];
    public List<NNS_NodeOutput> Outputs = [];
    public List<NNS_NodeField> Fields = [];

    private Dictionary<NNS_NodeInput, NNS_NodeTemplate.NNS_NodeInputFieldStruct> _inputFields = [];
    private Dictionary<NNS_NodeOutput, NNS_NodeTemplate.NNS_NodeOutputFieldStruct> _outputFields = [];


    private static Dictionary<string, NNS_NodeTemplate> NodeTemplates = new Dictionary<string, NNS_NodeTemplate>();
    private NNS_NodeTemplate? _nodeTemplate = null;



    /* UI logic */
    public UIController Controller = null!;
    public UICollection Collection = null!;

    private bool HasType = false;
    protected string Type = "";



    /* Connection logic */
    public static NNS_NodeConnector? SelectedConnection = null;


    public NNS_NodeBase(string name, Vector4 color, Vector2 position, string type = "")
    {
        if (!string.IsNullOrEmpty(type))
        {
            HasType = true;
            Type = type;
        }

        Name = name;
        Color = color;
        Position = position;
        Controller = NNS_NodeManager.NodeController;
        if (!NodeTemplates.TryGetValue(Name, out NNS_NodeTemplate? value))
        {
            _nodeTemplate = new NNS_NodeTemplate(GetType().Name);
            InitializeNodeData();
            NodeTemplates.Add(Name, _nodeTemplate);
        }
        else
        {
            _nodeTemplate = value;
        }
        Initialize();
        _nodeTemplate = null;
    }


    /* Connection functions */
    public void Connect(NNS_NodeInput input)
    {
        if (input.IsConnected)
        {
            input.Disconnect();
            input.Deselect();
            NNS_NodeManager.GenerateLines();
            return;
        }

        if (SelectedConnection == null)
        {
            SelectedConnection = input;
            input.Select();
            return;
        }

        if (SelectedConnection is NNS_NodeInput)
        {
            SelectedConnection = null;
            input.Deselect();
            return;
        }

        if (SelectedConnection is NNS_NodeOutput output)
        {
            if (output.Node == input.Node || output.IsConnectedTo(input))
            {
                output.Deselect();
                input.Deselect();
                SelectedConnection = null;
                return;
            }

            output.Connect(input);
            output.Deselect();
            input.Deselect();
            SelectedConnection = null;
            NNS_NodeManager.GenerateLines();
        }
        else
        {
            throw new InvalidOperationException("Selected connection is not a valid output.");
        }
    }

    public void Connect(NNS_NodeOutput output)
    {
        if (SelectedConnection == null)
        {
            SelectedConnection = output;
            output.Select();
            return;
        }

        if (SelectedConnection is NNS_NodeOutput)
        {
            SelectedConnection = null;
            output.Deselect();
            return;
        }

        if (SelectedConnection is NNS_NodeInput input)
        {
            if (input.Node == output.Node || output.IsConnectedTo(input))
            {
                SelectedConnection.Deselect();
                output.Deselect();
                SelectedConnection = null;
                return;
            }

            output.Connect(input);
            output.Deselect();
            input.Deselect();
            SelectedConnection = null;
            NNS_NodeManager.GenerateLines();
        }
        else
        {
            throw new InvalidOperationException("Selected connection is not a valid input.");
        }
    }


    /* Registration functions */
    protected void RegisterInputField(string identifier, NNS_ValueType type) => Internal_RegisterInputField(identifier, true, NNS_NodeValue.GetDefaultValue(type), false);
    protected void RegisterInputField(string identifier, bool hasInput, float value) => Internal_RegisterInputField(identifier, hasInput, value); // using the private method to avoid code duplication
    protected void RegisterInputField(string identifier, bool hasInput, int value) => Internal_RegisterInputField(identifier, hasInput, value);
    protected void RegisterInputField(string identifier, bool hasInput, Vector2 value) => Internal_RegisterInputField(identifier, hasInput, value);
    protected void RegisterInputField(string identifier, bool hasInput, Vector2i value) => Internal_RegisterInputField(identifier, hasInput, value);
    protected void RegisterInputField(string identifier, bool hasInput, Vector3 value) => Internal_RegisterInputField(identifier, hasInput, value);
    protected void RegisterInputField(string identifier, bool hasInput, Vector3i value) => Internal_RegisterInputField(identifier, hasInput, value);

    private void Internal_RegisterInputField(string identifier, bool hasInput, object value, bool hasDefaultValue = true)
    {
        if (_nodeTemplate == null)
            throw new InvalidOperationException("Please ensure you call the registration methods inside the InitializeNodeData method of your node class.");

        _nodeTemplate.RegisterInputField(identifier, hasInput, value, hasDefaultValue);
    }


    protected void RegisterOutputField(string identifier, NNS_ValueType type) => Internal_RegisterOutputField(identifier, type); // using the private method to avoid code duplication
    private void Internal_RegisterOutputField(string identifier, NNS_ValueType type)
    {
        if (_nodeTemplate == null)
            throw new InvalidOperationException("Please ensure you call the registration methods inside the InitializeNodeData method of your node class.");

        _nodeTemplate.RegisterOutputField(identifier, type);
    }


    protected void RegisterAction(string name, string type, string operation)
    {
        if (_nodeTemplate == null)
            throw new InvalidOperationException("Please ensure you call the registration methods inside the InitializeNodeData method of your node class.");

        _nodeTemplate.RegisterAction(name, type, operation);
    }



    /* Initialization */
    private void Initialize()
    {
        if (_nodeTemplate == null)
            throw new InvalidOperationException("Node template is not initialized. This should not happen. Please fix the bug you dumbass.");

        Collection = new UICollection($"{GetType().Name} Node collection", Controller, AnchorType.TopLeft) { Offset = (Position.X, Position.Y, 0, 0) };
        UIButton moveButton = new UIButton("Move button", Controller, AnchorType.TopLeft, PositionType.Relative, Color, (0, 0, 0), (20, 20), (0, 0, 0, 0), 0, 10, (10f, 0.05f), UIState.Interactable);
        moveButton.SetOnClick(SetOldMousePosition);
        moveButton.SetOnHold(MoveNode);
        UIImage background = new UIImage("Node background", Controller, AnchorType.ScaleFull, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (0, 0), (0, 20, 0, 0), 0, 10, (10f, 0.05f));
        UIText nodeName = new UIText("Node name", Controller, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (100, 20), (10, 30, 0, 0), 0);
        nodeName.SetTextCharCount(Name + (HasType ? " " + Type : ""), 1f);
        UIVerticalCollection inputFieldsCollection = new UIVerticalCollection("Node fields", Controller, AnchorType.ScaleTop, PositionType.Relative) { Border = (0, nodeName.Scale.Y + 15, 0, 0), Offset = (0, 20, 0, 0) };
        UICollection spacingCollection = new UICollection("Spacing collection", Controller, AnchorType.TopLeft, PositionType.Relative) { Offset = (0, 0, 0, 0), Scale = (0, 5) };

        float maxScaleX = nodeName.Scale.X + 20;

        UICollection outputSectionCollection = new UICollection("Output fields collection", Controller, AnchorType.TopCenter, PositionType.Relative) { Offset = (0, 0, 0, 0), Scale = (0, 15) };
        if (_nodeTemplate.RegisteredOutputFields.Count > 0)
        {
            UIImage outputFieldButton = new UIImage("Output fields background", Controller, AnchorType.ScaleFull, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (0, 15), (0, 0, 0, 0), 0, 10, (7.5f, 0.05f));
            UIText outputFieldsName = new UIText("Output fields name", Controller, AnchorType.MiddleCenter, PositionType.Relative, (1f, 1f, 1f, 1f), (0, 0, 0), (100, 15), (0, 0, 0, 0), 0);
            outputFieldsName.SetTextCharCount("Outputs", 0.8f);
            outputSectionCollection.AddElements(outputFieldButton, outputFieldsName);

            inputFieldsCollection.AddElement(outputSectionCollection);
            inputFieldsCollection.AddElement(spacingCollection);

            for (int i = 0; i < _nodeTemplate.RegisteredOutputFields.Count; i++)
            {
                var field = _nodeTemplate.RegisteredOutputFields[i];

                UICollection fieldCollection = new UICollection($"Field collection {field.Identifier}", Controller, AnchorType.ScaleTop, PositionType.Relative) { Scale = (0, 20) };

                UIButton outputButton = new UIButton($"Output button {field.Identifier} {i}", Controller, AnchorType.MiddleRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (20, 20), (-5, 0, 5, 0), 0, 11, (7.5f, 0.05f), UIState.Interactable);
                NNS_NodeOutput output = new NNS_NodeOutput(outputButton, this, field.Type);
                _outputFields.Add(output, field);
                Outputs.Add(output);
                outputButton.SetOnClick(() => { Connect(output); });
                fieldCollection.AddElement(outputButton);

                UIText fieldName = new UIText($"Field name {field.Identifier}", Controller, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (100, 20), (30, 0, 0, 0), 0);
                fieldName.SetMaxCharCount(10).SetText(field.Identifier, 0.8f);
                fieldCollection.AddElement(fieldName);

                inputFieldsCollection.AddElement(fieldCollection);
            }

            inputFieldsCollection.AddElement(spacingCollection);
        }

        UICollection inputSectionCollection = new UICollection("Output fields collection", Controller, AnchorType.TopCenter, PositionType.Relative) { Offset = (0, 0, 0, 0), Scale = (0, 15) };
        if (_nodeTemplate.RegisteredInputFields.Count > 0)
        {
            UIImage inputFieldButton = new UIImage("Output fields background", Controller, AnchorType.ScaleFull, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (0, 15), (0, 0, 0, 0), 0, 10, (7.5f, 0.05f));
            UIText inputFieldsName = new UIText("Output fields name", Controller, AnchorType.MiddleCenter, PositionType.Relative, (1f, 1f, 1f, 1f), (0, 0, 0), (100, 15), (0, 0, 0, 0), 0);
            inputFieldsName.SetTextCharCount("Inputs", 0.8f);
            inputSectionCollection.AddElements(inputFieldButton, inputFieldsName);

            inputFieldsCollection.AddElement(inputSectionCollection);
            inputFieldsCollection.AddElement(spacingCollection);

            for (int i = 0; i < _nodeTemplate.RegisteredInputFields.Count; i++)
            {
                var field = _nodeTemplate.RegisteredInputFields[i];
                NNS_NodeValue value = NNS_NodeValue.Get(field.Value);
                NNS_NodeInput? input = null;
                NNS_ValueType valueType = NNS_NodeValue.GetValueType(field.Value);

                UICollection fieldCollection = new UICollection($"Field collection {field.Identifier}", Controller, AnchorType.TopLeft, PositionType.Relative) { Scale = (0, 20) };

                if (field.HasInput)
                {
                    UIButton inputButton = new UIButton($"Input button {field.Identifier}", Controller, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (20, 20), (5, 0, 0, 0), 0, 11, (7.5f, 0.05f), UIState.Interactable);
                    input = new NNS_NodeInput(inputButton, this, valueType);
                    _inputFields.Add(input, field);
                    Inputs.Add(input);
                    inputButton.SetOnClick(() => Connect(input));
                    fieldCollection.AddElement(inputButton);
                }

                UIText fieldName = new UIText($"Field name {field.Identifier}", Controller, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (100, 20), (30, 0, 0, 0), 0);
                fieldName.SetMaxCharCount(10).SetText(field.Identifier, 0.8f);
                fieldCollection.AddElement(fieldName);

                maxScaleX = Math.Max(maxScaleX, fieldName.Scale.X + 40);

                if (field.HasDefaultValue)
                {
                    UICollection inputFields = value.GetInputFields(Controller);
                    inputFields.Offset.X = fieldName.Scale.X + 35;
                    maxScaleX = Math.Max(maxScaleX, inputFields.Scale.X + fieldName.Scale.X + 40);
                    fieldCollection.AddElement(inputFields);
                }

                inputFieldsCollection.AddElement(fieldCollection);
                Fields.Add(new NNS_NodeField(value, input));
            }
        }

        outputSectionCollection.SetScale((maxScaleX - 10, 15));
        inputSectionCollection.SetScale((maxScaleX - 10, 15));
        moveButton.SetScale((maxScaleX, 20));
        Collection.SetScale((maxScaleX, 15 + nodeName.Scale.Y + inputFieldsCollection.GetElementScaleY()));
        Collection.AddElements(moveButton, background, nodeName, inputFieldsCollection);
        Controller.AddElement(Collection);
    }

    /* Getters */
    public List<(NNS_NodeInput, NNS_NodeBase)> GetInputNodes()
    {
        List<(NNS_NodeInput, NNS_NodeBase)> inputNodes = [];
        foreach (var input in Inputs)
        {
            if (input.Output == null)
                continue;
                
            inputNodes.Add((input, input.Output.Node));
        }
        return inputNodes;
    }

    public string GetLine()
    {
        _nodeTemplate = NodeTemplates[Name];
        if (_nodeTemplate.RegisteredActions.Count == 0)
        {
            return $"// Node {Name} has no actions registered.";
        }

        var action = _nodeTemplate.GetAction(Type);

        if (action.Type == "operation")
        {
            if (_outputFields.Count == 0)
                return $"// Node {Name} has no outputs";

            var (output, outputField) = _outputFields.First();
            string line = $"{NNS_NodeValue.GetGLSLType(outputField.Type)} {output.VariableName} = ";
            for (int i = 0; i < Fields.Count; i++)
            {
                var field = Fields[i];
                if (i != 0)
                    line += $" {action.Action} ";
                line += field.GetVariable();
            }
            line += ";";
            return line;
        }
        if (action.Type == "function")
        {
            if (_outputFields.Count == 0)
                return $"// Node {Name} has no outputs";

            var (output, outputField) = _outputFields.First();
            string line = $"{NNS_NodeValue.GetGLSLType(outputField.Type)} {output.VariableName} = {action.Action}(";
            for (int i = 0; i < Fields.Count; i++)
            {
                var field = Fields[i];
                if (i != 0)
                    line += ", ";
                line += field.GetVariable();
            }
            line += ");";
            return line;
        }
        return $"// Node {Name} has an unknown action type: {action.Type}.";
    }

    public void ResetValueReferences()
    {
        foreach (var field in Fields)
        {
            field.Value.ResetValueReferences();
        }
    }

    public void SetValueReferences(List<float> values, ref int index)
    {
        foreach (var field in Fields)
        {
            if (!field.IsConnected())
                continue;

            field.Value.SetValueReferences(values, ref index);
        }
    }

    /* Abstract methods */
    public abstract void InitializeNodeData();

    private void SetOldMousePosition()
    {
        _oldMouseButtonPosition = Input.GetMousePosition();
    }

    private void MoveNode()
    {
        if (Input.GetMouseDelta() == Vector2.Zero)
            return;

        Vector2 mousePosition = Input.GetMousePosition();
        Vector2 delta = (mousePosition - _oldMouseButtonPosition) * (1 / Collection.UIController.Scale);

        Collection.SetOffset(Collection.Offset + new Vector4(delta.X, delta.Y, 0, 0));
        Collection.Align();
        Collection.UpdateTransformation();
        SetOldMousePosition();
        NNS_NodeManager.UpdateLines();
    }

    public void DeleteNode()
    {
        foreach (var input in Inputs)
        {
            input.Disconnect();
        }
        Inputs = [];

        foreach (var output in Outputs)
        {
            output.Disconnect();
        }
        Outputs = [];

        Fields = [];
        Collection.Delete();
    }

    public static void FinalClear()
    {
        foreach (var template in NodeTemplates.Values)
        {
            template.Clear();
        }
        NodeTemplates.Clear();     
    }

    private class NNS_NodeTemplate
    {
        public string NodeType = "";

        public List<NNS_NodeInputFieldStruct> RegisteredInputFields = [];
        public List<NNS_NodeOutputFieldStruct> RegisteredOutputFields = [];
        public Dictionary<string, NNS_NodeActionStruct> RegisteredActions = [];

        public NNS_NodeTemplate(string nodeType)
        {
            NodeType = nodeType;
        }

        /* Registration */
        public void RegisterInputField(string identifier, bool hasInput, object value, bool hasDefaultValue = true) => RegisteredInputFields.Add(new NNS_NodeInputFieldStruct(identifier, hasInput, value, hasDefaultValue));
        public void RegisterOutputField(string identifier, NNS_ValueType type) => RegisteredOutputFields.Add(new NNS_NodeOutputFieldStruct(identifier, type));
        public void RegisterAction(string name, string type, string action)
        {
            if (RegisteredActions.ContainsKey(name))
            {
                throw new InvalidOperationException($"Action with name '{name}' is already registered.");
            }
            RegisteredActions.Add(name, new NNS_NodeActionStruct(name, type, action));
        }

        public NNS_NodeActionStruct GetAction(string type)
        {
            if (RegisteredActions.Count == 0)
            {
                throw new InvalidOperationException($"No actions registered for node type '{NodeType}'.");
            }
            if (RegisteredActions.Count == 1)
            {
                return RegisteredActions.Values.First();
            }
            if (RegisteredActions.TryGetValue(type, out var action))
            {
                return action;
            }
            throw new InvalidOperationException($"No action registered for type '{type}' in node type '{NodeType}'.");
        }



        public void Clear()
        {
            RegisteredInputFields.Clear();
            RegisteredOutputFields.Clear();
            RegisteredActions.Clear();
        }

        /* Data */
        public struct NNS_NodeInputFieldStruct(string identifier, bool hasInput, object value, bool hasDefaultValue = true)
        {
            public string Identifier = identifier;
            public bool HasInput = hasInput;
            public object Value = value;
            public bool HasDefaultValue = hasDefaultValue;
        }

        public struct NNS_NodeOutputFieldStruct(string identifier, NNS_ValueType type)
        {
            public string Identifier = identifier;
            public NNS_ValueType Type = type;
        }

        public struct NNS_NodeActionStruct(string name, string type, string operation)
        {
            public string Name = name;
            public string Type = type;
            public string Action = operation;
        }
    }
}

public enum NNS_ValueType
{
    Float,
    Int,
    Vector2,
    Vector2i,
    Vector3,
    Vector3i
}