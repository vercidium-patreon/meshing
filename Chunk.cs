namespace meshing;

public unsafe partial struct Chunk
{
    // Client
    public void Reset(int cx, int cy, int cz, bool fake)
    {
        // Ensure we were disposed properly before being re-allocated
        Assert(!Allocated);


        // Store the world position of this chunk
        chunkPosX = (byte)cx;
        chunkPosY = (byte)cy;
        chunkPosZ = (byte)cz;


        // Allocate voxel data
        bytes_voxels = Constants.BYTES_PER_CHUNK;
        voxels = (Voxel*)Allocator.AllocZeroed(bytes_voxels);
        SetDirty();


        // Two fake chunks are allocated, one that's completely empty and one that's full
        // The empty chunk is used for raytracing, and full chunk is used to prevent meshing the outside of the map
        this.fake = fake;


        // To speed up meshing, each chunk stores two heightmaps, which contain the min and max altitude of each column in the chunk.
        //  This saves looping over the whole chunk when meshing
        bytes_maxAltitude = bytes_minAltitude = Constants.ChunkSizeSquared;
        minAltitude = (byte*)Allocator.AllocZeroed(bytes_minAltitude);
        maxAltitude = (byte*)Allocator.AllocZeroed(bytes_maxAltitude);


        // Prepare the min heightmap
        var ptr = minAltitude;
        for (int i = Constants.ChunkSizeSquared; i > 0; i--)
            *ptr++ = Constants.ChunkSize;
    }

    public void SetVoxel(int x, int y, int z, byte index)
    {
        var i = WorldToLocal(x);
        var j = WorldToLocal(y);
        var k = WorldToLocal(z);


        // Update the voxel
        var b = voxels + GetAccessLocal(i, j, k);
        b->Reset(index);


        // Update the client-side min+max heightmap for faster meshing
        if (index > 0)
            OnVoxelAdded(i, j, k);
        else
            OnVoxelRemoved(i, j, k);


        SetDirty();
    }


    // Shortcut functions
    public void SetDirty() => dirty = true;
    public void UnsetDirty() => dirty = false;
    public bool IsDirty() => dirty;

    public static int WorldToLocal(int a) => a & Constants.ChunkMask;
    public static int GetHeightmapAccess(int i, int k) => i | (k * Constants.ChunkSize);

    public static int GetAccessLocal(int i, int j, int k)
    {
        Assert(i >= 0);
        Assert(j >= 0);
        Assert(k >= 0);

        Assert(i < Constants.ChunkSize);
        Assert(j < Constants.ChunkSize);
        Assert(k < Constants.ChunkSize);

        return j | (i * Constants.ChunkSize) | (k * Constants.ChunkSizeSquared);
    }
}
