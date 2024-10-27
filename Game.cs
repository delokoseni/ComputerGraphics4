using OpenTK;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL;
using System;

public class Game : GameWindow
{
    public Game() : base(new GameWindowSettings(), new NativeWindowSettings()
    {
        Size = new OpenTK.Mathematics.Vector2i(800, 600),
        Title = "My OpenTK Game"
    })
    {
    }

    /**
     * Вызывается при загрузке окна.
     * Метод используется для инициализации настроек OpenGL.
     */
    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(0.5f, 0.5f, 0.5f, 1.0f); // Цвет фона с альфа-каналом
        GL.Enable(EnableCap.DepthTest); // Включаем тест глубины
    }

    /**
     * Вызывается при каждом рендеринге кадра.
     * Метод отвечает за отрисовку сцены. 
     */
    protected override void OnRenderFrame(OpenTK.Windowing.Common.FrameEventArgs e)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        // Рендеринг объектов здесь

        SwapBuffers();
    }
}