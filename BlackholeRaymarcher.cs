using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlackholeRaymarcher : MonoBehaviour {

    [Header("Post Processing")]
    [SerializeField] private RawImage _rawImage;
    [SerializeField] private ComputeShader _computeShader;
    [SerializeField] private float _evaporateSpeed = -0.82f;
    [SerializeField] private float _diffuseSpeed = 0.68f;
    [SerializeField][Range(0, 15)] private float _blurRadius = 10;
    private RenderTexture _mainTexture;
    private int _mainKernel;
    private int _processKernel;
    [SerializeField] private bool _blur;
    public Vector2Int resolution = new Vector2Int(1920, 1080);
    [Header("Ray marching")]
    [SerializeField] private float _fov = 45;
    [SerializeField] private int _maxSteps = 500;
    [SerializeField] private float _stepSize = 0.04f;
    [SerializeField][Range(0.00001f, 0.1f)] private float _minDist = 0.01f;
    [SerializeField] private Vector3 _roOffset = new Vector3(0, 0, -2.54f);
    [SerializeField] private Vector3 _roRotation = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 _rdRotation = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 _sdfRotation = new Vector3(0, 0, 0);
    [SerializeField] private float _brightness = 5f;
    [SerializeField][Range(0f, 20f)] private float _mipMapLevel = 0f;
    [Header("Black hole")]
    [SerializeField] private Vector3 _blackHolePosition = new Vector3(0, 0, 0);
    [SerializeField] private float _schwarzSchildRadius = 0.66f;
    [SerializeField] private float _spaceDistortion = 4.44f;
    [SerializeField] private Color _accretionDiskColor;
    [SerializeField] private float _accretionDiskRadius = 3.5f;
    [SerializeField] private float _accretionDiskHeight = 0.25f;
    [Header("Nebula")]
    [SerializeField] private Texture3D _nebulaTexture;
    [SerializeField] private float _nebulaScale = 3.2f;
    [SerializeField] private Vector3 _nebulaTwirl;
    [SerializeField] private float _nebulaCutoff = 1f;
    [Header("FBM")]
    [SerializeField] private Texture3D _fbmTexture;
    [SerializeField] private Vector3 _fbmOffset;
    [SerializeField] private float _fbmSpeed = 2f;
    [SerializeField] private float _fbmDiv = 3.2f;
    [SerializeField] private float _fbmH = 0.68f;
    [SerializeField] private float _fbmOctaves = 9.54f;
    [SerializeField] private float _fbmIntensity = 0.53f;
    [SerializeField] private Vector3 _fbmRotation = new Vector3(-0.3f, 0f, 0f);
    [SerializeField] private Vector3 _fbmDirection = new Vector3(0f, 0f, -0.09f);
    [Header("Light")]
    [SerializeField] private Transform _light;
    [SerializeField] private Texture2D _gradient;
    [SerializeField] private Vector2 _gradientTiling = Vector2.one;
    [SerializeField] private Vector2 _gradientOffset = Vector2.zero;
    [SerializeField][Range(0f, 1f)] private float _dopplerStrength = 0.15f;
    [SerializeField] private Vector3 _dopplerOffset;
    [SerializeField][Range(0f, 1f)] private float _densityScale = 0.04f;
    [SerializeField][Range(0f, 10f)] private float _shadowStrength = 2f;
    [SerializeField] private float _lightAbsorb = 0.59f;
    [SerializeField] private float _darknessThreshhold = 0.5f;
    [SerializeField] private float _transmittance = 0.26f;
    [SerializeField] private int _lightSteps = 12;
    [SerializeField] private float _maxLightDist = 10f;
    [SerializeField] private float _maxAmbientDist = 10f;
    [SerializeField][Range(0f, 10f)] private float _ambientStrength = 1f;
    [SerializeField][Range(0f, 1f)] private float _lightStepsSize = 0.04f;
    [SerializeField] private Vector3 _lightDir = new Vector3(-7.3f, -3.22f, -2.42f);
    [SerializeField][ColorUsageAttribute(true, true)] private Color _lightColor;// = new Vector3(255f, 1f, 5.32f);
    [SerializeField][ColorUsageAttribute(true, true)] private Color _ambientLight;// = new Vector3(4.16f, 15.86f, 8.9f);
    [SerializeField][ColorUsageAttribute(true, true)] private Color _backgroundColor;// = Vector3.zero;
    [Header("Stars")]
    [SerializeField] private Texture2D _starGradient;
    [SerializeField] private Vector2 _starGradientTiling;
    [SerializeField] private Texture3D _starTexture;
    [SerializeField] private Vector3 _starOffset;
    [SerializeField] private float _starDiv = 22.52f;
    [SerializeField] private float _starBrightness;
    [SerializeField][Range(0f, 1f)] private float _starSaturaiton = 1f;
    [SerializeField] private Vector3 _starRotation;
    [SerializeField] private float _starMinDistance;

    private int frameCount = 0;

    void Start() {
        _rawImage.color = Color.white;
        _mainKernel = _computeShader.FindKernel("Main");
        _processKernel = _computeShader.FindKernel("ProcessTexture");
        GenerateImage();
    }

    void Update() {
        frameCount++;

        if (frameCount % 2 == 0)
            GenerateImage();

    }

    private void GenerateImage() {
        if (Input.GetKeyDown(KeyCode.R) || _mainTexture == null || resolution.x != _mainTexture.width || resolution.y != _mainTexture.height) {
            if (_mainTexture != null) Destroy(_mainTexture);

            _mainTexture = new RenderTexture(resolution.x, resolution.y, 24);
            _mainTexture.enableRandomWrite = true;
            _mainTexture.filterMode = FilterMode.Trilinear;
            _mainTexture.Create();

            _computeShader.SetTexture(_mainKernel, "starGradient", _starGradient);
            _computeShader.SetTexture(_mainKernel, "nebulaTex", _nebulaTexture);
            _computeShader.SetTexture(_mainKernel, "fbmNoiseTex", _fbmTexture);
            _computeShader.SetTexture(_mainKernel, "renderTex", _mainTexture);
            _computeShader.SetTexture(_processKernel, "renderTex", _mainTexture);

            _computeShader.SetInt("width", resolution.x);
            _computeShader.SetInt("height", resolution.y);
            _rawImage.texture = _mainTexture;

        }

        _computeShader.SetTexture(_mainKernel, "starGradient", _starGradient);
        _computeShader.SetTexture(_mainKernel, "nebulaTex", _nebulaTexture);
        _computeShader.SetTexture(_mainKernel, "fbmNoiseTex", _fbmTexture);
        _computeShader.SetTexture(_mainKernel, "starTex", _starTexture);

        _computeShader.SetTexture(_mainKernel, "gradientTex", _gradient);
        _computeShader.SetVector("gradientOffset", _gradientOffset);
        _computeShader.SetVector("gradientTiling", _gradientTiling);
        _computeShader.SetFloat("time", Time.time);
        _computeShader.SetFloat("deltaTime", Time.deltaTime);

        //post processing
        _computeShader.SetFloat("evaporateSpeed", _evaporateSpeed);
        _computeShader.SetFloat("diffuseSpeed", _diffuseSpeed);
        _computeShader.SetFloat("blurRadius", _blurRadius);

        _computeShader.SetFloat("amplitude", 1f);
        //marching
        _computeShader.SetInt("maxSteps", _maxSteps);
        _computeShader.SetFloat("minDist", _minDist);
        _computeShader.SetFloat("stepSize", _stepSize);
        _computeShader.SetFloat("fov", _fov);
        _computeShader.SetFloat("mipMapLevel", _mipMapLevel);


        //lighting and color
        _computeShader.SetVector("lightDirection", _lightDir);  //Camera.main.transform.forward);
        _computeShader.SetVector("lightColor", new Vector4(_lightColor.r, _lightColor.g, _lightColor.b, 1f).normalized);
        _computeShader.SetVector("ambientLight", new Vector4(_ambientLight.r, _ambientLight.g, _ambientLight.b, 1f).normalized);
        _computeShader.SetVector("backgroundColor", _backgroundColor);
        _computeShader.SetFloat("brightness", _brightness);

        // translating + rotation
        _computeShader.SetVector("roOffset", _roOffset);
        _computeShader.SetVector("sdfRotation", _sdfRotation);
        _computeShader.SetVector("roRotation", _roRotation);
        _computeShader.SetVector("rdRotation", _rdRotation);

        _computeShader.SetFloat("time", Time.time);
        _computeShader.SetFloat("deltaTime", Time.deltaTime);

        //nebula
        _computeShader.SetFloat("nebulaScale", _nebulaScale);
        _computeShader.SetVector("nebulaTwirl", _nebulaTwirl);
        _computeShader.SetFloat("nebulaCutoff", _nebulaCutoff);

        //fbm
        _computeShader.SetFloat("fbmDiv", _fbmDiv);
        _computeShader.SetFloat("fbmSpeed", _fbmSpeed);
        _computeShader.SetFloat("fbmH", _fbmH);
        _computeShader.SetFloat("fbmOctaves", _fbmOctaves);
        _computeShader.SetVector("fbmRotation", _fbmRotation);
        _computeShader.SetVector("fbmDirection", _fbmDirection);
        _computeShader.SetFloat("fbmIntensity", _fbmIntensity);
        _computeShader.SetVector("fbmOffset", _fbmOffset);

        //black hole
        _computeShader.SetVector("blackHolePosition", _blackHolePosition);
        _computeShader.SetVector("accretionDiskColor", _accretionDiskColor);
        _computeShader.SetFloat("schwarzSchildRadius", _schwarzSchildRadius);
        _computeShader.SetFloat("spaceDistortion", _spaceDistortion);
        _computeShader.SetFloat("accretionDiskRadius", _accretionDiskRadius);
        _computeShader.SetFloat("accretionDiskHeight", _accretionDiskHeight);

        //light
        _computeShader.SetFloat("maxAmbientDist", _maxAmbientDist);
        _computeShader.SetFloat("maxLightDist", _maxLightDist);
        _computeShader.SetFloat("densityScale", _densityScale);
        _computeShader.SetFloat("darknessThreshhold", _darknessThreshhold);
        _computeShader.SetFloat("lightAbsorb", _lightAbsorb);
        _computeShader.SetFloat("transmittance", _transmittance);
        _computeShader.SetInt("lightSteps", _lightSteps);
        _computeShader.SetFloat("lightStepSize", _lightStepsSize);
        _computeShader.SetFloat("shadowStrength", _shadowStrength);
        _computeShader.SetFloat("ambientStrength", _ambientStrength);
        _computeShader.SetFloat("dopplerStrength", _dopplerStrength);
        _computeShader.SetVector("dopplerOffset", _dopplerOffset);


        _computeShader.Dispatch(_mainKernel, resolution.x / 16, resolution.y / 16, 1);
        if (_blur) _computeShader.Dispatch(_processKernel, resolution.x / 8, resolution.y / 8, 1);

    }
}