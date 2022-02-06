using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;


public class RayTracing : MonoBehaviour
{
    RenderTexture target;
    Camera camera;

    [SerializeField]
    ComputeShader rayTracer;

    [SerializeField]
    Light directionalLight;

    private RenderTexture converged;

    public Texture skyBoxTexture;

    float movementSpeed;
    float rotationSpeed;

    //Vector3 lightDirection;
    //float lightIntensity;

    Material addMaterial;
    ComputeBuffer sphereBuffer;

    private static List<MeshObject> meshObjects = new List<MeshObject>();
    private static List<Vector3> vertices = new List<Vector3>();
    private static List<int> indices = new List<int>();
    private static List<Vector3> normals = new List<Vector3>();
  
    private ComputeBuffer meshObjectBuffer;
    private ComputeBuffer vertexBuffer;
    private ComputeBuffer indexBuffer;
    private ComputeBuffer normalBuffer;
    

    private static List<AddToPathEngine> rayTracingMeshes = new List<AddToPathEngine>();
    private static bool meshBufferNeedRebuilding = false;

    uint sampleNumber = 0;

    void Awake()
    {
        // Default movement speeds.
        movementSpeed = 0.1f;
        rotationSpeed = 60f;
        //camera = Camera.current;
    }


    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        camera = Camera.current;
        //lightDirection = directionalLight.transform.forward;
        //lightIntensity = directionalLight.intensity;

        // If no render target defined, set one up.
        if (target == null)
        {
            target = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);

            target.enableRandomWrite = true;
            target.Create();
        }

        if (converged == null)
        {
            converged = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);

            converged.enableRandomWrite = true;
            converged.Create();
        }

        //SetUpGeometryBuffer();
        BuildMeshBuffer();

        // Send stuff to the computer shader.
        rayTracer.SetMatrix("_cameraToWorldProj", camera.cameraToWorldMatrix);
        rayTracer.SetMatrix("_cameraInverseProj", camera.projectionMatrix.inverse);
        rayTracer.SetVector("_cameraPosition", camera.transform.position);
        rayTracer.SetVector("_cameraRotation", camera.transform.eulerAngles);
        //rayTracer.SetVector("_LightVector", new Vector4(lightDirection.x, lightDirection.y, lightDirection.z, lightIntensity));

        //rayTracer.SetBuffer(0, "_SphereBuffer", sphereBuffer);
        rayTracer.SetBuffer(0, "_meshObjects", meshObjectBuffer);
        rayTracer.SetBuffer(0, "_vertices", vertexBuffer);
        rayTracer.SetBuffer(0, "_indices", indexBuffer);
        rayTracer.SetBuffer(0, "_normals", normalBuffer);

        //rayTracer.SetBuffer(0, "_speculars", _specularsBuffer);

        rayTracer.SetFloat("_seed", UnityEngine.Random.value);

        //rayTracer.SetTexture(0, "_source", source);
        rayTracer.SetTexture(0, "_target", target);
        rayTracer.SetTexture(0, "_skyBoxTexture", skyBoxTexture);

        rayTracer.SetVector("_pixelOffset", new Vector2(UnityEngine.Random.value, UnityEngine.Random.value));


        int threadGroupsX = (int)Mathf.Ceil(camera.pixelWidth / 8f);
        int threadGroupsY = (int)Mathf.Ceil(camera.pixelHeight / 8f);

        // Run the compute shdader and render the final texture to screen.
        rayTracer.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        if (addMaterial == null)
            addMaterial = new Material(Shader.Find("Hidden/multipleSampleShader"));

        addMaterial.SetFloat("_sampleNumber", sampleNumber);

        Graphics.Blit(target, converged, addMaterial);
        Graphics.Blit(converged, destination);


        sampleNumber++;

        ReleaseBuffer();
    }
    /*
    struct Sphere
    {
        public Vector3 Position;
        public float Radius;
        public Vector3 Specular;
        public Vector3 Albedo;
        public Vector3 Emission;
    };
    
    void SetUpGeometryBuffer()
    {
        List<Sphere> spheres = new List<Sphere>();

        Sphere sphere;
        sphere.Position = new Vector3(0, .5f, 0);
        sphere.Radius = .5f;
        sphere.Specular = new Vector3(0f, 0f, 0f);
        sphere.Albedo = new Vector3(1f, 1f, 1f);
        sphere.Emission = new Vector3(1f,1f,1f);

        spheres.Add(sphere);

        sphere.Position = new Vector3(4f, 2f, 1f);
        sphere.Radius = 1f;
        sphere.Specular = new Vector3(1f, 1f, 1f);
        sphere.Albedo = new Vector3(0f, 0f, 0f);
        sphere.Emission = new Vector3(0f, 0f, 0f);


        spheres.Add(sphere);

        sphere.Position = new Vector3(1f, 2f, 1f);
        sphere.Radius = 1f;
        sphere.Specular = new Vector3(0f, 0f, 0f);
        sphere.Albedo = new Vector3(1f, 0f, 0f);
        sphere.Emission = new Vector3(0f, 0f, 0f);


        spheres.Add(sphere);

        sphereBuffer = new ComputeBuffer(spheres.Count, 52);
        sphereBuffer.SetData(spheres);
    }
    */
    void OnEnable()
    {
        sampleNumber = 0;
        //BuildMeshBuffer();
    }

    void Update()
    {
        if (Input.GetKeyDown("x"))
        {
            movementSpeed /= Mathf.Sqrt(10f);
            print(movementSpeed);
        }

        if (Input.GetKeyDown("c"))
        {
            movementSpeed *= Mathf.Sqrt(10f);
            print(movementSpeed);
        }

        if (Input.GetKeyDown("v"))
        {
            rotationSpeed -= 5f;
            rotationSpeed = Mathf.Max(rotationSpeed, 0f);
            print(rotationSpeed);
        }

        if (Input.GetKeyDown("b"))
        {
            rotationSpeed += 5f;
            print(rotationSpeed);
        }

        if (Input.GetKey("n"))
        {
            sampleNumber = 0;
            camera.transform.Rotate(new Vector3(0, 0, rotationSpeed * Time.deltaTime));
        }

        if (Input.GetKey("m"))
        {
            sampleNumber = 0;
            camera.transform.Rotate(new Vector3(0, 0, -rotationSpeed * Time.deltaTime));
        }

        if (Input.GetKeyDown("r"))
        {
            camera.transform.localPosition = new Vector3(0f, 0f, 0f);
            camera.transform.eulerAngles = new Vector3(0f, 0f, 0f);
        }

        Vector3 currentPosition = camera.transform.localPosition;

        camera.transform.Rotate(new Vector3(0, Input.GetAxis("Horizontal") * rotationSpeed * Time.deltaTime, 0));
        camera.transform.Rotate(new Vector3(-Input.GetAxis("Vertical") * rotationSpeed * Time.deltaTime, 0, 0));

        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            sampleNumber = 0;
        }

        if (Input.GetKey("space"))
        {
            sampleNumber = 0;
            Vector3 Direction = camera.transform.forward;
            currentPosition += Direction * movementSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.LeftControl))
        {
            sampleNumber = 0;
            Vector3 Direction = camera.transform.forward;
            currentPosition -= Direction * movementSpeed * Time.deltaTime;
        }

        camera.transform.localPosition = currentPosition;

        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }

    }

    void ReleaseBuffer()
    {
        if (sphereBuffer != null)
            sphereBuffer.Release();
 
        if (meshObjectBuffer != null)
            meshObjectBuffer.Release();

        if (vertexBuffer != null)
            vertexBuffer.Release();

        if (indexBuffer != null)
            indexBuffer.Release();

        if (normalBuffer != null)
            normalBuffer.Release();

    }


    public static void RegisterObject(AddToPathEngine obj)
    {
        rayTracingMeshes.Add(obj);
        meshBufferNeedRebuilding = true;
    }
    public static void UnregisterObject(AddToPathEngine obj)
    {
        rayTracingMeshes.Remove(obj);
        meshBufferNeedRebuilding = true;
    }

    struct MeshObject
    {
        public Matrix4x4 localToWorldMatrix;
        public int indicesOffset;
        public int indicesCount;
        public Vector3 specular;
        public Vector3 albedo;
        public Vector3 emission;
        public int alpha;
    }

    private void BuildMeshBuffer()
    {
        meshBufferNeedRebuilding = false;

        meshObjects.Clear();
        vertices.Clear();
        indices.Clear();
        normals.Clear();

        // Add all vertices, indices and material properties to the relevant buffers.
        foreach (AddToPathEngine obj in rayTracingMeshes)
        {
            Mesh mesh = obj.GetComponent<MeshFilter>().sharedMesh;

            int firstVertex = vertices.Count;
            vertices.AddRange(mesh.vertices);

            int firstIndex = RayTracing.indices.Count;
            var indices = mesh.GetIndices(0);

            var normals = mesh.normals;
            RayTracing.normals.AddRange(normals);
             
            var indicesOffset = Array.ConvertAll(indices, x => x + firstVertex);

            RayTracing.indices.AddRange(indicesOffset);

            meshObjects.Add(new MeshObject()
            {
                localToWorldMatrix = obj.transform.localToWorldMatrix,
                indicesOffset = firstIndex,
                indicesCount = indices.Length,
                specular = obj.specular,
                albedo = obj.albedo,
                emission = obj.emission,
                alpha = obj.alpha
            });
        }

        // Set-up the buffers.
        meshObjectBuffer = new ComputeBuffer(meshObjects.Count, 112);
        meshObjectBuffer.SetData(meshObjects);

        vertexBuffer = new ComputeBuffer(vertices.Count, 12);
        vertexBuffer.SetData(vertices);

        indexBuffer = new ComputeBuffer(indices.Count, 4);
        indexBuffer.SetData(indices);

        normalBuffer = new ComputeBuffer(normals.Count, 12);
        normalBuffer.SetData(normals);

    }
}