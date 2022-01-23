using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]

// This script is to be added as a component to gameObjects which have to be rendered in the path tracer. 
// This is also where the colours for the mesh have to be set.

public class AddToPathEngine : MonoBehaviour
{
    public Vector3 Specular = new Vector3(0f,0f,0f);
    public Vector3 Albedo = new Vector3(0f, 0f, 0f);
    public Vector3 Emission = new Vector3(0f, 0f, 0f);

    private void OnEnable()
    {
        RayTracing.RegisterObject(this);
    }
    private void OnDisable()
    {
        RayTracing.UnregisterObject(this);
    }
}
