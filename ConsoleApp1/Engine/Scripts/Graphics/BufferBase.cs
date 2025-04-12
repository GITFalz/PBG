public abstract class BufferBase
{
    public static List<BufferBase> Buffers = [];

    public BufferBase()
    {
        Buffers.Add(this);
    }

    public void RemoveBuffer(BufferBase buffer)
    {
        Buffers.Remove(buffer);
    }

    public void DeleteInstance()
    {
        RemoveBuffer(this);
    }

    public virtual void DeleteBuffer()
    {
        DeleteInstance();
    }

    public abstract int GetBufferCount();
    public abstract string GetTypeName();

    public static void PrintBufferCount()
    {
        Dictionary<string, int> bufferCounts = new Dictionary<string, int>();

        Console.WriteLine("----- Buffer Count -----");
        foreach (var buffer in Buffers)
        {
            string typeName = buffer.GetTypeName();
            if (bufferCounts.ContainsKey(typeName))
            {
                bufferCounts[typeName]++;
            }
            else
            {
                bufferCounts[typeName] = 1;
            }
        }

        foreach (var kvp in bufferCounts)
        {
            Console.WriteLine($"{kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("------------------------");
    }

    public static void Delete()
    {
        foreach (var buffer in Buffers)
        {
            buffer.DeleteBuffer();
        }
        Buffers.Clear();
    }
}