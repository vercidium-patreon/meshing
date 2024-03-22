namespace meshing;

public unsafe class MeshVisiter
{
    const int BYTES = sizeof(int) * Constants.VOXELS_PER_CHUNK;

    public int Comparison = 1;
    public int* visitXN;
    public int* visitXP;
    public int* visitZN;
    public int* visitZP;
    public int* visitYN;
    public int* visitYP;

    public MeshVisiter()
    {
        visitXN = (int*)Allocator.AllocZeroed(BYTES);
        visitXP = (int*)Allocator.AllocZeroed(BYTES);
        visitZN = (int*)Allocator.AllocZeroed(BYTES);
        visitZP = (int*)Allocator.AllocZeroed(BYTES);
        visitYN = (int*)Allocator.AllocZeroed(BYTES);
        visitYP = (int*)Allocator.AllocZeroed(BYTES);
    }

    public void Dispose()
    {
        Allocator.Free(ref visitXN);
        Allocator.Free(ref visitXP);
        Allocator.Free(ref visitZN);
        Allocator.Free(ref visitZP);
        Allocator.Free(ref visitYN);
        Allocator.Free(ref visitYP);
    }
}