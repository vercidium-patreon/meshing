namespace meshing;

public unsafe partial class Map
{
    public static Chunk* EMPTY_CHUNK; // Helper chunk for raytracing through a null chunk
    public static Chunk* FULL_CHUNK;  // Helper chunk to prevent meshing the outside of a map

    static Map()
    {
        // Allocate an empty and a full chunk
        EMPTY_CHUNK = (Chunk*)Allocator.AllocZeroed(Marshal.SizeOf<Chunk>());
        EMPTY_CHUNK->Reset(0, 0, 0, true);

        FULL_CHUNK = (Chunk*)Allocator.AllocZeroed(Marshal.SizeOf<Chunk>());
        FULL_CHUNK->Reset(0, 0, 0, true);


        // Fill the chunk with solid voxels
        var start = FULL_CHUNK->voxels;
        var end = FULL_CHUNK->voxels + Constants.ChunkSizeCubed;

        for (; start < end; start++)
            start->index = 1;
    }
}