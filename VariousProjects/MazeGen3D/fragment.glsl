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

in vec3 fragPos;
in vec3 fragNorm; 
out vec4 fragColor;

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

	vec3 result = (ambient + diffuse + specular) * material.color;

	fragColor = vec4(result, 1.0);
}