#version 330 core

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;

uniform mat4 viewMatrix;
uniform mat4 worldMatrix;
uniform mat4 projectionMatrix;
uniform mat4 lightViewProjectionMatrix;

out vec3 fragPos;
out vec3 fragNorm;
out vec4 fragPosFromLight;

void main()
{
	gl_Position = projectionMatrix * viewMatrix * worldMatrix * vec4(position, 1.0);
	fragPos = vec3(worldMatrix * vec4(position, 1.0));
	fragNorm = mat3(transpose(inverse(worldMatrix))) * normal;
	fragPosFromLight = lightViewProjectionMatrix * worldMatrix * vec4(position, 1.0);
}