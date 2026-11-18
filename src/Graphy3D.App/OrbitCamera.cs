using System.Windows.Media.Media3D;

namespace Graphy3D.App;

internal sealed class OrbitCamera
{
    private readonly PerspectiveCamera camera;
    private Point3D target;
    private double azimuth;
    private double elevation;
    private double distance;

    internal OrbitCamera(PerspectiveCamera camera)
    {
        this.camera = camera;
        Reset();
    }

    internal void Reset()
    {
        target = new Point3D();
        azimuth = -Math.PI / 4;
        elevation = Math.PI / 6;
        distance = 12;
        Update();
    }

    internal void Orbit(double horizontal, double vertical)
    {
        azimuth = Math.IEEERemainder(azimuth + horizontal, 2 * Math.PI);
        elevation = Math.Clamp(elevation + vertical, -Math.PI * 4 / 9, Math.PI * 4 / 9);
        Update();
    }

    internal void Zoom(int wheelDelta)
    {
        distance = Math.Clamp(distance * Math.Exp(-(double)wheelDelta / 1200.0), 4, 40);
        Update();
    }

    internal void Pan(double horizontal, double vertical)
    {
        var forward = camera.LookDirection;
        forward.Normalize();
        var right = Vector3D.CrossProduct(forward, camera.UpDirection);
        right.Normalize();
        var up = Vector3D.CrossProduct(right, forward);
        target += (-horizontal * right + vertical * up) * (distance * 0.002);
        Update();
    }

    private void Update()
    {
        var horizontal = distance * Math.Cos(elevation);
        camera.Position = target + new Vector3D(
            horizontal * Math.Cos(azimuth),
            horizontal * Math.Sin(azimuth),
            distance * Math.Sin(elevation));
        camera.LookDirection = target - camera.Position;
        camera.UpDirection = new Vector3D(0, 0, 1);
    }
}
