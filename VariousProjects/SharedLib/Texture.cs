using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OpenTK.Graphics.OpenGL;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace SharedLib
{
    public class Texture
    {
        private readonly int id;
        private readonly int width;
        private readonly int height;
        private readonly TextureTarget textureTarget;

        public Texture(string filePath, TextureTarget textureTarget)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Texture load failed, please specify the correct file path", filePath);
            }

            id = GL.GenTexture();
            this.textureTarget = textureTarget;

            Bind();
            var bmp = new Bitmap(filePath);

            width = bmp.Width;
            height = bmp.Height;

            var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            Alloc(PixelInternalFormat.Rgba, PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);
            SetupFilters(TextureMinFilter.Linear, TextureMagFilter.Linear);
            SetupWraps(TextureWrapMode.Clamp, TextureWrapMode.Clamp);

            bmp.UnlockBits(bmpData);
            UnBind();
        }

        public Texture(int width, int height, TextureTarget textureTarget)
        {
            id = GL.GenTexture();
            this.width = width;
            this.height = height;
            this.textureTarget = textureTarget;
        }

        public void Alloc(PixelInternalFormat internalFormat, PixelFormat format, PixelType type, IntPtr data)
        {
            GL.TexImage2D(textureTarget, 0, internalFormat, width, height, 0, format, type, data);
        }

        public void SetupWraps(TextureWrapMode wrapSMode, TextureWrapMode wrapTMode)
        {
            GL.TextureParameter(id, TextureParameterName.TextureWrapS, (int) wrapSMode);
            GL.TextureParameter(id, TextureParameterName.TextureWrapT, (int) wrapTMode);
        }

        public void SetupFilters(TextureMinFilter minFilter, TextureMagFilter magFilter)
        {
            GL.TextureParameter(id, TextureParameterName.TextureMinFilter, (int) minFilter);
            GL.TextureParameter(id, TextureParameterName.TextureMagFilter, (int) magFilter);
        }

        public TextureTarget GetTextureTarget()
        {
            return textureTarget;
        }

        public void CleanUp()
        {
            GL.DeleteTexture(id);
        }

        public void UnBind()
        {
            GL.BindTexture(textureTarget, 0);
        }

        public void Bind()
        {
            GL.BindTexture(textureTarget, id);
        }

        public int GetWidth()
        {
            return width;
        }

        public int GetHeight()
        {
            return height;
        }

        public int GetId()
        {
            return id;
        }
    }
}
