using FloraEngine.Core.Components;
using ImGuiNET;
using System.Numerics;

namespace FloraEngine.UI.Overlays;

internal class MainOverlay : IImGuiOverlay
{
    public int ZOrder => 100;

    private readonly DiagnosticsData _diagnosticsData;

    public MainOverlay(DiagnosticsData diagnosticsData)
    {
        _diagnosticsData = diagnosticsData;
    }

    public void Draw(double deltaTime)
    {
        ImGui.SetNextWindowBgAlpha(0.2f);
        ImGui.SetNextWindowPos(Vector2.UnitY * 15);
        if (ImGui.Begin("Overlay", ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove))
        {
            Transform transform = _diagnosticsData.PlayerTransform;
            RenderConfig renderConfig = _diagnosticsData.RenderConfig;

            Vector3 reconstructedPos = transform.ChunkPos + transform.LocalVoxelPos;

            ImGui.Text($"Version: {Program.VERSION}");
            ImGui.Text($"FPS: {_diagnosticsData.FPS:0} ({_diagnosticsData.FrameTimeMs:F2}ms/frame)");
            ImGui.Text($"Screen res.: {Program.WindowResolution}");
            ImGui.Spacing();
            ImGui.Text($"Camera pos: {transform.Position:F2}");
            ImGui.Text($"Camera rot: <{transform.Yaw:F2}, {transform.Pitch:F2}>");
            ImGui.Text($"Camera speed: {_diagnosticsData.MoveSpeed:F2}");
            ImGui.Text($"Vertex count: {renderConfig.VertexCount}");
            ImGui.Spacing();
            ImGui.Text($"Seed: {_diagnosticsData.WorldSeed}");
            ImGui.Text($"Chunks count (rendered): {_diagnosticsData.ChunksLoaded}");
            ImGui.Spacing();
            ImGui.Text($"Chunk pos: {Game.Instance.Camera.Transform.ChunkPos:0}");
            ImGui.Text($"Voxel pos: {transform.LocalVoxelPos:0}");
            ImGui.Text($"Reconstruced pos: {reconstructedPos:0}");
            ImGui.End();
        }
    }
}
