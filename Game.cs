using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using Assimp;
using System.Collections.Generic;

public class Game : GameWindow
{
    private int _vertexBufferObject;
    private int _vertexArrayObject;
    private List<float> _vertices;

    public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
        : base(gameWindowSettings, nativeWindowSettings)
    {
        VSync = VSyncMode.On;
        CursorState = CursorState.Hidden;
    }

    /**
     * Вызывается при загрузке окна.
     * Метод используется для инициализации настроек OpenGL.
     */
    protected override void OnLoad()
    {
        base.OnLoad();
        LoadModel("C:/Users/artur/source/repos/ComputerGraphics4/bin/Debug/models/couch.obj");
        SetupBuffers();
    }

    private void LoadModel(string path)
    {
        AssimpContext importer = new AssimpContext();
        var scene = importer.ImportFile(path, PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs);

        _vertices = new List<float>();
        Vector3D min = new Vector3D(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3D max = new Vector3D(float.MinValue, float.MinValue, float.MinValue);

        foreach (var mesh in scene.Meshes)
        {
            foreach (var vertex in mesh.Vertices)
            {
                _vertices.Add(vertex.X);
                _vertices.Add(vertex.Y);
                _vertices.Add(vertex.Z);

                // Обновите минимумы и максимумы
                min.X = Math.Min(min.X, vertex.X);
                min.Y = Math.Min(min.Y, vertex.Y);
                min.Z = Math.Min(min.Z, vertex.Z);
                max.X = Math.Max(max.X, vertex.X);
                max.Y = Math.Max(max.Y, vertex.Y);
                max.Z = Math.Max(max.Z, vertex.Z);
            }
        }

        // Нормализация и центрирование
        Vector3D center = (min + max) / 2;
        float scaleFactor = 1f / Math.Max(Math.Max(max.X - min.X, max.Y - min.Y), max.Z - min.Z); // Скаляр для нормализации

        for (int i = 0; i < _vertices.Count; i += 3)
        {
            _vertices[i] = (_vertices[i] - center.X) * scaleFactor;   // X
            _vertices[i + 1] = (_vertices[i + 1] - center.Y) * scaleFactor; // Y
            _vertices[i + 2] = (_vertices[i + 2] - center.Z) * scaleFactor; // Z
        }
    }


    private void SetupBuffers()
    {
        _vertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(_vertexArrayObject);

        _vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Count * sizeof(float), _vertices.ToArray(), BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);
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
        GL.ClearColor(0.5f, 0.5f, 0.5f, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        GL.BindVertexArray(_vertexArrayObject);
        GL.DrawArrays(OpenTK.Graphics.OpenGL.PrimitiveType.Triangles, 0, _vertices.Count / 3);
        GL.BindVertexArray(0);

        SwapBuffers();
        base.OnRenderFrame(e);
    }

    /**
     * Метод отвечает за удаление загруженных при инициализации ресурсов.
     */
    protected override void OnUnload()
    {
        GL.DeleteVertexArray(_vertexArrayObject);
        GL.DeleteBuffer(_vertexBufferObject);
        base.OnUnload();
    }
}
