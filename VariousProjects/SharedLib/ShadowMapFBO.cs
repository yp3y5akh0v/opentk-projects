using System;
using OpenTK.Graphics.OpenGL;

namespace SharedLib
{
    public class ShadowMapFbo
    {
        private readonly int id;
        private readonly int width;
        private readonly int height;
        private readonly Texture texture;
        public ShadowMapFbo(int width, int height)
        {
            id = GL.GenFramebuffer();
            this.width = width;
            this.height = height;

            Bind();
            texture = new Texture(width, height, TextureTarget.Texture2D);
            texture.Bind();
            texture.Alloc(PixelInternalFormat.DepthComponent, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            texture.SetupFilters(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            texture.SetupWraps(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, texture.GetTextureTarget(), texture.GetId(), 0);

            GL.ReadBuffer(ReadBufferMode.None);
            GL.DrawBuffer(DrawBufferMode.None);

            texture.UnBind();
            UnBind();
        }

        public void BindTexture(TextureUnit texUnit)
        {
            GL.ActiveTexture(texUnit);
            GL.BindTexture(texture.GetTextureTarget(), texture.GetId());
        }

        public void UnBindTexture()
        {
            GL.BindTexture(texture.GetTextureTarget(), 0);
        }

        public void CleanUp()
        {
            GL.DeleteFramebuffer(id);
            texture.CleanUp();
        }

        public void Bind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, id);
        }

        public void UnBind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public int GetId()
        {
            return id;
        }

        public int GetWidth()
        {
            return width;
        }

        public int GetHeight()
        {
            return height;
        }
    }
}
