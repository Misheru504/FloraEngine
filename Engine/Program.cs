using FloraEngine.Player;
using FloreEngine.Diagnostics;
using FloreEngine.Rendering;
using FloreEngine.UI;
using FloreEngine.UI.Overlays;
using FloreEngine.World;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using System.Drawing;
using System.Numerics;

namespace FloreEngine;

public static class Program
{
    public const string NAME = "Flora-Engine";
    public const string VERSION = "alpha-0";
    internal static Vector2D<int> WindowResolution = new Vector2D<int>(1280, 720);
    private static readonly string appName = $"{NAME}@{VERSION}";

    internal static bool IsWireframe = false;

    public static float AspectRatio => (float)WindowResolution.X / WindowResolution.Y;

    public static GL Graphics { get; private set; } = null!;
    public static IWindow EngineWindow { get; private set; } = null!;
    public static IInputContext InputContext { get; private set; } = null!;
    public static IKeyboard Keyboard { get; private set; } = null!;
    public static ImGuiController ImGuiController { get; private set; } = null!;
    public static WindowManager WindowManager { get; private set; } = null!;
    public static OverlayManager OverlayManager { get; private set; } = null!;  

    internal static MainMenuBar MainMenuBar { get; private set; } = null!;

    private static BoxColliderAA spawnCollider = new BoxColliderAA(Vector3.Zero, 16, 16, 16);

    public static void Main()
    {
        AppDomain.CurrentDomain.UnhandledException += Reporter.OnUnhandledException;

        Logger.Print($"=== {appName} - {DateTime.Now} ===", Logger.LogLevel.INFO, true, "");
        Logger.Print("Creating window...");

        // Intializing the game window
        WindowOptions options = WindowOptions.Default;
        options.Size = WindowResolution;
        options.Title = appName;
        options.Samples = 4; // Multisampling (less sharp image)
        EngineWindow = Window.Create(options);
        EngineWindow.VSync = false;
        /* Fixed framerate:
         * Window.UpdatesPerSecond = 180;
         * Window.FramesPerSecond = 180;
         */

        Logger.Print("Window created successfully!");


        EngineWindow.Load += Load;
        EngineWindow.Update += Update;
        EngineWindow.Render += Render;
        EngineWindow.Closing += Closing;
        EngineWindow.FramebufferResize += FrameBufferResize;

        EngineWindow.Run();
        EngineWindow.Dispose();

        Console.ReadKey();
    }

    public static void Load()
    {
        GraphicsLoad();

        ImGuiController = new ImGuiController(Graphics, EngineWindow, InputContext);
        WindowManager = new WindowManager();
        OverlayManager = new OverlayManager();
        OverlayManager.AddWindow(new MainOverlay());
        MainMenuBar = new MainMenuBar(WindowManager);
    }
    private static void GraphicsLoad()
    {
        Logger.Print("Loading OpenGL...");
        if (EngineWindow == null) throw new NullReferenceException("GLWindow is null!");
        Graphics = EngineWindow.CreateOpenGL();

        InputContext = EngineWindow.CreateInput();
        if (InputContext.Keyboards.Count != 0)
        {
            Keyboard = InputContext.Keyboards[0];
            Keyboard.KeyDown += KeyDown;
        }

        if (InputContext.Mice.Count != 0)
        {
            InputContext.Mice[0].Cursor.CursorMode = CursorMode.Raw;
            InputContext.Mice[0].MouseMove += Player.Instance.MouseMove;
            InputContext.Mice[0].Scroll += Player.Instance.MouseWheel;
        }

        // Graphics settings
        Graphics.ClearColor(Color.CornflowerBlue); // Background color of the window
        Graphics.Enable(EnableCap.Blend); // Transparency
        Graphics.Enable(EnableCap.CullFace); // Only renders one face of a vertex
        Graphics.CullFace(GLEnum.Back); // Face to show when culling
        Graphics.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        Graphics.Enable(EnableCap.DepthTest);

        Graphics.ClearDepth(1.0f); // Distance
        Graphics.DepthFunc(DepthFunction.Less);

        Graphics.DepthMask(true);
        

        Graphics.ColorMask(true, true, true, true);

        Logger.Print("OpenGL loaded correctly");
    }

    public static void Update(double deltaTime)
    {
        ComputeFPS(deltaTime);

        //if (BoxColliderAA.IsColliding(spawnCollider, Controller.Instance.Collider)) Console.WriteLine($"{EngineWindow.Time} Inside!");

        ImGuiController.Update((float)deltaTime);
        Player.Instance.Update((float)deltaTime);
        WorldManager.Instance.Update(deltaTime);
    }

    public static void Render(double deltaTime)
    {
        Graphics.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        Renderer.Instance.Draw();


        MainMenuBar.DrawBar(deltaTime);
        WindowManager.DrawAll(deltaTime);
        OverlayManager.DrawAll(deltaTime);
        ImGuiController.Render();
    }

    public static void KeyDown(IKeyboard keyboard, Key key, int keyCode)
    {
        switch (key)
        {
            case Key.Escape:
                EngineWindow.Close();
                break;
            case Key.T:
                Player.Instance.IsFreecamMovement = !Player.Instance.IsFreecamMovement;
                break;
        }
    }

    public static void FrameBufferResize(Vector2D<int> newSize)
    {
        Graphics.Viewport(newSize);
        WindowResolution = newSize;
    }


    public static double FPS { get; private set; } = 0;
    public static double DeltaFPS { get; private set; } = 0;
    private static double totalTime = 1;
    public static void ComputeFPS(double deltaTime)
    {
        if (totalTime >= 1)
        {
            totalTime = 0;
            FPS = 1d / deltaTime;
            DeltaFPS = deltaTime;
        }
        totalTime += deltaTime;
    }

    public static void Closing()
    {
        Logger.Print("Closing...");

        Renderer.Instance.Dispose();

        WorldManager.Instance.Dispose();

        Logger.Print("See ya!");
        Logger.SaveLogFile();
    }
}