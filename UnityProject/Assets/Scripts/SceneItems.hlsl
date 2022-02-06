#ifndef SCENEITEMS
#define SCENEITEMS

// Structs to contain the material properties and positions of the objects in the scene.

struct Sphere
{
	float3 position;
	float radius;
	float3 specular;
	float3 albedo;
	float3 emission;
};

struct MeshObject
{
	float4x4 localToWorldMatrix;
	int indicesOffset;
	int indicesCount;
	float3 specular;
	float3 albedo;
	float3 emission;
	int alpha;
};

#endif