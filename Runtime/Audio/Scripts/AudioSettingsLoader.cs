using UnityEngine;
namespace QuietSam.Common
{
    public class AudioSettingsLoader : MonoBehaviour
    {
        private void Start()
        {
            LoadSavedVolumes();
        }
    
        private void LoadSavedVolumes()
        {
            foreach (var p in new[] { "MasterVol", "MusicVol", "SFXVol", "UIVol" })
            {
                var volume = PlayerPrefs.GetFloat(p, 0.5f);
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.SetVolume(p, volume);
                }
                else
                {
                    Debug.LogError("AudioManager not found. Make sure it is in the scene.");
                }
            }
        }
    }
}