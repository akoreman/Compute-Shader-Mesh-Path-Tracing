#ifndef SCENEITEMS
#define SCENEITEMS

// Structs to contain the material properties and positions of the objects in the scene.

struct Sphere
{
	float3 Position;
	float Radius;
	float3 Specular;
	float3 Albedo;
	float3 Emission;
};

struct MeshObject
{
	float4x4 localToWorldMatrix;
	int indices_offset;
	int indices_count;
	float3 Specular;
	float3 Albedo;
	float3 Emission;
};

#endif