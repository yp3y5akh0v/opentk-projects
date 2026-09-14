namespace OpenTkProjects.Core.App;

public enum CaptureMode
{
    /// <summary>Run interactively.</summary>
    None,

    /// <summary>One still, then exit.</summary>
    Screenshot,

    /// <summary>Several frames tiled into one contact sheet, then exit.</summary>
    Filmstrip,

    /// <summary>Numbered frames for an external encoder such as ffmpeg, then exit.</summary>
    Sequence,
}

/// <summary>
/// Command line options shared by every demo. Capture exists so rendering can be verified without
/// a human watching the window, which is the only way to catch problems the headless tests cannot
/// see: washed out fog, wrong lighting, a shimmering shadow edge.
/// </summary>
public readonly record struct AppOptions(
    CaptureMode Mode,
    string? OutputPath,
    int StartFrame,
    int FrameCount,
    int FrameInterval,
    int Downscale)
{
    public bool IsCapturing => Mode != CaptureMode.None;

    /// <summary>Last rendered frame index the capture needs before it can finish.</summary>
    public int LastNeededFrame => Mode == CaptureMode.Screenshot
        ? StartFrame
        : StartFrame + ((FrameCount - 1) * FrameInterval);

    public static AppOptions Parse(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        CaptureMode mode = CaptureMode.None;
        string? outputPath = null;
        int startFrame = 30;
        int frameCount = 12;
        int frameInterval = 4;
        int downscale = 3;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--screenshot":
                    mode = CaptureMode.Screenshot;
                    outputPath = RequireValue(args, ref i, "--screenshot");
                    break;

                case "--filmstrip":
                    mode = CaptureMode.Filmstrip;
                    outputPath = RequireValue(args, ref i, "--filmstrip");
                    break;

                case "--sequence":
                    mode = CaptureMode.Sequence;
                    outputPath = RequireValue(args, ref i, "--sequence");
                    break;

                case "--frame":
                    startFrame = RequirePositive(args, ref i, "--frame");
                    break;

                case "--frames":
                    frameCount = RequirePositive(args, ref i, "--frames");
                    break;

                case "--every":
                    frameInterval = RequirePositive(args, ref i, "--every");
                    break;

                case "--scale":
                    downscale = RequirePositive(args, ref i, "--scale");
                    break;

                default:
                    throw new ArgumentException($"Unknown argument '{args[i]}'.", nameof(args));
            }
        }

        return new AppOptions(mode, outputPath, startFrame, frameCount, frameInterval, downscale);
    }

    private static string RequireValue(string[] args, ref int index, string option)
    {
        if (index + 1 >= args.Length)
        {
            throw new ArgumentException($"{option} requires a path.", nameof(args));
        }

        return args[++index];
    }

    private static int RequirePositive(string[] args, ref int index, string option)
    {
        if (index + 1 >= args.Length || !int.TryParse(args[index + 1], out int value) || value < 1)
        {
            throw new ArgumentException($"{option} requires a positive integer.", nameof(args));
        }

        index++;
        return value;
    }
}
