#version 330 core

#include "atmosphere.glsl"

// Inverse of the camera's view-projection, used to turn a screen position back into a world ray.
uniform mat4 inverseViewProjection;
uniform vec3 cameraPos;

in vec2 fragUv;

// Linear radiance, like the lit pass. Tone mapping happens once, at composite.
out vec4 fragColor;

vec3 RayDirection(vec2 uv)
{
    vec4 clip = vec4(uv * 2.0 - 1.0, 1.0, 1.0);
    vec4 world = inverseViewProjection * clip;

    return normalize((world.xyz / world.w) - cameraPos);
}

void main()
{
    vec3 rayDir = RayDirection(fragUv);

    fragColor = vec4(SkyRadiance(rayDir) + SunDisc(rayDir), 1.0);
}
