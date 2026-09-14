using OpenTkProjects.Core.App;

namespace OpenTkProjects.Core.Tests.App;

public class AppOptionsTests
{
    [Fact]
    public void Parse_DefaultsToInteractive()
    {
        AppOptions options = AppOptions.Parse([]);

        Assert.Equal(CaptureMode.None, options.Mode);
        Assert.False(options.IsCapturing);
        Assert.Null(options.OutputPath);
    }

    [Theory]
    [InlineData("--screenshot", CaptureMode.Screenshot)]
    [InlineData("--filmstrip", CaptureMode.Filmstrip)]
    [InlineData("--sequence", CaptureMode.Sequence)]
    public void Parse_SelectsTheCaptureModeAndPath(string flag, CaptureMode expected)
    {
        AppOptions options = AppOptions.Parse([flag, "out"]);

        Assert.Equal(expected, options.Mode);
        Assert.Equal("out", options.OutputPath);
        Assert.True(options.IsCapturing);
    }

    [Fact]
    public void Parse_AcceptsArgumentsInEitherOrder()
    {
        AppOptions options = AppOptions.Parse(["--frames", "8", "--filmstrip", "a.png", "--every", "5"]);

        Assert.Equal("a.png", options.OutputPath);
        Assert.Equal(8, options.FrameCount);
        Assert.Equal(5, options.FrameInterval);
    }

    [Fact]
    public void LastNeededFrame_ForAScreenshotIsTheStartFrame()
    {
        AppOptions options = AppOptions.Parse(["--screenshot", "a.png", "--frame", "42"]);

        Assert.Equal(42, options.LastNeededFrame);
    }

    [Fact]
    public void LastNeededFrame_SpansTheWholeCaptureWindow()
    {
        AppOptions options = AppOptions.Parse(
            ["--filmstrip", "a.png", "--frame", "10", "--frames", "6", "--every", "4"]);

        // Frames land on 10, 14, 18, 22, 26 and 30.
        Assert.Equal(30, options.LastNeededFrame);
    }

    [Theory]
    [InlineData("--screenshot")]
    [InlineData("--filmstrip")]
    [InlineData("--sequence")]
    public void Parse_ThrowsWhenAPathIsMissing(string flag)
    {
        Assert.Throws<ArgumentException>(() => AppOptions.Parse([flag]));
    }

    [Theory]
    [InlineData("--frame", "soon")]
    [InlineData("--frames", "0")]
    [InlineData("--every", "-2")]
    [InlineData("--scale", "x")]
    public void Parse_ThrowsOnANonPositiveOrNonNumericCount(string flag, string value)
    {
        Assert.Throws<ArgumentException>(() => AppOptions.Parse([flag, value]));
    }

    [Fact]
    public void Parse_ThrowsOnAnUnknownArgument()
    {
        Assert.Throws<ArgumentException>(() => AppOptions.Parse(["--nope"]));
    }

    [Fact]
    public void Parse_RejectsNull()
    {
        Assert.Throws<ArgumentNullException>(() => AppOptions.Parse(null!));
    }
}
