using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace OpenTkProjects.Core.Graphics;

/// <summary>
/// A linked shader program. Compile and link status are checked and throw with the info log,
/// since GL otherwise reports a broken shader as nothing but a black screen.
/// </summary>
public sealed class Shader : IDisposable
{
    private readonly Dictionary<string, int> uniformLocations = [];
    private bool disposed;

    public Shader(string vertexSource, string fragmentSource)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(vertexSource);
        ArgumentException.ThrowIfNullOrWhiteSpace(fragmentSource);

        int vertex = CompileStage(ShaderType.VertexShader, vertexSource);
        int fragment = CompileStage(ShaderType.FragmentShader, fragmentSource);

        Handle = GL.CreateProgram();
        GL.AttachShader(Handle, vertex);
        GL.AttachShader(Handle, fragment);
        GL.LinkProgram(Handle);

        GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int linkStatus);
        if (linkStatus == 0)
        {
            string log = GL.GetProgramInfoLog(Handle);
            GL.DeleteProgram(Handle);
            GL.DeleteShader(vertex);
            GL.DeleteShader(fragment);
            throw new InvalidOperationException($"Shader program link failed: {log}");
        }

        GL.DetachShader(Handle, vertex);
        GL.DetachShader(Handle, fragment);
        GL.DeleteShader(vertex);
        GL.DeleteShader(fragment);

        CacheUniformLocations();
    }

    public int Handle { get; }

    public void Use() => GL.UseProgram(Handle);

    /// <summary>
    /// Whether the linked program actually uses this uniform. Shared uniform blocks are declared
    /// in an include that several shaders pull in, and GLSL strips whichever ones a given stage
    /// does not reference, so the shared setter has to ask before assigning.
    /// </summary>
    public bool HasUniform(string name) => uniformLocations.ContainsKey(name);

    public void SetUniform(string name, Matrix4 value)
    {
        Matrix4 local = value;
        GL.UniformMatrix4(Locate(name), false, ref local);
    }

    public void SetUniform(string name, Vector2 value) => GL.Uniform2(Locate(name), value);

    public void SetUniform(string name, Vector3 value) => GL.Uniform3(Locate(name), value);

    public void SetUniform(string name, Vector4 value) => GL.Uniform4(Locate(name), value);

    public void SetUniform(string name, float value) => GL.Uniform1(Locate(name), value);

    public void SetUniform(string name, int value) => GL.Uniform1(Locate(name), value);

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        GL.DeleteProgram(Handle);
        disposed = true;
    }

    private static int CompileStage(ShaderType type, string source)
    {
        int handle = GL.CreateShader(type);
        GL.ShaderSource(handle, source);
        GL.CompileShader(handle);

        GL.GetShader(handle, ShaderParameter.CompileStatus, out int status);
        if (status != 0)
        {
            return handle;
        }

        string log = GL.GetShaderInfoLog(handle);
        GL.DeleteShader(handle);
        throw new InvalidOperationException($"{type} compilation failed: {log}");
    }

    private void CacheUniformLocations()
    {
        GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out int count);

        for (int i = 0; i < count; i++)
        {
            string name = GL.GetActiveUniform(Handle, i, out _, out _);
            uniformLocations[name] = GL.GetUniformLocation(Handle, name);
        }
    }

    private int Locate(string name) =>
        uniformLocations.TryGetValue(name, out int location)
            ? location
            : throw new ArgumentException(
                $"Uniform '{name}' is not active in this program. Active: {string.Join(", ", uniformLocations.Keys)}",
                nameof(name));
}
