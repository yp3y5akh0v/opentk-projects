using OpenTK.Mathematics;
using OpenTkProjects.Core.Graphics;
using OpenTkProjects.Core.Lighting;
using OpenTkProjects.Core.SceneGraph;

namespace OpenTkProjects.Core.Rendering;

/// <summary>
/// The 3D frame graph: shadow depth, then sky and lit geometry into an HDR target, then a
/// composite that adds light shafts and resolves to the screen.
/// </summary>
public sealed class Renderer : IDisposable
{
    private readonly ShaderLibrary shaders = new();
    private readonly ShadowMap shadowMap;
    private readonly RenderTarget sceneTarget;
    private readonly FullscreenTriangle fullscreen = new();
    private readonly SkyPass skyPass;
    private readonly VolumetricPass volumetricPass;
    private readonly CompositePass compositePass;
    private readonly List<RenderItem> drawList = [];
    private bool diagnosticsReported;
    private bool disposed;

    /// <summary>Shadow map resolution, which sets the texel footprint on the ground.</summary>
    public Renderer(int shadowMapSize = 4096)
    {
        shadowMap = new ShadowMap(shadowMapSize, shadowMapSize);
        sceneTarget = new RenderTarget(1, 1);

        DepthPass = new ShadowDepthPass(shaders.Load("depth.vert.glsl", "depth.frag.glsl"), shadowMap);

        OcclusionPass = new AmbientOcclusionPass(
            shaders.Load("normals.vert.glsl", "normals.frag.glsl"),
            shaders.Load("fullscreen.vert.glsl", "ssao.frag.glsl"),
            shaders.Load("fullscreen.vert.glsl", "blur.frag.glsl"),
            fullscreen);

        LitPass = new ForwardLitPass(shaders.Load("lit.vert.glsl", "lit.frag.glsl"), shadowMap, OcclusionPass);

        skyPass = new SkyPass(shaders.Load("fullscreen.vert.glsl", "sky.frag.glsl"), fullscreen);
        volumetricPass = new VolumetricPass(
            shaders.Load("fullscreen.vert.glsl", "volumetric.frag.glsl"), fullscreen, shadowMap);
        compositePass = new CompositePass(shaders.Load("fullscreen.vert.glsl", "composite.frag.glsl"), fullscreen);
    }

    /// <summary>
    /// How far shadows are resolved. Kept short so texels stay fine; one shadow map stretched over
    /// a whole scene is too coarse to read at eye level. Cascades are the way past this trade.
    /// </summary>
    public float ShadowDistance { get; set; } = 3_000f;

    /// <summary>
    /// Ceiling on the adaptive range. The range must grow with altitude or shadows vanish once the
    /// ground is further off than <see cref="ShadowDistance"/>; texels stretch as it does.
    /// </summary>
    public float MaxShadowDistance { get; set; } = 24_000f;

    /// <summary>Writes the derived shadow frustum to stderr once, for diagnosing shadow problems.</summary>
    public bool DiagnosticsEnabled { get; set; }

    public ShadowDepthPass DepthPass { get; }

    public AmbientOcclusionPass OcclusionPass { get; }

    public ForwardLitPass LitPass { get; }

    public void Render(Scene scene, int width, int height)
    {
        ArgumentNullException.ThrowIfNull(scene);

        if (width <= 0 || height <= 0)
        {
            return;
        }

        SceneFlattener.Collect(scene, drawList);

        // Geometry and the camera are the only inputs; no caller supplies frustum dimensions.
        Aabb casterBounds = scene.ComputeShadowBounds();

        // Spend texels on the slice of view the camera can see, not the whole world.
        float aspectRatio = height == 0 ? 1f : width / (float)height;

        // Measured against real geometry, not casterBounds: those extend below ground by the
        // shadow sweep and would report a camera standing on the floor as airborne.
        float floor = scene.ComputeWorldBounds().Min.Y;
        float altitude = MathF.Max(scene.Camera.Position.Y - floor, 0f);
        float shadowRange = Math.Clamp(
            MathF.Max(ShadowDistance, altitude * 2.5f),
            ShadowDistance,
            MathF.Min(MaxShadowDistance, scene.Camera.FarPlane));

        Span<Vector3> viewCorners = stackalloc Vector3[8];
        scene.Camera.GetFrustumCorners(aspectRatio, scene.Camera.NearPlane, shadowRange, viewCorners);

        LightFrustum frustum = scene.Light.ComputeShadowFrustum(viewCorners, casterBounds, shadowMap.Width);
        var context = new RenderContext(width, height, frustum);

        if (DiagnosticsEnabled && !diagnosticsReported)
        {
            diagnosticsReported = true;
            Console.Error.WriteLine(
                $"[shadow] casters={casterBounds.Min}..{casterBounds.Max} " +
                $"ortho={frustum.WorldWidth:F0}x{frustum.WorldHeight:F0} " +
                $"depth={frustum.NearPlane:F1}..{frustum.FarPlane:F1} " +
                $"texel={frustum.TexelWorldSize(shadowMap.Width):F2}");
        }

        DepthPass.Execute(drawList, scene, context);

        // Occlusion must exist before anything is shaded, since it scales the ambient term.
        OcclusionPass.Execute(drawList, scene, context);

        sceneTarget.Resize(width, height);
        sceneTarget.BindForWriting();
        GraphicsDevice.ClearDepthOnly();

        skyPass.Execute(scene, width, height);
        LitPass.Execute(drawList, scene, context);

        // In-scattering is computed at reduced resolution, then added during composite.
        volumetricPass.Execute(scene, sceneTarget, context);

        compositePass.Execute(scene, sceneTarget, volumetricPass, width, height);
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        OcclusionPass.Dispose();
        volumetricPass.Dispose();
        sceneTarget.Dispose();
        fullscreen.Dispose();
        shadowMap.Dispose();
        shaders.Dispose();
        disposed = true;
    }
}
