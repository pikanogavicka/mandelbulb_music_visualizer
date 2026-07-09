using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;

[RequireComponent(typeof(AudioSource))]
public class AudioPeer : MonoBehaviour
{
    AudioSource _audioSource;
    public static float[] _samples = new float[128];
    private float maxValue = 0.0f;
    private float minValue = Mathf.Infinity;

    //Microphone input
    public AudioClip _audioClip;
    public static string _userAudioClipPath = "";
    public static bool _useMicrophone = false;
    public string _selectedDevice;
    public AudioMixerGroup _mixerGroupMicrophone, _mixerGroupMaster;

    //sound processing
    // ce spremenis stevilo bands (8) moras popravit loop v make frequency bands
    static float[] _freqBand = new float[7];
    public static float[] bandBuffer = new float[7];
    private static int[] samplesInBand = { 1, 1, 1, 10, 11, 12, 92 };
    float[] bufferDecrease = new float[7];

    // Start is called before the first frame update
    void Start()
    {
        _audioSource = GetComponent<AudioSource>();

        //Microphone input
        if (_useMicrophone)
        {
            if (Microphone.devices.Length > 0)
            {
                _selectedDevice = Microphone.devices[0].ToString();
                _audioSource.outputAudioMixerGroup = _mixerGroupMicrophone;
                _audioSource.clip = Microphone.Start(_selectedDevice, true, 1, AudioSettings.outputSampleRate);
                _audioSource.Play();
            }
            else
            {
                _useMicrophone = false;
                Debug.Log("No microphone connected.");
            }
        }
        else
        {
            _audioSource.outputAudioMixerGroup = _mixerGroupMaster;
            if (_userAudioClipPath == "")
            {
                _audioSource.clip = _audioClip;
                _audioSource.Play();
            }
            else
            {
                StartCoroutine(GetAudioClip());
                
            }
           
            //samplingFreq = _audioClip.frequency;
        }
    }

    IEnumerator GetAudioClip()
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(_userAudioClipPath, AudioType.WAV))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(www.error);
            }
            else
            {
                _audioSource.clip = DownloadHandlerAudioClip.GetContent(www);
                _audioSource.Play();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        GetSpectrumAudioSource();
        MakeFrequencyBands();
        BandBuffer();
    }

    void GetSpectrumAudioSource()
    {
        //W[n] = 0.42 - (0.5 * COS(n/N) ) + (0.08 * COS(2.0 * n/N) ).
        _audioSource.GetSpectrumData(_samples, 0, FFTWindow.Blackman);
    }

    void MakeFrequencyBands()
    {
        int count = 0;

        for (int i = 0; i < _freqBand.Length; i++)
        {
            float average = 0;


            for (int j = 0; j < samplesInBand[i]; j++)
            {
                average += _samples[count] * (count + 1);
                count++;
            }
            average /= count;

            //normalize values
            if (average > maxValue) maxValue = average;
            if (average < minValue) minValue = average;
            float normAvg = (average - minValue) / (maxValue - minValue);
            _freqBand[i] = Mathf.Min(normAvg + 0.1f, 1.0f);

        }
    }

    void BandBuffer()
    {
        //create buffer for values
        for (int i = 0; i < bandBuffer.Length; i++)
        {
            if (_freqBand[i] > 0.02f)
            {
                if (_freqBand[i] > bandBuffer[i])
                {
                    bandBuffer[i] = _freqBand[i];
                    //0.005f
                    bufferDecrease[i] = 0.005f;

                }
                else
                {
                    bandBuffer[i] -= bufferDecrease[i];
                    //1.2f
                    bufferDecrease[i] *= 1.2f;
                }
            }
        }

    }
}
