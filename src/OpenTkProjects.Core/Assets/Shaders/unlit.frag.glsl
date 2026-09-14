#version 330 core

// Per-node tint over the mesh's vertex colours. These are display values already, so no linear
// conversion or tone mapping.
uniform vec3 tint;

in vec4 vertexColor;

out vec4 fragColor;

void main()
{
    fragColor = vec4(vertexColor.rgb * tint, vertexColor.a);
}
