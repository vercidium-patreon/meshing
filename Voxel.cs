namespace meshing;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe partial struct Voxel
{
    public byte index;

    public void Reset(byte i)
    {
        index = i;
    }
}
