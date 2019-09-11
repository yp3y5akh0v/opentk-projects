#version 330 core

struct Material {
	vec3 color;
	float shininess;
	float specularStrength;
};

struct Light {
	vec3 color;
	vec3 position;
	float ambientStrength;
};

uniform Material material;
uniform Light light;
uniform vec3 viewPos;
uniform sampler2D shadowMap;

in vec3 fragPos;
in vec3 fragNorm; 
in vec4 fragPosFromLight;
out vec4 fragColor;

vec3 divideDropW(vec4 position) {
	return position.w > 0 ? position.xyz / position.w : position.xyz;
}

float calcShadowFactor(vec4 position) 
{
	float shadowFactor = 1.0;
	vec3 projCoords = divideDropW(position);
	
	projCoords = 0.5 * projCoords + 0.5;

	if (projCoords.z < texture(shadowMap, projCoords.xy).r) 
	{
		shadowFactor = 0.0;
	}

	return shadowFactor;
}

void main()
{
	vec3 norm = normalize(fragNorm);
	vec3 ambient = light.ambientStrength * light.color;
	
	vec3 lightDir = normalize(light.position - fragPos);

	float diffuseFactor = max(dot(lightDir, norm), 0);
	vec3 diffuse = diffuseFactor * light.color;

	vec3 refLightDir = normalize(reflect(-lightDir, norm));
	vec3 invViewDir = normalize(viewPos - fragPos);
	float specularFactor = max(dot(refLightDir, invViewDir), 0);
	vec3 specular = material.specularStrength * pow(specularFactor, material.shininess) * light.color;

	float shadowFactor = calcShadowFactor(fragPosFromLight);
	vec3 result = (ambient + (1 - shadowFactor) * (diffuse + specular)) * material.color;

	fragColor = vec4(result, 1.0);
}