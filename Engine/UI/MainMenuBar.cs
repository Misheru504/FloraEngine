using FloraEngine.Core;
using FloraEngine.Core.Logging;
using ImGuiNET;
using System.Numerics;

namespace FloraEngine.UI;

internal class MainMenuBar : IMainMenuBar
{
    private readonly WindowManager _windowManager;
    private readonly OverlayManager _overlayManager;
    private readonly RenderConfig _renderConfig;
    private readonly DiagnosticsData _diagnosticsData;
    private readonly Action _refreshChunks, _saveWorld;
    private readonly Action<Vector3> _resetPlayer;

    public MainMenuBar(WindowManager windowManager, OverlayManager overlayManager, RenderConfig renderConfig, DiagnosticsData diagnosticsData, Action refreshChunks, Action saveWorld, Action<Vector3> resetPlayer)
    {
        _windowManager = windowManager;
        _overlayManager = overlayManager;
        _renderConfig = renderConfig;
        _diagnosticsData = diagnosticsData;
        _refreshChunks = refreshChunks;
        _saveWorld = saveWorld;
        _resetPlayer = resetPlayer;
    }

    public void DrawBar(double deltaTime)
    {
        if (!ImGui.BeginMainMenuBar()) return;

        ShowGameMenu();
        ShowPlayerMenu();
        ShowWorldMenu();


        if (ImGui.BeginMenu("Windows"))
        {
            foreach(IImGuiWindow window in _windowManager.Windows)
            {
                bool isOpen = window.IsOpen;
                if (ImGui.MenuItem(window.Title, null, ref isOpen)) 
                    window.IsOpen = isOpen;
            }
            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Overlays"))
        {
            foreach (IImGuiOverlay overlay in _overlayManager.overlays)
            {
                bool isOpen = overlay.IsEnabled;
                if (ImGui.MenuItem(overlay.Title, null, ref isOpen)) 
                    overlay.IsEnabled = isOpen;
            }
            ImGui.EndMenu();
        }
        ImGui.EndMainMenuBar();
    }

    private void ShowGameMenu()
    {
        if (!ImGui.BeginMenu("Game")) return;

        if (ImGui.MenuItem("Delete logs folder")) Logger.ClearLogFolder();
        bool isWireframe = _renderConfig.IsWireframe;
        if (ImGui.MenuItem("Wireframe view", null, ref isWireframe))
        {
            _renderConfig.IsWireframe = isWireframe;
        }
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
        if (ImGui.MenuItem("Quit", "ALT+F4")) { Program.CloseGame(); }
        ImGui.EndMenu();
    }

    private void ShowPlayerMenu()
    {
        if (!ImGui.BeginMenu("Player")) return;

        if(ImGui.MenuItem("Freecam", "T", _renderConfig.IsFreecam)) _renderConfig.IsFreecam = !_renderConfig.IsFreecam;
        if (ImGui.MenuItem("Respawn", "R")) _resetPlayer(_diagnosticsData.RespawnPosition);

        ImGui.EndMenu();
    }

    private void ShowWorldMenu()
    {
        if (!ImGui.BeginMenu("World")) return;

        if (ImGui.BeginMenu("Mesher"))
        {
            bool isGenAo = _renderConfig.IsGeneratingAOs;
            if (ImGui.MenuItem("Generate AOs", null, ref isGenAo))
            {
                _renderConfig.IsGeneratingAOs = isGenAo;
                _refreshChunks();
            }
            ImGui.Separator();
            bool isGreedy = _renderConfig.IsUsingGreedyMeshing;
            if (ImGui.MenuItem("Greedy", null, ref isGreedy))
            {
                _renderConfig.IsUsingGreedyMeshing = isGreedy;
                _refreshChunks();
            }
            bool notGreedy = !_renderConfig.IsUsingGreedyMeshing;
            if (ImGui.MenuItem("Culled", null, ref notGreedy))
            {
                _renderConfig.IsUsingGreedyMeshing = !_renderConfig.IsUsingGreedyMeshing;
                _refreshChunks();
            }
            ImGui.EndMenu();
        }
        if(ImGui.MenuItem("Save world to disk")) _saveWorld();

        ImGui.EndMenu();
    }
}
