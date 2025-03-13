
uniform vec3 resolution;   
uniform float time;   

// NOTE: You may temporarily adjust these values to improve render performance.
#define MAX_STEPS 700 // max number of steps to take along ray
#define MAX_DIST 700. // max distance to travel along ray
#define HIT_DIST .01// distance to consider as "hit" to surface

/*
 * Helper: determines the material ID based on the closest distance.
 */
float getMaterial(vec2 d1, vec2 d2) {
    return (d1.x < d2.x) ? d1.y : d2.y;
}

/*
 * Hard union of two SDFs.
 */
float unionSDF( float d1, float d2 )
{

	/*
     * TODO: Implement the union of two SDFs.
     */
    if (d1 < d2) {
        return d1;
    }

    return d2;
}

/*
 * Smooth union of two SDFs.
 * Resource: `https://iquilezles.org/articles/smin/`
 */
float smoothUnionSDF( float d1, float d2, float k )
{
	/*
     * TODO: Implement the smooth union of two SDFs.
     */

    k *= log(2.0);
    float x = d2-d1;
    return d1 + x/(1.0-exp2(x/k));
    //return d2;
}


/*
    Helper: Computer SDF of a torus.
*/

float sdTorus(vec3 p, vec3 centreOfTorus, vec2 t, float rotationDegrees)
{
    // Convert degrees to radians
    float rotationRadians = rotationDegrees * 3.14159265358979 / 180.0;

    // Rotation matrix around the Y-axis
    mat3 rotationMatrix = mat3(
        cos(rotationRadians), 0.0, sin(rotationRadians),
        0.0, 1.0, 0.0,
        -sin(rotationRadians), 0.0, cos(rotationRadians)
    );

    // Apply the rotation to the point p
    p = rotationMatrix * (p-centreOfTorus);

    // Torus SDF calculation
    vec2 q = vec2(length(p.xz) - t.x, p.y);
    return length(q) - t.y;
}



/*
 * Helper: Computes the signed distance function (SDF) of a plane.
 */
vec2 Plane(vec3 p)
{
    vec3 n = vec3(0, 1, 0); // Normal
    float h = 0.0; // Height
    return vec2(dot(p, n) + h, 1.0);
}

/*
 * Sphere SDF.
 *
 * Parameters:
 *  - p: The point in 3D space to evaluate.
 *  - c: The center of the sphere.
 *  - r: The radius of the sphere.
 *
 * Returns:
 *  - A vec2 containing:
 *    - Signed distance to the surface of the sphere.
 *    - An identifier for material type.
 */
vec2 Sphere(vec3 p, vec3 c, float r)
{
    float dist = MAX_DIST;
    float sphere_id = 2.0;

    /*
     * TODO: Implement the signed distance function for a sphere.
     */

    dist = length(p - c) - r;
    if (r < 0.8) {
        sphere_id = 5.0;
    }
    
    return vec2(dist, sphere_id);
}

/*
 * Cylinder SDF.
 *
 * Parameters:
 *  - p: The point in 3D space to evaluate.
 *  - c: The center of the cylinder.
 *  - r: The radius of the cylinder.
 *  - h: The height of the cylinder.
 *  - rotate: A flag to apply rotation.
 *
 * Returns:
 *  - A vec2 containing:
 *    - Signed distance to the surface of the cylinder.
 *    - An identifier for material type.
 */
vec2 Cylinder(vec3 p, vec3 c, float r, float h, bool rotate)
{
    float dist = MAX_DIST;
    float hat_id = 3.0; 
    float button_id = 5.0;
    float id = hat_id;

    /*
     * TODO: Implement the signed distance function for a cylinder.
     *       use to rotate flag for surfaces that require rotation.
     */
  


    vec2 d = abs(vec2(length((p-c).xz),(p-c).y)) - vec2(r,h);
    dist = min(max(d.x,d.y),0.0) + length(max(d,0.0));
    return vec2(dist, id);
};

/*
 * Cone SDF. 
 *
 * Parameters:
 *  - p: The point in 3D space to evaluate.
 *  - c: The center of the cone base.
 *  - t: The angle of the cone.
 *  - h: The height of the cone.
 *
 * Returns:
 *  - A vec2 containing:
 *    - Signed distance to the surface of the cone.
 *    - An identifier for material type.
 */
vec2 Cone(vec3 p, vec3 c, float t, float h) 
{
    float dist = MAX_DIST;
    float cone_id = 4.0; 

    // Shift the input point `p` so that `c` is the origin
    p -= c;

    // Rotate the cone around the y-axis
    p = vec3(p.x, -p.z, p.y); 

    vec2 cxy = vec2(sin(t), cos(t)); 
    vec2 q = h * vec2(cxy.x / cxy.y, -1.0);
    vec2 w = vec2( length(p.xz), p.y );
    vec2 a = w - q * clamp(dot(w, q) / dot(q, q), 0.0, 1.0);
    vec2 b = w - q * vec2(clamp(w.x / q.x, 0.0, 1.0), 1.0);
    float k = sign(q.y);
    float d = min(dot(a, a), dot(b, b));
    float s = max(k * (w.x * q.y - w.y * q.x), k * (w.y - q.y));
    dist = sqrt(d) * sign(s);

    return vec2(dist, cone_id);
}

/*
 * Snowman SDF.
 *
 * Parameters:
 *  - p: The point in 3D space to evaluate.
 *
 * Returns:
 *  - A vec2 containing:
 *    - Signed distance to the surface of the snowman.
 *    - An identifier for material type.
 */

vec2 Snowman(vec3 p) 
{

    float dist = MAX_DIST;
    float id = 2.0;




    /*
     * TODO - Implement the signed distance function for a snowman.
     *        Make use of the helper SDF and blending functions
     *        to compute the final distance and material ID.
     */

    // Buttons - use cylinders to represent the buttons
    vec2 distButton1 = Cylinder(p, vec3(0.0, -11.5, 5.8), 0.2, 0.2, true);
    vec2 distButton2 = Cylinder(p, vec3(0.0, -11.5, 6.6), 0.2, 0.2, true);
    vec2 distButton3 = Cylinder(p, vec3(0.0, -11.5, 7.4), 0.2, 0.2, true);

    // positions of body sphere:
    vec3 baseSpherePos = vec3(0.0, 1.0, 15.0);
    vec3 bodyMidPos = vec3(0.0, 1.4, 15.0);
    vec3 headSpherePos = vec3(0.0, 2.1, 15.0);
    // raidus of bodysphere:
    float distanceToBase = Sphere(p, baseSpherePos, 1.00).x - 1.00;
    float distanceToBodyMid = Sphere(p, bodyMidPos, 0.70).x - 0.70;
    float distanceToHead = Sphere(p, headSpherePos, 0.65).x - 0.65;

    // positions of eye sphere:
    vec3 leftEyePos = vec3(0.2, 1.9, 13.00);
    vec3 rightEyePos = vec3(-0.2, 1.9, 13.00);
    // raidus of eye sphere:
    float distLeftEye = Sphere(p, leftEyePos, 0.05).x - 0.05;
    float distRightEye = Sphere(p, rightEyePos, 0.05).x - 0.05;


    // position of Nose cone:
    vec3 conePos = vec3(0.0, 1.8, 15.00);
    vec2 coneProperties = Cone(p, conePos, 0.1, 0.2);

    // position of hat:
    vec3 hatPos = vec3(0.0, 12.7, 15.00);
    vec2 hatProperties = Cylinder(p, hatPos, 2.0, 0.7, false);

    // position of hatBottomSupport:
    vec3 hatBottomPos = vec3(0.0, 12.0, 15.00);
    vec2 hatBottomProperties = Cylinder(p, hatBottomPos, 2.5, 0.1, false);

    // position of torus:
    vec3 torusPos = vec3(0.0, 6.0, 15.0);
    vec2 radiasOfInnerOuter = vec2(3.8, 0.9);
    float distTorus = sdTorus(p, torusPos, radiasOfInnerOuter, 30.0);


    
    float distSnowman = smoothUnionSDF(distanceToBase, distanceToBodyMid, 0.1);
    distSnowman = smoothUnionSDF(distanceToHead, distSnowman, 0.1);
    distSnowman = smoothUnionSDF(distButton1.x, distSnowman, 0.1);
    distSnowman = smoothUnionSDF(distButton2.x, distSnowman, 0.1);
    distSnowman = smoothUnionSDF(distButton3.x, distSnowman, 0.1);
    distSnowman = smoothUnionSDF(distLeftEye, distSnowman, 0.1);
    distSnowman = smoothUnionSDF(distRightEye, distSnowman, 0.1);
    distSnowman = smoothUnionSDF(coneProperties.x, distSnowman, 0.1);
    distSnowman = smoothUnionSDF(hatProperties.x, distSnowman, 0.1);
    distSnowman = smoothUnionSDF(hatBottomProperties.x, distSnowman, 0.1);
    distSnowman = smoothUnionSDF(distTorus, distSnowman, 0.1);
    
    
    if (abs(distSnowman - distButton1.x) < HIT_DIST) {
        dist = distButton1.x;
        id = distButton1.y;
    } else if (abs(distSnowman - distButton2.x) < HIT_DIST) {
        dist = distButton2.x;
        id = distButton2.y;
    } else if (abs(distSnowman - distButton3.x) < HIT_DIST) {
        dist = distButton3.x;
        id = distButton3.y;
    } else if (abs(distSnowman - distLeftEye) < HIT_DIST) {
        dist = distLeftEye;
        id = 5.0;
    } else if (abs(distSnowman - distRightEye) < HIT_DIST) {
        dist = distRightEye;
        id = 5.0;
    } else if (abs(distSnowman - coneProperties.x) < HIT_DIST) {
        dist = coneProperties.x;
        id = coneProperties.y;
    } else if (abs(distSnowman - hatProperties.x) < HIT_DIST) {
        dist = hatProperties.x;
        id = hatProperties.y;    
    } else if (abs(distSnowman - hatBottomProperties.x) < HIT_DIST) {
        dist = hatBottomProperties.x;
        id = hatBottomProperties.y;    
    /* {  else if (abs(distSnowman - distTorus) < HIT_DIST) {
        dist = distTorus;
        id = 6.0;    */
    } else {
        dist = distSnowman;
    };
    return vec2(dist, id);
}

/*
 * Helper: gets the distance and material ID to the closest surface in the scene.
 */
vec2 getSceneDist(vec3 p) {
    
    vec2 snowman = Snowman(p);
    vec2 plane = Plane(p);
    
    float dist = smoothUnionSDF(snowman.x, plane.x, .02);
    float id = getMaterial(snowman, plane);

    return vec2(dist, id);
}

/*
 * Performs ray marching to determine the closest surface intersection.
 *
 * Parameters:
 *  - ro: Ray origin.
 *  - rd: Ray direction.
 *
 * Returns:
 *  - A vec2 containing:
 *    - Distance to the closest surface intersection.
 *    - material ID of the closest intersected surface.
 */


vec2 rayMarch(vec3 ro, vec3 rd) {
	float d = 0.;
	float id = 0.;
    
    /*
     * TODO: Implement the ray marching loop for MAX_STEPS.
     *       At each step, use getSceneDist to get the nearest surface distance.
     *       Update the distance and material ID based on the closest surface.
     *       Break if the distance is less than HIT_DIST or the travelled distance is greater than MAX_DIST.
     *       Note, if MAX_DIST is reached, the material ID should be 0.0 (background color).
     */

    
     for(int i = 0; i < MAX_STEPS ; i++) {
        vec3 currentPos = ro + d*rd;
        float currentDistToCloesetSurf = getSceneDist(currentPos).x;
        id = getSceneDist(currentPos).y;
        d += currentDistToCloesetSurf;

        if(d > MAX_DIST) {
            id = 0.0;
            break;
        } 
        
        if(currentDistToCloesetSurf < HIT_DIST) {
            break;
        }
     }

    return vec2(d, id);
}

/* 
 * Helper: computes surface normal
 */
vec3 getNormal(vec3 p) {
	float d = getSceneDist(p).x;
    vec2 e = vec2(.01, 0);
    
    vec3 n = d - vec3(
        getSceneDist(p-e.xyy).x,
        getSceneDist(p-e.yxy).x,
        getSceneDist(p-e.yyx).x);
    
    return normalize(n);
}

/*
 * Helper: gets surface color.
 */
vec3 getColor(vec3 p, float id) {
    
    vec3 lightPos = vec3(3, 5, 2);
    vec3 l = normalize(lightPos - p);
    vec3 n = getNormal(p);
    
    float diffuse = clamp(dot(n, l), 0.2, 1.);

    // Perform shadow check using ray marching 
    { 
        // NOTE: Comment out to improve render performance
        float d = rayMarch(p + n * HIT_DIST * 2., l).x;
        if (d < length(lightPos - p)) diffuse *= 0.1; 
    }

    vec3 diffuseColor;

    switch (int(id)) {
        case 0: // background sky color (ray missed all surfaces) 
            diffuseColor = vec3(.3, .6, 1.);
            diffuse = .97;
            break;
        case 1: // plane (snow)
            diffuseColor = vec3(1, .98, .98);
            break;
        case 2: // snowman (slightly darker snow)
            diffuseColor = vec3(1, .9, .9);
            break;
        case 3: // hat
            diffuseColor = vec3(.8, .05, 0);
            break;
        case 4: // nose
            diffuseColor = vec3(.8, .2, .0);
            break;
        case 5: // eye/buttons
            diffuseColor = vec3(.1, .1, .1);
            break;
        case 6: // torus 
            diffuseColor = vec3(.4, .1, .6);
            break;
    }

    vec3 ambientColor = vec3(.9, .9, .9);
    float ambient = .1;

    return ambient * ambientColor + diffuse * diffuseColor;
}

/*
 * Helper: camera matrix.
 */
mat3 setCamera( in vec3 ro, in vec3 ta, float cr )
{
	vec3 cw = normalize(ta-ro);
	vec3 cp = vec3(sin(cr), cos(cr),0.0);
	vec3 cu = normalize( cross(cw,cp) );
	vec3 cv =          ( cross(cu,cw) );
    return mat3( cu, cv, cw );
}

void main() {

    //console.log("ray march!!! );
    // Get the fragment coordinate in screen space
    vec2 fragCoord = gl_FragCoord.xy;
    
    // normalize to UV coordinates
    vec2 uv = (fragCoord - 0.5 * resolution.xy) / resolution.y;

    // Look-at target (the point the camera is focusing on)
     vec3 ta = vec3(0, 3, 9); 
    //vec3 ta = vec3(0, 10, -5); 


    // Camera position 
    // NOTE: modify camera for development
     vec3 ro = vec3(0, 8, -9); // static 
    //vec3 ro = ta + vec3(4.0 * cos(0.7 * time), 2.0, 4.0 * sin(0.7 * time)); // dynamic camera

    // Compute the camera's coordinate frame
    mat3 ca = setCamera(ro, ta, 0.0); 

    // Compute the ray direction for this pixel with respect ot camera frame
    vec3 rd = ca * normalize(vec3(uv.x, uv.y, 1));

    // Perform ray marching to find intersection distance and surface material ID
    vec2 dist = rayMarch(ro, rd); 
    float d = dist.x; 
    float id = dist.y; 

    // Surface intersection point
    vec3 p = ro + rd * d;

    // Compute surface color
    vec3 color = getColor(p, id); 

    // Apply gamma correction to adjust brightness
    color = pow(color, vec3(0.4545)); 

    // Output the final color to the fragment shader
    gl_FragColor = vec4(color, 1.0); 
}
