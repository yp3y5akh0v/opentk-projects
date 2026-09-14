using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTkProjects.Core.Capture;
using OpenTkProjects.Core.Graphics;

namespace OpenTkProjects.Core.App;

/// <summary>
/// Shared window host: GL setup, input, capture driving and the per-frame loop.
/// </summary>
public abstract class GameApp : GameWindow
{
    /// <summary>Contact sheet width. Four across keeps a 12 frame strip close to square.</summary>
    private const int FilmstripColumns = 4;

    private readonly AppOptions options;
    private readonly List<Frame> captured = [];
    private int frameIndex;
    private double timingAccumulator;
    private int timingFrames;

    protected GameApp(AppOptions options, string title)
        : base(
            GameWindowSettings.Default,
            new NativeWindowSettings
            {
                ClientSize = new Vector2i(1600, 900),
                Title = title,
                APIVersion = new Version(3, 3),
                Profile = ContextProfile.Core,
                Flags = ContextFlags.ForwardCompatible | ContextFlags.Debug,

                // Multisampling resolves polygon edges in hardware. Without it every wall
                // silhouette in the maze is a visible staircase.
                NumberOfSamples = 8,
            })
    {
        this.options = options;
    }

    /// <summary>
    /// True when the app is recording rather than being driven by a person. Demos use it to run a
    /// deterministic fixed timestep and to leave the camera on its framed pose.
    /// </summary>
    protected bool IsCapturing => options.IsCapturing;

    /// <summary>
    /// True only for captures spanning several frames, so a demo that drives the camera along a
    /// diagnostic path leaves a single screenshot on the pose it was framed with.
    /// </summary>
    protected bool IsCapturingMotion => options.Mode is CaptureMode.Filmstrip or CaptureMode.Sequence;

    protected override void OnLoad()
    {
        base.OnLoad();

        Console.WriteLine($"[gpu] {GraphicsDevice.DescribeDevice()}");

        if (GraphicsDevice.IsLikelyIntegratedDevice())
        {
            Console.WriteLine(
                "[gpu] This looks like an integrated GPU. On a laptop with switchable graphics, " +
                "add this executable under Settings > System > Display > Graphics and set it to " +
                "High performance. The preference is keyed to the executable path, so it has to be " +
                "set again if the build moves.");
        }

        GraphicsDevice.EnableDebugOutput();
        GraphicsDevice.SetDepthTest(true);
        GraphicsDevice.SetMultisample(true);

        // Grabbing the cursor would fight an unattended screenshot run.
        if (!IsCapturing)
        {
            CursorState = CursorState.Grabbed;
        }

        OnLoadScene();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        if (KeyboardState.IsKeyDown(Keys.Escape))
        {
            Close();
            return;
        }

        OnUpdateScene((float)args.Time);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        OnRenderScene();

        frameIndex++;
        ReportFrameTiming(args.Time);
        CaptureIfDue();

        SwapBuffers();

        if (options.IsCapturing && frameIndex >= options.LastNeededFrame)
        {
            FinishCapture();
            Close();
        }
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        GraphicsDevice.SetViewport(e.Width, e.Height);
    }

    protected abstract void OnLoadScene();

    protected abstract void OnUpdateScene(float deltaSeconds);

    protected abstract void OnRenderScene();

    /// <summary>
    /// Prints frame cost once a second. Without a number on screen it is impossible to tell a
    /// renderer that is genuinely expensive from one that quietly landed on the wrong GPU.
    /// </summary>
    private void ReportFrameTiming(double elapsedSeconds)
    {
        timingAccumulator += elapsedSeconds;
        timingFrames++;

        if (timingAccumulator < 1.0)
        {
            return;
        }

        double millisecondsPerFrame = timingAccumulator / timingFrames * 1000.0;
        Console.WriteLine($"[perf] {timingFrames / timingAccumulator:F1} fps, {millisecondsPerFrame:F1} ms/frame");

        timingAccumulator = 0.0;
        timingFrames = 0;
    }

    /// <summary>Grabs the back buffer when this frame is one the capture asked for.</summary>
    private void CaptureIfDue()
    {
        if (!options.IsCapturing || frameIndex < options.StartFrame)
        {
            return;
        }

        if (options.Mode != CaptureMode.Screenshot &&
            (frameIndex - options.StartFrame) % options.FrameInterval != 0)
        {
            return;
        }

        Frame frame = FrameGrabber.Grab(ClientSize.X, ClientSize.Y);

        captured.Add(options.Mode == CaptureMode.Screenshot
            ? frame
            : FrameGrabber.Downscale(frame, options.Downscale));
    }

    private void FinishCapture()
    {
        string path = options.OutputPath
            ?? throw new InvalidOperationException("Capture requested with no output path.");

        switch (options.Mode)
        {
            case CaptureMode.Screenshot:
                CaptureWriter.WritePng(path, captured[0]);
                break;

            case CaptureMode.Filmstrip:
                CaptureWriter.WriteFilmstrip(path, captured, FilmstripColumns);
                break;

            case CaptureMode.Sequence:
                CaptureWriter.WriteSequence(path, captured);
                break;

            case CaptureMode.None:
            default:
                return;
        }

        Console.WriteLine($"{options.Mode} ({captured.Count} frames) written to {Path.GetFullPath(path)}");
    }
}
