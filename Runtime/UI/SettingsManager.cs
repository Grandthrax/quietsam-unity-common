using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace QuietSam.Common
{
    public class SettingsManager : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        [SerializeField] private TMP_Dropdown localeDropdown;

        [Header("Audio")]
        [SerializeField] private AudioMixer audioMixer;

        private const string KEY_MASTER = "MasterVol";
        private const string KEY_SFX = "SFXVol";
        private const string KEY_MUSIC = "MusicVol";
        private const string KEY_FULLSCREEN = "settings.fullscreen.bool";
        private const string KEY_RES_W = "settings.res.w";
        private const string KEY_RES_H = "settings.res.h";
        private const string KEY_RES_RR = "settings.res.rr";
        private const string KEY_DIFF = "settings.diff"; // 0: Easy, 1: Normal, 2: Hard
        private const string KEY_LOCALE = "settings.loc";

        private bool suppressCallbacks;
        private List<Resolution> uniqueResolutions;

        void Awake()
        {
            if (masterSlider) { masterSlider.minValue = 0f; masterSlider.maxValue = 1f; }
            if (sfxSlider) { sfxSlider.minValue = 0f; sfxSlider.maxValue = 1f; }
            if (musicSlider) { musicSlider.minValue = 0f; musicSlider.maxValue = 1f; }

            PopulateResolutions();
            LoadFromPrefsOrInspectorDefaults();
            ApplyToRuntime();
            ApplyToUI();

            HookUI();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDestroy()
        {
            UnhookUI();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            PopulateResolutions();
            ApplyToUI();
            ApplyToRuntime();
        }

        #region Loading and saving

        private void LoadFromPrefsOrInspectorDefaults()
        {
            // Use inspector slider values as sane defaults on first run
            float defMaster = masterSlider ? masterSlider.value : 1f;
            float defSfx = sfxSlider ? sfxSlider.value : 1f;
            float defMusic = musicSlider ? musicSlider.value : 1f;

            float master = PlayerPrefs.HasKey(KEY_MASTER) ? PlayerPrefs.GetFloat(KEY_MASTER) : defMaster;
            float sfx = PlayerPrefs.HasKey(KEY_SFX) ? PlayerPrefs.GetFloat(KEY_SFX) : defSfx;
            float music = PlayerPrefs.HasKey(KEY_MUSIC) ? PlayerPrefs.GetFloat(KEY_MUSIC) : defMusic;

            bool fullscreen = PlayerPrefs.HasKey(KEY_FULLSCREEN)
                ? PlayerPrefs.GetInt(KEY_FULLSCREEN) == 1
                : Screen.fullScreen;

            int resW = PlayerPrefs.HasKey(KEY_RES_W) ? PlayerPrefs.GetInt(KEY_RES_W) : Screen.width;
            int resH = PlayerPrefs.HasKey(KEY_RES_H) ? PlayerPrefs.GetInt(KEY_RES_H) : Screen.height;
            int resRR = PlayerPrefs.HasKey(KEY_RES_RR) ? PlayerPrefs.GetInt(KEY_RES_RR) : GetCurrentRefreshRate();

            int difficulty = PlayerPrefs.HasKey(KEY_DIFF) ? PlayerPrefs.GetInt(KEY_DIFF) : 1;

            int locale = PlayerPrefs.HasKey(KEY_LOCALE) ? PlayerPrefs.GetInt(KEY_LOCALE) : 0;

            // Cache into UI (without notifying), the actual sources of truth are PlayerPrefs + these fields
            suppressCallbacks = true;
            if (masterSlider) masterSlider.SetValueWithoutNotify(Mathf.Clamp01(master));
            if (sfxSlider) sfxSlider.SetValueWithoutNotify(Mathf.Clamp01(sfx));
            if (musicSlider) musicSlider.SetValueWithoutNotify(Mathf.Clamp01(music));
            if (fullscreenToggle) fullscreenToggle.SetIsOnWithoutNotify(fullscreen);
            if (localeDropdown) localeDropdown.SetValueWithoutNotify(locale);
            suppressCallbacks = false;

            // Ensure prefs exist after first boot
            PlayerPrefs.SetFloat(KEY_MASTER, Mathf.Clamp01(master));
            PlayerPrefs.SetFloat(KEY_SFX, Mathf.Clamp01(sfx));
            PlayerPrefs.SetFloat(KEY_MUSIC, Mathf.Clamp01(music));
            PlayerPrefs.SetInt(KEY_FULLSCREEN, fullscreen ? 1 : 0);
            PlayerPrefs.SetInt(KEY_RES_W, resW);
            PlayerPrefs.SetInt(KEY_RES_H, resH);
            PlayerPrefs.SetInt(KEY_RES_RR, resRR);
            PlayerPrefs.SetInt(KEY_DIFF, difficulty);
            PlayerPrefs.SetInt(KEY_LOCALE, locale);
            PlayerPrefs.Save();
        }

        private void SaveVolumes(float master, float sfx, float music)
        {
            PlayerPrefs.SetFloat(KEY_MASTER, Mathf.Clamp01(master));
            PlayerPrefs.SetFloat(KEY_SFX, Mathf.Clamp01(sfx));
            PlayerPrefs.SetFloat(KEY_MUSIC, Mathf.Clamp01(music));
            PlayerPrefs.Save();
            ApplyToRuntime();
        }

        private void SaveFullscreen(bool fullscreen)
        {
            PlayerPrefs.SetInt(KEY_FULLSCREEN, fullscreen ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void SaveLocale(int value)
        {
            PlayerPrefs.SetInt(KEY_LOCALE, value);
            PlayerPrefs.Save();
        }

        private void SaveResolution(Resolution r)
        {
            int rr = Mathf.RoundToInt((float)r.refreshRateRatio.value);

            PlayerPrefs.SetInt(KEY_RES_W, r.width);
            PlayerPrefs.SetInt(KEY_RES_H, r.height);
            PlayerPrefs.SetInt(KEY_RES_RR, rr);
            PlayerPrefs.Save();
        }

        private void SaveDifficulty(int difficulty)
        {
            PlayerPrefs.SetInt(KEY_DIFF, difficulty);
            PlayerPrefs.Save();
        }

        #endregion

        #region Apply changes to runtime

        private void ApplyToRuntime()
        {
            float master = PlayerPrefs.GetFloat(KEY_MASTER, masterSlider ? masterSlider.value : 1f);
            float sfx = PlayerPrefs.GetFloat(KEY_SFX, sfxSlider ? sfxSlider.value : 1f);
            float music = PlayerPrefs.GetFloat(KEY_MUSIC, musicSlider ? musicSlider.value : 1f);
            bool fullscreen = PlayerPrefs.GetInt(KEY_FULLSCREEN, Screen.fullScreen ? 1 : 0) == 1;

            

            if (audioMixer)
            {
                audioMixer.SetFloat("MasterVol", SliderToDb(master));
                audioMixer.SetFloat("SFXVol", SliderToDb(sfx));
                audioMixer.SetFloat("MusicVol", SliderToDb(music));
            }

            Screen.fullScreen = fullscreen;

            int w = PlayerPrefs.GetInt(KEY_RES_W, Screen.width);
            int h = PlayerPrefs.GetInt(KEY_RES_H, Screen.height);
            int rr = PlayerPrefs.GetInt(KEY_RES_RR, GetCurrentRefreshRate());
            ApplyResolution(w, h, rr);
        }

        private void UpdateResolutionInteractable(bool isFullscreenUI)
        {
            if (resolutionDropdown) resolutionDropdown.interactable = !isFullscreenUI;
        }

        private void ApplyToUI()
        {
            suppressCallbacks = true;

            bool fs = fullscreenToggle
                ? fullscreenToggle.isOn
                : PlayerPrefs.GetInt(KEY_FULLSCREEN, Screen.fullScreen ? 1 : 0) == 1;

            if (fullscreenToggle) fullscreenToggle.SetIsOnWithoutNotify(fs);

            if (resolutionDropdown)
            {
                int w = PlayerPrefs.GetInt(KEY_RES_W, Screen.width);
                int h = PlayerPrefs.GetInt(KEY_RES_H, Screen.height);
                int idx = FindResolutionIndex(w, h);
                if (idx < 0) idx = FindClosestResIndex(w, h);
                resolutionDropdown.SetValueWithoutNotify(Mathf.Clamp(idx, 0, Mathf.Max(0, uniqueResolutions.Count - 1)));
                resolutionDropdown.RefreshShownValue();
                UpdateResolutionInteractable(fs);
            }

            if (masterSlider) masterSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(KEY_MASTER, masterSlider.value));
            if (sfxSlider) sfxSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(KEY_SFX, sfxSlider.value));
            if (musicSlider) musicSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(KEY_MUSIC, musicSlider.value));

            if (localeDropdown) localeDropdown.SetValueWithoutNotify(PlayerPrefs.GetInt(KEY_LOCALE, localeDropdown.value));

            suppressCallbacks = false;
        }

        private static float SliderToDb(float v01)
        {
            if (v01 <= 0.0001f) return -80f;
            return Mathf.Log10(Mathf.Clamp01(v01)) * 20f; // 1 -> 0 dB, 0.5 -> ~-6 dB
        }

        #endregion

        #region UI Event handlers

        private void HookUI()
        {
            if (masterSlider) masterSlider.onValueChanged.AddListener(OnMasterChanged);
            if (sfxSlider) sfxSlider.onValueChanged.AddListener(OnSfxChanged);
            if (musicSlider) musicSlider.onValueChanged.AddListener(OnMusicChanged);
            if (fullscreenToggle) fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggled);
            if (resolutionDropdown) resolutionDropdown.onValueChanged.AddListener(OnResolutionSelected);
        }

        private void UnhookUI()
        {
            if (masterSlider) masterSlider.onValueChanged.RemoveListener(OnMasterChanged);
            if (sfxSlider) sfxSlider.onValueChanged.RemoveListener(OnSfxChanged);
            if (musicSlider) musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
            if (fullscreenToggle) fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenToggled);
            if (resolutionDropdown) resolutionDropdown.onValueChanged.RemoveListener(OnResolutionSelected);
        }

        private void OnMasterChanged(float v)
        {
            if (suppressCallbacks) return;
            if (audioMixer) audioMixer.SetFloat(KEY_MASTER, SliderToDb(v));
            SaveVolumes(v, PlayerPrefs.GetFloat(KEY_SFX, 1f), PlayerPrefs.GetFloat(KEY_MUSIC, 1f));
        }

        private void OnSfxChanged(float v)
        {
            if (suppressCallbacks) return;
            if (audioMixer) audioMixer.SetFloat(KEY_SFX, SliderToDb(v));
            SaveVolumes(PlayerPrefs.GetFloat(KEY_MASTER, 1f), v, PlayerPrefs.GetFloat(KEY_MUSIC, 1f));
        }

        private void OnMusicChanged(float v)
        {
            if (suppressCallbacks) return;
            if (audioMixer) audioMixer.SetFloat(KEY_MUSIC, SliderToDb(v));
            SaveVolumes(PlayerPrefs.GetFloat(KEY_MASTER, 1f), PlayerPrefs.GetFloat(KEY_SFX, 1f), v);
        }

        private void OnFullscreenToggled(bool isFullscreen)
        {
            if (suppressCallbacks) return;

            Screen.fullScreen = isFullscreen;
            SaveFullscreen(isFullscreen);

            // Use the UI's known state, not Screen.fullScreen (which may lag a frame).
            // This prevents a bug where the resolution dropdown remained uninteractable
            // after the fullscreen mode was disabled.
            UpdateResolutionInteractable(isFullscreen);

            StartCoroutine(RefreshResolutionUIAfterFlip(isFullscreen));
        }

        private IEnumerator RefreshResolutionUIAfterFlip(bool isFullscreen)
        {
            yield return null; // wait one frame so Screen.* reflects the new state

            if (!isFullscreen)
            {
                PopulateResolutions();
            }

            // Re-apply to UI using the toggle state (not Screen.fullScreen)
            suppressCallbacks = true;
            if (fullscreenToggle) fullscreenToggle.SetIsOnWithoutNotify(isFullscreen);
            if (resolutionDropdown)
            {
                int w = PlayerPrefs.GetInt(KEY_RES_W, Screen.width);
                int h = PlayerPrefs.GetInt(KEY_RES_H, Screen.height);
                int idx = FindResolutionIndex(w, h);
                if (idx < 0) idx = FindClosestResIndex(w, h);
                resolutionDropdown.SetValueWithoutNotify(Mathf.Clamp(idx, 0, Mathf.Max(0, uniqueResolutions.Count - 1)));
                resolutionDropdown.RefreshShownValue();
                UpdateResolutionInteractable(isFullscreen);
            }
            suppressCallbacks = false;
        }

        private void OnResolutionSelected(int index)
        {
            if (suppressCallbacks) return;
            if (!resolutionDropdown || index < 0 || index >= uniqueResolutions.Count) return;
            if (Screen.fullScreen) return;

            var r = uniqueResolutions[index];
            Screen.SetResolution(r.width, r.height, FullScreenMode.Windowed, r.refreshRateRatio);
            SaveResolution(r);
        }



        #endregion

        #region Resolutions

        private void PopulateResolutions()
        {
            if (!resolutionDropdown) return;

            var all = Screen.resolutions;

            uniqueResolutions = all
                .GroupBy(r => (r.width, r.height))
                .Select(g => g.OrderByDescending(r => r.refreshRateRatio.value).First())
                .OrderBy(r => r.width).ThenBy(r => r.height)
                .ToList();
            var options = uniqueResolutions
                .Select(r => $"{r.width} x {r.height} @ {r.refreshRateRatio.value:0.#}Hz")
                .ToList();

            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(options);
        }

        private void ApplyResolution(int width, int height, int refresh)
        {
            int idx = FindResolutionIndex(width, height);
            if (idx < 0 && uniqueResolutions.Count > 0) idx = FindClosestResIndex(width, height);

            if (idx >= 0)
            {
                var r = uniqueResolutions[idx];
                Screen.SetResolution(r.width, r.height, Screen.fullScreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed, r.refreshRateRatio);
            }
        }

        private int FindResolutionIndex(int w, int h)
        {
            return uniqueResolutions != null
                ? uniqueResolutions.FindIndex(r => r.width == w && r.height == h)
                : -1;
        }

        private int FindClosestResIndex(int w, int h)
        {
            if (uniqueResolutions == null || uniqueResolutions.Count == 0) return -1;
            int best = 0;
            int bestScore = int.MaxValue;
            for (int i = 0; i < uniqueResolutions.Count; i++)
            {
                var r = uniqueResolutions[i];
                int score = Mathf.Abs(r.width - w) + Mathf.Abs(r.height - h);
                if (score < bestScore) { bestScore = score; best = i; }
            }
            return best;
        }

        private int GetCurrentRefreshRate()
        {
            return Mathf.RoundToInt((float)Screen.currentResolution.refreshRateRatio.value);
        }

        #endregion
    }
}
