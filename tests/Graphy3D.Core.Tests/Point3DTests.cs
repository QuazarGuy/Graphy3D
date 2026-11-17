namespace Graphy3D.Core.Tests;

public class Point3DTests
{
    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(-2.5, 3.25, -4.75)]
    public void PreservesCoordinates(double x, double y, double z)
    {
        var point = new Point3D(x, y, z);

        Assert.Equal(x, point.X);
        Assert.Equal(y, point.Y);
        Assert.Equal(z, point.Z);
    }

    [Fact]
    public void EqualCoordinatesHaveValueEquality()
    {
        var point = new Point3D(1, 2, 3);

        Assert.Equal(point, new Point3D(1, 2, 3));
        Assert.NotEqual(point, new Point3D(1, 2, 4));
    }
}
