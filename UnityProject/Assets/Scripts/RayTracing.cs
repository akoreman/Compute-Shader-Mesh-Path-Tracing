using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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

    uint _SampleNumber = 0;

    void Awake()
    {
        // Default movement speeds.
        movementSpeed = 0.1f;
        rotationSpeed = 60f;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        camera = Camera.current;
        lightDirection = directionalLight.transform.forward;
        lightIntensity = directionalLight.intensity;

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

        SetUpGeometryBuffer();

        // Send stuff to the computer shader.
        rayTracer.SetMatrix("_CameraToWorldProj", camera.cameraToWorldMatrix);
        rayTracer.SetMatrix("_CameraInverseProj", camera.projectionMatrix.inverse);
        rayTracer.SetVector("_CameraPosition", camera.transform.position);
        rayTracer.SetVector("_CameraRotation", camera.transform.eulerAngles);
        rayTracer.SetVector("_LightVector", new Vector4(lightDirection.x, lightDirection.y, lightDirection.z, lightIntensity));

        rayTracer.SetBuffer(0, "_SphereBuffer", sphereBuffer);

        rayTracer.SetFloat("_Seed", Random.value);

        rayTracer.SetTexture(0, "Source", source);
        rayTracer.SetTexture(0, "Target", target);
        rayTracer.SetTexture(0, "_SkyBoxTexture", skyBoxTexture);

        rayTracer.SetVector("_PixelOffset", new Vector2(Random.value, Random.value));


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

    /*
    struct Plane
    {
        Vector3 Specular;
        Vector3 Albedo;
    };
    */

    void OnEnable()
    {
        _SampleNumber = 0;
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
    }

}