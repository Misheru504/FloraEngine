using FloraEngine.Entities.Player;
using FloraEngine.Rendering;
using FloraEngine.World;
using ImGuiNET;
using System.Numerics;

namespace FloraEngine.UI.Overlays;

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
            Vector3 voxelPos = Player.LocalVoxelPos;

            Vector3 reconstructedPos = Player.ChunkPos + voxelPos;
            ushort voxel = WorldManager.Instance.GetVoxelIdAtWorldPos((int) reconstructedPos.X, (int) reconstructedPos.Y, (int) reconstructedPos.Z, 0);
            string voxelName = Voxel.GetVoxelName(voxel);

            ImGui.Text($"Version: {Program.VERSION}");
            ImGui.Text($"FPS: {Program.FPS:0} ({Program.DeltaFPS*1000:F2}ms/frame)");
            ImGui.Text($"Screen res.: {Program.WindowResolution}");
            ImGui.Spacing();
            ImGui.Text($"Camera pos: {camera.Position:F2}");
            ImGui.Text($"Camera rot: <{camera.Yaw:F2}, {camera.Pitch:F2}>");
            ImGui.Text($"Camera speed: {Player.Instance.Speed:F2}");
            ImGui.Text($"Vertex count: {Renderer.Instance.VertexCount}");
            ImGui.Spacing();
            ImGui.Text($"Seed: {WorldManager.Instance.Noise.Seed}");
            ImGui.Text($"Chunks count (rendered): {WorldManager.Instance.RenderedChunks.Count}");
            ImGui.Text($"Chunks count (loaded): {WorldManager.Instance.LoadedChunks.Count}");
            ImGui.Spacing();
            ImGui.Text($"Chunk pos: {Player.ChunkPos:0}");
            ImGui.Text($"Voxel pos: {voxelPos:0}");
            ImGui.Text($"Reconstruced pos: {reconstructedPos:0}");
            ImGui.Text($"voxel type: {voxelName}");
            ImGui.End();
        }
    }
}
