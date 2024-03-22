namespace meshing;

public static class Helper
{
    public static Vector3 FromPitchYaw(float pitch, float yaw)
    {
        var cosPitch = MathF.Cos(pitch);
        return new Vector3(cosPitch * MathF.Sin(yaw), MathF.Sin(pitch), cosPitch * MathF.Cos(yaw));
    }

    public static Matrix4x4 CreateFPSView(Vector3 eye, float pitch, float yaw)
    {
        // Magic stuff
        eye.X = -eye.X;
        eye.Y = -eye.Y;

        var cosPitch = MathF.Cos(pitch);
        var sinPitch = MathF.Sin(pitch);
        var cosYaw = MathF.Cos(-yaw);
        var sinYaw = MathF.Sin(-yaw);

        var xAxis = new Vector3(cosYaw, 0, -sinYaw);
        var yAxis = new Vector3(sinYaw * sinPitch, cosPitch, cosYaw * sinPitch);
        var zAxis = new Vector3(sinYaw * cosPitch, -sinPitch, cosPitch * cosYaw);

        return new Matrix4x4(xAxis.X, yAxis.X, zAxis.X, 0,
                              xAxis.Y, yAxis.Y, zAxis.Y, 0,
                             -xAxis.Z, -yAxis.Z, -zAxis.Z, 0,
                             Vector3.Dot(xAxis, eye), Vector3.Dot(yAxis, eye), Vector3.Dot(zAxis, eye), 1);
    }
}