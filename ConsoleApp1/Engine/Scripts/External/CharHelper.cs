using System.Runtime.InteropServices;
using OpenTK.Windowing.GraphicsLibraryFramework;

public static class CharHelper
{
    [DllImport("user32.dll")]
    private static extern int ToUnicode(uint wVirtKey, uint wScanCode, byte[] lpKeyState,
        [Out] char[] pwszBuff, int cchBuff, uint wFlags);

    [DllImport("user32.dll")]
    private static extern bool GetKeyboardState(byte[] lpKeyState);
    
    public static char KeyToChar(Keys key, bool shift = false)
    {
        char[] buffer = new char[2];
        byte[] keyboardState = new byte[256];
        GetKeyboardState(keyboardState);

        if (shift)
        {
            keyboardState[0xA0] = 0x80;
            keyboardState[0xA1] = 0x80;
        }

        int result = ToUnicode((uint)key, 0, keyboardState, buffer, buffer.Length, 0);

        return result > 0 ? buffer[0] : '\0';
    }
}