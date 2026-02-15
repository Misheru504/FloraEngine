namespace FloraEngine.World;

internal class Voxel
{
    public static List<Voxel> Voxels = new List<Voxel>();

    public ushort ID { get; }
    public string Name { get; }
    public bool IsSolid { get; }

    public Voxel(ushort id, string name, bool isSolid)
    {
        ID = id;
        Name = name;

        Voxels.Add(this);
        IsSolid = isSolid;
    }

    public VoxelData GetDefaultData()
    {
        return new VoxelData(ID, 0);
    }

    public static Voxel GetVoxelByID(ushort id) => Voxels[id];

    public static Voxel AIR = new Voxel(0, "air", false);
    public static Voxel GRASS = new Voxel(1, "grass", true);
    public static Voxel DIRT = new Voxel(2, "dirt", true);
    public static Voxel STONE = new Voxel(3, "stone", true);

    public static string GetVoxelName(ushort id)
    {
        return GetVoxelByID(id)!.Name;
    }
}
