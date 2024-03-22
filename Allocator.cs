namespace meshing;

public static unsafe class Allocator
{
    public static void* Alloc(int byteCount) => NativeMemory.Alloc((nuint)byteCount);

    public static void* AllocZeroed(int byteCount) => NativeMemory.AllocZeroed((nuint)byteCount);

    public static void Free<T>(ref T* data, ref int bytes) where T : unmanaged
    {
        // Free if not already freed
        if (data != null)
        {
            bytes = 0;
            NativeMemory.Free(data);
        }

        data = null;
    }

    public static void Free<T>(ref T* data) where T : unmanaged
    {
        if (data != null)
            NativeMemory.Free(data);

        data = null;
    }
}
