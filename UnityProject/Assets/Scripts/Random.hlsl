#ifndef RANDOM
#define RANDOM

float2 Pixel;

// Set from RayTracing.cs ////////
float _Seed;
/////////////////////////////////

// One-liner to get pseudorandom numbers given some seed from the mains .cs file.
float rand()
{
    float result = frac(sin(_Seed / 100.0f * dot(Pixel, float2(12.9898f, 78.233f))) * 43758.5453f);
    _Seed += 1.0f;
    return result;
}

// Create a conversion matrix to generate a hemisphere relative to the normal.
float3x3 GetTangentSpace(float3 normal)
{
    // Choose a helper vector for the cross product
    float3 helper = float3(1, 0, 0);
    if (abs(normal.x) > 0.99f)
        helper = float3(0, 0, 1);
    // Generate vectors
    float3 tangent = normalize(cross(normal, helper));
    float3 binormal = normalize(cross(normal, tangent));
    return float3x3(tangent, binormal, normal);
}

// Importance sampling using cosine weighing, following https://blog.thomaspoulet.fr/uniform-sampling-on-unit-hemisphere/ .z
float3 SampleHemisphere(float3 normal, float alpha)
{
    // Uniformly sample hemisphere direction
    //float cosTheta = rand();
    float cosTheta = pow(rand(), 1.0f / (alpha + 1.0f));
    float sinTheta = sqrt(max(0.0f, 1.0f - cosTheta * cosTheta));
    float phi = 2 * PI * rand();
    float3 tangentSpaceDir = float3(cos(phi) * sinTheta, sin(phi) * sinTheta, cosTheta);
    // Transform direction to world space
    return mul(tangentSpaceDir, GetTangentSpace(normal));
}



#endif
