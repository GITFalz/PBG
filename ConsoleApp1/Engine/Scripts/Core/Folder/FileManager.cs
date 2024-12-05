namespace ConsoleApp1.Engine.Scripts.Core.Folder;

public static class FileManager
{
    public static string[] GetFolderFiles(string folderPath)
    {
        return Directory.GetFiles(folderPath);
    }
    
    public static string[] GetFolderPngFiles(string folderPath)
    {
        return Directory.GetFiles(folderPath);
    }
}