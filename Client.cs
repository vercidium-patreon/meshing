using Silk.NET.Input;
using Silk.NET.Windowing;

namespace meshing;

public unsafe partial class Client
{
    public Client()
    {
        // Create a Silk.NET window
        var options = WindowOptions.Default;
        options.API = new GraphicsAPI(ContextAPI.OpenGL, new APIVersion(3, 3));
        options.Position = new(200, 200);
        options.PreferredDepthBufferBits = 32;

        window = Window.Create(options);

        // Callback when the window is created
        window.Load += () =>
        {
            // Create an OpenGL Context
            Gl = window.CreateOpenGL();
            OnDidCreateOpenGLContext();


            // Precalculate input stuff
            inputContext = window.CreateInput();
            keyboard = inputContext.Keyboards[0];
            mouse = inputContext.Mice[0];
            mouse.DoubleClickTime = 1;
        };

        window.Render += (_) => Render();

        window.Size = new(1920, 1080);
        window.FramesPerSecond = 144;
        window.UpdatesPerSecond = 144;
        window.VSync = false;

        // Initialise OpenGL and input context
        window.Initialize();
    }


    public void Run()
    {
        // Run forever
        window.Run();
    }


    void OnDidCreateOpenGLContext()
    {
        var major = Gl.GetInteger(GetPName.MajorVersion);
        var minor = Gl.GetInteger(GetPName.MinorVersion);

        var version = major * 10 + minor;
        Console.WriteLine($"OpenGL Version: {version}");

        mapRenderer = new();


#if DEBUG
        // Set up the OpenGL debug message callback (NVIDIA only)
        debugDelegate = DebugCallback;

        Gl.Enable(EnableCap.DebugOutput);
        Gl.Enable(EnableCap.DebugOutputSynchronous);
        Gl.DebugMessageCallback(debugDelegate, null);
#endif
    }

    void Render()
    {
        if (firstRender)
        {
            cameraPos = new Vector3(0, 18, 0) - Helper.FromPitchYaw(cameraPitch, cameraYaw) * 32;
            lastMouse = mouse.Position;
            firstRender = false;
        }


        // Mouse movement
        var diff = lastMouse - mouse.Position;

        cameraYaw -= diff.X * 0.003f;
        cameraPitch += diff.Y * 0.003f;

        lastMouse = mouse.Position;


        // Fly camera movement
        float movementSpeed = 0.15f;

        if (keyboard.IsKeyPressed(Key.W))
            cameraPos += Helper.FromPitchYaw(cameraPitch, cameraYaw) * movementSpeed;
        else if (keyboard.IsKeyPressed(Key.S))
            cameraPos -= Helper.FromPitchYaw(cameraPitch, cameraYaw) * movementSpeed;

        if (keyboard.IsKeyPressed(Key.A))
            cameraPos += Helper.FromPitchYaw(0, cameraYaw - MathF.PI / 2) * movementSpeed;
        else if (keyboard.IsKeyPressed(Key.D))
            cameraPos += Helper.FromPitchYaw(0, cameraYaw + MathF.PI / 2) * movementSpeed;

        if (keyboard.IsKeyPressed(Key.E))
            cameraPos += Helper.FromPitchYaw(MathF.PI / 2, 0) * movementSpeed;
        else if (keyboard.IsKeyPressed(Key.Q))
            cameraPos += Helper.FromPitchYaw(-MathF.PI / 2, 0) * movementSpeed;

        

        // Prepare OpenGL
        PreRenderSetup();


        // Prepare the shader
        VoxelShader.UseProgram();
        VoxelShader.mvp.Set(GetViewProjection());
        VoxelShader.showWireframe.Set(keyboard.IsKeyPressed(Key.Space));


        // Render the map
        mapRenderer.cameraPitch = cameraPitch;
        mapRenderer.cameraYaw = cameraYaw;

        mapRenderer.Render();
    }

    public void PreRenderSetup()
    {
        // Prepare rendering
        Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        Gl.Enable(EnableCap.DepthTest);
        Gl.Disable(EnableCap.Blend);
        Gl.Disable(EnableCap.StencilTest);
        Gl.Enable(EnableCap.CullFace);
        Gl.FrontFace(FrontFaceDirection.CW);


        // Clear everything
        Gl.ClearDepth(1.0f);
        Gl.DepthFunc(DepthFunction.Less);

        Gl.ColorMask(true, true, true, true);
        Gl.DepthMask(true);

        Gl.ClearColor(0, 0, 0, 0);
        Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);


        // Set the viewport to the window size
        Gl.Viewport(0, 0, (uint)window.Size.X, (uint)window.Size.Y);
    }

    protected Matrix4x4 GetViewProjection()
    {
        var view = Helper.CreateFPSView(cameraPos, cameraPitch, cameraYaw);
        var proj = Matrix4x4.CreatePerspectiveFieldOfView(FieldOfView, Aspect, NearPlane, FarPlane);

        return view * proj;
    }


#if DEBUG
    static DebugProc debugDelegate;

    static unsafe void DebugCallback(GLEnum source, GLEnum type, int id, GLEnum severity, int length, nint messageInt, nint userParam)
    {
        // TODO
        var message = Marshal.PtrToStringAnsi(messageInt);

        if (message == "Pixel-path performance warning: Pixel transfer is synchronized with 3D rendering.")
        {
            // TODO: Use proper id for this
            return;
        }

        // Skip our own notifications
        if (severity == GLEnum.DebugSeverityNotification)
            return;

        // Buffer detailed info
        if (id == 131185)
            return;

        // "Program/shader state performance warning: Vertex shader in program 69 is being recompiled based on GL state."
        if (id == 131218)
            return;

        // "Buffer performance warning: Buffer object 15 (bound to NONE, usage hint is GL_DYNAMIC_DRAW) is being copied/moved from VIDEO memory to HOST memory."
        if (id == 131186)
            return;

        AssertFalse();
        Console.WriteLine(message);
    }
#endif



    // Silk
    IWindow window;
    IMouse mouse;
    IKeyboard keyboard;
    IInputContext inputContext;


    // Camera
    Vector2 lastMouse;
    Vector3 cameraPos;
    float cameraPitch = -MathF.PI / 6;
    float cameraYaw = MathF.PI / 4;


    // Rendering
    bool firstRender = true;

    float FieldOfView = 50.0f / 180.0f * MathF.PI;
    float Aspect => window.Size.X / (float)window.Size.Y;
    float NearPlane = 1.0f;
    float FarPlane = 256.0f;

    // Voxel data
    MapRenderer mapRenderer;
}