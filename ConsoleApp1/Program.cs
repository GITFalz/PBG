namespace ConsoleApp1;

class Program
{
    static void Main(string[] args)
    {
        using Game game = new Game(1500, 1000);
        game.Run();
    }
}