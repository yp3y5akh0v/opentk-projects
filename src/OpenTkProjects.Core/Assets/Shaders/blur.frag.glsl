#version 330 core

// Box blur for the occlusion buffer, averaging away the noise the rotated sampling basis produces.
// A plain box suffices because occlusion is low frequency.

const int Radius = 2;

uniform sampler2D source;
uniform vec2 texelSize;

in vec2 fragUv;

out vec4 fragColor;

void main()
{
    float total = 0.0;
    float samples = 0.0;

    for (int x = -Radius; x <= Radius; ++x)
    {
        for (int y = -Radius; y <= Radius; ++y)
        {
            total += texture(source, fragUv + (vec2(x, y) * texelSize)).r;
            samples += 1.0;
        }
    }

    fragColor = vec4(total / samples);
}
