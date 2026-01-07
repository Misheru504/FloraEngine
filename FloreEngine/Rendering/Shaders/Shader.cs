using Silk.NET.OpenGL;
using System.Numerics;

namespace FloreEngine.Rendering.Shaders;

internal abstract class Shader : IDisposable
{
    protected static GL Graphics => Program.Graphics;
    protected uint handle;

    internal static string LoadShaderFromFile(string path)
    {
        if (!File.Exists(path))
            throw new Exception($"Shader not found at {path}!");

        // Gets text and converts it into a shader
        string fileContent = File.ReadAllText(path);
        return fileContent;
    }

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

    public void SetUniform(string name, int value)
    {
        int location = Graphics.GetUniformLocation(handle, name);
        if (location == -1)
            throw new Exception($"{name} uniform was not found on shader.");

        Graphics.Uniform1(location, value);
    }

    public void SetUniform(string name, float value)
    {
        int location = Graphics.GetUniformLocation(handle, name);
        if (location == -1)
            throw new Exception($"{name} uniform was not found on shader.");

        Graphics.Uniform1(location, value);
    }

    public unsafe void SetUniform(string name, Matrix4x4 value)
    {
        int location = Graphics.GetUniformLocation(handle, name);
        if (location == -1)
            throw new Exception($"{name} uniform was not found on shader.");

        Graphics.UniformMatrix4(location, 1, false, (float*)&value);
    }

    public void SetUniform(string name, Vector2 vector)
    {
        int location = Graphics.GetUniformLocation(handle, name);
        if (location == -1)
            throw new Exception($"{name} uniform was not found on shader.");

        Graphics.Uniform2(location, vector);
    }

    public unsafe void SetUniform(string name, Vector2[] array)
    {
        int location = Graphics.GetUniformLocation(handle, name);
        if (location == -1)
            throw new Exception($"{name} uniform was not found on shader.");

        fixed (Vector2* ptr = array)
        {
            Graphics.Uniform2(location, (uint)array.Length, (float*)ptr);
        }
    }

    public void SetUniform(string name, Vector3 vector)
    {
        int location = Graphics.GetUniformLocation(handle, name);
        if (location == -1)
            throw new Exception($"{name} uniform was not found on shader.");

        Graphics.Uniform3(location, vector);
    }

    public unsafe void SetUniform(string name, Vector3[] array)
    {
        int location = Graphics.GetUniformLocation(handle, name);
        if (location == -1)
            throw new Exception($"{name} uniform was not found on shader.");

        fixed (Vector3* ptr = array)
        {
            Graphics.Uniform3(location, (uint)array.Length, (float*)ptr);
        }
    }

    public void SetUniform(string name, Vector4 vector)
    {
        int location = Graphics.GetUniformLocation(handle, name);
        if (location == -1)
            throw new Exception($"{name} uniform was not found on shader.");

        Graphics.Uniform4(location, vector);
    }

    public unsafe void SetUniform(string name, Vector4[] array)
    {
        int location = Graphics.GetUniformLocation(handle, name);
        if (location == -1)
            throw new Exception($"{name} uniform was not found on shader.");

        fixed (Vector4* ptr = array)
        {
            Graphics.Uniform4(location, (uint)array.Length, (float*)ptr);
        }
    }


    public void UseProgram() => Graphics.UseProgram(handle);
    public void Dispose() => Graphics.DeleteProgram(handle);
}
