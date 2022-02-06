#ifndef RAY_FUNCTIONS
#define RAY_FUNCTIONS

// Structs and functions to manage rays and their collisions.

struct Ray
{
	float3 origin;
	float3 direction;
	float3 intensity;
};

Ray CreateRay(float3 origin, float3 direction)
{
	Ray ray;

	ray.origin = origin;
	ray.direction = direction;
	ray.intensity = float3(1, 1, 1);

	return ray;
}

// Create rays from the pixel coordinates of the target texture.
Ray CreateCameraRay(float2 xy)
{
	float3 origin = _cameraPosition.xyz;

	float3 direction = mul(_cameraInverseProj, float4(xy, 0, 1)).xyz;
	direction = mul(_cameraToWorldProj, float4(direction, 0)).xyz;
	direction = normalize(direction);

	return CreateRay(origin, direction);
}

struct RayCollision
{
	float3 position;
	float distance;
	float3 positionNormal;

	float3 specular;
	int alpha;

	float3 albedo;

	float3 emission;
};

RayCollision CreateRayCollision()
{
	RayCollision collision;

	collision.position = float3(0, 0, 0);
	collision.distance = inf;
	collision.positionNormal = float3(0, 0, 0);
	collision.specular = float3(0, 0, 0);
	collision.albedo = float3(0, 0, 0);
	collision.alpha = 0;

	return collision;
};

#endif 