public static class TextShaderHelper
{
    public static int[] StringToIntArray(string text)
    {
        int[] result = new int[text.Length];
        for (int i = 0; i < text.Length; i++)
        {
            result[i] = CharPosition[text[i]];
        }
        return result;
    }
    
    public static readonly Dictionary<char, int> CharPosition = new Dictionary<char, int>()
    {
        { ' ', -1 },
        { 'A', 0 }, { 'B', 1 }, { 'C', 2 }, { 'D', 3 }, { 'E', 4 },
        { 'F', 5 }, { 'G', 6 }, { 'H', 7 }, { 'I', 8 }, { 'J', 9 },
        { 'K', 10 }, { 'L', 11 }, { 'M', 12 }, { 'N', 13 }, { 'O', 14 },
        { 'P', 15 }, { 'Q', 16 }, { 'R', 17 }, { 'S', 18 }, { 'T', 19 },
        { 'U', 20 }, { 'V', 21 }, { 'W', 22 }, { 'X', 23 }, { 'Y', 24 },
        { 'Z', 25 },
        { 'a', 40 }, { 'b', 41 }, { 'c', 42 }, { 'd', 43 }, { 'e', 44 },
        { 'f', 45 }, { 'g', 46 }, { 'h', 47 }, { 'i', 48 }, { 'j', 49 },
        { 'k', 50 }, { 'l', 51 }, { 'm', 52 }, { 'n', 53 }, { 'o', 54 },
        { 'p', 55 }, { 'q', 56 }, { 'r', 57 }, { 's', 58 }, { 't', 59 },
        { 'u', 60 }, { 'v', 61 }, { 'w', 62 }, { 'x', 63 }, { 'y', 64 },
        { 'z', 65 },
        { '1', 390 }, { '2', 391 }, { '3', 392 }, { '4', 393 }, { '5', 394 },
        { '6', 395 }, { '7', 396 }, { '8', 397 }, { '9', 398 }, { '0', 399 }
    };
}