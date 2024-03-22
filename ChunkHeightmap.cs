namespace meshing;


public unsafe partial struct Chunk
{
    public void OnVoxelAdded(int i, int j, int k)
    {
        int access = GetHeightmapAccess(i, k);

        if (j < *(minAltitude + access))
            *(minAltitude + access) = (byte)j;

        var max = maxAltitude + access;
        if (j >= *max)
            *max = (byte)j;
    }

    public void OnVoxelRemoved(int i, int j, int k)
    {
        // Precalculate 1D array accesses
        var hAccess = GetHeightmapAccess(i, k);
        var xzAccess = i * Constants.ChunkSize + k * Constants.ChunkSizeSquared;
        var access = xzAccess + j;

        // Update the min
        UpdateMinHeightmap(j, hAccess, access, xzAccess);

        // Update the max
        UpdateMaxHeightmap(j, hAccess, access, xzAccess);
    }

    void UpdateMinHeightmap(int j, int hAccess, int access, int xzAccess)
    {
        var min = minAltitude + hAccess;


        // Bail if we didn't remove the lowest voxel
        if (*min != j && *min != (byte)Constants.ChunkSize)
            return;


        // Calculate how far up to search
        var chunkTop = xzAccess + Constants.ChunkSize;
        var amount = chunkTop - access;
        var ptr = voxels + access;


        // Search up until we find a voxel
        for (; amount > 0; amount--)
        {
            if (ptr++->index == 0)
                continue;

            // Store the Y part of the access in the heightmap
            *min = (byte)(access & Constants.ChunkMask);
            return;
        }


        // If no voxel was found
        *min = (byte)Constants.ChunkSize;
    }

    void UpdateMaxHeightmap(int j, int hAccess, int access, int xzAccess)
    {
        var max = maxAltitude + hAccess;


        // Bail if we didn't remove the highest voxel
        if (*max != j && *max != 0)
            return;


        // Calculate how far down to search
        var chunkBottom = xzAccess;
        var amount = access - chunkBottom;
        var ptr = voxels + access;


        // Search down until we find a voxel
        for (; amount > 0; amount--)
        {
            if (ptr--->index == 0)
                continue;

            // Store the Y part of the access in the heightmap
            *max = (byte)(access & Constants.ChunkMask);
            return;
        }


        // If no voxel was found
        *max = 0;
    }
}
