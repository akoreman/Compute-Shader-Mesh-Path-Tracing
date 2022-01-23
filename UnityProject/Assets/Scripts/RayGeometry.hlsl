#ifndef RAY_GEOMETRY
#define RAY_GEOMETRY

#define EPSILON 0.00000001

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

		collision.Specular = float3(1.0f, 1.0f, 1.0f);
		collision.Albedo = float3(.0, .0, .0);
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

struct TriangleCollisionPoint
{
	//public float u;
	//public float v;
	float3 Position;
	float Distance;
};


// Moller-Trumbore triangle-ray intersection, following https://en.wikipedia.org/wiki/M%C3%B6ller%E2%80%93Trumbore_intersection_algorithm .
bool TriangleRayIntersection(Ray ray, float3 Vertex0, float3 Vertex1, float3 Vertex2, inout TriangleCollisionPoint triangleCollisionPoint)
{
	float3 Edge0 = Vertex1 - Vertex0;
	float3 Edge1 = Vertex2 - Vertex0;

	float3 h = cross(ray.Direction, Edge1);
	float a = dot(Edge0, h);

	if (a > -1 * EPSILON && a < EPSILON)
		return false;

	float f = 1.0f / a;
	float3 s = ray.Origin - Vertex0;

	float u = f * dot(s, h);
	
	if (u < 0.0f || u > 1.0f)
		return false;

	float3 q = cross(s, Edge0);
	float v = f * dot(ray.Direction, q);

	if (v < 0.0f || u + v > 1.0f)
		return false;

	float Distance = f * dot(Edge1, q);

	if (Distance > EPSILON)
	{
		triangleCollisionPoint.Position = ray.Origin + ray.Direction * Distance;
		triangleCollisionPoint.Distance = Distance;

		return true;
	}
	else
		return false;
}

float TriangleArea(float a, float b, float c)
{
	float s = (a + b + c) / 2.0f;

	return sqrt(s * (s - a) * (s - b) * (s - c));
}

void MeshRayIntersection(Ray ray, inout RayCollision collision, MeshObject meshObject)
{
	uint offset = meshObject.indices_offset;
	uint Count = meshObject.indices_count;

	for (uint i = offset; i < offset + Count; i += 3)
	{
		float3 v0 = (mul(meshObject.localToWorldMatrix, float4(_Vertices[_Indices[i]], 1))).xyz;
		float3 v1 = (mul(meshObject.localToWorldMatrix, float4(_Vertices[_Indices[i + 1]], 1))).xyz;
		float3 v2 = (mul(meshObject.localToWorldMatrix, float4(_Vertices[_Indices[i + 2]], 1))).xyz;

		
		TriangleCollisionPoint triangleCollisionPoint;

		if (TriangleRayIntersection(ray, v0, v1, v2, triangleCollisionPoint))
		{
			if (triangleCollisionPoint.Distance > 0 && triangleCollisionPoint.Distance < collision.Distance)
			{
				float3x3 localToWorldMatrix3x3 = (float3x3) meshObject.localToWorldMatrix;
				
				float3 n0 = mul(localToWorldMatrix3x3, _Normals[_Indices[i]]);
				float3 n1 = mul(localToWorldMatrix3x3, _Normals[_Indices[i + 1]]);
				float3 n2 = mul(localToWorldMatrix3x3, _Normals[_Indices[i + 2]]);

				//float3 n0 = _Normals[_Indices[i]];
				//float3 n1 = _Normals[_Indices[i+1]];
				//float3 n2 = _Normals[_Indices[i+2]];

				float Edge0 = length(v1 - v0);
				float Edge1 = length(v2 - v0);
				float Edge2 = length(v2 - v1);

				float r0 = length(triangleCollisionPoint.Position - v0);
				float r1 = length(triangleCollisionPoint.Position - v1);
				float r2 = length(triangleCollisionPoint.Position - v2);

				float u01 = TriangleArea(r0, r1, Edge0);
				float u02 = TriangleArea(r0, r2, Edge1);
				float u12 = TriangleArea(r1, r2, Edge2);

				collision.Distance = triangleCollisionPoint.Distance;
				collision.Position = ray.Origin + triangleCollisionPoint.Distance * ray.Direction;
				//collision.Position = triangleCollisionPoint.Position;
				collision.positionNormal = normalize(cross(v1 - v0, v2 - v0));

				//collision.positionNormal = normalize( u01 * n2 + u02 * n1 + u12 * n0 );
				//collision.positionNormal = normalize(n2 + n1 + n0);
				//collision.positionNormal = n0;

				/*
				collision.Albedo = normalize(u01 * float3(1, 0, 0) + u02 * float3(0, 1, 0) + u12 * float3(0, 0, 1));
				collision.Specular = 0.0f;
				collision.Emission = 0.0f;
				*/

				//collision.Albedo = (collision.positionNormal);

				collision.Albedo = meshObject.Albedo;
				collision.Specular = meshObject.Specular;
				collision.Emission = meshObject.Emission;
			}
		}
	}
}

#endif 