using OpenTK.Mathematics;

public static class UiLoader
{
    public static string sceneName = "";
    public static void Load(OldUIController controller, string[] lines)
    {
        sceneName = lines[0].Split(":")[1].Trim();
        
        controller.ClearUiMesh();
        
        for (int i = 2; i < lines.Length; i++)
        {
            string line = lines[i];
            
            UiElement? element = null;
            
            if (line.Trim() == "Static Panel")
            {
                Console.WriteLine("Static panel found: " + i);
                i = TextToPanel(lines, i, controller, out var staticPanel);
                //controller.SetParentPanel(staticPanel);
                controller.AddStaticElement(staticPanel);
                element = staticPanel;
            }
            else if (line.Trim() == "Static Button")
            {
                Console.WriteLine("Static button found: " + i);
                i = TextToButton(lines, i, out var staticButton);
                controller.AddStaticElement(staticButton);
                element = staticButton;
            }
            else if (line.Trim() == "Static Input Field")
            {
                Console.WriteLine("Static input field found: " + i);
                i = TextToInputField(lines, i, out var staticInputField);
                controller.AddStaticElement(staticInputField);
                element = staticInputField;
            }
            else if (line.Trim() == "Static Text")
            {
                Console.WriteLine("Static text found: " + i);
                i = TextToStaticText(lines, i, out var staticText);
                controller.AddStaticElement(staticText);
                element = staticText;
            }
            
            if (element != null)
            {
                element.SceneName = sceneName;
            }
        }
        
        controller.Generate();
    }
    
    public static int TextToPanel(string[] lines, int index, OldUIController controller, out StaticPanel panel)
    {
        index+=2;
        
        string name = lines[index].Split(":")[1].Trim();
        Vector3 position = TextToVector3(lines[index + 1].Split(":")[1].Trim());
        int textureIndex = int.Parse(lines[index + 6].Split(":")[1].Trim());
        
        panel = UI.CreateStaticPanel(
            name,
            (AnchorType) int.Parse(lines[index + 4].Split(":")[1].Trim()), 
            (PositionType) int.Parse(lines[index + 5].Split(":")[1].Trim()), 
            TextToVector3(lines[index + 2].Split(":")[1].Trim()), 
            TextToVector4(lines[index + 3].Split(":")[1].Trim()), 
            null
        );
        panel.Position = position;
        panel.TextureIndex = textureIndex;
        
        controller.SetParentPanel(panel);
        
        int elements = int.Parse(lines[index + 7].Split(":")[1].Trim());
        
        index += 8;
        
        if (elements == 0)
            return index;
        
        // While not end of lines
        while (lines.Length > index)
        {
            string line = lines[index];
                
            Console.WriteLine("Line: " + line);
                
            UiElement? element = null;
                
            if (line.Trim() == "Static Panel")
            {
                Console.WriteLine("Static panel found: " + index);
                index = TextToPanel(lines, index, controller, out var staticPanel);
                controller.SetParentPanel(panel);
                panel.AddElement(staticPanel);
                element = staticPanel;
            }
            if (line.Trim() == "Static Button")
            {
                Console.WriteLine("Static button found: " + index);
                index = TextToButton(lines, index, out var button);
                panel.AddElement(button);
                element = button;
            }
            else if (line.Trim() == "Static Input Field")
            {
                Console.WriteLine("Static input field found: " + index);
                index = TextToInputField(lines, index, out var inputField);
                panel.AddElement(inputField);
                element = inputField;
            }
            else if (line.Trim() == "Static Text")
            {
                Console.WriteLine("Static text found: " + index);
                index = TextToStaticText(lines, index, out var staticText);
                panel.AddElement(staticText);
                element = staticText;
            }
                
            if (element != null)
            {
                Console.WriteLine("The element is called: " + element.Name);
                element.SceneName = sceneName;
            }

            index++;
        }
        return index;
    }

    public static int TextToButton(string[] lines, int index, out StaticButton button)
    {
        index += 2;

        string name = lines[index].Split(":")[1].Trim();
        Vector3 position = TextToVector3(lines[index + 1].Split(":")[1].Trim());
        int textureIndex = int.Parse(lines[index + 6].Split(":")[1].Trim());

        button = UI.CreateStaticButton(
            name,
            (AnchorType) int.Parse(lines[index + 4].Split(":")[1].Trim()), 
            (PositionType) int.Parse(lines[index + 5].Split(":")[1].Trim()), 
            TextToVector3(lines[index + 2].Split(":")[1].Trim()), 
            TextToVector4(lines[index + 3].Split(":")[1].Trim()), 
            null
        );

        button.Position = position;
        button.TextureIndex = textureIndex;
        
        string targetMethod = lines[index + 7].Split(":")[1].Trim();
        if (targetMethod != "null")
        {
            button.OnClick = new SerializableEvent();
            if (!BindButton(button.OnClick, targetMethod))
                Console.WriteLine("Failed to bind button click event: " + targetMethod);
        }
        targetMethod = lines[index + 8].Split(":")[1].Trim();
        if (targetMethod != "null")
        {
            button.OnHover = new SerializableEvent();
            if (!BindButton(button.OnHover, targetMethod))
                Console.WriteLine("Failed to bind button hover event: " + targetMethod);
        }
        targetMethod = lines[index + 9].Split(":")[1].Trim();
        if (targetMethod != "null")
        {
            button.OnHold = new SerializableEvent();
            if (!BindButton(button.OnHold, targetMethod))
                Console.WriteLine("Failed to bind button hold event: " + targetMethod);
        }
        targetMethod = lines[index + 10].Split(":")[1].Trim();
        if (targetMethod != "null")
        {
            button.OnRelease = new SerializableEvent();
            if (!BindButton(button.OnRelease, targetMethod))
                Console.WriteLine("Failed to bind button release event: " + targetMethod);
        }

        index += 11;
        return index;
    }
    
    public static int TextToInputField(string[] lines, int index, out StaticInputField inputField)
    {
        index += 2;
        
        string name = lines[index].Split(":")[1].Trim();
        string text = "";
        try {
            text = lines[index + 1].Split(new[] { ": " }, 2, StringSplitOptions.None)[1].Trim();
        }
        catch (Exception e) {
            Console.WriteLine("Setting inputField to default");
        }
        float fontSize = float.Parse(lines[index + 2].Split(":")[1].Trim());
        Vector3 position = TextToVector3(lines[index + 3].Split(":")[1].Trim());
        TextType textType = (TextType)int.Parse(lines[index + 8].Split(":")[1].Trim());
        int textureIndex = int.Parse(lines[index + 9].Split(":")[1].Trim());
        
        inputField = UI.CreateStaticInputField(
            name,
            text,
            fontSize,
            (AnchorType) int.Parse(lines[index + 6].Split(":")[1].Trim()), 
            (PositionType) int.Parse(lines[index + 7].Split(":")[1].Trim()), 
            TextToVector3(lines[index + 4].Split(":")[1].Trim()), 
            TextToVector4(lines[index + 5].Split(":")[1].Trim())
        );

        inputField.Position = position;
        inputField.TextType = textType;
        inputField.TextureIndex = textureIndex;
        
        string targetMethod = lines[index + 10].Split(":")[1].Trim();
        if (targetMethod != "null")
        {
            inputField.OnClick = new SerializableEvent();
            if (!BindButton(inputField.OnClick, targetMethod))
                Console.WriteLine("Failed to bind button click event: " + targetMethod);
        }
        targetMethod = lines[index + 11].Split(":")[1].Trim();
        if (targetMethod != "null")
        {
            inputField.OnHover = new SerializableEvent();
            if (!BindButton(inputField.OnHover, targetMethod))
                Console.WriteLine("Failed to bind button hover event: " + targetMethod);
        }
        targetMethod = lines[index + 12].Split(":")[1].Trim();
        if (targetMethod != "null")
        {
            inputField.OnHold = new SerializableEvent();
            if (!BindButton(inputField.OnHold, targetMethod))
                Console.WriteLine("Failed to bind button hold event: " + targetMethod);
        }
        targetMethod = lines[index + 13].Split(":")[1].Trim();
        if (targetMethod != "null")
        {
            inputField.OnRelease = new SerializableEvent();
            if (!BindButton(inputField.OnRelease, targetMethod))
                Console.WriteLine("Failed to bind button release event: " + targetMethod);
        }
        targetMethod = lines[index + 14].Split(":")[1].Trim();
        if (targetMethod != "null")
        {
            inputField.OnTextChange = new SerializableEvent();
            if (!BindButton(inputField.OnTextChange, targetMethod))
                Console.WriteLine("Failed to bind button on text change event: " + targetMethod);
        }

        index += 15;
        return index;
    }

    public static int TextToStaticText(string[] lines, int index, out StaticText staticText)
    {
        index+=2;

        string name = lines[index].Split(":")[1].Trim();
        string text = lines[index + 1].Split(new[] { ": " }, 2, StringSplitOptions.None)[1].Trim();
        float fontSize = float.Parse(lines[index + 2].Split(":")[1].Trim());
        Vector3 position = TextToVector3(lines[index + 3].Split(":")[1].Trim());
        
        staticText = UI.CreateStaticText(
            name,
            text,
            fontSize,
            (AnchorType) int.Parse(lines[index + 6].Split(":")[1].Trim()), 
            (PositionType) int.Parse(lines[index + 7].Split(":")[1].Trim()), 
            TextToVector3(lines[index + 4].Split(":")[1].Trim()), 
            TextToVector4(lines[index + 5].Split(":")[1].Trim())
        );
        staticText.Position = position;
        
        index += 8;
        return index;
    }

    public static float[] TextToFloatArray(string value)
    {
        return value.Trim('(', ')').Split(',').Select(x => float.Parse(x.Trim())).ToArray();
    }

    public static Vector3 TextToVector3(string value)
    {
        var vectorParts = TextToFloatArray(value);
        return new Vector3(vectorParts[0], vectorParts[1], vectorParts[2]);
    }

    public static Vector2 TextToVector2(string value)
    {
        var vectorParts = TextToFloatArray(value);
        return new Vector2(vectorParts[0], vectorParts[1]);
    }

    public static Vector4 TextToVector4(string value)
    {
        var vectorParts = TextToFloatArray(value);
        return new Vector4(vectorParts[0], vectorParts[1], vectorParts[2], vectorParts[3]);
    }
    
    public static bool BindButton(SerializableEvent buttonEvent, string targetMethod)
    {
        string[] target = targetMethod.Split('.');
        string sceneName = target[0];
        string targetClass = target[1];
        string targetFunction = target[2];
        
        Console.WriteLine("Scene: " + sceneName);
        Console.WriteLine("Class: " + targetClass);
        Console.WriteLine("Function: " + targetFunction);
        
        bool isStatic = sceneName == "Static";
        
        Console.WriteLine("Is static: " + isStatic);

        if (targetFunction.Contains('('))
        {
            string[] function = targetFunction.Split(['(', ')']);
            targetFunction = function[0];
            string parameter = function[1];

            if (isStatic) 
                return buttonEvent.BindStaticMethod(targetFunction, targetClass, parameter);
            
            Scene? scene = Game.Instance.GetScene(sceneName);
            if (scene == null) return false;
            return scene.GetClass(targetClass, out object? component) && buttonEvent.BindMethod(targetFunction, component, parameter);
        }
        else
        {
            if (isStatic) 
                return buttonEvent.BindStaticMethod(targetFunction, targetClass);
            
            Scene? scene = Game.Instance.GetScene(sceneName);

            scene.GetClass(targetClass, out object? c);
            Console.WriteLine("Scene: " + (c == null));
            
            if (scene == null) return false;
            return scene.GetClass(targetClass, out object? component) && buttonEvent.BindMethod(targetFunction, component);
        }
    }
}