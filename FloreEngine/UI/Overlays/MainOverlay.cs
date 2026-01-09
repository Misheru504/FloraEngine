using FloreEngine.Rendering;
using FloreEngine.World;
using ImGuiNET;
using System.Numerics;

namespace FloreEngine.UI.Overlays;

internal class MainOverlay : IImGuiOverlay
{
    public int ZOrder => 100;

    public void Draw(double deltaTime)
    {
        ImGui.SetNextWindowBgAlpha(0.2f);
        ImGui.SetNextWindowPos(Vector2.UnitY * 15);
        if (ImGui.Begin("Overlay", ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove))
        {
            Camera camera = Camera.Instance;

            ImGui.Text($"Version: {Program.VERSION}");
            ImGui.Text($"FPS: {Program.FPS:0} ({Program.DeltaFPS*1000:F2}ms/frame)");
            ImGui.Text($"Screen res.: {Program.WindowResolution}");
            ImGui.Spacing();
            ImGui.Text($"Camera pos: {camera.Position:F2}");
            ImGui.Text($"Camera rot: <{camera.Yaw:F2}, {camera.Pitch:F2}>");
            ImGui.Text($"Camera speed: {Controller.Instance.Speed:F2}");
            ImGui.Spacing();
            ImGui.Text($"Seed: {WorldManager.Instance.Noise.Seed}");
            ImGui.Text($"Chunk pos: {Controller.ChunkPos:0}");
            ImGui.End();
        }
    }
}
