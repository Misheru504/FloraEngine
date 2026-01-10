using FloreEngine.Diagnostics;
using FloreEngine.Rendering;
using ImGuiNET;
using Silk.NET.OpenGL;

namespace FloreEngine.UI;

internal class MainMenuBar : IMainMenuBar
{
    private readonly WindowManager windowManager;

    public MainMenuBar(WindowManager windowManager)
    {
        this.windowManager = windowManager;
    }

    public void DrawBar(double deltaTime)
    {
        if (!ImGui.BeginMainMenuBar()) return;

        ShowGameMenu();
        if (ImGui.BeginMenu("Window"))
        {
            foreach(IImGuiWindow window in windowManager.windows)
            {
                bool isOpen = window.IsOpen;
                if (ImGui.MenuItem(window.Title, null, ref isOpen)) window.IsOpen = !isOpen;
            }
            ImGui.EndMenu();
        }
        ImGui.EndMainMenuBar();
    }

    private static void ShowGameMenu()
    {
        if (!ImGui.BeginMenu("Game")) return;

        if (ImGui.MenuItem("Delete logs folder")) Logger.ClearLogFolder();
        if (ImGui.MenuItem("Wireframe view", null, ref Program.IsWireframe)) Program.Graphics.PolygonMode(GLEnum.FrontAndBack, Program.IsWireframe ? GLEnum.Line : GLEnum.Fill);
        if (ImGui.MenuItem("Test console colors")) Logger.TestColors();
        if (ImGui.BeginMenu("Rendering mode"))
        {
            if (ImGui.MenuItem("Default")) MainRenderer.Instance.RenderingMode = MainRenderer.RenderMode.Default;
            if (ImGui.MenuItem("Depth")) MainRenderer.Instance.RenderingMode = MainRenderer.RenderMode.Depth;
            if (ImGui.MenuItem("Normals")) MainRenderer.Instance.RenderingMode = MainRenderer.RenderMode.Normals;
            if (ImGui.MenuItem("UVs")) MainRenderer.Instance.RenderingMode = MainRenderer.RenderMode.UV;
            ImGui.EndMenu();
        }
        ImGui.Separator();
        if (ImGui.MenuItem("/!\\ Crash game /!\\")) { throw new Exception("You crashed the game on purpose!"); }
        if (ImGui.MenuItem("Quit", "ALT+F4")) { Program.EngineWindow.Close(); }
        ImGui.EndMenu();
    }
}
