using OpenTK.Mathematics;

public static class UiLoader
{
    public static string sceneName = "";
    
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