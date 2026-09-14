using StbImageWriteSharp;

namespace OpenTkProjects.Core.Capture;

/// <summary>
/// Writes captured frames as PNG. Single stills, a contact sheet of several frames, or a numbered
/// sequence for an external encoder such as ffmpeg to turn into a GIF or video.
/// </summary>
/// <remarks>Stops at PNG: animation containers are a tooling concern, not a rendering one.</remarks>
public static class CaptureWriter
{
    public static void WritePng(string path, Frame frame)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(frame);

        EnsureDirectory(path);

        using FileStream stream = File.Create(path);
        new ImageWriter().WritePng(frame.Pixels, frame.Width, frame.Height, ColorComponents.RedGreenBlueAlpha, stream);
    }

    /// <summary>
    /// Tiles frames left to right, top to bottom into one image, with a separator between cells.
    /// A contact sheet shows motion in a single still, which is how a shimmering shadow edge or a
    /// simulation drifting out of stability becomes visible without watching a video.
    /// </summary>
    public static void WriteFilmstrip(string path, IReadOnlyList<Frame> frames, int columns)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(frames);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(columns);

        if (frames.Count == 0)
        {
            throw new ArgumentException("No frames to write.", nameof(frames));
        }

        const int gap = 2;

        int cellWidth = frames[0].Width;
        int cellHeight = frames[0].Height;
        int rows = (frames.Count + columns - 1) / columns;

        int sheetWidth = (columns * cellWidth) + ((columns - 1) * gap);
        int sheetHeight = (rows * cellHeight) + ((rows - 1) * gap);

        byte[] sheet = new byte[sheetWidth * sheetHeight * 4];

        for (int i = 0; i < frames.Count; i++)
        {
            Frame frame = frames[i];

            if (frame.Width != cellWidth || frame.Height != cellHeight)
            {
                throw new ArgumentException("All frames must share one size.", nameof(frames));
            }

            int originX = (i % columns) * (cellWidth + gap);
            int originY = (i / columns) * (cellHeight + gap);

            for (int y = 0; y < cellHeight; y++)
            {
                int source = y * cellWidth * 4;
                int target = (((originY + y) * sheetWidth) + originX) * 4;
                Array.Copy(frame.Pixels, source, sheet, target, cellWidth * 4);
            }
        }

        EnsureDirectory(path);

        using FileStream stream = File.Create(path);
        new ImageWriter().WritePng(sheet, sheetWidth, sheetHeight, ColorComponents.RedGreenBlueAlpha, stream);
    }

    /// <summary>
    /// Writes frames as frame_0000.png and so on, ready for:
    /// ffmpeg -framerate 30 -i frame_%04d.png -vf "split[a][b];[a]palettegen[p];[b][p]paletteuse" out.gif
    /// </summary>
    public static void WriteSequence(string directory, IReadOnlyList<Frame> frames)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentNullException.ThrowIfNull(frames);

        Directory.CreateDirectory(directory);

        for (int i = 0; i < frames.Count; i++)
        {
            WritePng(Path.Combine(directory, $"frame_{i:D4}.png"), frames[i]);
        }
    }

    private static void EnsureDirectory(string path)
    {
        string? directory = Path.GetDirectoryName(Path.GetFullPath(path));

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
