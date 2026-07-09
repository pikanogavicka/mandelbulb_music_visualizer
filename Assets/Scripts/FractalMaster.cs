using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class FractalMaster : MonoBehaviour {

    public ComputeShader fractalShader;
    float[] freqBandsBuffer;
    float timePassed = 0.0f;
    public static float[] sceneTimes = { 54, 20, 50, 81 };


    [Range(0, 3)]
    public int sceneNumber = 0;

    [Range (1, 20)]
    float fractalPower = 1.5f;
    float darkness = 55;

    RenderTexture target;
    Camera cam;
    Light directionalLight;

    [Header ("Animation Settings")]
    float spikey = 0.0f;
    float brocolliRot = 0.0f;
    float hilly = 0.0009f;


    float[] spectrumSaturation = { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f};
    float avgSpectrumSaturation = 0.5f;
    float timePassedSpectrum = 0.0f;
    int currSpecI = 0;



    void Start() {
        Application.targetFrameRate = 60;
        freqBandsBuffer = AudioPeer.bandBuffer;

        fractalShader.SetFloat("power", fractalPower);
        fractalShader.SetFloat("_spikey", 0.0f);
        fractalShader.SetFloat("_hilly", 0.0f);
        fractalShader.SetFloat("_rotY", brocolliRot);
    }
    
    void Init () {
        cam = Camera.current;
        directionalLight = FindObjectOfType<Light> ();
    }

    // Animate properties
    void Update () {
        if (Application.isPlaying) {
            timePassedSpectrum += Time.deltaTime;

            //recalculate colour saturation
            if (timePassedSpectrum > 0.1f)
            {
                float currSaturation = 0.0f;
                for (int i = 0; i < 5; i++)
                {
                    currSaturation += freqBandsBuffer[i];
                }
                currSaturation /= freqBandsBuffer.Length;

                spectrumSaturation[currSpecI % spectrumSaturation.Length] = currSaturation;
                currSpecI++;
                timePassedSpectrum = 0.0f;
            }


            //change scene if enough time passed and on beat
            if (timePassed > sceneTimes[sceneNumber] && (freqBandsBuffer[0] > 0.15f || timePassed - sceneTimes[sceneNumber] > 10))
            {
                //reset values
                timePassed = 0;
                //x_rotation = 0.0f;
                brocolliRot = 0.0f;
                spikey = 0.0f;
                hilly = 0.0009f;

                sceneNumber = (sceneNumber + 1) % 3 + 1; 

                if (sceneNumber == 0) fractalPower = 1.5f;
                else fractalPower = 7.6f;

            }
            MoveCamera(sceneNumber);
            //Debug.Log(freqBandsBuffer[0] > 0.1f);
        }
    }

    void OnRenderImage (RenderTexture source, RenderTexture destination) {
        //if (Application.isPlaying)
        //{
            Init();
            InitRenderTexture();
            SetParameters();

            int threadGroupsX = Mathf.CeilToInt(cam.pixelWidth / 8.0f);
            int threadGroupsY = Mathf.CeilToInt(cam.pixelHeight / 8.0f);
            fractalShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

            Graphics.Blit(target, destination);
       //}   
    }

    void SetParameters () {
        timePassed += Time.deltaTime;


        fractalShader.SetTexture (0, "Destination", target);
        fractalShader.SetFloat ("darkness", darkness);

        //color settings
        avgSpectrumSaturation = 0.0f;
        for (int i=0; i < spectrumSaturation.Length; i++)
        {
            avgSpectrumSaturation += spectrumSaturation[i];
        }
        avgSpectrumSaturation /= spectrumSaturation.Length;
        fractalShader.SetFloat("_spectrumSaturation", Mathf.Max(avgSpectrumSaturation * 8 - 0.18f, 0.0f));

        //scene specific color
        if (sceneNumber == 2)
        {
            fractalShader.SetFloat("_saturationStrength", 0.25f);
            fractalShader.SetFloat("_colorOffset", 0.4f);
            fractalShader.SetFloat("_brightness", 0.08f);
        }
        if (sceneNumber == 0)
        {
            fractalShader.SetFloat("_saturationStrength", 0.25f);
            fractalShader.SetFloat("_colorOffset", 0.3f);
            fractalShader.SetFloat("_brightness", 0.3f);
        }
        if (sceneNumber == 1)
        {
            fractalShader.SetFloat("_saturationStrength", 0.35f);
            fractalShader.SetFloat("_colorOffset", 0.5f);
            fractalShader.SetFloat("_brightness", 0.0f);
        }
        if (sceneNumber == 3)
        {
            fractalShader.SetFloat("_saturationStrength", 0.25f);
            fractalShader.SetFloat("_colorOffset", 0.28f);
            fractalShader.SetFloat("_brightness", 0.1f);
        }

        fractalShader.SetFloat("_timePassed", timePassed / 20);


        fractalShader.SetMatrix ("_CameraToWorld", cam.cameraToWorldMatrix);
        fractalShader.SetMatrix ("_CameraInverseProjection", cam.projectionMatrix.inverse);
        fractalShader.SetVector ("_LightDirection", directionalLight.transform.forward);

        //front view
        if (sceneNumber == 0)
        {
            //fractalPower = (float)System.Math.Round(freqBandsBuffer[3] + 6.5f, 2) * 0.5f + fractalPower * 0.5f;
            //if (fractalPower > 0.8f) fractalShader.SetFloat("power", Mathf.Max(fractalPower, 1.01f));
            //else fractalShader.SetFloat("power", 6.7f);
            fractalPower += (float)System.Math.Round(freqBandsBuffer[0] + freqBandsBuffer[1], 2) * 0.002f;
            fractalShader.SetFloat("power", fractalPower);

            //reset other values
            fractalShader.SetFloat("_spikey", spikey);
            fractalShader.SetFloat("_hilly", hilly);
            //fractalShader.SetFloat("_rotY", brocolliRot);
            brocolliRot = timePassed / 18;
            fractalShader.SetFloat("_rotY", brocolliRot + 3.2f);
        }
        // spikey side view
        else if (sceneNumber == 1)
        {
            spikey = (float)System.Math.Round(freqBandsBuffer[0], 2) * 1.5f + spikey * 0.9f;
            if (spikey > 0.15f) fractalShader.SetFloat("_spikey", spikey * 0.1f);
            else fractalShader.SetFloat("_spikey", 0.0f);

            //reset other values
            fractalShader.SetFloat("_hilly", hilly);
            fractalShader.SetFloat("_rotY", brocolliRot);
            fractalShader.SetFloat("power", fractalPower);
        }
        // coloseum side view
        else if (sceneNumber == 2)
        {
            fractalPower += (float)System.Math.Round(freqBandsBuffer[0], 2) * 0.1f;
            fractalShader.SetFloat("power", Mathf.Sin(fractalPower) * Mathf.Min((float)1e-3 * timePassed, 0.5f) + 6.8f);

            //smaller resolution as fractal moves away
            if (timePassed > 23) fractalShader.SetFloat("_hilly", Mathf.Min(0.0009f, timePassed*0.000002f));
            else fractalShader.SetFloat("_hilly", 0.0f);


            //reset other values
            fractalShader.SetFloat("_colorBeat", -0.2f);
            fractalShader.SetFloat("_rotY", brocolliRot);
            fractalShader.SetFloat("_spikey", spikey);
            fractalShader.SetFloat("darkness", 70.0f);
        }
        //Magic brocolli
        else if (sceneNumber == 3)
        {
            brocolliRot = -timePassed / 40;
            fractalShader.SetFloat("_rotY", brocolliRot);

            fractalShader.SetFloat("_colorBeat", -0.2f);

            hilly = (float)System.Math.Round(freqBandsBuffer[3] * 0.005f, 4) * 0.5f + hilly * 0.5f;
            fractalShader.SetFloat("_hilly", hilly * 0.07f);

            fractalShader.SetFloat("_spikey", spikey);
            fractalShader.SetFloat("power", fractalPower);
            fractalShader.SetFloat("darkness", 68.0f);
        }

    }

    void InitRenderTexture () {
        if (target == null || target.width != cam.pixelWidth || target.height != cam.pixelHeight) {
            if (target != null) {
                target.Release ();
            }
            target = new RenderTexture (cam.pixelWidth, cam.pixelHeight, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            target.enableRandomWrite = true;
            target.Create ();
        }
    }

    void MoveCamera(int sceneNumber)
    {
        if (sceneNumber == 0)
        {
            this.transform.position = new Vector3(0.0f, 0.0f, -4.2f + timePassed * 0.04f);
            this.transform.eulerAngles = new Vector3(0.0f, 0.0f, -90);
        }

        else if (sceneNumber == 1)
        {
            this.transform.position = new Vector3(1.6f, 0.1f, 0.11f);
            this.transform.eulerAngles = new Vector3(-25f + timePassed, 210.0f + timePassed * 2.2f, -20.0f + timePassed);
        }

        // colloseum side
        if (sceneNumber == 2)
        {
            this.transform.position = new Vector3(0.0f + timePassed / 70, 0.0f, -1.09f);
            this.transform.eulerAngles = new Vector3(0.0f, -37.62f - timePassed * 0.5f, 89.0f);
        }
        //Magic Broccoli
        if (sceneNumber == 3)
        {
            this.transform.position = new Vector3(0.9f, -0.36f, -0.7f);
            this.transform.eulerAngles = new Vector3(-15.81f, -2.1f, 270.11f);
        }
    }
}