global using static meshing.Globals;
global using System;
global using System.Numerics;
global using System.Runtime.InteropServices;
global using Silk.NET.OpenGL;
using System.Diagnostics;

namespace meshing;


// This allows every file to call Gl.DoStuff()
public static class Globals
{
    public static GL Gl;

    public static void Assert(bool condition) => Debug.Assert(condition);
    public static void AssertFalse() => Debug.Assert(false);
}


internal class Program
{
    static void Main(string[] args)
    {
        new Client().Run();
    }
}
