using System;

public class Program
{
    [STAThread]
    public static void Main()
    {
        using (Game game = new Game())
        {
            game.Run(); 
        }
    }
}