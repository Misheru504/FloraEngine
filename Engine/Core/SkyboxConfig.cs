namespace FloraEngine.Core;

public enum SkyboxMode
{
    Default = 0,
    Position = 1,
    SkyMask = 2,
    SunMask = 3,
    HorizonMask = 4,
}

public class SkyboxConfig
{
    public SkyboxMode SkyboxMode { get; set; }
}
