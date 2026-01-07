using FloreEngine.Diagnostics;
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
        
        if (ImGui.BeginMenu("Game"))
        {
            if (ImGui.MenuItem("Delete logs folder")) { Logger.ClearLogFolder(); }
            if (ImGui.MenuItem("Wireframe view", null, ref Program.IsWireframe)) { Program.Graphics.PolygonMode(GLEnum.FrontAndBack, Program.IsWireframe ? GLEnum.Line : GLEnum.Fill); }
            if (ImGui.MenuItem("Test console colors")) { Logger.TestColors(); }
            ImGui.Separator();
            if (ImGui.MenuItem("/!\\ Crash game /!\\")) { throw new Exception("You crashed the game on purpose!"); }
            if (ImGui.MenuItem("Quit", "ALT+F4")) { Program.EngineWindow.Close(); }
            ImGui.EndMenu();
        }
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
}
