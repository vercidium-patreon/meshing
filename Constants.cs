namespace meshing;

public static class Constants
{
    public const int ChunkSize = 32;
    public const int ChunkSizeSquared = ChunkSize * ChunkSize;
    public const int ChunkSizeCubed = ChunkSize * ChunkSize * ChunkSize;
    public const int ChunkSizeM1 = ChunkSize - 1; // M1 = 'Minus 1'


    public const int MaxChunkAmount = 32;
    public const int MaxChunkAmountSquared = MaxChunkAmount * MaxChunkAmount;
    public const int MaxChunkAmountCubed = MaxChunkAmount * MaxChunkAmount * MaxChunkAmount;

    public const int ChunkMask = 0x1f;
    public const int ChunkShift = 5;

    public const int VOXELS_PER_CHUNK = ChunkSizeCubed;
    public const int BYTES_PER_VOXEL = sizeof(byte);
    public const int BYTES_PER_CHUNK = VOXELS_PER_CHUNK * BYTES_PER_VOXEL;
}

public enum FaceType : byte
{
    yp = 0,
    yn,
    xp,
    xn,
    zp,
    zn,
}