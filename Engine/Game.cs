using System;
using FloraEngine.Core;
using FloraEngine.Diagnostics;
using FloraEngine.Physics;
using FloraEngine.Player;
using FloraEngine.Rendering;
using FloraEngine.Rendering.Meshing;
using FloraEngine.UI;
using FloraEngine.UI.Overlays;
using FloraEngine.World;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using System.Numerics;

namespace FloraEngine;

public class Game
{
    private static Game _instance = null!;
    public static Game Instance => _instance;

    private readonly GL _graphics;
    private readonly InputManager _inputManager;
    private readonly PlayerController _playerController;
    private readonly Camera _camera;
    private readonly Renderer _renderer;
    private readonly DiagnosticsData _diagnosticsData;
    private readonly Profiler _profiler;
    private readonly WorldManager _worldManager;

    // UI Related
    private readonly ImGuiController _imGuiController;
    private readonly WindowManager _windowManager;
    private readonly OverlayManager _overlayManager;
    private readonly MainMenuBar _mainMenuBar;

    public static void Initialize(GL graphics, IWindow window, IInputContext inputContext)
    {
        if (_instance != null) throw new Exception("Only one instance of Game can exist at a time");

        _instance = new Game(graphics, window, inputContext);
    }

    public Game(GL graphics, IWindow window, IInputContext inputContext)
    {
        _graphics = graphics;
        _inputManager = new InputManager(inputContext.Keyboards[0]);

        Transform transform = new Transform()
        {
            Position = Vector3.Zero,
            Up = Vector3.UnitY,
            Forward = Vector3.UnitX,
            Direction = Vector3.UnitX,

            Yaw = 0,
            Pitch = 0,
        };

        RenderConfig renderConfig = new RenderConfig()
        {
            IsGeneratingAOs = true,
            IsUsingGreedyMeshing = true,
            IsWireframe = false,
            IsFullbright = true,
            IsFreecam = false,
            VertexCount = 0,
            RenderMode = RenderMode.Default
        };

        SkyboxConfig skyboxConfig = new SkyboxConfig()
        {
            SkyboxMode = SkyboxMode.Default,
        };

        _diagnosticsData = new DiagnosticsData()
        {
            RenderConfig = renderConfig,
            PlayerTransform = transform,
        };

        _worldManager = new WorldManager(_diagnosticsData, transform);
        _worldManager.LoadWorld(new WorldData { name = "DevWorld", seed = 1444320271, chunks = [] });

        _camera = new Camera(transform);
        _playerController = new PlayerController(_inputManager, inputContext.Mice[0], transform, _diagnosticsData, _worldManager);

        _renderer = new Renderer(_graphics, renderConfig, skyboxConfig, _camera, _worldManager);
        _profiler = new Profiler(_diagnosticsData);

        _imGuiController = new ImGuiController(_graphics, window, inputContext);
        _windowManager = new WindowManager();
        _overlayManager = new OverlayManager();
        _overlayManager.AddWindow(new MainOverlay(_diagnosticsData));
        _mainMenuBar = new MainMenuBar(_windowManager, _overlayManager, renderConfig, skyboxConfig, _diagnosticsData, _worldManager.UpdateChunksMeshes, _worldManager.SaveActiveWorld, _playerController.SetPosition);
    }

    public void Update(double deltaTime)
    {
        _playerController.Update(deltaTime);
        _worldManager.Update(deltaTime);
        
        _profiler.Update(deltaTime);
        _imGuiController.Update((float) deltaTime);
    }

    public void Draw(double deltaTime)
    {
        _graphics.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _renderer.Draw(deltaTime);

        _mainMenuBar.DrawBar(deltaTime);
        _windowManager.DrawAll(deltaTime);
        _overlayManager.DrawAll(deltaTime);
        _imGuiController.Render();
    }

    public void Closing()
    {
        _renderer.Dispose();
        _worldManager.Dispose();
    }
}