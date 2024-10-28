using OpenTK.Windowing.Desktop;
using System;

public class Program
{
    [STAThread]
    public static void Main()
    {
        NativeWindowSettings nativeWindowSettings = new NativeWindowSettings()
        {
            ClientSize = new OpenTK.Mathematics.Vector2i(800, 600),
            Title = "Kitchen",
            Flags = OpenTK.Windowing.Common.ContextFlags.Default,
            Profile = OpenTK.Windowing.Common.ContextProfile.Compatability
        };

        using (Game game = new Game(GameWindowSettings.Default, nativeWindowSettings))
        {
            game.Run();
        }
    }
}