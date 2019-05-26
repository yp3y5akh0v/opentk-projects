using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace SharedLib
{
    public class ShaderProgram
    {
        private readonly int programId;

        private int vertexShaderId;

        private int fragmentShaderId;

        private readonly Dictionary<string, int> uniforms;

        public ShaderProgram()
        {
            programId = GL.CreateProgram();
            uniforms = new Dictionary<string, int>();
        }

        private int createShader(string shaderCode, ShaderType shaderType)
        {
            var shaderId = GL.CreateShader(shaderType);

            GL.ShaderSource(shaderId, shaderCode);
            GL.CompileShader(shaderId);
            GL.AttachShader(programId, shaderId);

            return shaderId;
        }

        public void createVertexShader(string shaderCode)
        {
            vertexShaderId = createShader(shaderCode, ShaderType.VertexShader);
        }

        public void createFragmentShader(string shaderCode)
        {
            fragmentShaderId = createShader(shaderCode, ShaderType.FragmentShader);
        }

        public void createUniform(string uniformName)
        {
            int uniformLocation = GL.GetUniformLocation(programId, uniformName);
            uniforms.Add(uniformName, uniformLocation);
        }

        public void SetUniform(string uniformName, Matrix4 value)
        {
            GL.UniformMatrix4(uniforms[uniformName], false, ref value);
        }

        public void SetUniform(string uniformName, Vector3 value)
        {
            GL.Uniform3(uniforms[uniformName], value);
        }

        public void SetUniform(string uniformName, float value)
        {
            GL.Uniform1(uniforms[uniformName], value);
        }

        public void link()
        {
            GL.LinkProgram(programId);

            if (vertexShaderId != 0)
            {
                GL.DetachShader(programId, vertexShaderId);
            }

            if (fragmentShaderId != 0)
            {
                GL.DetachShader(programId, fragmentShaderId);
            }
        }

        public void bind()
        {
            GL.UseProgram(programId);
        }

        public void unbind()
        {
            GL.UseProgram(0);
        }

        public void CleanUp()
        {
            bind();
            if (programId != 0)
            {
                GL.DeleteProgram(programId);
            }

            if (vertexShaderId != 0)
            {
                GL.DeleteShader(vertexShaderId);
            }

            if (fragmentShaderId != 0)
            {
                GL.DeleteShader(fragmentShaderId);
            }
        }
    }
}
