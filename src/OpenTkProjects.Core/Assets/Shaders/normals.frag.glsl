#version 330 core

in vec3 viewNormal;

out vec4 fragColor;

void main()
{
    // A back face must report the camera-facing normal, or occlusion samples into the surface.
    vec3 normal = normalize(viewNormal);
    if (!gl_FrontFacing)
    {
        normal = -normal;
    }

    fragColor = vec4(normal, 1.0);
}
