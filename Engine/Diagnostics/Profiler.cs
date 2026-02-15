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
        _lastFrames[_sampleIndex % SAMPLES_NUMBER] = (float)deltaTime;
        _sampleIndex++;
        int count = Math.Min(_sampleIndex, SAMPLES_NUMBER);
        _diagnosticsData.FPS = (int)(1f / _lastFrames.Take(count).Average());
    }
}
