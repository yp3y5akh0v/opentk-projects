#version 330 core

// Corners derived from gl_VertexID, so no vertex buffer is needed. One oversized triangle rather
// than two: no shared diagonal edge to rasterize twice, and quad utilization stays perfect.
out vec2 fragUv;

void main()
{
    vec2 corner = vec2((gl_VertexID << 1) & 2, gl_VertexID & 2);

    fragUv = corner;
    gl_Position = vec4(corner * 2.0 - 1.0, 0.0, 1.0);
}
