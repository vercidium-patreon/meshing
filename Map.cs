namespace meshing;

public unsafe partial class Map
{
    public Map(int mx, int my, int mz)
    {
        // Always allocate 32x32x32 chunks, even though we don't use them all
        bytes_chunks = Marshal.SizeOf<Chunk>() * Constants.MaxChunkAmountCubed;
        chunks = (Chunk*)Allocator.AllocZeroed(bytes_chunks);

        // Ensure map size is divisible by 32
        Assert(mx % Constants.ChunkSize == 0);
        Assert(my % Constants.ChunkSize == 0);
        Assert(mz % Constants.ChunkSize == 0);

        MapSizeX = mx;
        MapSizeY = my;
        MapSizeZ = mz;

        ChunkAmountX = MapSizeX / Constants.ChunkSize;
        ChunkAmountY = MapSizeY / Constants.ChunkSize;
        ChunkAmountZ = MapSizeZ / Constants.ChunkSize;
        ChunkAmountXM1 = ChunkAmountX - 1;
        ChunkAmountYM1 = ChunkAmountY - 1;
        ChunkAmountZM1 = ChunkAmountZ - 1;
    }

    public bool OutOfBounds(int x, int y, int z) => (uint)x >= MapSizeX || (uint)y >= MapSizeY || (uint)z >= MapSizeZ;

    public int GetAccess(int x, int y, int z) => GetAccessLocal(x >> Constants.ChunkShift, y >> Constants.ChunkShift, z >> Constants.ChunkShift);
    public int GetAccessLocal(int f, int g, int h)
    {
        if ((uint)f >= ChunkAmountX || (uint)g >= ChunkAmountY || (uint)h >= ChunkAmountZ)
        {
            AssertFalse();
            return -1;
        }

        var y0 = g;
        var x0 = f * Constants.MaxChunkAmount;
        var z0 = h * Constants.MaxChunkAmountSquared;

        return y0 | x0 | z0;
    }

    public void AddVoxel(int x, int y, int z, byte index)
    {
        if (OutOfBounds(x, y, z))
            return;


        // Get and initialise (if null) the chunk
        var c = InitChunk(x, y, z);


        // Add a voxel to the chunk
        c->SetVoxel(x, y, z, index);
    }

    public Chunk* InitChunk(int x, int y, int z)
    {
        // i, j, k are voxel positions within a chunk
        // f, g, h are chunk positions
        // x, y, z are global voxel positions
        var f = x >> Constants.ChunkShift;
        var g = y >> Constants.ChunkShift;
        var h = z >> Constants.ChunkShift;


        // Precalculate
        var chunkAccess = GetAccessLocal(f, g, h);
        var c = chunks + chunkAccess;


        // If already initialised, return it
        if (c->Allocated)
        {
            Assert(meshChunks[chunkAccess] != null);
            return c;
        }


        // Ensure we're not trying to initialise the empty or full chunk
        Assert(!c->fake);


        // Initialise it and return it
        c->Reset(f, g, h, false);
        Assert(c->Allocated);

        // Create a meshing wrapper for this chunk
        meshChunks[chunkAccess] = new ChunkMesh(this, c);

        Assert(c->voxels != null);

        return c;
    }


    // Meshing data
    public ChunkMesh[] meshChunks = new ChunkMesh[Constants.MaxChunkAmountCubed];

    public long meshingTime;
    public int meshSize;
    public int meshedChunkCount;


    // Map dimensions
    public int MapSizeX;
    public int MapSizeY;
    public int MapSizeZ;
    public int ChunkAmountX;
    public int ChunkAmountY;
    public int ChunkAmountZ;


    // Precalculated values. M1 = 'Minus One'
    public int ChunkAmountXM1;
    public int ChunkAmountYM1;
    public int ChunkAmountZM1;


    // Chunk data
    public int bytes_chunks;
    public Chunk* chunks;


    // Meshing settings
    public bool ShouldMeshExterior = true;
    public bool ShouldMeshBetweenChunks;

}
