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

    private RenderTexture _converged;

    public Texture skyBoxTexture;

    float movementSpeed;
    float rotationSpeed;

    //Vector3 lightDirection;
    //float lightIntensity;

    Material _addMaterial;
    ComputeBuffer sphereBuffer;

    private static List<MeshObject> _meshObjects = new List<MeshObject>();
    private static List<Vector3> _vertices = new List<Vector3>();
    private static List<int> _indices = new List<int>();
    private static List<Vector3> _normals = new List<Vector3>();
  
    private ComputeBuffer _meshObjectBuffer;
    private ComputeBuffer _vertexBuffer;
    private ComputeBuffer _indexBuffer;
    private ComputeBuffer _normalBuffer;
    

    private static List<AddToPathEngine> _rayTracingMeshes = new List<AddToPathEngine>();
    private static bool _meshBufferNeedRebuilding = false;

    uint _SampleNumber = 0;

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

        if (_converged == null)
        {
            _converged = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);

            _converged.enableRandomWrite = true;
            _converged.Create();
        }

        //SetUpGeometryBuffer();
        BuildMeshBuffer();

        // Send stuff to the computer shader.
        rayTracer.SetMatrix("_CameraToWorldProj", camera.cameraToWorldMatrix);
        rayTracer.SetMatrix("_CameraInverseProj", camera.projectionMatrix.inverse);
        rayTracer.SetVector("_CameraPosition", camera.transform.position);
        rayTracer.SetVector("_CameraRotation", camera.transform.eulerAngles);
        //rayTracer.SetVector("_LightVector", new Vector4(lightDirection.x, lightDirection.y, lightDirection.z, lightIntensity));

        //rayTracer.SetBuffer(0, "_SphereBuffer", sphereBuffer);
        rayTracer.SetBuffer(0, "_MeshObjects", _meshObjectBuffer);
        rayTracer.SetBuffer(0, "_Vertices", _vertexBuffer);
        rayTracer.SetBuffer(0, "_Indices", _indexBuffer);
        rayTracer.SetBuffer(0, "_Normals", _normalBuffer);

        //rayTracer.SetBuffer(0, "_speculars", _specularsBuffer);

        rayTracer.SetFloat("_Seed", UnityEngine.Random.value);

        rayTracer.SetTexture(0, "Source", source);
        rayTracer.SetTexture(0, "Target", target);
        rayTracer.SetTexture(0, "_SkyBoxTexture", skyBoxTexture);

        rayTracer.SetVector("_PixelOffset", new Vector2(UnityEngine.Random.value, UnityEngine.Random.value));


        int threadGroupsX = (int)Mathf.Ceil(camera.pixelWidth / 8f);
        int threadGroupsY = (int)Mathf.Ceil(camera.pixelHeight / 8f);

        // Run the compute shdader and render the final texture to screen.
        rayTracer.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        if (_addMaterial == null)
            _addMaterial = new Material(Shader.Find("Hidden/multipleSampleShader"));

        _addMaterial.SetFloat("_SampleNumber", _SampleNumber);

        Graphics.Blit(target, _converged, _addMaterial);
        Graphics.Blit(_converged, destination);

        _SampleNumber++;

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
        _SampleNumber = 0;
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
            _SampleNumber = 0;
            camera.transform.Rotate(new Vector3(0, 0, rotationSpeed * Time.deltaTime));
        }

        if (Input.GetKey("m"))
        {
            _SampleNumber = 0;
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
            _SampleNumber = 0;
        }

        if (Input.GetKey("space"))
        {
            _SampleNumber = 0;
            Vector3 Direction = camera.transform.forward;
            currentPosition += Direction * movementSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.LeftControl))
        {
            _SampleNumber = 0;
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
 
        if (_meshObjectBuffer != null)
            _meshObjectBuffer.Release();

        if (_vertexBuffer != null)
            _vertexBuffer.Release();

        if (_indexBuffer != null)
            _indexBuffer.Release();

        if (_normalBuffer != null)
            _normalBuffer.Release();

    }


    public static void RegisterObject(AddToPathEngine obj)
    {
        _rayTracingMeshes.Add(obj);
        _meshBufferNeedRebuilding = true;
    }
    public static void UnregisterObject(AddToPathEngine obj)
    {
        _rayTracingMeshes.Remove(obj);
        _meshBufferNeedRebuilding = true;
    }

    struct MeshObject
    {
        public Matrix4x4 localToWorldMatrix;
        public int indices_offset;
        public int indices_count;
        public Vector3 Specular;
        public Vector3 Albedo;
        public Vector3 Emission;
    }

    private void BuildMeshBuffer()
    {
        _meshBufferNeedRebuilding = false;

        _meshObjects.Clear();
        _vertices.Clear();
        _indices.Clear();
        _normals.Clear();

        // Add all vertices, indices and material properties to the relevant buffers.
        foreach (AddToPathEngine obj in _rayTracingMeshes)
        {
            Mesh mesh = obj.GetComponent<MeshFilter>().sharedMesh;

            int firstVertex = _vertices.Count;
            _vertices.AddRange(mesh.vertices);

            int firstIndex = _indices.Count;
            var indices = mesh.GetIndices(0);

            var normals = mesh.normals;
            _normals.AddRange(normals);
             
            var indicesOffset = Array.ConvertAll(indices, x => x + firstVertex);

            _indices.AddRange(indicesOffset);

            _meshObjects.Add(new MeshObject()
            {
                localToWorldMatrix = obj.transform.localToWorldMatrix,
                indices_offset = firstIndex,
                indices_count = indices.Length,
                Specular = obj.Specular,
                Albedo = obj.Albedo,
                Emission = obj.Emission
            });
        }

        // Set-up the buffers.
        _meshObjectBuffer = new ComputeBuffer(_meshObjects.Count, 108);
        _meshObjectBuffer.SetData(_meshObjects);

        _vertexBuffer = new ComputeBuffer(_vertices.Count, 12);
        _vertexBuffer.SetData(_vertices);

        _indexBuffer = new ComputeBuffer(_indices.Count, 4);
        _indexBuffer.SetData(_indices);

        _normalBuffer = new ComputeBuffer(_normals.Count, 12);
        _normalBuffer.SetData(_normals);


    }
}