using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Graphy3D.App;
using Xunit.Abstractions;

namespace Graphy3D.App.Tests;

public class RenderingSpikeTests(ITestOutputHelper output)
{
    [Fact]
    public void SurfaceHasFiniteGeometryAndConsistentWinding()
    {
        RunSta(() =>
        {
            var stopwatch = Stopwatch.StartNew();
            var mesh = RenderingSpike.CreateSurface();
            stopwatch.Stop();
            output.WriteLine($"Mesh build: {stopwatch.Elapsed.TotalMilliseconds:F1} ms");
            Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(2), "Mesh build exceeded 2 seconds.");
            Assert.True(mesh.IsFrozen);
            Assert.Equal(10_201, mesh.Positions.Count);
            Assert.Equal(60_000, mesh.TriangleIndices.Count);
            Assert.Equal(mesh.Positions.Count, mesh.Normals.Count);
            Assert.All(mesh.Positions, point =>
                Assert.True(double.IsFinite(point.X) && double.IsFinite(point.Y) && double.IsFinite(point.Z)));
            Assert.All(mesh.Normals, normal => Assert.Equal(1, normal.Length, 10));

            for (var i = 0; i < mesh.TriangleIndices.Count; i += 3)
            {
                var a = mesh.TriangleIndices[i];
                var b = mesh.TriangleIndices[i + 1];
                var c = mesh.TriangleIndices[i + 2];
                Assert.InRange(a, 0, mesh.Positions.Count - 1);
                Assert.InRange(b, 0, mesh.Positions.Count - 1);
                Assert.InRange(c, 0, mesh.Positions.Count - 1);
                var normal = Vector3D.CrossProduct(
                    mesh.Positions[b] - mesh.Positions[a],
                    mesh.Positions[c] - mesh.Positions[a]);
                Assert.True(normal.Z > 0);
            }
        });
    }

    [Fact]
    public void CameraSupportsOrbitPanZoomAndResetWithoutSingularities()
    {
        RunSta(() =>
        {
            var camera = new PerspectiveCamera();
            var controller = new OrbitCamera(camera);
            var initialPosition = camera.Position;
            var initialDirection = camera.LookDirection;

            controller.Orbit(0.5, 0.2);
            Assert.NotEqual(initialPosition, camera.Position);
            controller.Pan(30, -20);
            Assert.True((camera.Position + camera.LookDirection - new Point3D()).Length > 0);

            controller.Zoom(int.MaxValue);
            Assert.Equal(4, camera.LookDirection.Length, 10);
            controller.Zoom(int.MinValue);
            Assert.Equal(40, camera.LookDirection.Length, 10);
            controller.Orbit(0, 100);
            Assert.True(Vector3D.CrossProduct(camera.LookDirection, camera.UpDirection).Length > 0.1);
            controller.Orbit(0, -200);
            Assert.True(Vector3D.CrossProduct(camera.LookDirection, camera.UpDirection).Length > 0.1);

            controller.Reset();
            Assert.Equal(initialPosition, camera.Position);
            Assert.Equal(initialDirection, camera.LookDirection);
        });
    }

    [Fact]
    public void WindowWiresKeyboardWheelAndResetToCamera()
    {
        RunSta(() =>
        {
            var window = new MainWindow();
            try
            {
                window.Show();
                var camera = Assert.IsType<PerspectiveCamera>(window.SceneViewport.Camera);
                var original = camera.Position;
                var source = PresentationSource.FromVisual(window);
                Assert.NotNull(source);
                window.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, source, 0, Key.Right)
                {
                    RoutedEvent = Keyboard.KeyDownEvent
                });
                Assert.NotEqual(original, camera.Position);
                var distance = camera.LookDirection.Length;
                window.SceneViewport.RaiseEvent(new MouseWheelEventArgs(Mouse.PrimaryDevice, 0, 120)
                {
                    RoutedEvent = Mouse.MouseWheelEvent
                });
                Assert.True(camera.LookDirection.Length < distance);
                window.ResetViewButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Assert.Equal(original, camera.Position);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void WindowRendersVisibleSurfacePixels()
    {
        RunSta(() =>
        {
            var window = new MainWindow { Width = 960, Height = 600 };
            try
            {
                window.Show();
                window.UpdateLayout();
                var viewport = window.SceneBorder;
                var width = (int)viewport.ActualWidth;
                var height = (int)viewport.ActualHeight;
                Assert.True(width > 0 && height > 0);
                var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
                bitmap.Render(viewport);
                var pixels = new byte[width * height * 4];
                bitmap.CopyPixels(pixels, width * 4, 0);
                var bluePixels = 0;
                for (var i = 0; i < pixels.Length; i += 4)
                {
                    if (pixels[i] > pixels[i + 2] + 30 && pixels[i] > pixels[i + 1] + 20)
                    {
                        bluePixels++;
                    }
                }

                output.WriteLine($"Surface-colored pixels: {bluePixels} / {width * height}");
                Assert.True(bluePixels > width * height / 100, "Expected visible blue surface geometry.");
                Assert.Contains($"{20_000:N0} triangles", window.SceneStatus.Text);
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static void RunSta(Action action)
    {
        ExceptionDispatchInfo? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                failure = ExceptionDispatchInfo.Capture(exception);
            }
        }) { IsBackground = true };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(30)), "STA rendering test timed out.");
        failure?.Throw();
    }
}
