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
        return new VoxelData(ID, "");
    }

    public static Voxel? GetVoxelByID(ushort id)
    {
        int indice = 0;

        while (Voxels[indice].ID != id && indice < Voxels.Count - 1)
            indice++;

        Voxel? voxel = Voxels[indice].ID == id ? Voxels[indice] : null;

        return voxel;
    }

    public static Voxel AIR = new Voxel(0, "air", false);
    public static Voxel PURPLE = new Voxel(1, "purple", true);

    public static string GetVoxelName(ushort id)
    {
        return GetVoxelByID(id)!.Name;
    }
}
