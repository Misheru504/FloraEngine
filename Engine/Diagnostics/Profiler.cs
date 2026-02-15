using FloraEngine.Core;

namespace FloraEngine.Diagnostics;

public class Profiler
{
    private const int SAMPLES_NUMBER = 120;

    private readonly DiagnosticsData _diagnosticsData;
    private readonly float[] _lastFrames = new float[SAMPLES_NUMBER];
    private int _sampleIndex;

    public Profiler(DiagnosticsData diagnosticsData)
    {
        _diagnosticsData = diagnosticsData;
    }

    public void Update(double deltaTime)
    {
        _diagnosticsData.FrameTimeMs = (float) deltaTime * 1000f;
        _lastFrames[_sampleIndex++ % SAMPLES_NUMBER] = (float) deltaTime;
        _diagnosticsData.FPS = (int) (1f / _lastFrames.Average());
    }
}
