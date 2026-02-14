using Silk.NET.OpenGL;

namespace FloraEngine.Rendering.Shaders;

/// <summary>
/// Shader to process vertices and indices to show them on screen
/// </summary>
internal class FragVertShader : Shader, IDisposable 
{
    public FragVertShader(GL graphics, string vertexShaderCode, string fragmentShaderCode)
    {
        _graphics = graphics;

        // Reading shaders from file
        uint vertexShader = LoadShader(_graphics, ShaderType.VertexShader, vertexShaderCode);
        uint fragmentShader = LoadShader(_graphics, ShaderType.FragmentShader, fragmentShaderCode);
        handle = _graphics.CreateProgram();

        // Attaching the shaders to the handle
        _graphics.AttachShader(handle, vertexShader);
        _graphics.AttachShader(handle, fragmentShader);

        _graphics.LinkProgram(handle);

        // Checking for any failures
        _graphics.GetProgram(handle, ProgramPropertyARB.LinkStatus, out int status);
        if (status != (int)GLEnum.True)
            throw new Exception($"Shaders failed to link : {_graphics.GetProgramInfoLog(handle)}");

        // Deleting the shaders (now that they're stored on the GPU, we do not need them
        _graphics.DeleteShader(vertexShader);
        _graphics.DeleteShader(fragmentShader);
    }
}
