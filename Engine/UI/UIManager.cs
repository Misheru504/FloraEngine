namespace FloraEngine.UI;

public class WindowManager
{
    internal readonly List<IImGuiWindow> windows = new List<IImGuiWindow>();

    public void AddWindow(IImGuiWindow window) => windows.Add(window);

    public void DrawAll(double deltaTime)
    {
        foreach(IImGuiWindow window in windows)
        {
            if(window.IsOpen) window.Draw(deltaTime);
        }
    }
}

public class OverlayManager
{
    private readonly List<IImGuiOverlay> overlays = new List<IImGuiOverlay>();

    public void AddWindow(IImGuiOverlay overlay) => overlays.Add(overlay);

    public void DrawAll(double deltaTime)
    {
        foreach (IImGuiOverlay overlay in overlays.OrderBy(ov => ov.ZOrder))
        {
            overlay.Draw(deltaTime);
        }
    }
}
