using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace ComputerGraphics4
{
    // Простой класс, предназначенный для упрощения создания шейдеров.
    public class Shader
    {
        public readonly int Handle;

        private readonly Dictionary<string, int> _uniformLocations;

        // Вот как вы создаете простой шейдер.
        // Шейдеры написаны на GLSL, который по своей семантике очень похож на C.
        // Исходный код GLSL компилируется *во время выполнения*, так что он может оптимизировать себя для той видеокарты, на которой в данный момент используется.
        // Пример GLSL с комментариями можно найти в shader.vert.
        public Shader(string vertPath, string fragPath)
        {
            // Существует несколько различных типов шейдеров, но для базового рендеринга нужны только вершинный и фрагментный шейдеры.
            // Вершинный шейдер отвечает за перемещение вершин и передачу этих данных во фрагментный шейдер.
            //   Вершинный шейдер здесь не так важен, но он будет более важен позже.
            // Фрагментный шейдер отвечает за преобразование вершин в "фрагменты", которые представляют все данные, необходимые OpenGL для рисования пикселя.
            //   Фрагментный шейдер - это то, что мы будем использовать больше всего здесь.

            // Загружаем вершинный шейдер и компилируем его
            var shaderSource = File.ReadAllText(vertPath);

            // GL.CreateShader создаст пустой шейдер (очевидно). Перечисление ShaderType указывает, какой тип шейдера будет создан.
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);

            // Теперь привязываем исходный код GLSL
            GL.ShaderSource(vertexShader, shaderSource);

            // И затем компилируем
            CompileShader(vertexShader);

            // То же самое делаем для фрагментного шейдера.
            shaderSource = File.ReadAllText(fragPath);
            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, shaderSource);
            CompileShader(fragmentShader);

            // Эти два шейдера затем должны быть объединены в шейдерную программу, которую может использовать OpenGL.
            // Для этого создаем программу...
            Handle = GL.CreateProgram();

            // Присоединяем оба шейдера...
            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);

            // И затем связываем их вместе.
            LinkProgram(Handle);

            // Когда шейдерная программа связана, она больше не нуждается в индивидуальных шейдерах, прикрепленных к ней; скомпилированный код копируется в шейдерную программу.
            // Отцепляем их, а затем удаляем.
            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);

            // Шейдер теперь готов к использованию, но сначала мы кэшируем все расположения униформ.
            // Запрос этой информации из шейдера очень медленный, поэтому мы делаем это один раз при инициализации и повторно используем значения
            // позже.

            // Сначала мы должны получить количество активных униформ в шейдере.
            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

            // Далее выделяем словарь для хранения расположений.
            _uniformLocations = new Dictionary<string, int>();

            // Проходим по всем униформам,
            for (var i = 0; i < numberOfUniforms; i++)
            {
                // получаем имя этого униформа,
                var key = GL.GetActiveUniform(Handle, i, out _, out _);

                // получаем расположение,
                var location = GL.GetUniformLocation(Handle, key);

                // и затем добавляем его в словарь.
                _uniformLocations.Add(key, location);
            }
        }

        private static void CompileShader(int shader)
        {
            // Пытаемся скомпилировать шейдер
            GL.CompileShader(shader);

            // Проверяем наличие ошибок компиляции
            GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
            if (code != (int)All.True)
            {
                // Мы можем использовать `GL.GetShaderInfoLog(shader)`, чтобы получить информацию об ошибке.
                var infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Ошибка при компиляции шейдера({shader}).\n\n{infoLog}");
            }
        }

        private static void LinkProgram(int program)
        {
            // Связываем программу
            GL.LinkProgram(program);

            // Проверяем наличие ошибок связывания
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
            if (code != (int)All.True)
            {
                // Мы можем использовать `GL.GetProgramInfoLog(program)`, чтобы получить информацию об ошибке.
                throw new Exception($"Ошибка при связывании программы({program})");
            }
        }

        // Обертка, которая включает шейдерную программу.
        public void Use()
        {
            GL.UseProgram(Handle);
        }

        // Исходные шейдеры, предоставленные с этим проектом, используют жестко закодированные layout(location)-ы. Если вы хотите сделать это динамически,
        // вы можете опустить строки layout(location=X) в вершинном шейдере и использовать это в VertexAttribPointer вместо жестко закодированных значений.
        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(Handle, attribName);
        }

        // Установщики униформ
        // Униформы - это переменные, которые могут быть установлены пользовательским кодом, вместо чтения их из VBO.
        // Вы используете VBO для данных, связанных с вершинами, а униформы для почти всего остального.

        // Установка униформа почти всегда одинаковая, поэтому я объясню это здесь один раз, вместо каждого метода:
        //     1. Привязать программу, на которую вы хотите установить униформу
        //     2. Получить хэндл расположения униформа с помощью GL.GetUniformLocation.
        //     3. Использовать соответствующую функцию GL.Uniform*, чтобы установить униформу.

        /// <summary>
        /// Установить униформный int в этом шейдере.
        /// </summary>
        /// <param name="name">Имя униформа</param>
        /// <param name="data">Данные для установки</param>
        public void SetInt(string name, int data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(_uniformLocations[name], data);
        }

        /// <summary>
        /// Установить униформный float в этом шейдере.
        /// </summary>
        /// <param name="name">Имя униформа</param>
        /// <param name="data">Данные для установки</param>
        public void SetFloat(string name, float data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(_uniformLocations[name], data);
        }

        /// <summary>
        /// Установить униформный Matrix4 в этом шейдере
        /// </summary>
        /// <param name="name">Имя униформа</param>
        /// <param name="data">Данные для установки</param>
        /// <remarks>
        ///   <para>
        ///   Матрица транспонируется перед отправкой в шейдер.
        ///   </para>
        /// </remarks>
        public void SetMatrix4(string name, Matrix4 data)
        {
            GL.UseProgram(Handle);
            GL.UniformMatrix4(_uniformLocations[name], true, ref data);
        }

        /// <summary>
        /// Установить униформный Vector3 в этом шейдере.
        /// </summary>
        /// <param name="name">Имя униформа</param>
        /// <param name="data">Данные для установки</param>
        public void SetVector3(string name, Vector3 data)
        {
            GL.UseProgram(Handle);
            GL.Uniform3(_uniformLocations[name], data);
        }
    }
}
