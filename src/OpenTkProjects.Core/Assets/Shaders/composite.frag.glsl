#version 330 core

// Resolves the HDR scene to the screen: adds reduced-resolution in-scattering, upscaled bilinearly
// by the sampler, then exposure, tone mapping and sRGB encoding.

uniform sampler2D sceneColor;
uniform sampler2D scatteringColor;
uniform float scatteringEnabled;
uniform float exposure;

in vec2 fragUv;

out vec4 fragColor;

const float Gamma = 2.2;

// Narkowicz 2015 ACES filmic approximation. Rolls highlights off instead of clipping them flat.
vec3 Tonemap(vec3 x)
{
    const float a = 2.51;
    const float b = 0.03;
    const float c = 2.43;
    const float d = 0.59;
    const float e = 0.14;

    return clamp((x * (a * x + b)) / (x * (c * x + d) + e), 0.0, 1.0);
}

void main()
{
    vec3 radiance = texture(sceneColor, fragUv).rgb;
    radiance += texture(scatteringColor, fragUv).rgb * scatteringEnabled;

    fragColor = vec4(pow(Tonemap(radiance * exposure), vec3(1.0 / Gamma)), 1.0);
}
