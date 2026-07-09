using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    public Toggle MicOnOff;

    public Slider Sc1Slider;
    public Slider Sc2Slider;
    public Slider Sc3Slider;
    public Slider Sc4Slider;

    public InputField pathToSong;

    void Start()
    {
        MicOnOff = MicOnOff.GetComponent<Toggle>();

        Sc1Slider = Sc1Slider.GetComponent<Slider>();
        Sc2Slider = Sc2Slider.GetComponent<Slider>();
        Sc3Slider = Sc3Slider.GetComponent<Slider>();
        Sc4Slider = Sc4Slider.GetComponent<Slider>();
    }

    public void OnMicChanged()
    {
        AudioPeer._useMicrophone = MicOnOff.isOn;
    }

    public void OnSlider1Changed(float newValue)
    {
        FractalMaster.sceneTimes[0] = (float)System.Math.Round(FractalMaster.sceneTimes[0] * newValue, 2);
    }

    public void OnSlider2Changed(float newValue)
    {
        FractalMaster.sceneTimes[1] = (float)System.Math.Round(FractalMaster.sceneTimes[1] * newValue, 2);
    }

    public void OnSlider3Changed(float newValue)
    {
        FractalMaster.sceneTimes[2] = (float)System.Math.Round(FractalMaster.sceneTimes[2] * newValue, 2);
    }

    public void OnSlider4Changed(float newValue)
    {
        FractalMaster.sceneTimes[3] = (float)System.Math.Round(FractalMaster.sceneTimes[3] * newValue, 2);
    }

    public void ChangedSong(string newPath)
    {
        AudioPeer._userAudioClipPath = "file:///" + newPath;
    }


}
