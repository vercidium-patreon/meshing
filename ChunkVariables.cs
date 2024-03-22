namespace meshing;

public unsafe partial struct Chunk
{
    // Voxel data
    public Voxel* voxels;
    public int bytes_voxels;

    public bool Allocated => voxels != null && minAltitude != null && maxAltitude != null;

    public bool dirty;
    public bool fake;

    public byte chunkPosX;
    public byte chunkPosY;
    public byte chunkPosZ;

    // Min and max heightmap for faster meshing
    public byte* maxAltitude;
    public byte* minAltitude;
    int bytes_maxAltitude;
    int bytes_minAltitude;
}
