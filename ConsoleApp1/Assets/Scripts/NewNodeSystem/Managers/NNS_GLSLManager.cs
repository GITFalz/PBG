using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class NNS_GLSLManager
{
    public static ShaderProgram DisplayShaderProgram = new ShaderProgram("Utils/Rectangle.vert", "Noise/NNS_WordNoise_Compiled.frag");
    public static ComputeShader SetFloatComputeShader = new ComputeShader("ComputeShaders/List/SetFloatIndex.glsl");

    private static VAO _displayVAO = new VAO();
    private static SSBO<float> _valueSSBO = new();

    private static int modelLocation = -1;
    private static int projectionLocation = -1;
    private static int sizeLocation = -1;
    private static int noiseSizeLocation = -1;
    private static int offsetLocation = -1;

    private static int indexLocation = -1;
    private static int valueLocation = -1;

    public static string NoiseFragmentFile = "Noise/NNS_WordNoise.frag";
    private static string NoiseFragmentPath;
    private static string NoiseFragmentPathCopy;

    private static int _lineInsert = 434; // The line where the new lines will be inserted in the shader file
    private static string[] _startingLines = [];

    static NNS_GLSLManager()
    {
        modelLocation = DisplayShaderProgram.GetLocation("model");
        projectionLocation = DisplayShaderProgram.GetLocation("projection");
        sizeLocation = DisplayShaderProgram.GetLocation("size");
        noiseSizeLocation = DisplayShaderProgram.GetLocation("noiseSize");
        offsetLocation = DisplayShaderProgram.GetLocation("offset");

        indexLocation = SetFloatComputeShader.GetLocation("index");
        valueLocation = SetFloatComputeShader.GetLocation("value");

        NoiseFragmentPath = Path.Combine(Game.shaderPath, NoiseFragmentFile);
        string fileName = NoiseFragmentFile.Replace(".frag", "_copy.frag");
        NoiseFragmentPathCopy = Path.Combine(Game.shaderPath, fileName);

        List<string> startingLines = [];
        string[] lines = File.ReadAllLines(NoiseFragmentPath);
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (line.EndsWith("// Start line"))
                _lineInsert = i + 1;
            startingLines.Add(line);
        }
        _startingLines = [.. startingLines];
    }

    public static void Compile(List<NNS_NodeBase> nodes)
    {
        List<string> lines = [.. _startingLines];
        int lineIndex = _lineInsert;

        List<float> values = [];
        int index = 0;

        for (int i = 0; i < nodes.Count; i++)
        {
            NNS_NodeBase node = nodes[i];
            node.ResetValueReferences();
            node.SetValueReferences(values, ref index);
            string newLine = node.GetLine();
            lines.Insert(lineIndex, newLine);
            lineIndex++;
        }

        _valueSSBO.Renew(values);

        File.WriteAllLines(NoiseFragmentPathCopy, lines);

        Reload();
    }

    public static void UpdateValue(int index, float value)
    {
        if (index == -1)
            return;

        SetFloatComputeShader.Bind();

        GL.Uniform1(indexLocation, index);
        GL.Uniform1(valueLocation, value);

        _valueSSBO.Bind(0);

        SetFloatComputeShader.DispatchCompute(1, 1, 1);

        _valueSSBO.Unbind();
        SetFloatComputeShader.Unbind();
    }

    public static void Reload()
    {
        DisplayShaderProgram.Renew("Utils/Rectangle.vert", "Noise/NNS_WordNoise_Compiled.frag");

        DisplayShaderProgram.Bind();

        modelLocation = DisplayShaderProgram.GetLocation("model");
        projectionLocation = DisplayShaderProgram.GetLocation("projection");
        sizeLocation = DisplayShaderProgram.GetLocation("size");
        noiseSizeLocation = DisplayShaderProgram.GetLocation("noiseSize");
        offsetLocation = DisplayShaderProgram.GetLocation("offset");

        DisplayShaderProgram.Unbind();
    }

    public static void Render(Matrix4 DisplayProjectionMatrix, Vector2 DisplayPosition, Vector2 DisplaySize, float NoiseSize, Vector2 Offset, Vector4 color)
    {
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.Enable(EnableCap.CullFace);
        GL.Enable(EnableCap.DepthTest);

        Matrix4 model = Matrix4.CreateTranslation((DisplayPosition.X, DisplayPosition.Y, 2.2f));

        DisplayShaderProgram.Bind();

        int colorLocation = DisplayShaderProgram.GetLocation("color");

        GL.UniformMatrix4(modelLocation, true, ref model);
        GL.UniformMatrix4(projectionLocation, true, ref DisplayProjectionMatrix);
        GL.Uniform2(sizeLocation, ref DisplaySize);
        GL.Uniform1(noiseSizeLocation, NoiseSize);
        GL.Uniform2(offsetLocation, ref Offset);
        GL.Uniform4(colorLocation, color);

        _displayVAO.Bind();
        _valueSSBO.Bind(0);

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        //Shader.Error("Error rendering noise shader: ");

        _valueSSBO.Unbind();
        _displayVAO.Unbind();

        DisplayShaderProgram.Unbind();
    }
}