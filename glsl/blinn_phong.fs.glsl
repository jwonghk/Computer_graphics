
uniform vec3 ambientColor;
uniform float kAmbient;

uniform vec3 diffuseColor;
uniform float kDiffuse;

uniform vec3 specularColor;
uniform float kSpecular;
uniform float shininess;

uniform mat4 modelMatrix;

uniform vec3 spherePosition;

// The value of our shared variable is given as the interpolation between normals computed in the vertex shader
// below we can see the shared variable we passed from the vertex shader using the 'in' classifier
in vec3 interpolatedNormal;
in vec3 viewPosition;
in vec3 worldPosition;


void main() {
    // TODO:
    // HINT: compute the following - light direction, ambient + diffuse + specular component,
    // then set the final color as a combination of these component


    
    vec3 toLight = normalize(spherePosition - worldPosition);
    vec3 h = normalize(viewPosition + toLight);

    float specular = pow(max(0.0, dot(interpolatedNormal, toLight)), shininess);
    float diffuse = max(0.0, dot(interpolatedNormal, toLight));
    vec3 intensity = kAmbient*ambientColor + diffuseColor*kDiffuse*diffuse + specularColor*specular;
    
    
    gl_FragColor = vec4(intensity.x, intensity.y, intensity.z, 1.0);
}
