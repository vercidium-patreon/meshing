namespace meshing;

public struct VoxelVertex
{
    public void Reset(float pX, float pY, float pZ, float nX, float nY, float nZ, float bX, float bY, int tID)
    {
        position = new(pX, pY, pZ);
        normal = new(nX, nY, nZ);
        bary = new(bX, bY);
        textureID = tID;
    }

    public Vector3 position;
    public Vector3 normal;
    public Vector2 bary;
    public int textureID;
}

public unsafe class VoxelVertexBuffer : VertexBuffer<VoxelVertex>
{
    public VoxelVertexBuffer() : base(36) { }

    protected override void SetupVAO()
    {
        Gl.EnableVertexAttribArray(0);
        Gl.EnableVertexAttribArray(1);
        Gl.EnableVertexAttribArray(2);
        Gl.EnableVertexAttribArray(3);

        Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize, null);
        Gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)12);
        Gl.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, vertexSize, (void*)24);
        Gl.VertexAttribIPointer(3, 1, VertexAttribIType.Int, vertexSize, (void*)32);
    }
}