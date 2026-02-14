using Silk.NET.OpenGL;

namespace FloraEngine.Rendering.Shaders;

/// <summary>
/// Shader computing inputs and returns output. Code is executed on the GPU
/// </summary>
internal class ComputeShader : Shader, IDisposable
{
    public ComputeShader(GL graphics, string computeShaderCode)
    {
        _graphics = graphics;
        uint computeShader = LoadShader(_graphics, ShaderType.ComputeShader, computeShaderCode);
        handle = _graphics.CreateProgram();
        _graphics.AttachShader(handle, computeShader);
        _graphics.LinkProgram(handle);

        // Checking for any failures
        _graphics.GetProgram(handle, ProgramPropertyARB.LinkStatus, out int status);
        if (status != (int)GLEnum.True)
            throw new Exception($"Shaders failed to link : {_graphics.GetProgramInfoLog(handle)}");

        _graphics.DeleteShader(computeShader);
    }
}
