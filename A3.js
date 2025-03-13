/*
 * UBC CPSC 314
 * Assignment 3 Template
 */
import { setup, createScene, createRayMarchingScene, loadAndPlaceGLB, loadAndPlaceOBJ } from './js/setup.js';
import * as THREE from './js/three.module.js';
import { SourceLoader } from './js/SourceLoader.js';
import { THREEx } from './js/KeyboardState.js';

// Setup the renderer
// You should look into js/setup.js to see what exactly is done here.
const { renderer, canvas } = setup();

/////////////////////////////////
//   YOUR WORK STARTS BELOW    //
/////////////////////////////////

// Uniforms - Pass these into the appropriate vertex and fragment shader files
const spherePosition = { type: 'v3', value: new THREE.Vector3(0.0, 0.0, 0.0) };

const ambientColor = { type: 'c', value: new THREE.Color(0.0, 0.0, 1.0) };
const diffuseColor = { type: 'c', value: new THREE.Color(0.0, 1.0, 1.0) };
const specularColor = { type: 'c', value: new THREE.Color(1.0, 1.0, 1.0) };

const kAmbient = { type: "f", value: 0.3 };
const kDiffuse = { type: "f", value: 0.6 };
const kSpecular = { type: "f", value: 1.0 };
const shininess = { type: "f", value: 50.0 };
const ticks = { type: "f", value: 0.0 };
const resolution =  { type: 'v3', value: new THREE.Vector3() };

const sphereLight = new THREE.PointLight(0xffffff, 200);


// Shader materials
const sphereMaterial = new THREE.ShaderMaterial({
  uniforms: {
    spherePosition: spherePosition
  }
});

const blinnPhongMaterial = new THREE.ShaderMaterial({
  uniforms: {
    spherePosition: spherePosition,
    ambientColor: ambientColor,
    diffuseColor: diffuseColor,
    specularColor: specularColor,
    kAmbient: kAmbient,
    kDiffuse: kDiffuse,
    kSpecular: kSpecular,
    shininess: shininess
  }
});

const rayMarchingMaterial = new THREE.ShaderMaterial({
  uniforms: {
    time: ticks,
    resolution: resolution,
  }
});

const helmetAlbedoMap = new THREE.TextureLoader().load( 'gltf/Default_albedo.jpg' );
helmetAlbedoMap.colorSpace = THREE.SRGBColorSpace;
helmetAlbedoMap.flipY = false;
helmetAlbedoMap.wrapS = 1000;
helmetAlbedoMap.wrapT = 1000;

// TODO: implement helmetMetalRoughnessMap, helmetEmissiveMap, helmetNormalMap, helmetAOMap
// similarly to how helmetAlbedoMap is implemented

const helmetMetalRoughnessMap = new THREE.TextureLoader().load('gltf/Default_metalRoughness.jpg');
helmetMetalRoughnessMap.flipY = false;  // Prevent flipping of the texture (common practice for consistency in 3D rendering)
helmetMetalRoughnessMap.wrapS = THREE.RepeatWrapping;  // Repeat the texture horizontally
helmetMetalRoughnessMap.wrapT = THREE.RepeatWrapping;  // Repeat the texture vertically

const helmetEmissiveMap = new THREE.TextureLoader().load('gltf/Default_emissive.jpg');
helmetEmissiveMap.flipY = false;  // Prevent flipping of the texture
helmetEmissiveMap.wrapS = THREE.RepeatWrapping;  // Repeat the texture horizontally
helmetEmissiveMap.wrapT = THREE.RepeatWrapping;  // Repeat the texture vertically

const helmetNormalMap = new THREE.TextureLoader().load('gltf/Default_normal.jpg');
helmetNormalMap.flipY = false;  // Prevent flipping of the texture
helmetNormalMap.wrapS = THREE.RepeatWrapping;  // Repeat the texture horizontally
helmetNormalMap.wrapT = THREE.RepeatWrapping;  // Repeat the texture vertically

const helmetA0Map = new THREE.TextureLoader().load('gltf/Default_AO.jpg');
helmetA0Map.flipY = false;  // Prevent flipping of the texture
helmetA0Map.wrapS = THREE.RepeatWrapping;  // Repeat the texture horizontally
helmetA0Map.wrapT = THREE.RepeatWrapping;  // Repeat the texture vertically

const helmetPBRMaterial = new THREE.MeshStandardMaterial({
  // TODO: pass texture maps to the material. Note that
  // both metalnessMap and roughnessMap should be set to the same
  // texture map
  map: helmetAlbedoMap,
  metalnessMap: helmetMetalRoughnessMap,
  roughnessMap: helmetEmissiveMap,
  emissiveMap: helmetEmissiveMap,
  normalMap: helmetNormalMap,
  aoMap: helmetA0Map,
  roughness: 0.5,   
  color: new THREE.Color(1, 1, 1),
  envMapIntensity: 1.0  
});
// TODO: set the material's emissive color and metalness
helmetPBRMaterial.emissive = new THREE.Color(0x444444);
helmetPBRMaterial.metalness = 1.0;
helmetPBRMaterial.emissiveIntensity = 1.0; 


// Load shaders
const shaderFiles = [
  'glsl/sphere.vs.glsl',
  'glsl/sphere.fs.glsl',
  'glsl/blinn_phong.vs.glsl',
  'glsl/blinn_phong.fs.glsl',
  'glsl/raymarching.vs.glsl',
  'glsl/raymarching.fs.glsl',
];

new SourceLoader().load(shaderFiles, function (shaders) {
  sphereMaterial.vertexShader = shaders['glsl/sphere.vs.glsl'];
  sphereMaterial.fragmentShader = shaders['glsl/sphere.fs.glsl'];

  blinnPhongMaterial.vertexShader = shaders['glsl/blinn_phong.vs.glsl'];
  blinnPhongMaterial.fragmentShader = shaders['glsl/blinn_phong.fs.glsl'];

  rayMarchingMaterial.vertexShader = shaders['glsl/raymarching.vs.glsl'];
  rayMarchingMaterial.fragmentShader = shaders['glsl/raymarching.fs.glsl'];
});

// Define the shader modes
const shaders = {
  BLINNPHONG: { key: 0, material: blinnPhongMaterial },
  RAYMARCHING: { key: 1, material: rayMarchingMaterial },
  PBR: { key: 2, material: helmetPBRMaterial },
};

let mode = shaders.RAYMARCHING.key; // Default

// Set up scenes
let scenes = [];
for (let shader of Object.values(shaders)) {
  // Create the scene
  let scene, camera, worldFrame;
  if (shader.material == rayMarchingMaterial) {
    ({ scene, camera } = createRayMarchingScene(canvas, renderer));
    const plane = new THREE.PlaneGeometry(2, 2);
    scene.add(new THREE.Mesh(plane, shaders.RAYMARCHING.material));
  } else {
    ({ scene, camera, worldFrame } = createScene(canvas, renderer));
    
    // Create the main sphere geometry (light source)
    // https://threejs.org/docs/#api/en/geometries/SphereGeometry
    const sphereGeometry = new THREE.SphereGeometry(1.0, 32.0, 32.0);
    const sphere = new THREE.Mesh(sphereGeometry, sphereMaterial);
    sphere.position.set(0.0, 1.5, 0.0);
    sphere.parent = worldFrame;
    scene.add(sphere);
  }

  // Load the helmet, for scene key 3.
  if(shader.material == helmetPBRMaterial){
    
    loadAndPlaceGLB('gltf/DamagedHelmet.glb', shaders.PBR.material, function(helmet) {
      helmet.position.set(0, 0, -10.0);
      helmet.scale.set(7, 7, 7);
      helmet.parent = worldFrame;
      scene.add(helmet);
    });

    const ambientLight = new THREE.AmbientLight(0xffffff, 3.0);
    scene.add(ambientLight);

    sphereLight.parent = worldFrame;
    scene.add(sphereLight);
  } else {
    // If there's no helmet, then only place the snowman. i.e. key 1, 2
    loadAndPlaceOBJ('obj/snowman.obj', shader.material, function (snowman) {
      snowman.position.set(0.0, 0.0, -10.0);
      snowman.rotation.y = 0.0;
      snowman.scale.set(1.0e-3, 1.0e-3, 1.0e-3);
      snowman.parent = worldFrame;
      scene.add(snowman);
    });
  }

  scenes.push({ scene, camera });
}



// Listen to keyboard events.
const keyboard = new THREEx.KeyboardState();
function checkKeyboard() {

  if (keyboard.pressed("1"))
    mode = shaders.BLINNPHONG.key;
  else if (keyboard.pressed("2"))
    mode = shaders.RAYMARCHING.key;
  else if (keyboard.pressed("3"))
    mode = shaders.PBR.key;

  if (mode != shaders.RAYMARCHING.key) {
    if (keyboard.pressed("W"))
      spherePosition.value.z -= 0.3;
    else if (keyboard.pressed("S"))
      spherePosition.value.z += 0.3;

    if (keyboard.pressed("A"))
      spherePosition.value.x -= 0.3;
    else if (keyboard.pressed("D"))
      spherePosition.value.x += 0.3;

    if (keyboard.pressed("E"))
      spherePosition.value.y -= 0.3;
    else if (keyboard.pressed("Q"))
      spherePosition.value.y += 0.3;

    sphereLight.position.set(spherePosition.value.x, spherePosition.value.y, spherePosition.value.z);
  } else {
    const canvas = renderer.domElement;
    resolution.value.set(canvas.width, canvas.height, 1);
  }

  // The following tells three.js that some uniforms might have changed
  sphereMaterial.needsUpdate = true;
  blinnPhongMaterial.needsUpdate = true;
  helmetPBRMaterial.needsUpdate = true;
  rayMarchingMaterial.needsUpdate = true;
}

let clock = new THREE.Clock;

// Setup update callback
function update() {
  checkKeyboard();
  ticks.value += clock.getDelta();

  // Requests the next update call, this creates a loop
  requestAnimationFrame(update);
  const { scene, camera } = scenes[mode];
  renderer.render(scene, camera);
}

// Start the animation loop.
update();
