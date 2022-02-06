#ifndef RAY_GEOMETRY
#define RAY_GEOMETRY

#define EPSILON 0.00000001

void GroundPlaneRayIntersection(Ray ray, inout RayCollision collision)
{
	// Direction is unit length, so origin-plane intersection is at distance O.y/D.y, D.y < 0 if it hits the plane so we need a negative.
	float distance = -ray.origin.y / ray.direction.y;

	// Check whether it points down and whether it hits the plane before reaching the skybox.
	if (distance > 0.0 && distance < collision.distance)
	{
		collision.distance = distance;
		collision.position = ray.origin + distance * ray.direction;
		collision.positionNormal = float3(0, 1, 0);

		collision.specular = float3(1.0f, 1.0f, 1.0f);
		collision.albedo = float3(.0, .0, .0);
		collision.emission = float3(0, 0, 0);
	}

}

void SphereRayIntersection(Ray ray, inout RayCollision collision, Sphere sphere)//, float3 sphereposition, float sphereradius)
{
	float3 spherePosition = sphere.position;
	float sphereRadius = sphere.radius;

	// Vector pointing from the Origin to the center of the sphere.
	float3 distanceVector = ray.origin - spherePosition;

	// p1 defines the point where the ray enters the sphere, the dot product is correlated with the overlap between the distance vector and the ray direction. 
	// Large overlap -> ray direction and distance vector close to the same direction.
	float p1 = -dot(ray.direction, distanceVector);

	// Check whether the ray hits the sphere.
	float p2Squared = p1 * p1 - dot(distanceVector, distanceVector) + sphereRadius * sphereRadius;

	if (p2Squared < 0)
		return;

	// Find the point where the ray exits the sphere.
	float p2 = sqrt(p2Squared);

	// Find the distance between the ray origin and the entry (or exit) point of the sphere.
	float distance;

	if (p1 - p2 > 0)
		distance = p1 - p2;
	else
		distance = p1 + p2;

	if (distance > 0.0 && distance < collision.distance)
	{
		collision.distance = distance;
		collision.position = ray.origin + distance * ray.direction;
		// Find the normal vector.
		collision.positionNormal = normalize(collision.position - spherePosition);

		collision.albedo = sphere.albedo;
		collision.specular = sphere.specular;
		collision.emission = sphere.emission;
	}
}

struct TriangleCollisionPoint
{
	//public float u;
	//public float v;
	float3 position;
	float distance;
};


// Moller-Trumbore triangle-ray intersection, following https://en.wikipedia.org/wiki/M%C3%B6ller%E2%80%93Trumbore_intersection_algorithm .
bool TriangleRayIntersection(Ray ray, float3 vertex0, float3 vertex1, float3 vertex2, inout TriangleCollisionPoint trianglecollisionpoint)
{
	float3 edge0 = vertex1 - vertex0;
	float3 edge1 = vertex2 - vertex0;

	float3 h = cross(ray.direction, edge1);
	float a = dot(edge0, h);

	if (a > -1 * EPSILON && a < EPSILON)
		return false;

	float f = 1.0f / a;
	float3 s = ray.origin - vertex0;

	float u = f * dot(s, h);
	
	if (u < 0.0f || u > 1.0f)
		return false;

	float3 q = cross(s, edge0);
	float v = f * dot(ray.direction, q);

	if (v < 0.0f || u + v > 1.0f)
		return false;

	float distance = f * dot(edge1, q);

	if (distance > EPSILON)
	{
		trianglecollisionpoint.position = ray.origin + ray.direction * distance;
		trianglecollisionpoint.distance = distance;

		return true;
	}
	else
		return false;
}

// Uses Heron's formula, https://www.mathopenref.com/heronsformula.html .
float TriangleArea(float a, float b, float c)
{
	float s = (a + b + c) / 2.0f;

	return sqrt(s * (s - a) * (s - b) * (s - c));
}

void MeshRayIntersection(Ray ray, inout RayCollision collision, MeshObject meshobject)
{
	uint offset = meshobject.indicesOffset;
	uint count = meshobject.indicesCount;

	for (uint i = offset; i < offset + count; i += 3)
	{
		float3 v0 = (mul(meshobject.localToWorldMatrix, float4(_vertices[_indices[i]], 1))).xyz;
		float3 v1 = (mul(meshobject.localToWorldMatrix, float4(_vertices[_indices[i + 1]], 1))).xyz;
		float3 v2 = (mul(meshobject.localToWorldMatrix, float4(_vertices[_indices[i + 2]], 1))).xyz;

		
		TriangleCollisionPoint triangleCollisionPoint;

		if (TriangleRayIntersection(ray, v0, v1, v2, triangleCollisionPoint))
		{
			if (triangleCollisionPoint.distance > 0 && triangleCollisionPoint.distance < collision.distance)
			{
				float3x3 localToWorldMatrix3x3 = (float3x3) meshobject.localToWorldMatrix;
				
				float3 n0 = mul(localToWorldMatrix3x3, _normals[_indices[i]]);
				float3 n1 = mul(localToWorldMatrix3x3, _normals[_indices[i + 1]]);
				float3 n2 = mul(localToWorldMatrix3x3, _normals[_indices[i + 2]]);

				float edge0 = length(v1 - v0);
				float edge1 = length(v2 - v0);
				float edge2 = length(v2 - v1);

				float r0 = length(triangleCollisionPoint.position - v0);
				float r1 = length(triangleCollisionPoint.position - v1);
				float r2 = length(triangleCollisionPoint.position - v2);

				float u01 = TriangleArea(r0, r1, edge0);
				float u02 = TriangleArea(r0, r2, edge1);
				float u12 = TriangleArea(r1, r2, edge2);

				collision.distance = triangleCollisionPoint.distance;
				collision.position = triangleCollisionPoint.position;

				//collision.positionNormal = normalize(cross(v1 - v0, v2 - v0));
				collision.positionNormal = normalize( u01 * n2 + u02 * n1 + u12 * n0 );

				// NORMAL INTERPOLATION VISUALISATION ///////////
				//collision.Albedo = normalize(u01 * float3(1, 0, 0) + u02 * float3(0, 1, 0) + u12 * float3(0, 0, 1));
				//collision.Emission = normalize(u01 * float3(1, 0, 0) + u02 * float3(0, 1, 0) + u12 * float3(0, 0, 1));;
				/////////////////////////////////////////////////
		
				collision.albedo = meshobject.albedo;
				collision.specular = meshobject.specular;
				collision.emission = meshobject.emission;
				collision.alpha = meshobject.alpha;
				
			}
		}
	}
}

#endif 