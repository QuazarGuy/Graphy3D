using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Graphy3D.App;

public partial class MainWindow : Window
{
    private readonly OrbitCamera orbitCamera;
    private Point? previousPointer;
    private MouseButton dragButton;

    public MainWindow()
    {
        InitializeComponent();
        orbitCamera = new OrbitCamera(SceneCamera);
        var stopwatch = Stopwatch.StartNew();
        var mesh = RenderingSpike.CreateSurface();
        SurfaceModel.Geometry = mesh;
        stopwatch.Stop();
        SceneStatus.Text =
            $"{mesh.Positions.Count:N0} vertices | {mesh.TriangleIndices.Count / 3:N0} triangles" +
            $" | Mesh build: {stopwatch.Elapsed.TotalMilliseconds:F1} ms" +
            $" | WPF tier: {RenderCapability.Tier >> 16}";
    }

    private void OnViewportMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton is not (MouseButton.Left or MouseButton.Right))
        {
            return;
        }

        SceneViewport.Focus();
        if (SceneViewport.CaptureMouse())
        {
            previousPointer = e.GetPosition(SceneViewport);
            dragButton = e.ChangedButton;
        }

        e.Handled = true;
    }

    private void OnViewportMouseMove(object sender, MouseEventArgs e)
    {
        if (previousPointer is not Point previous || !SceneViewport.IsMouseCaptured)
        {
            return;
        }

        var current = e.GetPosition(SceneViewport);
        var delta = current - previous;
        previousPointer = current;
        if (dragButton == MouseButton.Left)
        {
            orbitCamera.Orbit(-delta.X * 0.01, delta.Y * 0.01);
        }
        else
        {
            orbitCamera.Pan(delta.X, delta.Y);
        }
    }

    private void OnViewportMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (previousPointer is not null && e.ChangedButton == dragButton)
        {
            SceneViewport.ReleaseMouseCapture();
            previousPointer = null;
            e.Handled = true;
        }
    }

    private void OnViewportLostMouseCapture(object sender, MouseEventArgs e)
    {
        previousPointer = null;
    }

    private void OnViewportMouseWheel(object sender, MouseWheelEventArgs e)
    {
        orbitCamera.Zoom(e.Delta);
        e.Handled = true;
    }

    private void OnResetClick(object sender, RoutedEventArgs e) => orbitCamera.Reset();

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Left: orbitCamera.Orbit(-0.1, 0); break;
            case Key.Right: orbitCamera.Orbit(0.1, 0); break;
            case Key.Up: orbitCamera.Orbit(0, 0.1); break;
            case Key.Down: orbitCamera.Orbit(0, -0.1); break;
            case Key.Add:
            case Key.OemPlus: orbitCamera.Zoom(120); break;
            case Key.Subtract:
            case Key.OemMinus: orbitCamera.Zoom(-120); break;
            case Key.R: orbitCamera.Reset(); break;
            default: return;
        }

        e.Handled = true;
    }
}