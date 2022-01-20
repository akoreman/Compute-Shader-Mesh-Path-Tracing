#ifndef RAY_FUNCTIONS
#define RAY_FUNCTIONS

struct Ray
{
	float3 Origin;
	float3 Direction;
	float3 Energy;
};

Ray CreateRay(float3 Origin, float3 Direction)
{
	Ray ray;

	ray.Origin = Origin;
	ray.Direction = Direction;
	ray.Energy = float3(1, 1, 1);

	return ray;
}

Ray CreateCameraRay(float2 xy)
{
	//float3 Origin = mul(_CameraToWorldProj, float4(0,0,0,1)).xyz;
	float3 Origin = _CameraPosition.xyz;

	float3 Direction = mul(_CameraInverseProj, float4(xy, 0, 1)).xyz;
	Direction = mul(_CameraToWorldProj, float4(Direction, 0)).xyz;
	Direction = normalize(Direction);

	return CreateRay(Origin, Direction);
}

struct RayCollision
{
	float3 Position;
	float Distance;
	float3 positionNormal;

	float3 Specular;
	float3 Albedo;
};

RayCollision CreateRayCollision()
{
	RayCollision Collision;

	Collision.Position = float3(0, 0, 0);
	Collision.Distance = inf;
	Collision.positionNormal = float3(0, 0, 0);
	Collision.Specular = float3(0, 0, 0);
	Collision.Albedo = float3(0, 0, 0);

	return Collision;
};

#endif 