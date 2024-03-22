using System.Collections.Concurrent;

namespace meshing;

public static class MeshVisiterAllocator
{
    static BlockingCollection<MeshVisiter> chunks = new();

    // Have 32 visiters active but allow higher bursts for when meshing the entire map at once
    const int BURST_THRESHOLD = 32;

    public static MeshVisiter Get()
    {
        // Reuse
        if (chunks.TryTake(out var visiter))
            return visiter;

        // Burst
        return new();
    }


    public static void Recycle(ref MeshVisiter visiter)
    {
        if (visiter == null)
        {
            AssertFalse();
            return;
        }

        // Dispose burst chunks rather than keeping them around forever
        if (chunks.Count >= BURST_THRESHOLD)
        {
            visiter.Dispose();
            return;
        }

        visiter.Comparison++;
        chunks.Add(visiter);
        visiter = null;
    }
}