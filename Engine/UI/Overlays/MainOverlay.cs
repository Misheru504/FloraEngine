using FloraEngine.Core.Components;
using FloraEngine.Rendering;
using FloraEngine.World;
using ImGuiNET;
using System.Numerics;

namespace FloraEngine.UI.Overlays;

internal class MainOverlay : IImGuiOverlay
{
    public int ZOrder => 100;

    private Transform _transform;
    public MainOverlay(Transform transform)
    {
        _transform = transform;
    }

    public void Draw(double deltaTime)
    {
        ImGui.SetNextWindowBgAlpha(0.2f);
        ImGui.SetNextWindowPos(Vector2.UnitY * 15);
        if (ImGui.Begin("Overlay", ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove))
        {

            Vector3 reconstructedPos = _transform.ChunkPos + _transform.LocalVoxelPos;
            ushort voxel = WorldManager.Instance.GetVoxelIdAtWorldPos((int) reconstructedPos.X, (int) reconstructedPos.Y, (int) reconstructedPos.Z, 0);
            string voxelName = Voxel.GetVoxelName(voxel);

            ImGui.Text($"Version: {Program.VERSION}");
            ImGui.Text($"FPS: {Program.FPS:0} ({Program.DeltaFPS*1000:F2}ms/frame)");
            ImGui.Text($"Screen res.: {Program.WindowResolution}");
            ImGui.Spacing();
            ImGui.Text($"Camera pos: {_transform.Position:F2}");
            ImGui.Text($"Camera rot: <{_transform.Yaw:F2}, {_transform.Pitch:F2}>");
            //ImGui.Text($"Camera speed: {PlayerControllerOld.Instance.Speed:F2}");
            ImGui.Text($"Vertex count: {Renderer.Instance.VertexCount}");
            ImGui.Spacing();
            ImGui.Text($"Seed: {WorldManager.Instance.Noise.Seed}");
            ImGui.Text($"Chunks count (rendered): {WorldManager.Instance.RenderedChunks.Count}");
            ImGui.Spacing();
            ImGui.Text($"Chunk pos: {Game.Instance.Camera.Transform.ChunkPos:0}");
            ImGui.Text($"Voxel pos: {_transform.LocalVoxelPos:0}");
            ImGui.Text($"Reconstruced pos: {reconstructedPos:0}");
            ImGui.Text($"voxel type: {voxelName}");
            ImGui.End();
        }
    }
}
