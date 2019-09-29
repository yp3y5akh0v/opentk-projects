#version 330 core

layout (location = 0) in vec3 position;
layout (location = 2) in vec2 texCoord;

uniform mat4 viewMatrix;
uniform mat4 worldMatrix;
uniform mat4 projectionMatrix;

out vec2 outTexCoord;

void main()
{
	gl_Position = projectionMatrix * viewMatrix * worldMatrix * vec4(position, 1.0);
	outTexCoord = texCoord;
}