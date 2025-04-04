using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public static class DataMerger
{
    private static ComputeShader _merge1Shader = new ComputeShader("DataTransfer/Merge1SSBO.compute");
    private static ComputeShader _merge2Shader = new ComputeShader("DataTransfer/Merge2SSBO.compute");
    private static ComputeShader _merge3Shader = new ComputeShader("DataTransfer/Merge3SSBO.compute");
    private static ComputeShader _merge4Shader = new ComputeShader("DataTransfer/Merge4SSBO.compute");
    private static ComputeShader _merge5Shader = new ComputeShader("DataTransfer/Merge5SSBO.compute");


    public static void Merge<T>(SSBO<T> outSSBO, SSBO<T> inSSBO1, int ssbo1Offset) where T : struct
    {
        _merge1Shader.Bind();

        Vector2i ssbo1Size = new Vector2i(inSSBO1.DataCount, ssbo1Offset);

        int ssbo1DataLoc = GL.GetUniformLocation(_merge1Shader.ID, "ssbo1Data");
        int ssboOutSizeLoc = GL.GetUniformLocation(_merge1Shader.ID, "ssboOutSize");

        GL.Uniform2(ssbo1DataLoc, ssbo1Size.X, ssbo1Size.Y);
        GL.Uniform1(ssboOutSizeLoc, outSSBO.DataCount);

        outSSBO.Bind(0);
        inSSBO1.Bind(1);

        int workGroupCount = (outSSBO.DataCount + 63) / 64;
        _merge1Shader.DispatchCompute(workGroupCount, 1, 1);

        outSSBO.Unbind();
        inSSBO1.Unbind();
        _merge1Shader.Unbind();
    }

    public static void Merge<T>(SSBO<T> outSSBO, SSBO<T> inSSBO1, SSBO<T> inSSBO2, int ssbo1Offset, int ssbo2Offset) where T : struct
    {
        _merge2Shader.Bind();

        Vector2i ssbo1Size = new Vector2i(inSSBO1.DataCount, ssbo1Offset);
        Vector2i ssbo2Size = new Vector2i(inSSBO2.DataCount, ssbo2Offset);

        int ssbo1DataLoc = GL.GetUniformLocation(_merge2Shader.ID, "ssbo1Data");
        int ssbo2DataLoc = GL.GetUniformLocation(_merge2Shader.ID, "ssbo2Data");
        int ssboOutSizeLoc = GL.GetUniformLocation(_merge2Shader.ID, "ssboOutSize");

        GL.Uniform2(ssbo1DataLoc, ssbo1Size.X, ssbo1Size.Y);
        GL.Uniform2(ssbo2DataLoc, ssbo2Size.X, ssbo2Size.Y);
        GL.Uniform1(ssboOutSizeLoc, outSSBO.DataCount);

        outSSBO.Bind(0);
        inSSBO1.Bind(1);
        inSSBO2.Bind(2);

        int workGroupCount = (outSSBO.DataCount + 63) / 64;
        _merge2Shader.DispatchCompute(workGroupCount, 1, 1);

        outSSBO.Unbind();
        inSSBO1.Unbind();
        inSSBO2.Unbind();
        _merge2Shader.Unbind();
    }

    public static void Merge<T>(SSBO<T> outSSBO, SSBO<T> inSSBO1, SSBO<T> inSSBO2, SSBO<T> inSSBO3, int ssbo1Offset, int ssbo2Offset, int ssbo3Offset) where T : struct
    {
        _merge3Shader.Bind();

        Vector2i ssbo1Size = new Vector2i(inSSBO1.DataCount, ssbo1Offset);
        Vector2i ssbo2Size = new Vector2i(inSSBO2.DataCount, ssbo2Offset);
        Vector2i ssbo3Size = new Vector2i(inSSBO3.DataCount, ssbo3Offset);

        int ssbo1DataLoc = GL.GetUniformLocation(_merge3Shader.ID, "ssbo1Data");
        int ssbo2DataLoc = GL.GetUniformLocation(_merge3Shader.ID, "ssbo2Data");
        int ssbo3DataLoc = GL.GetUniformLocation(_merge3Shader.ID, "ssbo3Data");
        int ssboOutSizeLoc = GL.GetUniformLocation(_merge3Shader.ID, "ssboOutSize");

        GL.Uniform2(ssbo1DataLoc, ssbo1Size.X, ssbo1Size.Y);
        GL.Uniform2(ssbo2DataLoc, ssbo2Size.X, ssbo2Size.Y);
        GL.Uniform2(ssbo3DataLoc, ssbo3Size.X, ssbo3Size.Y);
        GL.Uniform1(ssboOutSizeLoc, outSSBO.DataCount);

        outSSBO.Bind(0);
        inSSBO1.Bind(1);
        inSSBO2.Bind(2);
        inSSBO3.Bind(3);

        int workGroupCount = (outSSBO.DataCount + 63) / 64;
        _merge3Shader.DispatchCompute(workGroupCount, 1, 1);

        outSSBO.Unbind();
        inSSBO1.Unbind();
        inSSBO2.Unbind();
        inSSBO3.Unbind();
        _merge3Shader.Unbind();
    }

    public static void Merge<T>(SSBO<T> outSSBO, SSBO<T> inSSBO1, SSBO<T> inSSBO2, SSBO<T> inSSBO3, SSBO<T> inSSBO4, int ssbo1Offset, int ssbo2Offset, int ssbo3Offset, int ssbo4Offset) where T : struct
    {
        _merge4Shader.Bind();

        Vector2i ssbo1Size = new Vector2i(inSSBO1.DataCount, ssbo1Offset);
        Vector2i ssbo2Size = new Vector2i(inSSBO2.DataCount, ssbo2Offset);
        Vector2i ssbo3Size = new Vector2i(inSSBO3.DataCount, ssbo3Offset);
        Vector2i ssbo4Size = new Vector2i(inSSBO4.DataCount, ssbo4Offset);

        int ssbo1DataLoc = GL.GetUniformLocation(_merge4Shader.ID, "ssbo1Data");
        int ssbo2DataLoc = GL.GetUniformLocation(_merge4Shader.ID, "ssbo2Data");
        int ssbo3DataLoc = GL.GetUniformLocation(_merge4Shader.ID, "ssbo3Data");
        int ssbo4DataLoc = GL.GetUniformLocation(_merge4Shader.ID, "ssbo4Data");
        int ssboOutSizeLoc = GL.GetUniformLocation(_merge4Shader.ID, "ssboOutSize");

        GL.Uniform2(ssbo1DataLoc, ssbo1Size.X, ssbo1Size.Y);
        GL.Uniform2(ssbo2DataLoc, ssbo2Size.X, ssbo2Size.Y);
        GL.Uniform2(ssbo3DataLoc, ssbo3Size.X, ssbo3Size.Y);
        GL.Uniform2(ssbo4DataLoc, ssbo4Size.X, ssbo4Size.Y);
        GL.Uniform1(ssboOutSizeLoc, outSSBO.DataCount);

        outSSBO.Bind(0);
        inSSBO1.Bind(1);
        inSSBO2.Bind(2);
        inSSBO3.Bind(3);
        inSSBO4.Bind(4);

        int workGroupCount = (outSSBO.DataCount + 63) / 64;
        _merge4Shader.DispatchCompute(workGroupCount, 1, 1);

        outSSBO.Unbind();
        inSSBO1.Unbind();
        inSSBO2.Unbind();
        inSSBO3.Unbind();
        inSSBO4.Unbind();
        _merge4Shader.Unbind();
    }

    public static void Merge<T>(SSBO<T> outSSBO, SSBO<T> inSSBO1, SSBO<T> inSSBO2, SSBO<T> inSSBO3, SSBO<T> inSSBO4, SSBO<T> inSSBO5, int ssbo1Offset, int ssbo2Offset, int ssbo3Offset, int ssbo4Offset, int ssbo5Offset) where T : struct
    {
        _merge5Shader.Bind();

        Vector2i ssbo1Size = new Vector2i(inSSBO1.DataCount, ssbo1Offset);
        Vector2i ssbo2Size = new Vector2i(inSSBO2.DataCount, ssbo2Offset);
        Vector2i ssbo3Size = new Vector2i(inSSBO3.DataCount, ssbo3Offset);
        Vector2i ssbo4Size = new Vector2i(inSSBO4.DataCount, ssbo4Offset);
        Vector2i ssbo5Size = new Vector2i(inSSBO5.DataCount, ssbo5Offset);

        int ssbo1DataLoc = GL.GetUniformLocation(_merge5Shader.ID, "ssbo1Data");
        int ssbo2DataLoc = GL.GetUniformLocation(_merge5Shader.ID, "ssbo2Data");
        int ssbo3DataLoc = GL.GetUniformLocation(_merge5Shader.ID, "ssbo3Data");
        int ssbo4DataLoc = GL.GetUniformLocation(_merge5Shader.ID, "ssbo4Data");
        int ssbo5DataLoc = GL.GetUniformLocation(_merge5Shader.ID, "ssbo5Data");
        int ssboOutSizeLoc = GL.GetUniformLocation(_merge5Shader.ID, "ssboOutSize");

        GL.Uniform2(ssbo1DataLoc, ssbo1Size.X, ssbo1Size.Y);
        GL.Uniform2(ssbo2DataLoc, ssbo2Size.X, ssbo2Size.Y);
        GL.Uniform2(ssbo3DataLoc, ssbo3Size.X, ssbo3Size.Y);
        GL.Uniform2(ssbo4DataLoc, ssbo4Size.X, ssbo4Size.Y);
        GL.Uniform2(ssbo5DataLoc, ssbo5Size.X, ssbo5Size.Y);
        GL.Uniform1(ssboOutSizeLoc, outSSBO.DataCount);

        outSSBO.Bind(0);
        inSSBO1.Bind(1);
        inSSBO2.Bind(2);
        inSSBO3.Bind(3);
        inSSBO4.Bind(4);
        inSSBO5.Bind(5);

        int workGroupCount = (outSSBO.DataCount + 63) / 64;
        _merge5Shader.DispatchCompute(workGroupCount, 1, 1);

        outSSBO.Unbind();
        inSSBO1.Unbind();
        inSSBO2.Unbind();
        inSSBO3.Unbind();
        inSSBO4.Unbind();
        inSSBO5.Unbind();
        _merge5Shader.Unbind();
    }
}