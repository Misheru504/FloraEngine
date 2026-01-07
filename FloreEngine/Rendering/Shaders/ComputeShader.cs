using Silk.NET.OpenGL;

namespace FloreEngine.Rendering.Shaders;

internal class ComputeShader : Shader, IDisposable
{
    public ComputeShader(string computeShaderCode)
    {
        uint computeShader = LoadShader(ShaderType.ComputeShader, computeShaderCode);
        handle = Graphics.CreateProgram();
        Graphics.AttachShader(handle, computeShader);
        Graphics.LinkProgram(handle);

        // Checking for any failures
        Graphics.GetProgram(handle, ProgramPropertyARB.LinkStatus, out int status);
        if (status != (int)GLEnum.True)
            throw new Exception($"Shaders failed to link : {Graphics.GetProgramInfoLog(handle)}");

        Graphics.DeleteShader(computeShader);
    }
}
