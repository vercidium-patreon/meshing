namespace meshing;

public unsafe partial class ChunkMesh
{
    public Map m;

    // Store a reference to the linked chunk and its neighbours
    public Chunk* chunk;
    Chunk* chunkXN, chunkXP, chunkYN, chunkYP, chunkZN, chunkZP;

    public MeshVisiter meshVisiter;
    public TempMeshData<VoxelVertex> tempMeshData;

    public VoxelVertexBuffer voxelBuffer;


    // Shortcuts
    int chunkPosX => chunk->chunkPosX;
    int chunkPosY => chunk->chunkPosY;
    int chunkPosZ => chunk->chunkPosZ;

    public Vector3 WorldPos;


    public ChunkMesh(Map m, Chunk* c)
    {
        Assert(c != null);
        Assert(c->Allocated);

        this.m = m;
        chunk = c;

        WorldPos = new(chunkPosX * Constants.ChunkSize, chunkPosY * Constants.ChunkSize, chunkPosZ * Constants.ChunkSize);

        GetNeighbourReferences();
    }

    public void GetNeighbourReferences()
    {
        var chunkAccess = m.GetAccessLocal(chunkPosX, chunkPosY, chunkPosZ);
        var exteriorChunk = m.ShouldMeshExterior ? Map.EMPTY_CHUNK : Map.FULL_CHUNK;

        chunkXN = chunkPosX > 0 ? m.chunks + chunkAccess - CHUNK_STEP_X : exteriorChunk;
        chunkXP = chunkPosX < m.ChunkAmountXM1 ? m.chunks + chunkAccess + CHUNK_STEP_X : exteriorChunk;

        chunkYN = chunkPosY > 0 ? m.chunks + chunkAccess - CHUNK_STEP_Y : exteriorChunk;
        chunkYP = chunkPosY < m.ChunkAmountYM1 ? m.chunks + chunkAccess + CHUNK_STEP_Y : exteriorChunk;

        chunkZN = chunkPosZ > 0 ? m.chunks + chunkAccess - CHUNK_STEP_Z : exteriorChunk;
        chunkZP = chunkPosZ < m.ChunkAmountZM1 ? m.chunks + chunkAccess + CHUNK_STEP_Z : exteriorChunk;
    }

    const int CHUNK_STEP_Y = 1;
    const int CHUNK_STEP_X = Constants.MaxChunkAmount;
    const int CHUNK_STEP_Z = Constants.MaxChunkAmountSquared;
}
