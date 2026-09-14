// Percentage-closer soft shadows for a directional light.
//
// Penumbra width follows from the source's angular size alone, not from any distance to it:
//     width = 2 * blockerToReceiverDistance * tan(angularRadius)
// Orthographic depth is linear, so a [0,1] depth difference scales to world units by multiplying
// by the frustum's depth range.

uniform sampler2D shadowMap;
uniform float shadowTexelSize;      // 1.0 / resolution
uniform float shadowTexelWorldSize; // world units covered by one texel
uniform float shadowDepthRange;     // far minus near, in world units
uniform float sunTanAngularRadius;
uniform float shadowMaxFilterTexels;

const int BlockerSamples = 24;
const int FilterSamples = 24;

// Poisson disc, unit radius. Irregular spacing turns banding into noise.
const vec2 PoissonDisk[24] = vec2[](
    vec2(0.1379, 0.3266), vec2(-0.2792, 0.1298), vec2(0.3567, -0.1348), vec2(-0.0982, -0.3781),
    vec2(0.5931, 0.3162), vec2(-0.5478, 0.3819), vec2(0.1146, 0.6935), vec2(-0.1832, -0.6821),
    vec2(0.6702, -0.3627), vec2(-0.6913, -0.2418), vec2(0.4218, 0.6461), vec2(-0.4109, 0.7183),
    vec2(0.8791, 0.0532), vec2(-0.8503, 0.1197), vec2(0.0261, -0.8794), vec2(0.3078, -0.7359),
    vec2(-0.3392, -0.8102), vec2(0.7695, -0.5462), vec2(-0.7381, -0.5849), vec2(0.9384, 0.3128),
    vec2(-0.9217, 0.3471), vec2(0.5072, 0.8384), vec2(-0.4938, 0.8517), vec2(-0.0384, 0.9631)
);

float ShadowHash(vec2 seed)
{
    return fract(sin(dot(seed, vec2(12.9898, 78.233))) * 43758.5453);
}

mat2 ShadowRotation(vec2 seed)
{
    float angle = ShadowHash(seed) * 6.28318530718;
    float s = sin(angle);
    float c = cos(angle);
    return mat2(c, -s, s, c);
}

// Depth of the occluders above this receiver, or a negative value if there are none.
//
// Unrotated on purpose: penumbra width comes from this, and a per-pixel rotation would make
// neighbouring pixels estimate different widths, which reads as a shimmering edge. Dither belongs
// in the filter taps instead.
//
// Nearest blocker rather than mean, because a shadow map stores only the closest occluder per
// texel; averaging two casters at different heights describes a surface that does not exist.
float FindBlockerDepth(vec2 uv, float receiverDepth, float searchTexels)
{
    float nearest = 1.0;
    int count = 0;

    for (int i = 0; i < BlockerSamples; ++i)
    {
        vec2 offset = PoissonDisk[i] * searchTexels * shadowTexelSize;
        float sampled = texture(shadowMap, uv + offset).r;

        if (sampled < receiverDepth)
        {
            nearest = min(nearest, sampled);
            count++;
        }
    }

    return count == 0 ? -1.0 : nearest;
}

float FilterShadow(vec2 uv, float receiverDepth, float filterTexels, float bias, mat2 rotation)
{
    float lit = 0.0;

    for (int i = 0; i < FilterSamples; ++i)
    {
        vec2 offset = (rotation * PoissonDisk[i]) * filterTexels * shadowTexelSize;
        float sampled = texture(shadowMap, uv + offset).r;

        lit += (receiverDepth - bias) <= sampled ? 1.0 : 0.0;
    }

    return lit / float(FilterSamples);
}

// Returns 1.0 fully lit, 0.0 fully occluded.
float SoftShadowVisibility(vec4 positionFromLight, float ndotl, vec2 pixelSeed)
{
    vec3 projected = positionFromLight.xyz / positionFromLight.w;
    projected = projected * 0.5 + 0.5;

    // Outside the light frustum there is no shadow data; sampling would clamp to the border texel
    // and paint bands across the scene.
    if (projected.x < 0.0 || projected.x > 1.0 ||
        projected.y < 0.0 || projected.y > 1.0 ||
        projected.z < 0.0 || projected.z > 1.0)
    {
        return 1.0;
    }

    float receiverDepth = projected.z;
    mat2 rotation = ShadowRotation(pixelSeed);

    // Search far enough out to find a blocker that could still cast a visible penumbra here.
    float maxPenumbraWorld = shadowDepthRange * sunTanAngularRadius;
    float searchTexels = clamp(maxPenumbraWorld / shadowTexelWorldSize, 2.0, shadowMaxFilterTexels);

    float blockerDepth = FindBlockerDepth(projected.xy, receiverDepth, searchTexels);

    if (blockerDepth < 0.0)
    {
        return 1.0;
    }

    float separationWorld = max(receiverDepth - blockerDepth, 0.0) * shadowDepthRange;
    float penumbraWorld = 2.0 * separationWorld * sunTanAngularRadius;
    float filterTexels = clamp(penumbraWorld / shadowTexelWorldSize, 0.75, shadowMaxFilterTexels);

    // Bias scales with the filter radius: a wider kernel reaches across more surface slope, so it
    // needs more tolerance before a surface starts shadowing itself.
    float slope = sqrt(max(1.0 - ndotl * ndotl, 0.0)) / max(ndotl, 0.15);
    float biasWorld = filterTexels * shadowTexelWorldSize * clamp(slope, 0.5, 4.0) * 0.6;
    float bias = biasWorld / max(shadowDepthRange, 1e-4);

    return FilterShadow(projected.xy, receiverDepth, filterTexels, bias, rotation);
}
