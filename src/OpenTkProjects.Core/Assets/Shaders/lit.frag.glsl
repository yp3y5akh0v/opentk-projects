#version 330 core

#include "atmosphere.glsl"
#include "shadows.glsl"

const float Gamma = 2.2;

struct Material {
    vec3 color;
    float shininess;
    float specularStrength;
};

uniform Material material;
uniform vec3 cameraPos;
uniform float ambientStrength;

uniform sampler2D albedoMap;
uniform float useAlbedoMap;

uniform sampler2D occlusionMap;
uniform vec2 screenSize;

in vec3 fragPos;
in vec3 fragNorm;
in vec2 fragUv;
in vec4 fragPosFromLight;

// Linear radiance. Tone mapping and sRGB encoding happen once, in the composite pass.
out vec4 fragColor;

vec3 ToLinear(vec3 srgb)
{
    return pow(srgb, vec3(Gamma));
}

void main()
{
    // Single-sided geometry drawn two-sided: a back face must be flipped or it lights inverted.
    vec3 normal = normalize(fragNorm);
    if (!gl_FrontFacing)
    {
        normal = -normal;
    }

    vec3 sampled = mix(vec3(1.0), ToLinear(texture(albedoMap, fragUv).rgb), useAlbedoMap);
    vec3 albedo = ToLinear(material.color) * sampled;

    float ndotl = max(dot(normal, sunDirection), 0.0);

    // Hemispheric ambient: sky above, warmer bounce below, so shadows do not crush to black.
    float hemisphere = normal.y * 0.5 + 0.5;
    vec3 ambient = mix(ToLinear(groundColor), ToLinear(skyZenithColor), hemisphere) * ambientStrength;

    // Skylight only. Sunlight is already handled by the shadow map; darkening it here double counts.
    float occlusion = texture(occlusionMap, gl_FragCoord.xy / screenSize).r;
    ambient *= occlusion;

    vec3 viewDir = normalize(cameraPos - fragPos);
    vec3 halfway = normalize(sunDirection + viewDir);
    float specularFactor = pow(max(dot(normal, halfway), 0.0), material.shininess);
    vec3 specular = specularFactor * material.specularStrength * sunColor * step(0.0001, ndotl);

    float visibility = SoftShadowVisibility(fragPosFromLight, ndotl, gl_FragCoord.xy);

    vec3 lit = (albedo * (ambient + (visibility * ndotl * sunColor))) + (visibility * specular);

    // Same sky function the sky pass uses, so fully hazed geometry matches the sky behind it.
    vec3 toFragment = fragPos - cameraPos;
    float viewDistance = length(toFragment);
    vec3 rayDir = toFragment / max(viewDistance, 1e-4);

    float haze = FogAmount(cameraPos, rayDir, viewDistance);
    lit = mix(lit, FogColor(rayDir), clamp(haze, 0.0, 1.0));

    fragColor = vec4(lit, 1.0);
}
