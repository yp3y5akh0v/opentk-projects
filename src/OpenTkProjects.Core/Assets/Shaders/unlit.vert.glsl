#version 330 core

layout (location = 0) in vec3 position;
layout (location = 3) in vec4 color;

uniform mat4 worldMatrix;
uniform mat4 viewProjection;

out vec4 vertexColor;

void main()
{
    vertexColor = color;
    gl_Position = viewProjection * worldMatrix * vec4(position, 1.0);
}
