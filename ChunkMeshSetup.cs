namespace meshing;

public unsafe partial class ChunkMesh
{
    public void PreMeshing()
    {
        Assert(chunk->voxels != null);
        Assert(chunk->dirty);
        Assert(voxelBuffer == null);

        chunk->UnsetDirty();
    }

    public void PostMeshing()
    {
        // Create a buffer if we meshed any faces
        var size = tempMeshData.Length;

        if (size > 0)
        {
            voxelBuffer = new();
            voxelBuffer.BufferData(size, tempMeshData.data);
        }


        MeshVisiterAllocator.Recycle(ref meshVisiter);
        TempStorageAllocator<VoxelVertex>.Recycle(ref tempMeshData);
    }
}