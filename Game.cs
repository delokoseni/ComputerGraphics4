using OpenTK;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL;
using System;
using OpenTK.Windowing.Common;

public class Game : GameWindow
{
    public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
    {
        
    }

    /**
     * Вызывается при загрузке окна.
     * Метод используется для инициализации настроек OpenGL.
     */
    protected override void OnLoad()
    {
        base.OnLoad();
    }

    /**
     * Вызывается при изменении размера окна.
     */
    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
    }

    /**
     * Метод для учёта обновлений в кадре.
     */
    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
    }

    /**
     * Вызывается при каждом рендеринге кадра.
     * Метод отвечает за отрисовку сцены. 
     */
    protected override void OnRenderFrame(FrameEventArgs e)
    {
        GL.ClearColor(0.5f, 0.5f, 0.5f, 0.5f); // Цвет фона с альфа-каналом
        GL.Clear(ClearBufferMask.ColorBufferBit);
        //GL.Enable(EnableCap.DepthTest); // Включаем тест глубины
        SwapBuffers();
        base.OnRenderFrame(e);
    }

    /**
     * Метод отвечает за удаление загруженных при инициализации ресурсов.
     */
    protected override void OnUnload()
    {
        base.OnUnload();
    }
}