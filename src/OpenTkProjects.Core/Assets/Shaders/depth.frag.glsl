#version 330 core

// Intentionally empty: depth is written by the fixed-function pipeline. Assigning gl_FragDepth
// here would disable early depth testing for no benefit.
void main()
{
}
