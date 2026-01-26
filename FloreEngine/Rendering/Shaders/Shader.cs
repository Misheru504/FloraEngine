using Silk.NET.OpenGL;
using System.Numerics;

namespace FloreEngine.Rendering.Shaders;

/// <summary>
/// Shaders are programs used in the GPU
/// </summary>
internal abstract class Shader : IDisposable
{
    protected private static GL Graphics => Program.Graphics;
    protected private Dictionary<string, int> uniformLocations = new Dictionary<string, int>();
    protected private uint handle;

    /// <summary>
    /// Load a shader into the GPU
    /// </summary>
    /// <param name="type">The type of the shader</param>
    /// <param name="content">The shader code</param>
    /// <returns>The shader handle</returns>
    /// <exception cref="Exception"></exception>
    internal static uint LoadShader(ShaderType type, string content)
    {
        uint shader = Graphics.CreateShader(type);

        // Add it to GL
        Graphics.ShaderSource(shader, content);
        Graphics.CompileShader(shader);
        Graphics.GetShader(shader, ShaderParameterName.CompileStatus, out int status);

        // Check for compilation
        if (status != (int)GLEnum.True)
            throw new Exception($"Shader at '{content}' failed to compile: {Graphics.GetShaderInfoLog(shader)}");

        return shader;
    }

    /// <summary>
    /// Returns the handle of a uniform
    /// </summary>
    /// <param name="name">Name of the uniform</param>
    /// <returns>The location of the uniform</returns>
    /// <exception cref="Exception"></exception>
    private int GetUniformLocation(string name)
    {
        if(uniformLocations.TryGetValue(name, out int location)) return location; // Caching locations

        location = Graphics.GetUniformLocation(handle, name);
        if (location == -1)
            throw new Exception($"{name} uniform was not found on shader.");

        uniformLocations[name] = location;

        return location;
    }

    /// <summary>
    /// Changes the value of a uniform in this shader
    /// </summary>
    /// <param name="name">Name of the uniform</param>
    /// <param name="value">Value to set the uniform to</param>
    public void SetUniform(string name, int value) => Graphics.Uniform1(GetUniformLocation(name), value);

    /// <summary>
    /// Changes the value of a uniform in this shader
    /// </summary>
    /// <param name="name">Name of the uniform</param>
    /// <param name="value">Value to set the uniform to</param>
    public void SetUniform(string name, float value) => Graphics.Uniform1(GetUniformLocation(name), value);

    /// <summary>
    /// Changes the value of a uniform in this shader
    /// </summary>
    /// <param name="name">Name of the uniform</param>
    /// <param name="value">Value to set the uniform to</param>
    public unsafe void SetUniform(string name, Matrix4x4 value) => Graphics.UniformMatrix4(GetUniformLocation(name), 1, false, (float*)&value);

    /// <summary>
    /// Changes the value of a uniform in this shader
    /// </summary>
    /// <param name="name">Name of the uniform</param>
    /// <param name="vector">Value to set the uniform to</param>
    public void SetUniform(string name, Vector2 vector) => Graphics.Uniform2(GetUniformLocation(name), vector);

    /// <summary>
    /// Changes the value of a uniform in this shader
    /// </summary>
    /// <param name="name">Name of the uniform</param>
    /// <param name="array">Value to set the uniform to</param>
    public unsafe void SetUniform(string name, Vector2[] array)
    {
        fixed (Vector2* ptr = array)
        {
            Graphics.Uniform2(GetUniformLocation(name), (uint)array.Length, (float*)ptr);
        }
    }

    /// <summary>
    /// Changes the value of a uniform in this shader
    /// </summary>
    /// <param name="name">Name of the uniform</param>
    /// <param name="vector">Value to set the uniform to</param>
    public void SetUniform(string name, Vector3 vector) => Graphics.Uniform3(GetUniformLocation(name), vector);

    /// <summary>
    /// Changes the value of a uniform in this shader
    /// </summary>
    /// <param name="name">Name of the uniform</param>
    /// <param name="array">Value to set the uniform to</param>
    public unsafe void SetUniform(string name, Vector3[] array)
    {
        fixed (Vector3* ptr = array)
        {
            Graphics.Uniform3(GetUniformLocation(name), (uint)array.Length, (float*)ptr);
        }
    }

    /// <summary>
    /// Changes the value of a uniform in this shader
    /// </summary>
    /// <param name="name">Name of the uniform</param>
    /// <param name="vector">Value to set the uniform to</param>
    public void SetUniform(string name, Vector4 vector) => Graphics.Uniform4(GetUniformLocation(name), vector);

    /// <summary>
    /// Changes the value of a uniform in this shader
    /// </summary>
    /// <param name="name">Name of the uniform</param>
    /// <param name="array">Value to set the uniform to</param>
    public unsafe void SetUniform(string name, Vector4[] array)
    {
        fixed (Vector4* ptr = array)
        {
            Graphics.Uniform4(GetUniformLocation(name), (uint)array.Length, (float*)ptr);
        }
    }

    /// <summary>
    /// Sets this shader as active
    /// </summary>
    public void UseProgram() => Graphics.UseProgram(handle);

    /// <summary>
    /// Delete the shader in the GPU
    /// </summary>
    public void Dispose() => Graphics.DeleteProgram(handle);
}
