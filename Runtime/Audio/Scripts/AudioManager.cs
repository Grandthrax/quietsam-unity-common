using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace QuietSam.Common
{
    public class AudioManager : MonoBehaviour
    {

        [Header("Mixer")]
        public AudioMixer Mixer; // make sure to expose volumes in the mixer

        [Header("Music")]
        [SerializeField] private AudioMixerGroup _musicGroup;
        [SerializeField] private AudioMixerGroup _sfxGroup;
        [SerializeField] private float _defaultCrossfadeSecs = 1.5f;
        [SerializeField] private MusicTrack[] _startingMusicTracks;

        private AudioSource _musicA, _musicB;
        private AudioSource _activeMusic, _idleMusic;
        private Coroutine _xfadeCo;

        // Music queue system
        private Queue<MusicTrack> _musicQueue = new Queue<MusicTrack>();
        private MusicTrack _currentTrack;
        private Coroutine _queueCheckCo;


        //pooling
        [SerializeField] int poolSize = 12;
        Queue<AudioSource> _sfxPool = new Queue<AudioSource>();
        public static AudioManager Instance;


        private readonly Dictionary<SfxEvent, float> _lastPlayTime = new();

        void Awake()
        {
            if (Instance != null) { Destroy(this); return; }
            Instance = this;
            DontDestroyOnLoad(this);

            for (int i = 0; i < poolSize; i++)
            {
                _sfxPool.Enqueue(CreateAudioSource());
            }
            BuildMusicPlayers();
        }

        private void Start()
        {
            if (_startingMusicTracks.Length > 0)
            {
                // Add all starting tracks to the queue
                foreach (var track in _startingMusicTracks)
                {
                    _musicQueue.Enqueue(track);
                }

                // Start playing the first track
                PlayNextInQueue();
            }
            LoadSavedVolumes();
        }

        private AudioSource CreateAudioSource()
        {
            var go = new GameObject("SFX_AudioSource");
            go.transform.SetParent(transform);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            return src;
        }

        public AudioSource Play(SfxEvent sfxEvent)
            => PlayAt(sfxEvent, null, Vector3.zero);


        // Simple playback
        //AudioManager.Instance.Play(mySfxEvent);
        // 3D positioned sound
        //AudioManager.Instance.PlayAt(mySfxEvent, null, transform.position);
        // Sound that follows an object
        //AudioManager.Instance.PlayAt(mySfxEvent, playerTransform, Vector3.zero);

        public AudioSource PlayAt(SfxEvent sfxEvent, Transform follow, Vector3 position)
        {
            if (sfxEvent == null) return null;
            if (!sfxEvent.allowMultiple && _lastPlayTime.TryGetValue(sfxEvent, out var last))
            {
                float now = sfxEvent.respectTime ? Time.time : Time.unscaledTime;
                if (now - last < sfxEvent.minRepeatDelay) return null;
            }

            var clip = sfxEvent.GetRandomClip();
            if (clip == null) return null;

            AudioSource src;
            if (_sfxPool.Count > 0)
            {
                src = _sfxPool.Dequeue();
                if (src == null) src = CreateAudioSource();
            }
            else
            {
                src = CreateAudioSource();
            }
            src.playOnAwake = false;

            src.clip = clip;

            // randomize volume/pitch to sound a bit different each time
            float vol = sfxEvent.volume + Random.Range(-sfxEvent.volumeJitter.y, sfxEvent.volumeJitter.y);
            src.volume = Mathf.Clamp01(vol);
            src.pitch = Random.Range(sfxEvent.pitchJitter.x, sfxEvent.pitchJitter.y);

            src.spatialBlend = sfxEvent.spatialBlend;
            src.rolloffMode = sfxEvent.rolloff;
            src.minDistance = sfxEvent.minDistance;
            src.maxDistance = sfxEvent.maxDistance;
            src.priority = sfxEvent.priority;

            // routing
            if (sfxEvent.outputGroup != null)
            {
                src.outputAudioMixerGroup = sfxEvent.outputGroup;
            }
            else
            {
                src.outputAudioMixerGroup = _sfxGroup;
            }

            // transform handling
            if (follow != null)
            {
                src.transform.SetParent(follow, worldPositionStays: false);
                src.transform.localPosition = Vector3.zero;
            }
            else
            {
                src.transform.SetParent(transform);
                src.transform.position = position;
            }

            // timeScale independence
            src.ignoreListenerPause = !sfxEvent.respectTime;

            src.loop = sfxEvent.loop;
            src.Play();

            if (!sfxEvent.loop)
            {
                StartCoroutine(ReturnWhenDone(src));

            }

            _lastPlayTime[sfxEvent] = sfxEvent.respectTime ? Time.time : Time.unscaledTime;
            return src;
        }

        IEnumerator ReturnWhenDone(AudioSource src)
        {
            // Optionally wait for clip.length / pitch with a small safety margin
            while (src.isPlaying) yield return null;
            _sfxPool.Enqueue(src);
        }

        public void Stop(AudioSource src)
        {
            if (src == null) return;
            src.Stop();
            if (src.gameObject != null)
            {
                Destroy(src.gameObject);
            }
        }

        public void SetVolume(string exposedParam, float linear01)
        {
            //  -80 dB (mute) to 0 dB
            float dB = Mathf.Approximately(linear01, 0f) ? -80f : Mathf.Log10(linear01) * 20f;
            Mixer.SetFloat(exposedParam, dB);
            PlayerPrefs.SetFloat(exposedParam, linear01);
        }

        public void PlayMusic(MusicTrack track, float fade = -1f)
        {
            if (track == null) return;
            if (fade < 0f) fade = _defaultCrossfadeSecs;

            // prepare idle source with new track
            _idleMusic.outputAudioMixerGroup = track.output ? track.output : _musicGroup;
            _idleMusic.volume = track.volume; //scaled by mixer

            _idleMusic.clip = track.track;
            _idleMusic.loop = false; // Don't loop since we want to queue to next track
            _idleMusic.Play();
            StartCrossfade(fade);
        }

        public void StopMusic(float fade = 0.8f)
        {
            if (_xfadeCo != null) StopCoroutine(_xfadeCo);
            _xfadeCo = StartCoroutine(FadeOutAndStop(_activeMusic, fade));

            // Clear the queue and stop queue checking
            ClearQueue();
            if (_queueCheckCo != null)
            {
                StopCoroutine(_queueCheckCo);
                _queueCheckCo = null;
            }
            _currentTrack = null;
        }

        bool isPaused = false;

        public void PauseMusic(bool paused)
        {
            if (paused) { _activeMusic.Pause(); _idleMusic.Pause(); isPaused = true; }
            else { _activeMusic.UnPause(); _idleMusic.UnPause(); isPaused = false; }
        }

        public void SetMusicPitch(float pitch)
        {
            _activeMusic.pitch = pitch;
            _idleMusic.pitch = pitch;
        }

        // ---- Crossfade helpers ----
        private void StartCrossfade(float seconds)
        {
            if (_xfadeCo != null) StopCoroutine(_xfadeCo);
            _xfadeCo = StartCoroutine(CrossfadeCo(seconds));
        }

        private IEnumerator CrossfadeCo(float sec)
        {
            var from = _activeMusic;
            var to = _idleMusic;

            float toStartVol = to.volume;
            to.volume = 0f;

            // start 'to' if not already playing
            if (!to.isPlaying) to.Play();

            float t = 0f;
            while (t < sec)
            {
                t += Time.unscaledDeltaTime; // independent of timescale
                float a = t / sec;
                from.volume = Mathf.Lerp(from.volume, 0f, a);
                to.volume = Mathf.Lerp(0f, toStartVol, a);
                yield return null;
            }
            from.Stop();
            to.volume = toStartVol;

            //'to' is now active
            _activeMusic = to;
            _idleMusic = (from == _musicA) ? _musicB : _musicA;
            _xfadeCo = null;
        }

        private IEnumerator FadeOutAndStop(AudioSource src, float sec)
        {
            if (!src.isPlaying) yield break;
            float start = src.volume, t = 0f;
            while (t < sec)
            {
                t += Time.unscaledDeltaTime;
                src.volume = Mathf.Lerp(start, 0f, t / sec);
                yield return null;
            }
            src.Stop();
            src.volume = start;
        }

        // Music queue management methods
        public void AddToQueue(MusicTrack track)
        {
            if (track == null) return;
            _musicQueue.Enqueue(track);

            // If no music is currently playing, start playing
            if (_currentTrack == null && !_activeMusic.isPlaying)
            {
                PlayNextInQueue();
            }
        }

        public void AddToQueueFront(MusicTrack track)
        {
            if (track == null) return;

            // Create a temporary queue with the new track first
            var tempQueue = new Queue<MusicTrack>();
            tempQueue.Enqueue(track);

            // Add all existing tracks after
            while (_musicQueue.Count > 0)
            {
                tempQueue.Enqueue(_musicQueue.Dequeue());
            }

            _musicQueue = tempQueue;

            // If no music is currently playing, start playing
            if (_currentTrack == null && !_activeMusic.isPlaying)
            {
                PlayNextInQueue();
            }
        }

        public void ClearQueue()
        {
            _musicQueue.Clear();
        }

        public void SkipToNext()
        {
            if (_musicQueue.Count > 0)
            {
                PlayNextInQueue();
            }
            else
            {
                // If queue is empty, restart with starting music
                RestartStartingMusic();
            }
        }

        public int GetQueueCount()
        {
            return _musicQueue.Count;
        }

        public MusicTrack GetCurrentTrack()
        {
            return _currentTrack;
        }

        public bool IsQueueEmpty()
        {
            return _musicQueue.Count == 0;
        }

        private void PlayNextInQueue()
        {
            if (_musicQueue.Count == 0)
            {
                _currentTrack = null;
                return;
            }

            _currentTrack = _musicQueue.Dequeue();
            PlayMusic(_currentTrack);

            // Start checking if the current track has finished
            StartQueueCheck();
        }

        private void StartQueueCheck()
        {
            if (_queueCheckCo != null)
            {
                StopCoroutine(_queueCheckCo);
            }
            _queueCheckCo = StartCoroutine(CheckMusicEnd());
        }

        private void RestartStartingMusic()
        {
            // Add all starting tracks back to the queue
            foreach (var track in _startingMusicTracks)
            {
                _musicQueue.Enqueue(track);
            }

            // Start playing the first track
            if (_musicQueue.Count > 0)
            {
                PlayNextInQueue();
            }
        }

        private IEnumerator CheckMusicEnd()
        {
            if (_currentTrack == null || _activeMusic == null) yield break;

            // Allow crossfade to swap active/idle first
            yield return null;

            // Wait until one of the music sources is actually playing the current track
            AudioSource trackSource = null;
            while (_currentTrack != null && trackSource == null)
            {
                if (isPaused) yield return null;
                else if (_musicA != null && _musicA.clip == _currentTrack.track && _musicA.isPlaying) trackSource = _musicA;
                else if (_musicB != null && _musicB.clip == _currentTrack.track && _musicB.isPlaying) trackSource = _musicB;
                else yield return null;
            }

            if (_currentTrack == null || trackSource == null) yield break;

            // Wait for that source to finish playback (music sources do not loop)
            while (isPaused || (trackSource.isPlaying && trackSource.clip == _currentTrack.track))
            {
                yield return null;
            }

            // Advance to the next track or restart starting music
            if (_musicQueue.Count > 0)
            {
                PlayNextInQueue();
            }
            else
            {
                RestartStartingMusic();
            }
        }


        //private methods

        private void BuildMusicPlayers()
        {
            _musicA = CreateMusicSource("Music_A"); // empty music source
            _musicB = CreateMusicSource("Music_B");
            _activeMusic = _musicA; _idleMusic = _musicB;
        }

        private AudioSource CreateMusicSource(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = false; // Loop will be controlled by the queue system
            src.spatialBlend = 0f;           // music is 2D
            src.outputAudioMixerGroup = _musicGroup;
            return src;
        }

        private void LoadSavedVolumes()
        {
            foreach (var p in new[] { "MasterVol", "MusicVol", "SFXVol", "UIVol" })
            {
                var volume = PlayerPrefs.GetFloat(p, 0.5f);
                SetVolume(p, volume);
            }
        }
    }
}
