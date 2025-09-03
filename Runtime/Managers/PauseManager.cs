using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace QuietSam.Common
{
    public class PauseManager : MonoBehaviour
    {
        public static PauseManager Instance { get; private set; }
        private InputManager _input => InputManager.Instance;

        public bool Paused = false;
        public bool CanPause = true;
        public bool CanUnpause = true;
        

        public UnityEvent<bool> PausedChanged;

            private void Awake()
            {
                if (Instance != null) { Destroy(this); return; }
                Instance = this;
                PausedChanged = new UnityEvent<bool>();
                PausedChanged.AddListener(OnPausedChanged);
            }

            private void Start()
            {
                TogglePause(false, true);
                CanPause = true;
                CanUnpause = true;

                _input.Player.Pause.performed += OnEscape;
            }
        private void OnDestroy()
        {
            _input.Player.Pause.performed -= OnEscape;
        }

        private void OnEscape(InputAction.CallbackContext obj)
        {
            TogglePause(!Paused);
            return;
        }


        public bool TogglePause(bool paused, bool forced = false)
        {
            if (paused && (CanPause || forced))
            {
                Paused = true;
                PausedChanged.Invoke(true);
                return true;
            }
            else if (!paused && (CanUnpause || forced))
            {
                Paused = false;
                PausedChanged.Invoke(false);
                return true;
            }

            return false;
        }
        private void OnPausedChanged(bool paused)
        {
            if (paused)
            {
                Pause();
            }
            else
            {
                Unpause();
            }
        
        }

        public void Pause()
            {
                Time.timeScale = 0f;
            }
        public void Unpause()
        {
            Time.timeScale = 1f;
        }
    }
}