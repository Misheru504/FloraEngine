using FloraEngine.Core;
using FloraEngine.Core.Components;
using FloraEngine.Player;
using FloraEngine.Rendering;
using FloraEngine.UI;
using FloraEngine.UI.Overlays;
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

    public RenderMode RenderingMode;

    private readonly GL _graphics;
    private readonly InputManager _inputManager;
    private readonly PlayerController _playerController;
    private readonly Camera _camera;
    private readonly Renderer _renderer;

    // UI Related
    private readonly ImGuiController _imGuiController;
    private readonly WindowManager _windowManager;
    private readonly OverlayManager _overlayManager;
    private readonly MainMenuBar _mainMenuBar;

    public Camera Camera => _camera;

    public static void Initialize(GL graphics, IWindow window, IInputContext inputContext, IKeyboard keyboard, IMouse mouse)
    {
        _instance = new Game(graphics, window, inputContext, keyboard, mouse);
    }

    public Game(GL graphics, IWindow window, IInputContext inputContext, IKeyboard keyboard, IMouse mouse)
    {
        _graphics = graphics;
        _inputManager = new InputManager(keyboard);

        Transform transform = new Transform()
        {
            Position = Vector3.Zero,
            Up = Vector3.UnitY,
            Forward = -Vector3.UnitZ,
            Direction = Vector3.Zero,

            Yaw = 0,
            Pitch = 0,
        };

        RenderConfig config = new RenderConfig()
        {
            RenderMode = RenderMode.Default,
            IsUsingGreedyMeshing = true,
            IsGeneratingAOs = true,
            VertexCount = 0
        };

        _camera = new Camera(transform);
        _playerController = new PlayerController(_inputManager, mouse, transform);

        _renderer = new Renderer(_graphics, config);


        _imGuiController = new ImGuiController(_graphics, window, inputContext);
        _windowManager = new WindowManager();
        _overlayManager = new OverlayManager();
        _overlayManager.AddWindow(new MainOverlay(transform, config));
        _mainMenuBar = new MainMenuBar(_graphics, _windowManager, config, _playerController);
    }

    public void Update(double deltaTime)
    {
        _playerController.Update(deltaTime);

        _imGuiController.Update((float) deltaTime);
    }

    public void Draw(double deltaTime)
    {
        _renderer.Draw();

        _mainMenuBar.DrawBar(deltaTime);
        _windowManager.DrawAll(deltaTime);
        _overlayManager.DrawAll(deltaTime);
        _imGuiController.Render();
    }

    public void Closing()
    {
        _renderer.Dispose();
    }
}