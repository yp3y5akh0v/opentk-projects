#version 330 core

layout (location = 0) in vec3 position;

uniform mat4 worldMatrix;
uniform mat4 lightViewMatrix;
uniform mat4 lightProjectionMatrix;

void main()
{
	gl_Position = lightProjectionMatrix * lightViewMatrix * worldMatrix * vec4(position, 1.0);
}