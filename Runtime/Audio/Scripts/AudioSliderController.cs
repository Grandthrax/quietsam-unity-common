using System;
using UnityEngine;
using UnityEngine.UI;


[Serializable] public enum VolumeType
{
    MasterVol,
    MusicVol,
    SFXVol,
    UIVol
}

public class AudioSliderController : MonoBehaviour
{
    private AudioManager _aud => AudioManager.Instance;

    public Slider Slider;
    [SerializeField] private VolumeType volumeType;

    void Start()
    {
        Slider = Slider == null ? GetComponent<Slider>() : Slider;
        Slider.value = PlayerPrefs.GetFloat(volumeType.ToString(), 1f);
        Slider.onValueChanged.AddListener(SetVolume);
    }

    
    public void SetVolume(float volume)
    {
        _aud.SetVolume(volumeType.ToString(), volume);
    }
}
