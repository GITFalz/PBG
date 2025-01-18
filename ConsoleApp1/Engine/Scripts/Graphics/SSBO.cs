using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

public class SSBO
{
    public int ID;
    private const int Matrix4SizeInBytes = sizeof(float) * 16;

    public SSBO(List<Matrix4> data)
    {
        ID = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ID);
        GL.BufferData(BufferTarget.ShaderStorageBuffer, data.Count * Matrix4SizeInBytes, data.ToArray(), BufferUsageHint.DynamicDraw);
    }

    public void Bind(int bindingPoint)
    {
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ID);
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, bindingPoint, ID);
    }

    public void Unbind()
    {
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
    }

    public void Delete()
    {
        GL.DeleteBuffer(ID);
    }

    public void Update(List<Matrix4> newData, int bindingPoint)
    {
        Bind(bindingPoint);
        GL.BufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, newData.Count * Matrix4SizeInBytes, newData.ToArray());
        Unbind();
    }
}