using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using Assimp;
using System.Collections.Generic;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Drawing;                
using System.Drawing.Imaging;
using ComputerGraphics4;

public class Game : GameWindow
{
    private int _coachBufferObject;
    private int _coachArrayObject;
    private List<float> _coachVertices;

    private int _tableandchairsBufferObject;
    private int _tableandchairsArrayObject;
    private List<float> _tableandchairsVertices;

    private Vector3 _cameraPosition;
    private float _cameraYaw;
    private float _cameraPitch;
    private float _sensitivity = 0.05f;
    private bool _isMouseMoving; // Добавляем переменную для отслеживания движения мыши

    private int _floorVertexBufferObject;
    private int _floorVertexArrayObject;
    private List<float> _floorVertices;

    private int _roofVertexBufferObject;
    private int _roofVertexArrayObject;
    private List<float> _roofVertices;

    private Shader _shader;

    private Texture _texture;

    public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
        : base(gameWindowSettings, nativeWindowSettings)
    {
        VSync = VSyncMode.On;
        CursorState = CursorState.Grabbed; // Убираем курсор и захватываем его
        _cameraPosition = new Vector3(0, 0, 3); // Начальная позиция камеры
    }

    /**
     * Вызывается при загрузке окна.
     * Метод используется для инициализации настроек OpenGL.
     */
    protected override void OnLoad()
    {
        base.OnLoad();
        float[] borderColor = { 1.0f, 1.0f, 0.0f, 1.0f };
        Vector3D pos = new Vector3D(0.0f, 0.0f, 0.0f);
        _coachVertices = new List<float>();
        LoadModel("Models/couch.obj", _coachVertices, pos);
        SetupBuffers(_coachVertices, out _coachBufferObject, out _coachArrayObject);

        Vector3D pos1 = new Vector3D(5.0f, -2.5f, 0.0f);
        _tableandchairsVertices = new List<float>();
        LoadModel("Models/Table And Chairs.obj", _tableandchairsVertices, pos1);
        SetupBuffers(_tableandchairsVertices, out _tableandchairsBufferObject, out _tableandchairsArrayObject);

        SetupFloorBuffers();
        SetupRoofBuffers();
    }

    private void LoadModel(string path, List<float> vertices, Vector3D position)
    {
        AssimpContext importer = new AssimpContext();
        var scene = importer.ImportFile(path, PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs);

        Vector3D min = new Vector3D(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3D max = new Vector3D(float.MinValue, float.MinValue, float.MinValue);

        foreach (var mesh in scene.Meshes)
        {
            foreach (var vertex in mesh.Vertices)
            {
                vertices.Add(vertex.X);
                vertices.Add(vertex.Y + 300);
                vertices.Add(vertex.Z);

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

        for (int i = 0; i < vertices.Count; i += 3)
        {
            vertices[i] = (vertices[i] - center.X) * scaleFactor + position.X;   // X
            vertices[i + 1] = (vertices[i + 1] - center.Y) * scaleFactor + position.Y; // Y
            vertices[i + 2] = (vertices[i + 2] - center.Z) * scaleFactor + position.Z; // Z
        }
    }


    private void SetupBuffers(List<float> vertices, out int bufferObject, out int arrayObject)
    {
        arrayObject = GL.GenVertexArray();
        GL.BindVertexArray(arrayObject);

        bufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, bufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count * sizeof(float), vertices.ToArray(), BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);
    }


    private void SetupFloorBuffers()
    {
        _floorVertices = new List<float>
    {
        // Позиции               // Текстурные координаты
        -25f, 0f, -25f, 0.0f, 0.0f, // Нижний левый угол
        25f, 0f, -25f, 1.0f, 0.0f,  // Нижний правый угол
        25f, 0f, 25f, 1.0f, 1.0f,   // Верхний правый угол
        -25f, 0f, 25f, 0.0f, 1.0f   // Верхний левый угол
    };

        _floorVertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(_floorVertexArrayObject);

        _floorVertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _floorVertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, _floorVertices.Count * sizeof(float), _floorVertices.ToArray(), BufferUsageHint.StaticDraw);

        // Вершинные атрибуты
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0); // Позиции
        GL.EnableVertexAttribArray(0);

        // Текстурные атрибуты
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float)); // UV
        GL.EnableVertexAttribArray(1);

        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);
    }



    private void SetupRoofBuffers()
    {
        _roofVertices = new List<float>
    {

        -25f, 25f, -25f,
         25f, 25f, -25f,
         25f, 25f, 25f,
        -25f, 25f, 25f,
    };

        _roofVertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(_roofVertexArrayObject);

        _roofVertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _roofVertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, _roofVertices.Count * sizeof(float), _roofVertices.ToArray(), BufferUsageHint.StaticDraw);

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
        GL.Viewport(0, 0, e.Width, e.Height);
        // Настройка матрицы перспективы
        Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45), (float)e.Width / e.Height, 0.1f, 100f);
        GL.MatrixMode(MatrixMode.Projection);
        GL.LoadMatrix(ref projection);
    }

    /**
     * Метод для учёта обновлений в кадре.
     */
    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        // Управление камерой
        Vector3 front = new Vector3(
            (float)(Math.Cos(MathHelper.DegreesToRadians(_cameraYaw)) * Math.Cos(MathHelper.DegreesToRadians(_cameraPitch))),
            (float)(Math.Sin(MathHelper.DegreesToRadians(_cameraPitch))),
            (float)(Math.Sin(MathHelper.DegreesToRadians(_cameraYaw)) * Math.Cos(MathHelper.DegreesToRadians(_cameraPitch)))
        ).Normalized();

        Vector3 right = Vector3.Cross(front, Vector3.UnitY).Normalized(); // Вектор вправо
        Vector3 up = Vector3.Cross(right, front).Normalized(); // Вектор вверх

        if (KeyboardState.IsKeyDown(Keys.W))
            _cameraPosition += front * 0.1f; // Движение вперед
        if (KeyboardState.IsKeyDown(Keys.S))
            _cameraPosition -= front * 0.1f; // Движение назад
        if (KeyboardState.IsKeyDown(Keys.A))
            _cameraPosition -= right * 0.1f; // Движение влево
        if (KeyboardState.IsKeyDown(Keys.D))
            _cameraPosition += right * 0.1f; // Движение вправо
        if (KeyboardState.IsKeyDown(Keys.Escape))
            Close();

        // Обработка вращения камеры с помощью мыши
        var mouseState = MouseState;

        if (mouseState.IsButtonDown(MouseButton.Left)) // Проверка нажатия кнопки мыши
        {
            float deltaX = mouseState.X - (Size.X / 2); // Текущая позиция мыши по X
            float deltaY = mouseState.Y - (Size.Y / 2); // Текущая позиция мыши по Y

            _cameraYaw += deltaX * _sensitivity;
            _cameraPitch -= deltaY * _sensitivity;

            // Ограничиваем угол наклона, чтобы предотвратить переворот камеры
            _cameraPitch = MathHelper.Clamp(_cameraPitch, -60f, 60f); // Установите правильные ограничения
            CursorState = CursorState.Grabbed; // Захватываем курсор для отслеживания
            // Сброс позиции курсора в центр окна
            MousePosition = new Vector2(Size.X / 2, Size.Y / 2);
        }
        CursorState = CursorState.Normal; // Захватываем курсор для отслеживания
        // Центруем курсор
        MousePosition = new Vector2(Size.X / 2, Size.Y / 2);
    }

    /**
     * Вызывается при каждом рендеринге кадра.
     * Метод отвечает за отрисовку сцены. 
     */
    protected override void OnRenderFrame(FrameEventArgs e)
    {
        // Очистка экрана
        GL.ClearColor(0.5f, 0.5f, 0.5f, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        // Установка матрицы видового преобразования
        Vector3 front = new Vector3(
            (float)(Math.Cos(MathHelper.DegreesToRadians(_cameraYaw)) * Math.Cos(MathHelper.DegreesToRadians(_cameraPitch))),
            (float)(Math.Sin(MathHelper.DegreesToRadians(_cameraPitch))),
            (float)(Math.Sin(MathHelper.DegreesToRadians(_cameraYaw)) * Math.Cos(MathHelper.DegreesToRadians(_cameraPitch)))
        ).Normalized();

        Matrix4 view = Matrix4.LookAt(_cameraPosition, _cameraPosition + front, Vector3.UnitY);
        GL.MatrixMode(MatrixMode.Modelview);
        GL.LoadMatrix(ref view);

        GL.Enable(EnableCap.DepthTest); //Тестирование глубины, так объекты "непрозрачны"

        // Отрисовка пола
        GL.Color3(0.8f, 0.52f, 0.25f);
        GL.BindVertexArray(_floorVertexArrayObject);
        GL.DrawArrays(OpenTK.Graphics.OpenGL.PrimitiveType.Quads, 0, 4); // Отрисовываем пол
        GL.BindVertexArray(0);

        // Отрисовка потолка
        GL.Color3(0.95f, 0.87f, 0.68f);
        GL.BindVertexArray(_roofVertexArrayObject);
        GL.DrawArrays(OpenTK.Graphics.OpenGL.PrimitiveType.Quads, 0, 4); // Отрисовываем потолок
        GL.BindVertexArray(0);

        // Отрисовка дивана
        GL.Color3(1.0f, 1.0f, 1.0f);
        GL.BindVertexArray(_coachArrayObject);
        GL.DrawArrays(OpenTK.Graphics.OpenGL.PrimitiveType.Quads, 0, _coachVertices.Count / 3); // Отрисовываем диван
        GL.BindVertexArray(0);

        // Отрисовка стола и стульев
        GL.BindVertexArray(_tableandchairsArrayObject);
        GL.DrawArrays(OpenTK.Graphics.OpenGL.PrimitiveType.Quads, 0, _tableandchairsVertices.Count / 3); // Отрисовываем стол и стулья
        GL.BindVertexArray(0);

        // Обмен буферов
        SwapBuffers();

        base.OnRenderFrame(e);
    }

    /**
     * Метод отвечает за удаление загруженных при инициализации ресурсов.
     */
    protected override void OnUnload()
    {
        GL.DeleteVertexArray(_coachArrayObject);
        GL.DeleteBuffer(_coachBufferObject);
        GL.DeleteVertexArray(_floorVertexArrayObject); 
        GL.DeleteBuffer(_floorVertexBufferObject);
        base.OnUnload();
    }
}
