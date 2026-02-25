using System;
using Silk.NET.OpenGL;

namespace FloraEngine.Rendering.Shaders;

/// <summary>
/// Shader to process vertices and indices to show them on screen
/// </summary>
public class FragVertShader : Shader, IDisposable 
{
    public FragVertShader(GL graphics, string vertexShaderCode, string fragmentShaderCode)
    {
        _graphics = graphics;

        // Reading shaders from file
        uint vertexShader = LoadShader(_graphics, ShaderType.VertexShader, vertexShaderCode);
        uint fragmentShader = LoadShader(_graphics, ShaderType.FragmentShader, fragmentShaderCode);
        _handle = _graphics.CreateProgram();

        // Attaching the shaders to the handle
        _graphics.AttachShader(_handle, vertexShader);
        _graphics.AttachShader(_handle, fragmentShader);

        _graphics.LinkProgram(_handle);

        // Checking for any failures
        _graphics.GetProgram(_handle, ProgramPropertyARB.LinkStatus, out int status);
        if (status != (int)GLEnum.True)
            throw new Exception($"Shaders failed to link : {_graphics.GetProgramInfoLog(_handle)}");

        // Deleting the shaders (now that they're stored on the GPU, we do not need them
        _graphics.DeleteShader(vertexShader);
        _graphics.DeleteShader(fragmentShader);
    }
}
