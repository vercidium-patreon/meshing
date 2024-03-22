namespace meshing;

public unsafe class ShaderValue
{
    public int location;

    public ShaderValue(ShaderProgram shader, string name)
    {
        location = shader.GetUniformLocation(name);

        if (location < 0)
            Console.WriteLine($"The {name} uniform is not used in the shader");
    }


    // Boolean
    public void Set(bool v)
    {
        Gl.Uniform1(location, v ? 1 : 0);
    }


    // Matrix4
    Matrix4x4* cache_matrix_single = (Matrix4x4*)Allocator.AllocZeroed(Marshal.SizeOf<Matrix4x4>());
    public void Set(Matrix4x4 v)
    {
        *cache_matrix_single = v;
        Gl.UniformMatrix4(location, 1, false, (float*)cache_matrix_single);
    }


    // Vector3
    public void Set3(Vector3 v)
    {
        Gl.Uniform3(location, v.X, v.Y, v.Z);
    }
}
