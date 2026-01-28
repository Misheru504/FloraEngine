namespace FloreEngine.UI;

/// <summary>
/// Base interface for UI elements
/// </summary>
public interface IImGuiDrawable
{
    void Draw(double deltaTime);
}

/// <summary>
/// Interface for normal windows
/// </summary>
public interface IImGuiWindow : IImGuiDrawable
{
    public string Title { get; }
    public bool IsOpen { get; set; }
}

/// <summary>
/// Interface for overlays
/// </summary>
public interface IImGuiOverlay : IImGuiDrawable
{
    public int ZOrder { get; } // Draw order
}

/// <summary>
/// Interface for the main menu bar
/// </summary>
public interface IMainMenuBar
{
    void DrawBar(double deltaTime);
}
