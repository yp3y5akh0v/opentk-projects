#version 330 core

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;

uniform mat4 worldMatrix;
uniform mat4 normalMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;

out vec3 viewNormal;

void main()
{
    vec4 worldPos = worldMatrix * vec4(position, 1.0);

    // Ambient occlusion works in view space, so normals are delivered there directly.
    viewNormal = mat3(viewMatrix) * normalize(mat3(normalMatrix) * normal);

    gl_Position = projectionMatrix * viewMatrix * worldPos;
}
