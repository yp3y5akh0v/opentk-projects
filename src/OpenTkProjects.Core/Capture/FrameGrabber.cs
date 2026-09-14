using OpenTK.Graphics.OpenGL4;

namespace OpenTkProjects.Core.Capture;

/// <summary>One captured frame as tightly packed RGBA bytes, top row first.</summary>
public sealed record Frame(int Width, int Height, byte[] Pixels);

public static class FrameGrabber
{
    /// <summary>
    /// Reads the back buffer. Must be called before SwapBuffers: front buffer reads are
    /// unreliable once a compositor owns the surface.
    /// </summary>
    public static Frame Grab(int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        byte[] pixels = new byte[width * height * 4];

        GL.ReadBuffer(ReadBufferMode.Back);
        GL.PixelStore(PixelStoreParameter.PackAlignment, 1);
        GL.ReadPixels(0, 0, width, height, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

        FlipVertically(pixels, width, height);

        return new Frame(width, height, pixels);
    }

    /// <summary>
    /// Box-filtered downscale by an integer factor. Averaging rather than point sampling matters
    /// for capture: a point-sampled shadow edge or thin wall turns into crawling noise between
    /// frames, which is exactly the sort of artefact a capture is meant to reveal rather than add.
    /// </summary>
    public static Frame Downscale(Frame frame, int factor)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentOutOfRangeException.ThrowIfLessThan(factor, 1);

        if (factor == 1)
        {
            return frame;
        }

        int width = Math.Max(frame.Width / factor, 1);
        int height = Math.Max(frame.Height / factor, 1);
        byte[] result = new byte[width * height * 4];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int red = 0;
                int green = 0;
                int blue = 0;
                int samples = 0;

                for (int sy = 0; sy < factor; sy++)
                {
                    int sourceY = (y * factor) + sy;
                    if (sourceY >= frame.Height)
                    {
                        break;
                    }

                    for (int sx = 0; sx < factor; sx++)
                    {
                        int sourceX = (x * factor) + sx;
                        if (sourceX >= frame.Width)
                        {
                            break;
                        }

                        int offset = ((sourceY * frame.Width) + sourceX) * 4;
                        red += frame.Pixels[offset];
                        green += frame.Pixels[offset + 1];
                        blue += frame.Pixels[offset + 2];
                        samples++;
                    }
                }

                int target = ((y * width) + x) * 4;
                result[target] = (byte)(red / samples);
                result[target + 1] = (byte)(green / samples);
                result[target + 2] = (byte)(blue / samples);
                result[target + 3] = 255;
            }
        }

        return new Frame(width, height, result);
    }

    /// <summary>GL reads rows bottom-up; every image format here stores them top-down.</summary>
    private static void FlipVertically(byte[] pixels, int width, int height)
    {
        int stride = width * 4;
        byte[] row = new byte[stride];

        for (int y = 0; y < height / 2; y++)
        {
            int top = y * stride;
            int bottom = (height - 1 - y) * stride;

            Array.Copy(pixels, top, row, 0, stride);
            Array.Copy(pixels, bottom, pixels, top, stride);
            Array.Copy(row, 0, pixels, bottom, stride);
        }
    }
}
