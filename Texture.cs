using OpenTK.Graphics.OpenGL4;
using System.Drawing;
using System.Drawing.Imaging;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;
using StbImageSharp;
using System.IO;

namespace ComputerGraphics4
{
    // Вспомогательный класс, подобный Shader, предназначенный для упрощения загрузки текстур.
    public class Texture
    {
        public readonly int Handle;

        public static Texture LoadFromFile(string path)
        {
            // Генерируем хэндл
            int handle = GL.GenTexture();

            // Привязываем хэндл
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, handle);

            // В этом примере мы будем использовать встроенную библиотеку System.Drawing .NET для загрузки текстур.

            // У OpenGL начало текстуры находится в левом нижнем углу, в отличие от верхнего левого угла,
            // поэтому мы говорим StbImageSharp перевернуть изображение при загрузке.
            StbImage.stbi_set_flip_vertically_on_load(1);

            // Здесь мы открываем поток к файлу и передаем его StbImageSharp для загрузки.
            using (Stream stream = File.OpenRead(path))
            {
                ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

                // Теперь, когда наши пиксели подготовлены, пора генерировать текстуру. Мы делаем это с помощью GL.TexImage2D.
                // Аргументы:
                //   Тип текстуры, которую мы генерируем. Существует несколько различных типов текстур, но сейчас нам нужна только Texture2D.
                //   Уровень детализации. Мы можем начать с меньшего мипа (если захотим), но в этом нет необходимости, поэтому оставляем 0.
                //   Целевой формат пикселей. Это формат, в котором OpenGL будет хранить наше изображение.
                //   Ширина изображения
                //   Высота изображения.
                //   Граница изображения. Это всегда должно быть 0; это устаревший параметр, который Khronos никогда не убрал.
                //   Формат пикселей, объясненный выше. Поскольку мы загрузили пиксели как RGBA ранее, нам нужно использовать PixelFormat.Rgba.
                //   Тип данных пикселей.
                //   И, наконец, сами пиксели.
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
            }

            // Теперь, когда наша текстура загружена, мы можем установить несколько параметров, влияющих на то, как изображение будет отображаться при рендеринге.

            // Сначала мы устанавливаем параметры min и mag фильтра. Эти параметры используются для увеличения и уменьшения текстуры соответственно.
            // Здесь мы используем Linear для обеих. Это означает, что OpenGL постарается смешать пиксели, что приведет к размытию текстур при слишком сильном масштабировании.
            // Вы также можете использовать (среди других вариантов) Nearest, который просто захватывает ближайший пиксель, делая текстуру пикселизированной при слишком большом масштабировании.
            // ПРИМЕЧАНИЕ: Выбор по умолчанию для обоих параметров - LinearMipmap. Если оставить их по умолчанию, но не генерировать мипмапы,
            // ваше изображение вообще не будет отображаться (обычно будет черным).
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            // Теперь устанавливаем режим обертки. S - это ось X, а T - ось Y.
            // Мы устанавливаем это в Repeat, чтобы текстуры повторялись при обертывании. Это не продемонстрировано здесь, так как текстурные координаты точно совпадают.
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            // Далее генерируем мипмапы.
            // Мипмапы - это меньшие копии текстуры, уменьшенные в размере. Каждый уровень мипмапа в два раза меньше предыдущего.
            // Сгенерированные мипмапы продолжаются вплоть до одного пикселя.
            // OpenGL автоматически переключается между мипмапами, когда объект оказывается на достаточном расстоянии.
            // Это предотвращает муаровые эффекты, а также экономит пропускную способность текстуры.
            // Здесь вы можете увидеть и прочитать о муаровых эффектах https://en.wikipedia.org/wiki/Moir%C3%A9_pattern
            // Вот пример действия мипов https://en.wikipedia.org/wiki/File:Mipmap_Aliasing_Comparison.png
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return new Texture(handle);
        }

        public Texture(int glHandle)
        {
            Handle = glHandle;
        }

        // Активировать текстуру
        // Можно привязывать несколько текстур, если вашему шейдеру нужно больше одной.
        // Если вы хотите сделать это, используйте GL.ActiveTexture, чтобы установить, к какому слоту привязывает GL.BindTexture.
        // Стандарт OpenGL требует, чтобы было как минимум 16, но в зависимости от вашей графической карты может быть и больше.
        public void Use(TextureUnit unit)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }
    }
}
