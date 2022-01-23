# Compute Shader Mesh Path Tracing
WIP. Working on a path trace engine to raytrace meshes. Implemented using HLSL compute shaders in Unity3D.

**Currently implemented:**
- Raytracing for opaque planes and spheres.
- Phong specular reflections.
- Lambert diffuse relections.
- Importance sampling to speed-up convergence.
- Pathtracing for meshes from Unity gameObjects.

**To do:**
- Non-opaque materials.
- Directional lights + multi-importance sampling.

# Screenshots

<img src="https://raw.github.com/akoreman/Compute-Shader-Mesh-Ray-Tracing/main/images/SpecReflections.PNG" width="400">  

<img src="https://raw.github.com/akoreman/Compute-Shader-Mesh-Ray-Tracing/main/images/Geometry.PNG" width="400">  

