namespace meshing;

public unsafe class TempMeshData<T> where T : unmanaged
{
    const int MAX_SIZE = Constants.ChunkSizeCubed * 6 * 6; // 6 faces, 6 vertices

    public T* data;
    public T* write;

    public int Length => (int)(write - data);

    public TempMeshData()
    {
        data = (T*)Allocator.Alloc(sizeof(T) * MAX_SIZE);
    }

    public void PreMeshing()
    {
        write = data;
    }

    public void Dispose()
    {
        Allocator.Free(ref data);
        write = null;
    }
}