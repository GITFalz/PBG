using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public static class NoiseGlslNodeManager
{
    public static ShaderProgram DisplayShaderProgram = new ShaderProgram("Utils/Rectangle.vert", "Noise/WorldNoise_copy.frag");

    private static VAO _displayVAO = new VAO();

    private static int modelLocation = -1;
    private static int projectionLocation = -1;
    private static int sizeLocation = -1;
    private static int noiseSizeLocation = -1;
    private static int offsetLocation = -1;

    public static string NoiseFragmentFile = "Noise/WorldNoise.frag";
    private static string NoiseFragmentPath;
    private static string NoiseFragmentPathCopy;

    private static int _lineInsert = 153;

    static NoiseGlslNodeManager()
    {
        modelLocation = DisplayShaderProgram.GetLocation("model");
        projectionLocation = DisplayShaderProgram.GetLocation("projection");
        sizeLocation = DisplayShaderProgram.GetLocation("size");
        noiseSizeLocation = DisplayShaderProgram.GetLocation("noiseSize");
        offsetLocation = DisplayShaderProgram.GetLocation("offset");

        NoiseFragmentPath = Path.Combine(Game.shaderPath, NoiseFragmentFile);
        string fileName = NoiseFragmentFile.Replace(".frag", "_copy.frag");
        NoiseFragmentPathCopy = Path.Combine(Game.shaderPath, fileName);
    }

    public static void Compile()
    {
        Compile(NoiseNodeManager.ConnectedNodeList);
    }

    public static void Compile(List<ConnectorNode> nodes)
    {
        Console.WriteLine("Compiling NoiseGlslNodeManager...");
        
        List<string> lines = File.ReadAllLines(NoiseFragmentPath).ToList();
        int index = 0;
        int lineIndex = _lineInsert;

        for (int i = 0; i < nodes.Count; i++)
        {
            ConnectorNode node = nodes[i];
            string newLine = node.GetLine();
            lines.Insert(lineIndex, newLine);
            lineIndex++;
        }

        /*
        var cWorldNodes = CWorldNodeManager.GetNodeDictionary();

        string finalLine = "";

        if (cWorldNodes.Count != 0)
            finalLine = "    color =";
        

        foreach (var (name, node) in cWorldNodes)
        {
            if (node is CWorldSampleNode sampleNode)
            {
                Vector2 offset = sampleNode.Offset;
                Vector2 size = sampleNode.Size;

                Console.WriteLine($"SampleNode: {name} Offset: {offset} Size: {size}");
                string newLine = $"    float {name} = SampleNoise({Convert(size)});";
                lines.Insert(lineIndex, newLine);
                lineIndex++;
                
                foreach (var parameter in sampleNode.Parameters)
                {
                    string parameterLine = $"    {name} = {parameter.GetFunction(name)};";
                    Console.WriteLine($"Parameter: {parameterLine}");
                    lines.Insert(lineIndex, parameterLine);
                    lineIndex++;
                }

                Console.WriteLine($"Offset: {sampleNode.Offset}");
                if (sampleNode.Invert)
                {
                    string invertLine = $"    {name} = 1 - {name};";
                    lines.Insert(lineIndex, invertLine);
                    lineIndex++;
                }

                Console.WriteLine($"Amplitude: {sampleNode.Amplitude}");
                if (sampleNode.Amplitude != 1.0f)
                {
                    string amplitudeLine = $"    {name} *= {sampleNode.Amplitude};";
                    lines.Insert(lineIndex, amplitudeLine);
                    lineIndex++;
                }

                finalLine += $" {name}";
                if (index < cWorldNodes.Count - 1)
                {
                    finalLine += " +";
                }
            }

            index++;
        }

        if (cWorldNodes.Count != 0)
            finalLine += ";";
        lines.Insert(lineIndex, finalLine);
        */

        File.WriteAllLines(NoiseFragmentPathCopy, lines);

        DisplayShaderProgram.Renew("Utils/Rectangle.vert", "Noise/WorldNoise_copy.frag");

        DisplayShaderProgram.Bind();

        modelLocation = DisplayShaderProgram.GetLocation("model");
        projectionLocation = DisplayShaderProgram.GetLocation("projection");
        sizeLocation = DisplayShaderProgram.GetLocation("size");
        noiseSizeLocation = DisplayShaderProgram.GetLocation("noiseSize");
        offsetLocation = DisplayShaderProgram.GetLocation("offset");

        DisplayShaderProgram.Unbind();
    }

    public static void Render(Matrix4 DisplayProjectionMatrix, Vector2 DisplayPosition, Vector2 DisplaySize, float NoiseSize, Vector2 Offset)
    {
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.Enable(EnableCap.CullFace);
        GL.Enable(EnableCap.DepthTest);

        Matrix4 model = Matrix4.CreateTranslation((DisplayPosition.X, DisplayPosition.Y, 2.2f));

        DisplayShaderProgram.Bind();

        GL.UniformMatrix4(modelLocation, true, ref model);
        GL.UniformMatrix4(projectionLocation, true, ref DisplayProjectionMatrix);
        GL.Uniform2(sizeLocation, ref DisplaySize);
        GL.Uniform1(noiseSizeLocation, NoiseSize);
        GL.Uniform2(offsetLocation, ref Offset);

        _displayVAO.Bind();

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        _displayVAO.Unbind();

        DisplayShaderProgram.Unbind();
    }

    private static string Convert(Vector2 vector)
    {
        return $"vec2({vector.X}, {vector.Y})";
    }
}