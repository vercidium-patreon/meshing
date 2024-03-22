namespace meshing;

public static class VoxelShader
{
    public static ShaderProgram shader;
    public static bool Active => shader?.Active ?? false;

    public static void Initialise()
    {
        shader = new(VertexShader, FragmentShader);

        mvp = new(shader, "mvp");
        worldPosition = new(shader, "worldPosition");
        showWireframe = new(shader, "showWireframe");
    }


    public static void UseProgram()
    {
        if (shader == null)
            Initialise();

        shader.UseProgram();
    }

    public static ShaderValue mvp;
    public static ShaderValue worldPosition;
    public static ShaderValue showWireframe;

    public static string FragmentShader = @"
out vec4 gColor;

in vec2 vUV;
in vec2 vBary;
flat in float vBrightness;

uniform bool showWireframe;

float barycentric(vec2 vBC, float width)
{
    vec3 bary = vec3(vBC.x, vBC.y, 1.0 - vBC.x - vBC.y);
    vec3 d = fwidth(bary);
    vec3 a3 = smoothstep(d * (width - 0.5), d * (width + 0.5), bary);
    return min(min(a3.x, a3.y), a3.z);
}


void main()
{
    // Grid pattern
    bool gridX = mod(vUV.x, 1.0) > 0.5;
    bool gridY = mod(vUV.y, 1.0) > 0.5;

    float grid = gridX != gridY ? 1.0 : 0.7;

    gColor = vec4(vBrightness * grid);

    if (showWireframe)
    {
        gColor.rgb *= 0.25;
        gColor.rgb += vec3(1.0 - barycentric(vBary, 1.0));
    }
}
";

    public static string VertexShader = @$"

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aBary;
layout (location = 3) in int aTextureID;

out vec2 vUV;
out vec2 vBary;
flat out float vBrightness;

uniform mat4 mvp;
uniform vec3 worldPosition;

void main()
{{
    vec3 vPosition = aPosition + worldPosition;
    vBary = aBary;

    int face;

    if (aNormal.y >= 0.5)
    {{
        face = {(int)FaceType.yp};
        vBrightness = 1.0;
        vUV = vPosition.xz;
    }}
    else if (aNormal.y <= -0.5)
    {{
        face = {(int)FaceType.yn};
        vBrightness = 0.25;
        vUV = vPosition.xz;
    }}
    else if (aNormal.x >= 0.5)
    {{
        face = {(int)FaceType.xp};
        vBrightness = 0.75;
        vUV = vPosition.zy;
    }}
    else if (aNormal.x <= -0.5)
    {{
        face = {(int)FaceType.xn};
        vBrightness = 0.75;
        vUV = vPosition.zy;
    }}
    else if (aNormal.z >= 0.5)
    {{
        face = {(int)FaceType.zp};
        vBrightness = 0.5;
        vUV = vPosition.xy;
    }}
    else
    {{
        face = {(int)FaceType.zn};
        vBrightness = 0.5;
        vUV = vPosition.xy;
    }}

    vUV *= 2.0;


    // World to screen pos
    gl_Position = mvp * vec4(vPosition, 1.0);
}}
";
}
