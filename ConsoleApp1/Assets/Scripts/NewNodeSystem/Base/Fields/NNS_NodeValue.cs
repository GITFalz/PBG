using OpenTK.Mathematics;

public interface NNS_NodeValue
{
    void ResetValueReferences();
    void SetValueReferences(List<float> values, ref int index);
    string GetVariable();
    UICollection GetInputFields(UIController controller);

    public static NNS_NodeValue Get(object value)
    {
        if (value is float f)
            return new NNS_NodeValue_Float(f);
        if (value is int i)
            return new NNSNodeValueInt(i);
        if (value is Vector2 v2)
            return new NNS_NodeValue_Vector2(v2.X, v2.Y);
        if (value is Vector2i v2i)
            return new NNS_NodeValue_Vector2Int(v2i.X, v2i.Y);
        if (value is Vector3 v3)
            return new NNS_NodeValue_Vector3(v3.X, v3.Y, v3.Z);
        if (value is Vector3i v3i)
            return new NNS_NodeValue_Vector3Int(v3i.X, v3i.Y, v3i.Z);
        throw new ArgumentException($"Unsupported type: {value.GetType()}");
    }

    public static NNS_ValueType GetValueType(object value)
    {
        if (value is float)
            return NNS_ValueType.Float;
        if (value is int)
            return NNS_ValueType.Int;
        if (value is Vector2)
            return NNS_ValueType.Vector2;
        if (value is Vector2i)
            return NNS_ValueType.Vector2i;
        if (value is Vector3)
            return NNS_ValueType.Vector3;
        if (value is Vector3i)
            return NNS_ValueType.Vector3i;
        throw new ArgumentException($"Unsupported type: {value.GetType()}");
    }

    public static object GetDefaultValue(NNS_ValueType type)
    {
        return type switch
        {
            NNS_ValueType.Float => 0f,
            NNS_ValueType.Int => 0,
            NNS_ValueType.Vector2 => new Vector2(0, 0),
            NNS_ValueType.Vector2i => new Vector2i(0, 0),
            NNS_ValueType.Vector3 => new Vector3(0, 0, 0),
            NNS_ValueType.Vector3i => new Vector3i(0, 0, 0),
            _ => throw new ArgumentException($"Unsupported value type: {type}")
        };
    }

    public static string GetGLSLType(NNS_ValueType type)
    {
        return type switch
        {
            NNS_ValueType.Float => "float",
            NNS_ValueType.Int => "int",
            NNS_ValueType.Vector2 => "vec2",
            NNS_ValueType.Vector2i => "ivec2",
            NNS_ValueType.Vector3 => "vec3",
            NNS_ValueType.Vector3i => "ivec3",
            _ => throw new ArgumentException($"Unsupported value type: {type}")
        };
    }
}

public class NNS_NodeValue_Float(float value) : NNS_NodeValue
{
    public float Value = value;
    private float _defaultValue = value; // Store the default value for reset functionality
    public int Index = -1;

    public void ResetValueReferences()
    {
        Index = -1;
    }

    public void SetValueReferences(List<float> values, ref int index)
    {
        Index = index;
        values.Add(Value);
        index++;
    }
    
    public string GetVariable()
    {
        return $"values[{Index}]";
    }

    public UICollection GetInputFields(UIController controller)
    {
        UICollection collection = new UICollection("Float Input Fields", controller, AnchorType.TopLeft, PositionType.Relative) { Scale = (0, 20) };

        UIInputField valueInput = new UIInputField("Value Input", controller, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (100, 20), (5, 0, 0, 0), 0, 11, (7.5f, 0.05f));
        valueInput.SetTextType(TextType.Decimal).SetMaxCharCount(8).SetText(Value.ToString(), 0.8f);
        valueInput.SetOnTextChange(() => NNS_NodeHelper.SetValue(ref Value, valueInput, _defaultValue, Index));
        valueInput.SetOnHold(() => NNS_NodeHelper.SetSlideValue(ref Value, valueInput, NNS_NodeHelper.SlideSpeed, Index));

        UIImage valueBackground = new UIImage("Value Background", controller, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (valueInput.ScaleX + 10, 20), (0, 0, 0, 0), 0, 11, (7.5f, 0.05f));

        collection.ScaleX = valueInput.ScaleX + 10;
        collection.AddElements(valueBackground, valueInput);
        return collection;
    }
}

public class NNSNodeValueInt(int value) : NNS_NodeValue
{
    public int Value = value;
    private int _defaultValue = value; // Store the default value for reset functionality
    public int Index = -1;

    public void ResetValueReferences()
    {
        Index = -1;
    }

    public void SetValueReferences(List<float> values, ref int index)
    {
        Index = index;
        values.Add(Value);
        index++;
    }
    
    public string GetVariable()
    {
        return $"int(values[{Index}])";
    }

    public UICollection GetInputFields(UIController controller)
    {
        UICollection collection = new UICollection("Float Input Fields", controller, AnchorType.TopLeft, PositionType.Relative) { Scale = (0, 20) };

        UIInputField valueInput = new UIInputField("Value Input", controller, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (100, 20), (5, 0, 0, 0), 0, 11, (7.5f, 0.05f));
        valueInput.SetTextType(TextType.Numeric).SetMaxCharCount(8).SetText(Value.ToString(), 0.8f);
        valueInput.SetOnTextChange(() => NNS_NodeHelper.SetValue(ref Value, valueInput, _defaultValue, Index));
        valueInput.SetOnHold(() => NNS_NodeHelper.SetSlideValue(ref Value, valueInput, NNS_NodeHelper.SlideSpeed, Index));

        UIImage valueBackground = new UIImage("Value Background", controller, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (valueInput.ScaleX + 10, 20), (0, 0, 0, 0), 0, 11, (7.5f, 0.05f));

        collection.ScaleX = valueInput.ScaleX + 10;
        collection.AddElements(valueBackground, valueInput);
        return collection;
    }
}

public class NNS_NodeValue_Vector2(float x, float y) : NNS_NodeValue
{
    public float X = x;
    public float Y = y;
    private float _defaultX = x; // Store the default value for reset functionality
    private float _defaultY = y; // Store the default value for reset functionality
    public int Index1 = -1;
    public int Index2 = -1;

    public void ResetValueReferences()
    {
        Index1 = -1;
        Index2 = -1;
    }

    public void SetValueReferences(List<float> values, ref int index)
    {
        Index1 = index;
        values.Add(X);
        index++;
        Index2 = index;
        values.Add(Y);
        index++;
    }
    
    public string GetVariable()
    {
        return $"vec2(values[{Index1}], values[{Index2}])";
    }

    public UICollection GetInputFields(UIController controller)
    {
        UICollection collection = new UICollection("Vector2 Input Fields", controller, AnchorType.TopLeft, PositionType.Relative) { Scale = (0, 20) };

        UIInputField xInput = new UIInputField("X Input", controller, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (100, 20), (5, 0, 0, 0), 0, 11, (7.5f, 0.05f));
        xInput.SetTextType(TextType.Decimal).SetMaxCharCount(8).SetText(X.ToString(), 0.8f);
        xInput.SetOnTextChange(() => NNS_NodeHelper.SetValue(ref X, xInput, _defaultX, Index1));
        xInput.SetOnHold(() => NNS_NodeHelper.SetSlideValue(ref X, xInput, NNS_NodeHelper.SlideSpeed, Index1));

        UIInputField yInput = new UIInputField("Y Input", controller, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (100, 20), (xInput.ScaleX + 20, 0, 0, 0), 0, 11, (7.5f, 0.05f));
        yInput.SetTextType(TextType.Decimal).SetMaxCharCount(8).SetText(Y.ToString(), 0.8f);
        yInput.SetOnTextChange(() => NNS_NodeHelper.SetValue(ref Y, yInput, _defaultY, Index2));
        yInput.SetOnHold(() => NNS_NodeHelper.SetSlideValue(ref Y, yInput, NNS_NodeHelper.SlideSpeed, Index2));

        UIImage xBackground = new UIImage("X Background", controller, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (xInput.ScaleX + 10, 20), (0, 0, 0, 0), 0, 11, (7.5f, 0.05f));
        UIImage yBackground = new UIImage("Y Background", controller, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (yInput.ScaleX + 10, 20), (xInput.ScaleX + 15, 0, 0, 0), 0, 11, (7.5f, 0.05f));

        collection.ScaleX = xInput.ScaleX * 2 + 25;
        collection.AddElements(xBackground, xInput, yBackground, yInput);
        return collection;
    }
}

public class NNS_NodeValue_Vector2Int(int x, int y) : NNS_NodeValue
{
    public int X = x;
    public int Y = y;
    private int _defaultX = x; // Store the default value for reset functionality
    private int _defaultY = y; // Store the default value for reset functionality
    public int Index1 = -1;
    public int Index2 = -1;

    public void ResetValueReferences()
    {
        Index1 = -1;
        Index2 = -1;
    }

    public void SetValueReferences(List<float> values, ref int index)
    {
        Index1 = index;
        values.Add(X);
        index++;
        Index2 = index;
        values.Add(Y);
        index++;
    }
    
    public string GetVariable()
    {
        return $"ivec2(int(values[{Index1}]), int(values[{Index2}]))";
    }

    public UICollection GetInputFields(UIController controller)
    {
        UICollection collection = new UICollection("Vector2 Input Fields", controller, AnchorType.TopLeft, PositionType.Relative) { Scale = (0, 20) };

        UIInputField xInput = new UIInputField("X Input", controller, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (100, 20), (5, 0, 0, 0), 0, 11, (7.5f, 0.05f));
        xInput.SetTextType(TextType.Numeric).SetMaxCharCount(8).SetText(X.ToString(), 0.8f);
        xInput.SetOnTextChange(() => NNS_NodeHelper.SetValue(ref X, xInput, _defaultX, Index1));
        xInput.SetOnHold(() => NNS_NodeHelper.SetSlideValue(ref X, xInput, NNS_NodeHelper.SlideSpeed, Index1));

        UIInputField yInput = new UIInputField("Y Input", controller, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (100, 20), (xInput.ScaleX + 20, 0, 0, 0), 0, 11, (7.5f, 0.05f));
        yInput.SetTextType(TextType.Numeric).SetMaxCharCount(8).SetText(Y.ToString(), 0.8f);
        yInput.SetOnTextChange(() => NNS_NodeHelper.SetValue(ref Y, yInput, _defaultY, Index2));
        yInput.SetOnHold(() => NNS_NodeHelper.SetSlideValue(ref Y, yInput, NNS_NodeHelper.SlideSpeed, Index2));

        UIImage xBackground = new UIImage("X Background", controller, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (xInput.ScaleX + 10, 20), (0, 0, 0, 0), 0, 11, (7.5f, 0.05f));
        UIImage yBackground = new UIImage("Y Background", controller, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (xInput.ScaleX + 10, 20), (xInput.ScaleX + 15, 0, 0, 0), 0, 11, (7.5f, 0.05f));

        collection.ScaleX = xInput.ScaleX * 2 + 25;
        collection.AddElements(xBackground, xInput, yBackground, yInput);
        return collection;
    }
}

public class NNS_NodeValue_Vector3(float x, float y, float z) : NNS_NodeValue
{
    public float X = x;
    public float Y = y;
    public float Z = z;
    private float _defaultX = x; // Store the default value for reset functionality
    private float _defaultY = y; // Store the default value for reset functionality
    private float _defaultZ = z; // Store the default value for reset functionality
    public int Index1 = -1;
    public int Index2 = -1;
    public int Index3 = -1;

    public void ResetValueReferences()
    {
        Index1 = -1;
        Index2 = -1;
        Index3 = -1;
    }

    public void SetValueReferences(List<float> values, ref int index)
    {
        Index1 = index;
        values.Add(X);
        index++;
        Index2 = index;
        values.Add(Y);
        index++;
        Index3 = index;
        values.Add(Z);
        index++;
    }
    
    public string GetVariable()
    {
        return $"vec3(values[{Index1}], values[{Index2}], values[{Index3}])";
    }

    public UICollection GetInputFields(UIController controller)
    {
        UICollection collection = new UICollection("Vector3 Input Fields", controller, AnchorType.TopLeft, PositionType.Relative) { Scale = (0, 20) };

        UIInputField xInput = new UIInputField("X Input", controller, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (100, 20), (5, 0, 0, 0), 0, 11, (7.5f, 0.05f));
        xInput.SetTextType(TextType.Decimal).SetMaxCharCount(8).SetText(X.ToString(), 0.8f);
        xInput.SetOnTextChange(() => NNS_NodeHelper.SetValue(ref X, xInput, _defaultX, Index1));
        xInput.SetOnHold(() => NNS_NodeHelper.SetSlideValue(ref X, xInput, NNS_NodeHelper.SlideSpeed, Index1));

        UIInputField yInput = new UIInputField("Y Input", controller, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (100, 20), (xInput.ScaleX + 20, 0, 0, 0), 0, 11, (7.5f, 0.05f));
        yInput.SetTextType(TextType.Decimal).SetMaxCharCount(8).SetText(Y.ToString(), 0.8f);
        yInput.SetOnTextChange(() => NNS_NodeHelper.SetValue(ref Y, yInput, _defaultY, Index2));
        yInput.SetOnHold(() => NNS_NodeHelper.SetSlideValue(ref Y, yInput, NNS_NodeHelper.SlideSpeed, Index2));

        UIInputField zInput = new UIInputField("Z Input", controller, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (100, 20), (xInput.ScaleX * 2 + 35, 0, 0, 0), 0, 11, (7.5f, 0.05f));
        zInput.SetTextType(TextType.Decimal).SetMaxCharCount(8).SetText(Z.ToString(), 0.8f);
        zInput.SetOnTextChange(() => NNS_NodeHelper.SetValue(ref Z, zInput, _defaultZ, Index3));
        zInput.SetOnHold(() => NNS_NodeHelper.SetSlideValue(ref Z, zInput, NNS_NodeHelper.SlideSpeed, Index3));

        UIImage xBackground = new UIImage("X Background", controller, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (xInput.ScaleX + 10, 20), (0, 0, 0, 0), 0, 11, (7.5f, 0.05f));
        UIImage yBackground = new UIImage("Y Background", controller, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (xInput.ScaleX + 10, 20), (xInput.ScaleX + 15, 0, 0, 0), 0, 11, (7.5f, 0.05f));
        UIImage zBackground = new UIImage("Z Background", controller, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (xInput.ScaleX + 10, 20), (xInput.ScaleX * 2 + 30, 0, 0, 0), 0, 11, (7.5f, 0.05f));

        collection.ScaleX = xInput.ScaleX * 3 + 40;
        collection.AddElements(xBackground, xInput, yBackground, yInput, zBackground, zInput);
        return collection;
    }
}

public class NNS_NodeValue_Vector3Int(int x, int y, int z) : NNS_NodeValue
{
    public int X = x;
    public int Y = y;
    public int Z = z;
    private int _defaultX = x; // Store the default value for reset functionality
    private int _defaultY = y; // Store the default value for reset functionality
    private int _defaultZ = z; // Store the default value for reset functionality
    public int Index1 = -1;
    public int Index2 = -1;
    public int Index3 = -1;

    public void ResetValueReferences()
    {
        Index1 = -1;
        Index2 = -1;
        Index3 = -1;
    }

    public void SetValueReferences(List<float> values, ref int index)
    {
        Index1 = index;
        values.Add(X);
        index++;
        Index2 = index;
        values.Add(Y);
        index++;
        Index3 = index;
        values.Add(Z);
        index++;
    }
    
    public string GetVariable()
    {
        return $"ivec3(int(values[{Index1}]), int(values[{Index2}]), int(values[{Index3}]))";
    }

    public UICollection GetInputFields(UIController controller)
    {
        UICollection collection = new UICollection("Vector3 Input Fields", controller, AnchorType.TopLeft, PositionType.Relative) { Scale = (0, 20) };

        UIInputField xInput = new UIInputField("X Input", controller, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (100, 20), (5, 0, 0, 0), 0, 11, (7.5f, 0.05f));
        xInput.SetTextType(TextType.Numeric).SetMaxCharCount(8).SetText(X.ToString(), 0.8f);
        xInput.SetOnTextChange(() => NNS_NodeHelper.SetValue(ref X, xInput, _defaultX, Index1));
        xInput.SetOnHold(() => NNS_NodeHelper.SetSlideValue(ref X, xInput, NNS_NodeHelper.SlideSpeed, Index1));

        UIInputField yInput = new UIInputField("Y Input", controller, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (100, 20), (xInput.ScaleX + 20, 0, 0, 0), 0, 11, (7.5f, 0.05f));
        yInput.SetTextType(TextType.Numeric).SetMaxCharCount(8).SetText(Y.ToString(), 0.8f);
        yInput.SetOnTextChange(() => NNS_NodeHelper.SetValue(ref Y, yInput, _defaultY, Index2));
        yInput.SetOnHold(() => NNS_NodeHelper.SetSlideValue(ref Y, yInput, NNS_NodeHelper.SlideSpeed, Index2));

        UIInputField zInput = new UIInputField("Z Input", controller, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (100, 20), (xInput.ScaleX * 2 + 35, 0, 0, 0), 0, 11, (7.5f, 0.05f));
        zInput.SetTextType(TextType.Numeric).SetMaxCharCount(8).SetText(Z.ToString(), 0.8f);
        zInput.SetOnTextChange(() => NNS_NodeHelper.SetValue(ref Z, zInput, _defaultZ, Index3));
        zInput.SetOnHold(() => NNS_NodeHelper.SetSlideValue(ref Z, zInput, NNS_NodeHelper.SlideSpeed, Index3));

        UIImage xBackground = new UIImage("X Background", controller, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (xInput.ScaleX + 10, 20), (0, 0, 0, 0), 0, 11, (7.5f, 0.05f));
        UIImage yBackground = new UIImage("Y Background", controller, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (xInput.ScaleX + 10, 20), (xInput.ScaleX + 15, 0, 0, 0), 0, 11, (7.5f, 0.05f));
        UIImage zBackground = new UIImage("Z Background", controller, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (xInput.ScaleX + 10, 20), (xInput.ScaleX * 2 + 30, 0, 0, 0), 0, 11, (7.5f, 0.05f));

        collection.ScaleX = xInput.ScaleX * 3 + 40;
        collection.AddElements(xBackground, xInput, yBackground, yInput, zBackground, zInput);
        return collection;
    }
}