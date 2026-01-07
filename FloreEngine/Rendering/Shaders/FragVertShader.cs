using Silk.NET.OpenGL;
using System.Numerics;

namespace FloreEngine.Rendering.Shaders;

/// <summary>
/// Shaders are programs that runs on the GPU
/// </summary>
internal class FragVertShader : Shader, IDisposable 
{
    public FragVertShader(string vertexShaderCode, string fragmentShaderCode)
    {
        // Reading shaders from file
        uint vertexShader = LoadShader(ShaderType.VertexShader, vertexShaderCode);
        uint fragmentShader = LoadShader(ShaderType.FragmentShader, fragmentShaderCode);
        handle = Graphics.CreateProgram();

        // Attaching the shaders to the handle
        Graphics.AttachShader(handle, vertexShader);
        Graphics.AttachShader(handle, fragmentShader);

        Graphics.LinkProgram(handle);

        // Checking for any failures
        Graphics.GetProgram(handle, ProgramPropertyARB.LinkStatus, out int status);
        if (status != (int)GLEnum.True)
            throw new Exception($"Shaders failed to link : {Graphics.GetProgramInfoLog(handle)}");

        // Deleting the shaders (now that they're stored on the GPU, we do not need them
        Graphics.DeleteShader(vertexShader);
        Graphics.DeleteShader(fragmentShader);
    }
}
