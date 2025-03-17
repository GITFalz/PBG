using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

public class SSBO<T> where T : struct
{
    public int ID;
    private int _currentSize;  // Keep track of allocated size
    private readonly int _elementSize;

    public SSBO(List<T> data)
    {
        _elementSize = System.Runtime.InteropServices.Marshal.SizeOf<T>();
        _currentSize = data.Count * _elementSize;

        ID = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ID);
        GL.BufferData(BufferTarget.ShaderStorageBuffer, _currentSize, data.ToArray(), BufferUsageHint.DynamicDraw);
    }

    public SSBO(int size)
    {
        GL.GenBuffers(1, out ID);
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ID);

        GL.BufferData(BufferTarget.ShaderStorageBuffer, size, IntPtr.Zero, BufferUsageHint.DynamicDraw);

        Unbind();
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

    public void Update(List<T> newData)
    {
        int newSize = newData.Count * _elementSize;

        if (newSize > _currentSize)
        {
            ResizeBuffer(newSize);
        }

        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ID);
        GL.BufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, newSize, newData.ToArray());
        Unbind();
    }

    public void ResizeBuffer(int newSizeInElements)
    {
        if (newSizeInElements == _currentSize) return;
        
        int newBuffer = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, newBuffer);
        GL.BufferData(BufferTarget.ShaderStorageBuffer, newSizeInElements * _elementSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);

        GL.BindBuffer(BufferTarget.CopyReadBuffer, ID);
        GL.BindBuffer(BufferTarget.CopyWriteBuffer, newBuffer);

        int sizeToCopy = Math.Min(_currentSize, newSizeInElements) * _elementSize;
        GL.CopyBufferSubData(BufferTarget.CopyReadBuffer, BufferTarget.CopyWriteBuffer, IntPtr.Zero, IntPtr.Zero, sizeToCopy);
        
        GL.DeleteBuffer(ID);
        ID = newBuffer;
        _currentSize = newSizeInElements;
    }

    public T[] ReadData(int count)
    {
        T[] data = new T[count];
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ID);
        GL.GetBufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, count * _elementSize, data);
        Unbind();
        return data;
    }

    public void Delete()
    {
        GL.DeleteBuffer(ID);
    }
}
