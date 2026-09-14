#version 330 core

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;
layout (location = 2) in vec2 texCoord;

uniform mat4 worldMatrix;
uniform mat4 normalMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;
uniform mat4 lightViewProjection;

// Declared in the fragment stage as well; the linker resolves both to one uniform.
uniform vec3 sunDirection;
uniform float shadowTexelWorldSize;

out vec3 fragPos;
out vec3 fragNorm;
out vec2 fragUv;
out vec4 fragPosFromLight;

void main()
{
    vec4 worldPos = worldMatrix * vec4(position, 1.0);

    // Inverse transpose is computed once per node on the CPU, not per vertex here.
    vec3 worldNormal = normalize(mat3(normalMatrix) * normal);

    fragPos = worldPos.xyz;
    fragNorm = worldNormal;
    fragUv = texCoord;

    // Normal offset shadows: lift the lookup off the surface by about one shadow texel, widening
    // at grazing angles. Removes acne geometrically, so shadows stay attached to their casters.
    // Oriented toward the light first, because two-sided geometry may store a normal facing away
    // and using it unchanged would push the lookup into the surface and guarantee self-shadowing.
    float facing = dot(worldNormal, sunDirection);
    vec3 shadowNormal = facing < 0.0 ? -worldNormal : worldNormal;

    float ndotl = clamp(abs(facing), 0.0, 1.0);
    float grazing = sqrt(1.0 - ndotl * ndotl);
    vec3 offset = shadowNormal * (shadowTexelWorldSize * (0.9 + 2.2 * grazing));

    fragPosFromLight = lightViewProjection * vec4(worldPos.xyz + offset, 1.0);

    gl_Position = projectionMatrix * viewMatrix * worldPos;
}
