namespace FloraEngine.UI;

public class WindowManager
{
    public List<IImGuiWindow> Windows { get; } = new List<IImGuiWindow>();

    public void AddWindow(IImGuiWindow window) => Windows.Add(window);

    public void DrawAll(double deltaTime)
    {
        foreach(IImGuiWindow window in Windows)
        {
            if(window.IsOpen) window.Draw(deltaTime);
        }
    }
}

public class OverlayManager
{
    public List<IImGuiOverlay> overlays { get; } = new List<IImGuiOverlay>();

    public void AddWindow(IImGuiOverlay overlay)
    {
        overlays.Add(overlay);
        overlays.OrderBy(ov => ov.ZOrder);
    }

    public void DrawAll(double deltaTime)
    {
        foreach (IImGuiOverlay overlay in overlays)
        {
            if(overlay.IsEnabled) overlay.Draw(deltaTime);
        }
    }
}
