﻿using OpenTK.Mathematics;

public static class UiLoader
{
    public static string sceneName = "";
    public static void Load(UIController controller, string[] lines)
    {
        sceneName = lines[0].Split(":")[1].Trim();
        
        controller.ClearUiMesh();
        
        for (int i = 2; i < lines.Length; i++)
        {
            string line = lines[i];
            
            Console.WriteLine("Line: " + line);
            
            UiElement? element = null;
            
            if (line.Trim() == "Static Panel")
            {
                Console.WriteLine("Panel");
                i = TextToPanel(lines, i, controller, out var staticPanel);
                controller.SetParentPanel(staticPanel);
                controller.AddStaticElement(staticPanel);
                element = staticPanel;
            }
            else if (line.Trim() == "Static Button")
            {
                i = TextToButton(lines, i, out var staticButton);
                controller.AddStaticElement(staticButton);
                element = staticButton;
            }
            else if (line.Trim() == "Static Input Field")
            {
                i = TextToInputField(lines, i, out var staticInputField);
                controller.AddStaticElement(staticInputField);
                element = staticInputField;
            }
            else if (line.Trim() == "Static Text")
            {
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
    
    public static int TextToPanel(string[] lines, int index, UIController controller, out StaticPanel panel)
    {
        index+=2;
        
        Console.WriteLine(index + " " + lines[index]);

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
        
        index += 7;
        
        int elements = int.Parse(lines[index].Split(":")[1].Trim());
        if (elements >= 1)
        {
            for (int i = 0; i < elements; i++)
            {
                string line = lines[index + 1];
                
                UiElement? element = null;
                
                if (line.Trim() == "Static Panel")
                {
                    index = TextToPanel(lines, index + 1, controller, out var staticPanel);
                    controller.SetParentPanel(staticPanel);
                    panel.AddElement(staticPanel);
                    element = staticPanel;
                }
                if (line.Trim() == "Static Button")
                {
                    index = TextToButton(lines, index + 1, out var button);
                    Console.WriteLine("Button added: " + button);
                    panel.AddElement(button);
                    element = button;
                }
                else if (line.Trim() == "Static Input Field")
                {
                    index = TextToInputField(lines, index + 1, out var inputField);
                    Console.WriteLine("Input Field added: " + inputField);
                    panel.AddElement(inputField);
                    element = inputField;
                }
                else if (line.Trim() == "Static Text")
                {
                    index = TextToStaticText(lines, index + 1, out var staticText);
                    Console.WriteLine("Static Text added: " + staticText);
                    panel.AddElement(staticText);
                    element = staticText;
                }
                
                if (element != null)
                {
                    element.SceneName = sceneName;
                }
            }
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
        string text = lines[index + 1].Split(new[] { ": " }, 2, StringSplitOptions.None)[1].Trim();
        float fontSize = float.Parse(lines[index + 2].Split(":")[1].Trim());
        Vector3 position = TextToVector3(lines[index + 3].Split(":")[1].Trim());
        int textureIndex = int.Parse(lines[index + 8].Split(":")[1].Trim());
        
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
        inputField.TextureIndex = textureIndex;
        
        string targetMethod = lines[index + 9].Split(":")[1].Trim();
        if (targetMethod != "null")
        {
            inputField.OnClick = new SerializableEvent();
            if (!BindButton(inputField.OnClick, targetMethod))
                Console.WriteLine("Failed to bind button click event: " + targetMethod);
        }
        targetMethod = lines[index + 10].Split(":")[1].Trim();
        if (targetMethod != "null")
        {
            inputField.OnHover = new SerializableEvent();
            if (!BindButton(inputField.OnHover, targetMethod))
                Console.WriteLine("Failed to bind button hover event: " + targetMethod);
        }
        targetMethod = lines[index + 11].Split(":")[1].Trim();
        if (targetMethod != "null")
        {
            inputField.OnHold = new SerializableEvent();
            if (!BindButton(inputField.OnHold, targetMethod))
                Console.WriteLine("Failed to bind button hold event: " + targetMethod);
        }
        targetMethod = lines[index + 12].Split(":")[1].Trim();
        if (targetMethod != "null")
        {
            inputField.OnRelease = new SerializableEvent();
            if (!BindButton(inputField.OnRelease, targetMethod))
                Console.WriteLine("Failed to bind button release event: " + targetMethod);
        }

        index += 13;
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

        if (targetFunction.Contains('('))
        {
            string[] function = targetFunction.Split(['(', ')']);
            targetFunction = function[0];
            string parameter = function[1];
            
            Scene? scene = Game.Instance.GetScene(sceneName);
            if (scene == null)
                return false;
            if (!scene.GetClass(targetClass, out object? component))
                return false;
            buttonEvent.BindMethod(targetFunction, component, parameter);
            return true;
        }
        else
        {
            Scene? scene = Game.Instance.GetScene(sceneName);
            if (scene == null)
                return false;
            if (!scene.GetClass(targetClass, out object? component))
                return false;
            buttonEvent.BindMethod(targetFunction, component);
            return true;
        }
    }
}