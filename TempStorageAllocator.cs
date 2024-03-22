using System.Collections.Concurrent;

namespace meshing;

public static unsafe class TempStorageAllocator<T> where T : unmanaged
{
    // Have 32 visiters active but allow higher bursts for when meshing the entire map at once
    const int BURST_THRESHOLD = 32;

    static BlockingCollection<TempMeshData<T>> storage = new();

    public static TempMeshData<T> Get()
    {
        // Reuse
        if (storage.TryTake(out var data))
            return data;
        
        // Burst
        return new();
    }

    public static void Recycle(ref TempMeshData<T> data)
    {
        if (data == null)
        {
            AssertFalse();
            return;
        }

        // Dispose burst data rather than keeping them around forever
        if (storage.Count >= BURST_THRESHOLD)
        {
            data.Dispose();
            data = null;
            return;
        }

        storage.Add(data);
        data = null;
    }
}