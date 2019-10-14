#version 330 core

in vec2 outTexCoord;
in vec4 outColor;
out vec4 fragColor;

uniform sampler2D texSampler;

void main()
{
	fragColor = texture(texSampler, outTexCoord) * outColor;
}