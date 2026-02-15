using FloraEngine.Core.Components;
using ImGuiNET;
using System.Numerics;

namespace FloraEngine.UI.Overlays;

internal class MainOverlay : IImGuiOverlay
{
    public string Title { get; } = "Main overlay";
    public int ZOrder => 100;
    public bool IsEnabled { get; set; } = true;

    private readonly DiagnosticsData _diagnosticsData;

    public MainOverlay(DiagnosticsData diagnosticsData)
    {
        _diagnosticsData = diagnosticsData;
    }

    public void Draw(double deltaTime)
    {
        if (!IsEnabled) return; 

        ImGui.SetNextWindowBgAlpha(0.2f);
        ImGui.SetNextWindowPos(Vector2.UnitY * 15);
        if (ImGui.Begin("Overlay", ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove))
        {
            Transform transform = _diagnosticsData.PlayerTransform;
            RenderConfig renderConfig = _diagnosticsData.RenderConfig;

            // App related
            ImGui.Text($"Version: {Program.VERSION}");
            ImGui.Text($"FPS: {_diagnosticsData.FPS:0} ({_diagnosticsData.FrameTimeMs:F2}ms/frame)");
            ImGui.Text($"Screen res.: {Program.WindowResolution}");
            ImGui.Spacing();

            // Rendering related
            ImGui.Text($"Render time: {_diagnosticsData.RenderTimeMs}");
            ImGui.Text($"Vertex count: {renderConfig.VertexCount}");
            ImGui.Text($"Rendering mode: {renderConfig.RenderMode}");
            ImGui.Text($"AOs ? {renderConfig.IsGeneratingAOs}");
            ImGui.Text($"Greedy meshing ? {renderConfig.IsUsingGreedyMeshing}");
            ImGui.Text($"Wireframe ? {renderConfig.IsWireframe}");
            ImGui.Text($"Chunks rendered: {_diagnosticsData.ChunksRendered}");
            ImGui.Spacing();

            // World related
            ImGui.Text($"Seed: {_diagnosticsData.WorldSeed}");
            ImGui.Text($"MaxLOD: {_diagnosticsData.MaxLod}");
            ImGui.Text($"Render distance: {_diagnosticsData.RenderDistance}");
            ImGui.Spacing();

            // Player
            ImGui.Text($"Camera pos: {transform.Position:F2}");
            ImGui.Text($"Camera rot: <{transform.Yaw:F2}, {transform.Pitch:F2}>");
            ImGui.Text($"Camera speed: {_diagnosticsData.MoveSpeed:F2}");
            ImGui.Text($"Chunk pos: {transform.ChunkPos:0}");
            ImGui.Text($"Voxel pos: {transform.LocalVoxelPos:0}");
            ImGui.End();
        }
    }
}
