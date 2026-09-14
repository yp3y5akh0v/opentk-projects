#version 330 core

layout (location = 0) in vec3 position;

uniform mat4 worldMatrix;
uniform mat4 lightViewProjection;

void main()
{
    gl_Position = lightViewProjection * worldMatrix * vec4(position, 1.0);
}
