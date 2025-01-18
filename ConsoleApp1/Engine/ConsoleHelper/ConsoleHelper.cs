using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public static class ConsoleHelper
{
    public static void WriteList<T>(List<T> list)
    {
        string output = string.Join(", ", list);
        Console.WriteLine(output);
    }
}