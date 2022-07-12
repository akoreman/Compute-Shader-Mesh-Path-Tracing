using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]

// This script is to be added as a component to gameObjects which have to be rendered in the path tracer. 
// This is also where the colours for the mesh have to be set.

public class AddToPathEngine : MonoBehaviour
{
    public bool hasSpecular;
    public Vector3 specular;
    public int alpha;

    public bool hasAlbedo;
    public Vector3 albedo = new Vector3(0f, 0f, 0f);

    public bool hasEmission;
    public Vector3 emissionColor = new Vector3(0f, 0f, 0f);
    public float emissionStrength = 0.0f;

    Vector3 centerPosition;
    float radius;

    [HideInInspector]
    public Vector3 emission;

    private void OnEnable()
    {
        if (!hasAlbedo)
            albedo = new Vector3(0f, 0f, 0f);

        specular = Vector3.Normalize(specular);


        if (!hasSpecular)
            specular = new Vector3(0f, 0f, 0f);

        albedo = Vector3.Normalize(albedo);

        if (!hasEmission)
            emissionColor = new Vector3(0f, 0f, 0f);

        emissionColor = Vector3.Normalize(emissionColor) ;
        emission = emissionStrength * emissionColor;

        RayTracing.RegisterObject(this);


    }

    private void OnDisable()
    {
        RayTracing.UnregisterObject(this);
    }

    Vector3 meshAverage(Mesh mesh)
    {
        Vector3[] vertexList = mesh.vertices;

        Vector3 average = Vector3.zero;
        int count = 0;

        foreach (Vector3 vertex in vertexList)
        {
            average += vertex;
            count++;
        }

        return average / count;
    }
}
