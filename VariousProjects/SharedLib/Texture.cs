using System;
using OpenTK.Graphics.OpenGL;

namespace SharedLib
{
    public class Texture
    {
        private readonly int id;
        private readonly int width;
        private readonly int height;
        private readonly TextureTarget textureTarget;

        public Texture(int width, int height, TextureTarget textureTarget)
        {
            id = GL.GenTexture();
            this.width = width;
            this.height = height;
            this.textureTarget = textureTarget;
        }

        public void Alloc(PixelInternalFormat internalFormat, PixelFormat format, PixelType type)
        {
            GL.TexImage2D(textureTarget, 0, internalFormat, width, height, 0, format, type, IntPtr.Zero);
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
