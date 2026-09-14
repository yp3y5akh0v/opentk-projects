#version 330 core

#include "atmosphere.glsl"

// Ray marched single-scattering volumetric lighting. Per step:
//   inScatter += transmittance * visibility * phase * sunColor * density * stepSize
//   transmittance *= exp(-density * stepSize)                          (Beer-Lambert)
// Sampling the shadow map at each step is what makes shafts respect real occlusion, unlike a
// screen-space blur that only smears brightness outward from the sun's screen position.

const int MarchSteps = 64;

uniform sampler2D sceneDepth;
uniform sampler2D shadowMap;

uniform mat4 inverseViewProjection;
uniform mat4 lightViewProjection;
uniform vec3 cameraPos;

uniform float scatteringAnisotropy;
uniform float scatteringStrength;
uniform float marchDistance;

in vec2 fragUv;

out vec4 fragColor;

vec3 WorldFromDepth(vec2 uv, float depth)
{
    vec4 clip = vec4((uv * 2.0) - 1.0, (depth * 2.0) - 1.0, 1.0);
    vec4 world = inverseViewProjection * clip;

    return world.xyz / world.w;
}

// Henyey-Greenstein, normalized over the sphere. g of 0 is isotropic; positive g scatters forward.
float PhaseHenyeyGreenstein(float cosTheta, float g)
{
    float gg = g * g;
    float denominator = 1.0 + gg - (2.0 * g * cosTheta);

    return (1.0 - gg) / (4.0 * Pi * pow(max(denominator, 1e-4), 1.5));
}

// Fog density at a point, following the same exponential height profile the surface fog uses.
float DensityAt(vec3 position)
{
    return fogDensity * exp(-(position.y - fogHeight) * max(fogHeightFalloff, 1e-6));
}

float LitAt(vec3 position)
{
    vec4 lightSpace = lightViewProjection * vec4(position, 1.0);
    vec3 projected = (lightSpace.xyz / lightSpace.w) * 0.5 + 0.5;

    // Air outside the shadow frustum has no occlusion information, so treat it as lit.
    if (projected.x < 0.0 || projected.x > 1.0 ||
        projected.y < 0.0 || projected.y > 1.0 ||
        projected.z < 0.0 || projected.z > 1.0)
    {
        return 1.0;
    }

    return projected.z <= texture(shadowMap, projected.xy).r ? 1.0 : 0.0;
}

// Interleaved gradient noise. Offsetting each ray's start turns the banding a fixed step size
// produces into fine dither.
float InterleavedGradientNoise(vec2 position)
{
    return fract(52.9829189 * fract(dot(position, vec2(0.06711056, 0.00583715))));
}

void main()
{
    float depth = texture(sceneDepth, fragUv).r;

    vec3 rayEnd = WorldFromDepth(fragUv, depth);
    vec3 toEnd = rayEnd - cameraPos;
    float distanceToSurface = length(toEnd);
    vec3 rayDir = toEnd / max(distanceToSurface, 1e-4);

    // Nothing was drawn here, so the ray runs to the march limit instead of to a surface.
    float travel = depth >= 1.0 ? marchDistance : min(distanceToSurface, marchDistance);

    float stepSize = travel / float(MarchSteps);
    float t = stepSize * InterleavedGradientNoise(gl_FragCoord.xy);

    float phase = PhaseHenyeyGreenstein(dot(rayDir, sunDirection), scatteringAnisotropy);

    vec3 inScatter = vec3(0.0);
    float transmittance = 1.0;

    for (int i = 0; i < MarchSteps; ++i)
    {
        vec3 position = cameraPos + (rayDir * t);
        float density = DensityAt(position) * scatteringStrength;

        if (density > 1e-7)
        {
            float visibility = LitAt(position);

            inScatter += transmittance * visibility * phase * sunColor * density * stepSize;
            transmittance *= exp(-density * stepSize);
        }

        t += stepSize;
    }

    fragColor = vec4(inScatter, 1.0);
}
