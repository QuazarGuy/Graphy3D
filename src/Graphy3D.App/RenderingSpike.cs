using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Graphy3D.App;

internal static class RenderingSpike
{
    internal const int SamplesPerAxis = 101;

    internal static MeshGeometry3D CreateSurface()
    {
        var positions = new Point3DCollection(SamplesPerAxis * SamplesPerAxis);
        var normals = new Vector3DCollection(SamplesPerAxis * SamplesPerAxis);
        var indices = new Int32Collection(6 * (SamplesPerAxis - 1) * (SamplesPerAxis - 1));

        for (var row = 0; row < SamplesPerAxis; row++)
        {
            var y = -3 + 6.0 * row / (SamplesPerAxis - 1);
            for (var column = 0; column < SamplesPerAxis; column++)
            {
                var x = -3 + 6.0 * column / (SamplesPerAxis - 1);
                positions.Add(new Point3D(x, y, 0.15 * (x * x - y * y)));
                var normal = new Vector3D(-0.3 * x, 0.3 * y, 1);
                normal.Normalize();
                normals.Add(normal);

                if (row == SamplesPerAxis - 1 || column == SamplesPerAxis - 1)
                {
                    continue;
                }

                var a = row * SamplesPerAxis + column;
                var b = a + 1;
                var c = a + SamplesPerAxis;
                var d = c + 1;
                indices.Add(a);
                indices.Add(b);
                indices.Add(c);
                indices.Add(b);
                indices.Add(d);
                indices.Add(c);
            }
        }

        var mesh = new MeshGeometry3D
        {
            Positions = positions,
            Normals = normals,
            TriangleIndices = indices
        };
        mesh.Freeze();
        return mesh;
    }
}
