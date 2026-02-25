using FloraEngine.Core.Data;
using FloraEngine.Core.Logging;
using Silk.NET.OpenGL;
using System.Collections.Concurrent;

namespace FloraEngine.Rendering.Shaders;

public class ShaderWatcher : IDisposable
{
    private GL _graphics;
    private DateTime _lastEvent = DateTime.MinValue;
    private readonly ConcurrentQueue<string> _pendingReloads = new ConcurrentQueue<string>();
    private readonly Dictionary<string, FragVertShader> _shaders = new Dictionary<string, FragVertShader>();
    private readonly FileSystemWatcher _watcher;
    private static readonly string SHADERS_FOLDER = Path.Combine(AppContext.BaseDirectory, Program.ASSETS_FOLDER, "Shaders");


    public ShaderWatcher(GL graphics)
    {
        _graphics = graphics;
        Logger.Debug($"Watching shaders in {SHADERS_FOLDER}");
        _watcher = new FileSystemWatcher(SHADERS_FOLDER) {
            Filter = "*.glsl",
            NotifyFilter = NotifyFilters.LastWrite,
            EnableRaisingEvents = true,
            IncludeSubdirectories = true,
        };
        
        _watcher.Changed += OnFileChanged;
    }

    public void RegisterFragVertShader(string name)
    {
        FragVertShader shader = new FragVertShader(_graphics, TextParser.ReadFile(Path.Combine(SHADERS_FOLDER, $"{name}_vert.glsl")), TextParser.ReadFile(Path.Combine(SHADERS_FOLDER, $"{name}_frag.glsl")));
        _shaders[name] = shader;
    }

    public void OnFileChanged(object _, FileSystemEventArgs e)
    {
        if ((DateTime.Now - _lastEvent).TotalMilliseconds < 200) return;
        
        Logger.Debug("Detected shaders changes...");
        _lastEvent = DateTime.Now;
        _pendingReloads.Enqueue(e.FullPath);
        
    }

    public void Update()
    {
        while (_pendingReloads.TryDequeue(out string _))
        {
            foreach (string name in _shaders.Keys)
            {
                try
                {
                    FragVertShader newShader = new FragVertShader(_graphics, TextParser.ReadFile(Path.Combine(SHADERS_FOLDER, $"{name}_vert.glsl")), TextParser.ReadFile(Path.Combine(SHADERS_FOLDER, $"{name}_frag.glsl")));   
                    _shaders[name] = newShader;
                    Logger.Debug("Shader successfully reloaded !");
                }
                catch (Exception e)
                {
                    Logger.Error($"Unable to load shaders for {name} : {e.Message}");
                    Logger.Debug($"Using old shader cached...");
                }
            }
        }
    }
    
    public FragVertShader GetShader(string name) => _shaders[name];
    
    public void Dispose()
    {
        foreach (FragVertShader shader in _shaders.Values)
        {
            shader.Dispose();
        }
    }
}
