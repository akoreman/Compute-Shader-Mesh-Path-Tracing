#ifndef RAY_GEOMETRY
#define RAY_GEOMETRY

void GroundPlaneRayIntersection(Ray ray, inout RayCollision collision)
{
	// Direction is unit length, so origin-plane intersection is at distance O.y/D.y, D.y < 0 if it hits the plane so we need a negative.
	float Distance = -ray.Origin.y / ray.Direction.y;

	// Check whether it points down and whether it hits the plane before reaching the skybox.
	if (Distance > 0.0 && Distance < collision.Distance)
	{
		collision.Distance = Distance;
		collision.Position = ray.Origin + Distance * ray.Direction;
		collision.positionNormal = float3(0, 1, 0);

		collision.Specular = float3(0, 0, 0);
		collision.Albedo = float3(.8, .8, .8);
		collision.Emission = float3(0, 0, 0);
	}

}

void SphereRayIntersection(Ray ray, inout RayCollision collision, Sphere sphere)//, float3 sphereposition, float sphereradius)
{
	float3 sphereposition = sphere.Position;
	float sphereradius = sphere.Radius;

	// Vector pointing from the Origin to the center of the sphere.
	float3 distanceVector = ray.Origin - sphereposition;

	// p1 defines the point where the ray enters the sphere, the dot product is correlated with the overlap between the distance vector and the ray direction. 
	// Large overlap -> ray direction and distance vector close to the same direction.
	float p1 = -dot(ray.Direction, distanceVector);

	// Check whether the ray hits the sphere.
	float p2Squared = p1 * p1 - dot(distanceVector, distanceVector) + sphereradius * sphereradius;

	if (p2Squared < 0)
		return;

	// Find the point where the ray exits the sphere.
	float p2 = sqrt(p2Squared);

	// Find the distance between the ray origin and the entry (or exit) point of the sphere.
	float Distance;

	if (p1 - p2 > 0)
		Distance = p1 - p2;
	else
		Distance = p1 + p2;

	if (Distance > 0.0 && Distance < collision.Distance)
	{
		collision.Distance = Distance;
		collision.Position = ray.Origin + Distance * ray.Direction;
		// Find the normal vector.
		collision.positionNormal = normalize(collision.Position - sphereposition);

		collision.Albedo = sphere.Albedo;
		collision.Specular = sphere.Specular;
		collision.Emission = sphere.Emission;
	}
}

#endif 