#version 330 core

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;

uniform mat4 viewMatrix;
uniform mat4 worldMatrix;
uniform mat4 projectionMatrix;
uniform mat4 lightViewMatrix;
uniform mat4 lightProjectionMatrix;
uniform vec3 viewPos;

out vec3 fragPos;
out vec3 fragViewPos;
out vec3 fragNorm;
out vec4 fragPosFromLight;

void main()
{
	gl_Position = projectionMatrix * viewMatrix * worldMatrix * vec4(position, 1.0);
	fragPos = vec3(worldMatrix * vec4(position, 1.0));
	fragViewPos = vec3(worldMatrix * vec4(viewPos, 1.0));
	fragNorm = mat3(transpose(inverse(worldMatrix))) * normal;
	fragPosFromLight = lightProjectionMatrix * lightViewMatrix * worldMatrix * vec4(position, 1.0);
}