#version 330 core

// Screen-space ambient occlusion by hemisphere sampling.
//
// Shadow mapping only answers whether the sun reaches a point, never how enclosed it is, so
// creases and contact points would otherwise read as flat as open ground. Samples are spread in
// the hemisphere around the normal, projected to screen space and compared against depth.

const int SampleCount = 24;
const float Pi = 3.14159265359;

uniform sampler2D normalTexture;
uniform sampler2D depthTexture;

uniform mat4 projectionMatrix;
uniform mat4 inverseProjectionMatrix;

uniform float radius;
uniform float bias;
uniform float intensity;

in vec2 fragUv;

out vec4 fragColor;

vec3 ViewPositionFromDepth(vec2 uv)
{
    float depth = texture(depthTexture, uv).r;
    vec4 clip = vec4((uv * 2.0) - 1.0, (depth * 2.0) - 1.0, 1.0);
    vec4 view = inverseProjectionMatrix * clip;

    return view.xyz / view.w;
}

float Hash(vec2 seed)
{
    return fract(sin(dot(seed, vec2(12.9898, 78.233))) * 43758.5453);
}

// Deterministic hemisphere direction, weighted toward the surface so near samples dominate.
vec3 HemisphereSample(int index, float angleOffset)
{
    float u = (float(index) + 0.5) / float(SampleCount);

    float angle = (u * Pi * 2.0 * 7.0) + angleOffset;
    float cosTheta = sqrt(1.0 - u);
    float sinTheta = sqrt(u);

    // Cluster samples nearer the origin; close geometry matters far more than far geometry.
    float scale = mix(0.1, 1.0, u * u);

    return vec3(cos(angle) * sinTheta, sin(angle) * sinTheta, cosTheta) * scale;
}

void main()
{
    float depth = texture(depthTexture, fragUv).r;

    // Nothing was drawn here, so there is nothing to occlude.
    if (depth >= 1.0)
    {
        fragColor = vec4(1.0);
        return;
    }

    vec3 origin = ViewPositionFromDepth(fragUv);
    vec3 normal = normalize(texture(normalTexture, fragUv).xyz);

    // Rotated per pixel so undersampling becomes noise the blur removes, not banding it cannot.
    float angleOffset = Hash(gl_FragCoord.xy) * Pi * 2.0;
    vec3 randomVec = vec3(cos(angleOffset), sin(angleOffset), 0.0);
    vec3 tangent = normalize(randomVec - (normal * dot(randomVec, normal)));
    vec3 bitangent = cross(normal, tangent);
    mat3 basis = mat3(tangent, bitangent, normal);

    float occlusion = 0.0;

    for (int i = 0; i < SampleCount; ++i)
    {
        vec3 samplePos = origin + (basis * HemisphereSample(i, angleOffset) * radius);

        vec4 clip = projectionMatrix * vec4(samplePos, 1.0);
        vec2 sampleUv = ((clip.xy / clip.w) * 0.5) + 0.5;

        if (sampleUv.x < 0.0 || sampleUv.x > 1.0 || sampleUv.y < 0.0 || sampleUv.y > 1.0)
        {
            continue;
        }

        float sceneDepth = ViewPositionFromDepth(sampleUv).z;

        // View space looks down -Z, so a larger z is nearer the camera and therefore occluding.
        if (sceneDepth >= samplePos.z + bias)
        {
            // Without this, a wall far behind the sample still counts as an occluder and paints
            // a dark halo around every silhouette.
            float rangeCheck = smoothstep(0.0, 1.0, radius / abs(origin.z - sceneDepth));
            occlusion += rangeCheck;
        }
    }

    float visibility = 1.0 - ((occlusion / float(SampleCount)) * intensity);

    fragColor = vec4(clamp(visibility, 0.0, 1.0));
}
