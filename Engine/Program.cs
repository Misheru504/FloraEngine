using FloraEngine.Diagnostics;
using FloraEngine.World;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.Drawing;

namespace FloraEngine;

public static class Program
{
    public const string NAME = "Flora-Engine";
    public const string VERSION = "alpha-2";
    private const string APP_NAME = $"{NAME}@{VERSION}";

    internal static Vector2D<int> WindowResolution { get; private set; } = new Vector2D<int>(1280, 720);

    public static float AspectRatio => (float)WindowResolution.X / WindowResolution.Y;

    private static GL _graphics = null!;
    private static IWindow _engineWindow = null!;
    private static IInputContext _inputContext = null!;

    public static void Main()
    {
        AppDomain.CurrentDomain.UnhandledException += Reporter.OnUnhandledException;

        Logger.Print($"=== {APP_NAME} - {DateTime.Now} ===", Logger.LogLevel.INFO, true, "");
        Logger.Print("Creating window...");

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

        Logger.Print("Window created successfully!");

        _engineWindow.Load += Load;
        _engineWindow.Closing += Closing;
        _engineWindow.FramebufferResize += ChangeResolution;

        _engineWindow.Run();
        _engineWindow.Dispose();

        Console.ReadKey();
    }

    private static void Load()
    {
        GraphicsLoad();

        Game.Initialize(_graphics, _engineWindow, _inputContext);

        _engineWindow.Update += Game.Instance.Update;
        _engineWindow.Render += Game.Instance.Draw;
    }
    private static void GraphicsLoad()
    {
        Logger.Print("Loading OpenGL...");
        if (_engineWindow == null) throw new NullReferenceException("GLWindow is null!");
        _graphics = _engineWindow.CreateOpenGL();

        _inputContext = _engineWindow.CreateInput();

        // Graphics settings
        _graphics.ClearColor(Color.CornflowerBlue); // Background color of the window
        _graphics.Enable(EnableCap.Blend); // Transparency
        _graphics.Enable(EnableCap.CullFace); // Only renders one face of a vertex
        _graphics.Enable(EnableCap.DepthTest); // Hides objects behind others
        _graphics.CullFace(GLEnum.Back); // Face to show when culling
        _graphics.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        _graphics.DepthFunc(DepthFunction.Less);
        _graphics.ClearDepth(1.0f); // Distance

        _graphics.DepthMask(true);
        _graphics.ColorMask(true, true, true, true);

        Logger.Print("OpenGL loaded correctly");
    }

    public static void ChangeResolution(Vector2D<int> newSize)
    {
        _graphics.Viewport(newSize);
        WindowResolution = newSize;
    }

    private static void Closing()
    {
        Logger.Print("Closing...");

        Game.Instance.Closing();

        Logger.Print("See ya!");
        Logger.SaveLogFile();
    }

    public static void CloseGame() => _engineWindow.Close();
}