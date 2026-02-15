using System.Numerics;

namespace FloraEngine.Core;

public class Transform
{
    private float _pitch;

    public Vector3 Position { get; set; }
    public Vector3 Up { get; set; }
    public Vector3 Forward { get; set; }
    public Vector3 Direction { get; set; }

    public Vector3 ChunkPos => MathUtils.WorldToChunkCoord(Position, WorldConstants.CHUNK_SIZE);
    public Vector3 LocalVoxelPos => MathUtils.WorldToTilePosition(Position);

    public float Yaw { get; set; }
    public float Pitch { get { return _pitch; } set { _pitch = Math.Clamp(value, -89.0f, 89.0f); } }
}