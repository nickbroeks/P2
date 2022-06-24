#version 460

struct Light
{
    vec4 intensity;
    vec3 location;
}; 

layout(std140, binding = 3) buffer ssbo_lights
{
    Light lights[];
};

// shader input
in vec2 uv;			// interpolated texture coordinates
in vec4 normal;			// interpolated normal
in vec3 position;
uniform sampler2D pixels;	// texture sampler
uniform vec4 ambLight;
uniform mat4 transform;


// shader output
out vec4 outputColor;

// fragment shader
void main()
{
    outputColor = vec4(0, 0, 0, 0);
    for (int i = 0; i < lights.length(); i++) {
        vec3 lightRay = normalize(transform * vec4(position - lights[i].location, 1)).xyz;
        outputColor += lights[i].intensity * 
        25.0f / pow(distance(position, lights[i].location), 2) * (
            texture(pixels, uv) * max(0, dot(normal.xyz, (-lightRay))) +
            texture(pixels, uv) * pow(max(0, dot(
                normalize(normalize((transform * vec4(position, 1)).xyz)),
                normalize(lightRay - 2 * dot(lightRay, normal.xyz) * normal.xyz)
            )), 4000)
        );
    }
    outputColor += ambLight * texture( pixels, uv );
}