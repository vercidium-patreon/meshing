namespace meshing;

public unsafe class ShaderProgram
{
    public static string VERSION = "#version 330\n";

    public ShaderProgram(string vertexShaderSource, string fragmentShaderSource)
    {
        Initialise(vertexShaderSource, fragmentShaderSource);
    }

    void Initialise(string vertexShaderSource, string fragmentShaderSource)
    {
        try
        {
            vertexShader = new Shader(ShaderType.VertexShader, VERSION + vertexShaderSource);
            fragmentShader = new Shader(ShaderType.FragmentShader, VERSION + fragmentShaderSource);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Dispose();
            throw;
        }

        // Create shader program.
        programHandle = Gl.CreateProgram();


        // Attach shaders to the program
        Gl.AttachShader(programHandle, vertexShader.shaderHandle);
        Gl.AttachShader(programHandle, fragmentShader.shaderHandle);
        Gl.LinkProgram(programHandle);


        // Get the log
        Gl.GetProgram(programHandle, ProgramPropertyARB.InfoLogLength, out int logLen);

        if (logLen > 0)
        {
            Gl.GetProgramInfoLog(programHandle, (uint)logLen, out _, out string log);

            if (!string.IsNullOrEmpty(log))
                Console.WriteLine($"Program link log:\n{log}");
        }


        // Clean up
        Gl.DetachShader(programHandle, vertexShader.shaderHandle);
        Gl.DetachShader(programHandle, fragmentShader.shaderHandle);


        // Ensure the shaders were linked correctly
        Gl.GetProgram(programHandle, ProgramPropertyARB.LinkStatus, out int status);

        if (status == 0)
        {
            Console.WriteLine($"Shader link failed. Status: {status}");
            AssertFalse();
        }

        Unbind();
    }

    public virtual void UseProgram()
    {
        // Already current, all good
        if (Active)
            return;

        Gl.UseProgram(programHandle);
        CurrentHandle = programHandle;
    }


    public static void Unbind()
    {
        Gl.UseProgram(0);
        CurrentHandle = 0;
    }

    public int GetUniformLocation(string name) => Gl.GetUniformLocation(programHandle, name);

    public void Dispose()
    {
        if (vertexShader != null)
        {
            vertexShader.Dispose();
            vertexShader = null;
        }

        if (fragmentShader != null)
        {
            fragmentShader.Dispose();
            fragmentShader = null;
        }

        if (programHandle != 0)
        {
            Gl.DeleteProgram(programHandle);
            programHandle = 0;
        }
    }


    // Remember which is the current shader program
    static uint CurrentHandle;
    public bool Active => CurrentHandle == programHandle;

    Shader vertexShader;
    Shader fragmentShader;
    protected uint programHandle;

}
