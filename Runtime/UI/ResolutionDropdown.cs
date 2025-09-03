using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace QuietSam.Common
{
    public class ResolutionDropdown : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown dropdown;
        [SerializeField] private Toggle fullscreenToggle;

        private List<Resolution> uniqueResolutions;

        private void Awake()
        {
            if (fullscreenToggle != null)
            {
                fullscreenToggle.isOn = Screen.fullScreen;
                fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggled);
            }

            PopulateResolutions();
            dropdown.onValueChanged.AddListener(OnResolutionSelected);
        }

        private void PopulateResolutions()
        {
            dropdown.ClearOptions();

            // Get all available modes from the OS/GPU
            var all = Screen.resolutions;

            uniqueResolutions = all
                .GroupBy(r => (r.width, r.height))
                .Select(g => g.OrderByDescending(r => r.refreshRateRatio.value).First())
                .OrderBy(r => r.width).ThenBy(r => r.height)
                .ToList();
            var options = uniqueResolutions
                .Select(r => $"{r.width} x {r.height} @ {r.refreshRateRatio.value:0.#}Hz")
                .ToList();

            dropdown.AddOptions(options);

            // Select current resolution (match by WxH)
            var idx = uniqueResolutions.FindIndex(r => r.width == Screen.currentResolution.width && r.height == Screen.currentResolution.height);
            dropdown.value = Mathf.Clamp(idx, 0, uniqueResolutions.Count - 1);
            dropdown.RefreshShownValue();

            // Disable dropdown in fullscreen
            dropdown.interactable = !Screen.fullScreen;
        }

        private void OnResolutionSelected(int index)
        {
            if (Screen.fullScreen) return; // Only allow changing when not fullscreen

            if (index < 0 || index >= uniqueResolutions.Count) return;

            var r = uniqueResolutions[index];

            Screen.SetResolution(r.width, r.height, FullScreenMode.Windowed, r.refreshRateRatio);
        }

        private void OnFullscreenToggled(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
            dropdown.interactable = !isFullscreen;

            if (!isFullscreen)
            {
                PopulateResolutions();
            }
        }
    }
}