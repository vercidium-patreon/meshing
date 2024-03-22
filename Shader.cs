namespace meshing;

public class Shader
{
    public Shader(ShaderType type, string source)
    {
        // Create and compile a shader
        shaderHandle = Gl.CreateShader(type);
        Gl.ShaderSource(shaderHandle, source);
        Gl.CompileShader(shaderHandle);


        // Check the log
        Gl.GetShader(shaderHandle, ShaderParameterName.InfoLogLength, out var logLen);

        if (logLen > 0)
        {
            Gl.GetShaderInfoLog(shaderHandle, (uint)logLen, out _, out string log);

            if (!string.IsNullOrEmpty(log))
                Console.WriteLine($"Shader compile log:\n{log}");
        }


        // Check it compiled successfully
        Gl.GetShader(shaderHandle, ShaderParameterName.CompileStatus, out var status);

        if (status == (int)GLEnum.True)
            return;



        // Delete it
        Gl.DeleteShader(shaderHandle);
        shaderHandle = 0;

        AssertFalse();
    }

    public void Dispose()
    {
        // Already disposed
        if (shaderHandle == 0)
        {
            AssertFalse();
            return;
        }

        Gl.DeleteShader(shaderHandle);
        shaderHandle = 0;
    }

    public uint shaderHandle;
}
