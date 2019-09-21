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

        private int CreateShader(string shaderCode, ShaderType shaderType)
        {
            var shaderId = GL.CreateShader(shaderType);

            GL.ShaderSource(shaderId, shaderCode);
            GL.CompileShader(shaderId);
            GL.AttachShader(programId, shaderId);

            return shaderId;
        }

        public string GetVertexInfoLog()
        {
            return GL.GetShaderInfoLog(vertexShaderId);
        }

        public string GetFragmentInfoLog()
        {
            return GL.GetShaderInfoLog(fragmentShaderId);
        }

        public void CreateVertexShader(string shaderCode)
        {
            vertexShaderId = CreateShader(shaderCode, ShaderType.VertexShader);
        }

        public void CreateFragmentShader(string shaderCode)
        {
            fragmentShaderId = CreateShader(shaderCode, ShaderType.FragmentShader);
        }

        public void CreateUniform(string uniformName)
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

        public void SetUniform(string uniformName, int value)
        {
            GL.Uniform1(uniforms[uniformName], value);
        }

        public void Link()
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

        public void Bind()
        {
            GL.UseProgram(programId);
        }

        public void Unbind()
        {
            GL.UseProgram(0);
        }

        public void CleanUp()
        {
            Bind();
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
