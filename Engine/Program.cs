using FloraEngine.Core.Logging;
using FloraEngine.Diagnostics;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.Drawing;

namespace FloraEngine;

public static class Program
{
    public const string NAME = "Flora-Engine";
    public const string VERSION = "alpha-3";
    private const string APP_NAME = $"{NAME}@{VERSION}";
    public const string ASSETS_FOLDER = "Assets";

    internal static Vector2D<int> WindowResolution { get; private set; } = new Vector2D<int>(1280, 720);
    public static float AspectRatio => (float)_engineWindow.FramebufferSize.X / _engineWindow.FramebufferSize.Y;

    private static GL _graphics = null!;
    private static IWindow _engineWindow = null!;
    private static IInputContext _inputContext = null!;

    public static void Main()
    {
        AppDomain.CurrentDomain.UnhandledException += Reporter.OnUnhandledException;

        Logger.Info($"=== {APP_NAME} - {DateTime.Now} ===", "");
        Logger.Info("Creating window...");

        // Intializing the game window
        WindowOptions options = WindowOptions.Default;
        options.Size = WindowResolution;
        options.Title = APP_NAME;
        options.Samples = 4; // Multisampling (less sharp image)
        _engineWindow = Window.Create(options);
        _engineWindow.VSync = false;
        /* Fixed framerate:
         * Window.UpdatesPerSecond = 180;
         * Window.FramesPerSecond = 180;
         */

        Logger.Info("Window created successfully!");

        _engineWindow.Load += Load;
        _engineWindow.Closing += Closing;
        _engineWindow.FramebufferResize += ChangeResolution;

        _engineWindow.Run();
        _engineWindow.Dispose();
    }

    private static void Load()
    {
        GraphicsLoad();

        Game.Initialize(_graphics, _engineWindow, _inputContext);

        _engineWindow.Update += Game.Instance.Update;
        _engineWindow.Render += Game.Instance.Draw;

        ApplyResolution(WindowResolution);
    }
    private static void GraphicsLoad()
    {
        Logger.Info("Loading OpenGL...");
        if (_engineWindow == null) throw new NullReferenceException("GLWindow is null!");
        _graphics = _engineWindow.CreateOpenGL();

        _inputContext = _engineWindow.CreateInput();

        // Graphics settings
        _graphics.ClearColor(Color.Black); // Background color of the window
        _graphics.Enable(EnableCap.Blend); // Transparency
        _graphics.Enable(EnableCap.CullFace); // Only renders one face of a vertex
        _graphics.Enable(EnableCap.DepthTest); // Hides objects behind others
        _graphics.Enable(EnableCap.Multisample); // MSAA
        _graphics.CullFace(GLEnum.Back); // Face to show when culling
        _graphics.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        _graphics.DepthFunc(DepthFunction.Less);
        _graphics.ClearDepth(1.0f); // Distance

        _graphics.DepthMask(true);
        _graphics.ColorMask(true, true, true, true);
        
        Logger.Info("OpenGL loaded correctly");
    }
    
    public static void ApplyResolution(Vector2D<int> resolution)
    {
        // Using this wrapper so that the frame buffer res is the resolution we want
        
        float scale = (float)_engineWindow.FramebufferSize.X / _engineWindow.Size.X;
        _engineWindow.Size = new Vector2D<int>(
            (int)(resolution.X / scale),
            (int)(resolution.Y / scale)
        );
    }

    private static void ChangeResolution(Vector2D<int> newFramebufferSize)
    {
        _graphics.Viewport(0, 0, (uint)newFramebufferSize.X, (uint)newFramebufferSize.Y);
        WindowResolution = newFramebufferSize;
        
        Game.Instance.ChangeResolution(_engineWindow);
    }

    private static void Closing()
    {
        Logger.Info("Closing...");

        Game.Instance.Closing();

        Logger.Info("See ya!");
        Logger.SaveLogFile();
    }

    public static void CloseGame() => _engineWindow.Close();
}