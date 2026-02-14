using FloraEngine.Core.Components;
using FloraEngine.Diagnostics;
using FloraEngine.Player;
using FloraEngine.World;
using ImGuiNET;
using Silk.NET.OpenGL;

namespace FloraEngine.UI;

internal class MainMenuBar : IMainMenuBar
{
    private readonly WindowManager _windowManager;
    private readonly RenderConfig _renderConfig;
    private readonly PlayerController _playerController;
    private readonly GL _graphics;

    public MainMenuBar(GL graphics, WindowManager windowManager, RenderConfig renderConfig, PlayerController playerController)
    {
        _graphics = graphics;
        _windowManager = windowManager;
        _renderConfig = renderConfig;
        _playerController = playerController;
    }

    public void DrawBar(double deltaTime)
    {
        if (!ImGui.BeginMainMenuBar()) return;

        ShowGameMenu();
        ShowPlayerMenu();
        ShowWorldMenu();


        if (ImGui.BeginMenu("Window"))
        {
            foreach(IImGuiWindow window in _windowManager.windows)
            {
                bool isOpen = window.IsOpen;
                if (ImGui.MenuItem(window.Title, null, ref isOpen)) window.IsOpen = !isOpen;
            }
            ImGui.EndMenu();
        }
        ImGui.EndMainMenuBar();
    }

    private void ShowGameMenu()
    {
        if (!ImGui.BeginMenu("Game")) return;

        if (ImGui.MenuItem("Delete logs folder")) Logger.ClearLogFolder();
        if (ImGui.MenuItem("Wireframe view", null, ref _renderConfig.IsWireframe)) Program.Graphics.PolygonMode(GLEnum.FrontAndBack, _renderConfig.IsWireframe ? GLEnum.Line : GLEnum.Fill);
        if (ImGui.MenuItem("Test console colors")) Logger.TestColors();
        if (ImGui.BeginMenu("Rendering mode"))
        {
            if (ImGui.MenuItem("Default")) _renderConfig.RenderMode = RenderMode.Default;
            if (ImGui.MenuItem("Depth")) _renderConfig.RenderMode = RenderMode.Depth;
            if (ImGui.MenuItem("Normals")) _renderConfig.RenderMode = RenderMode.Normals;
            if (ImGui.MenuItem("UVs")) _renderConfig.RenderMode = RenderMode.UV;
            if (ImGui.MenuItem("AOs")) _renderConfig.RenderMode = RenderMode.AO;
            if (ImGui.MenuItem("Layer")) _renderConfig.RenderMode = RenderMode.Layer;
            ImGui.EndMenu();
        }
        ImGui.Separator();
        if (ImGui.MenuItem("/!\\ Crash game /!\\")) { throw new Exception("You crashed the game on purpose!"); }
        if (ImGui.MenuItem("Quit", "ALT+F4")) { Program.EngineWindow.Close(); }
        ImGui.EndMenu();
    }

    private void ShowPlayerMenu()
    {
        if (!ImGui.BeginMenu("Player")) return;

        if(ImGui.MenuItem("Freecam", "T", _playerController.IsFreecam)) _playerController.IsFreecam = !_playerController.IsFreecam;
        if (ImGui.MenuItem("Respawn", "R", _playerController.IsFreecam)) _playerController.SetPosition(_playerController.SpawnPosition);

        ImGui.EndMenu();
    }

    private void ShowWorldMenu()
    {
        if (!ImGui.BeginMenu("World")) return;

        if (ImGui.BeginMenu("Mesher"))
        {
            if (ImGui.MenuItem("Generate AOs", null, ref _renderConfig.IsGeneratingAOs)) WorldManager.Instance.UpdateChunksMeshes();
            ImGui.Separator();
            if (ImGui.MenuItem("Greedy", null, ref _renderConfig.IsUsingGreedyMeshing)) WorldManager.Instance.UpdateChunksMeshes();
            bool notGreedy = !_renderConfig.IsUsingGreedyMeshing;
            if (ImGui.MenuItem("Culled", null, ref notGreedy))
            {
                _renderConfig.IsUsingGreedyMeshing = !_renderConfig.IsUsingGreedyMeshing;
                WorldManager.Instance.UpdateChunksMeshes();
            }
            ImGui.EndMenu();
        }
        if(ImGui.MenuItem("Save world to disk")) WorldManager.Instance.SaveActiveWorld();

        ImGui.EndMenu();
    }
}
