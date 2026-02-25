using System;
using System.Collections.Generic;
using Silk.NET.OpenGL;
using System.Numerics;

namespace FloraEngine.Rendering.Shaders;

/// <summary>
/// Shaders are programs used in the GPU
/// </summary>
public abstract class Shader : IDisposable
{
    protected private static GL _graphics = null!;
    protected private Dictionary<string, int> _uniformLocations = new Dictionary<string, int>();
    protected private uint _handle;

    /// <summary>
    /// Load a shader into the GPU
    /// </summary>
    /// <param name="type">The type of the shader</param>
    /// <param name="content">The shader code</param>
    /// <returns>The shader handle</returns>
    /// <exception cref="Exception"></exception>
    internal static uint LoadShader(GL graphics, ShaderType type, string content)
    {
        _graphics = graphics;
        uint shader = _graphics.CreateShader(type);

        // Add it to GL
        _graphics.ShaderSource(shader, content);
        _graphics.CompileShader(shader);
        _graphics.GetShader(shader, ShaderParameterName.CompileStatus, out int status);

        // Check for compilation
        if (status != (int)GLEnum.True)
            throw new Exception($"Shader at '{content}' failed to compile: {_graphics.GetShaderInfoLog(shader)}");

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
        if(_uniformLocations.TryGetValue(name, out int location)) return location; // Caching locations

        location = _graphics.GetUniformLocation(_handle, name);
        if (location == -1)
            return -1;

        _uniformLocations[name] = location;

        return location;
    }

    /// <summary>
    /// Changes the value of a uniform in this shader
    /// </summary>
    /// <param name="name">Name of the uniform</param>
    /// <param name="value">Value to set the uniform to</param>
    public void SetUniform(string name, int value) => _graphics.Uniform1(GetUniformLocation(name), value);

    /// <summary>
    /// Changes the value of a uniform in this shader
    /// </summary>
    /// <param name="name">Name of the uniform</param>
    /// <param name="value">Value to set the uniform to</param>
    public void SetUniform(string name, float value) => _graphics.Uniform1(GetUniformLocation(name), value);

    public int GetAttribLocation(string name)
    {
        return _graphics.GetAttribLocation(_handle, name);
    }
    
    /// <summary>
    /// Changes the value of a uniform in this shader
    /// </summary>
    /// <param name="name">Name of the uniform</param>
    /// <param name="value">Value to set the uniform to</param>
    public unsafe void SetUniform(string name, Matrix4x4 value) => _graphics.UniformMatrix4(GetUniformLocation(name), 1, false, (float*)&value);
    
    /// <summary>
    /// Changes the value of a uniform in this shader
    /// </summary>
    /// <param name="name">Name of the uniform</param>
    /// <param name="value">Value to set the uniform to</param>
    public unsafe void SetUniform(string name, ReadOnlySpan<float> value) => _graphics.UniformMatrix4(GetUniformLocation(name), 1, false, value);

    /// <summary>
    /// Changes the value of a uniform in this shader
    /// </summary>
    /// <param name="name">Name of the uniform</param>
    /// <param name="vector">Value to set the uniform to</param>
    public void SetUniform(string name, Vector2 vector) => _graphics.Uniform2(GetUniformLocation(name), vector);

    /// <summary>
    /// Changes the value of a uniform in this shader
    /// </summary>
    /// <param name="name">Name of the uniform</param>
    /// <param name="array">Value to set the uniform to</param>
    public unsafe void SetUniform(string name, Vector2[] array)
    {
        fixed (Vector2* ptr = array)
        {
            _graphics.Uniform2(GetUniformLocation(name), (uint)array.Length, (float*)ptr);
        }
    }

    /// <summary>
    /// Changes the value of a uniform in this shader
    /// </summary>
    /// <param name="name">Name of the uniform</param>
    /// <param name="vector">Value to set the uniform to</param>
    public void SetUniform(string name, Vector3 vector) => _graphics.Uniform3(GetUniformLocation(name), vector);

    /// <summary>
    /// Changes the value of a uniform in this shader
    /// </summary>
    /// <param name="name">Name of the uniform</param>
    /// <param name="array">Value to set the uniform to</param>
    public unsafe void SetUniform(string name, Vector3[] array)
    {
        fixed (Vector3* ptr = array)
        {
            _graphics.Uniform3(GetUniformLocation(name), (uint)array.Length, (float*)ptr);
        }
    }

    /// <summary>
    /// Changes the value of a uniform in this shader
    /// </summary>
    /// <param name="name">Name of the uniform</param>
    /// <param name="vector">Value to set the uniform to</param>
    public void SetUniform(string name, Vector4 vector) => _graphics.Uniform4(GetUniformLocation(name), vector);

    /// <summary>
    /// Changes the value of a uniform in this shader
    /// </summary>
    /// <param name="name">Name of the uniform</param>
    /// <param name="array">Value to set the uniform to</param>
    public unsafe void SetUniform(string name, Vector4[] array)
    {
        fixed (Vector4* ptr = array)
        {
            _graphics.Uniform4(GetUniformLocation(name), (uint)array.Length, (float*)ptr);
        }
    }

    /// <summary>
    /// Sets this shader as active
    /// </summary>
    public void UseProgram() => _graphics.UseProgram(_handle);

    /// <summary>
    /// Delete the shader in the GPU
    /// </summary>
    public void Dispose() => _graphics.DeleteProgram(_handle);
}
