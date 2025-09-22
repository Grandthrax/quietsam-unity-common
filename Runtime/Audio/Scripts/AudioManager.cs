using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace QuietSam.Common
{
    public class AudioManager : MonoBehaviour
    {

        [Header("Mixer")]
        public AudioMixer Mixer;

        [Header("Music")]
        [SerializeField] private AudioMixerGroup m_musicGroup;
        [SerializeField] private AudioMixerGroup m_sfxGroup;
        [SerializeField] private AudioMixerGroup m_uiGroup;

        [Tooltip("When one song stops and another starts, this is the crossfade time")]
        [SerializeField] private float m_defaultCrossfadeSecs = 1.5f;

        [Tooltip("These music track will be played when the game starts and will loop once the queue empties")]
        [SerializeField] private MusicTrack[] m_startingMusicTracks;

        private bool m_isPaused = false;
        //for crossfade magement we use two audiosources we can fade together
        private AudioSource m_musicA, m_musicB;
        private AudioSource m_activeMusic, m_idleMusic;
        private Coroutine m_xfadeCo;

        // Music queue system
        private Queue<MusicTrack> m_musicQueue = new Queue<MusicTrack>();
        private MusicTrack m_currentTrack;
        private Coroutine m_queueCheckCo;
        //we need to keep track of the last time we played a sound effect to avoid playing the same sound effect too soon
        private Dictionary<SfxEvent, float> m_lastPlayTime = new();


        //pooling. we don't want to create a new audio source every time we play a sound effect so we reuse them.
        [SerializeField] int poolSize = 12;
        private Queue<AudioSource> m_sfxPool = new Queue<AudioSource>();

        //singleton
        public static AudioManager Instance;

        void Awake()
        {
            //one instance and persist through scenes
            if (Instance != null) { Destroy(this); return; }
            Instance = this;
            DontDestroyOnLoad(this);

            for (int i = 0; i < poolSize; i++)
            {
                m_sfxPool.Enqueue(CreateAudioSource());
            }
            BuildMusicPlayers();
        }

        private void Start()
        {
            if (m_startingMusicTracks.Length > 0)
            {
                // Add all starting tracks to the queue
                foreach (var track in m_startingMusicTracks)
                {
                    m_musicQueue.Enqueue(track);
                }

                // Start playing the first track
                PlayNextInQueue();
            }
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


        // Simple playback:
        //AudioManager.Instance.Play(mySfxEvent);
        // 3D positioned sound
        //AudioManager.Instance.PlayAt(mySfxEvent, null, transform.position);
        // Sound that follows an object:
        //AudioManager.Instance.PlayAt(mySfxEvent, playerTransform, Vector3.zero);

        /// <summary>
        /// Play a sound effect at a specific position, following an object or not.
        /// </summary>
        /// <param name="sfxEvent">The SfxEvent to play.</param>
        /// <param name="follow">The Transform to follow.</param>
        /// <param name="position">The position to play the sound effect at.</param>
        /// <returns>The AudioSource of the sound effect.</returns>
        public AudioSource PlayAt(SfxEvent sfxEvent, Transform follow, Vector3 position)
        {
            if (sfxEvent == null) return null;
            if (!sfxEvent.allowMultiple && m_lastPlayTime.TryGetValue(sfxEvent, out var last))
            {
                float now = sfxEvent.respectTime ? Time.time : Time.unscaledTime;
                if (now - last < sfxEvent.minRepeatDelay) return null;
            }

            var clip = sfxEvent.GetRandomClip();
            if (clip == null) return null;

            AudioSource src;
            if (m_sfxPool.Count > 0)
            {
                src = m_sfxPool.Dequeue();
                if (src == null) src = CreateAudioSource(); //sometimes it has been destroyed
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
                src.outputAudioMixerGroup = m_sfxGroup;
            }

            // transform handling
            if (follow != null)
            {
                src.transform.SetParent(follow, worldPositionStays: false);
                src.transform.localPosition = Vector3.zero;
            }
            else
            {
                // keeping as transform means it would last through scene changes
                src.transform.SetParent(null);
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

            m_lastPlayTime[sfxEvent] = sfxEvent.respectTime ? Time.time : Time.unscaledTime;
            return src;
        }

        IEnumerator ReturnWhenDone(AudioSource src)
        {
            while (src.isPlaying) yield return null;
            ReturnToPool(src);
        }

        void ReturnToPool(AudioSource src)
        {
            src.transform.SetParent(transform);
            m_sfxPool.Enqueue(src);
        }

        public void Stop(AudioSource src)
        {
            if (src == null) return;
            src.Stop();
            // if not looping we should return when done anyway 
            if (src.loop)
            {
                ReturnToPool(src);
            }
        }

        public void SetVolume(string exposedParam, float linear01)
        {
            //  -80 dB (mute) to 0 dB
            float dB = Mathf.Approximately(linear01, 0f) ? -80f : Mathf.Log10(linear01) * 20f;
            Mixer.SetFloat(exposedParam, dB);
            PlayerPrefs.SetFloat(exposedParam, linear01);
        }

        public void PlayMusicNow(MusicTrack track)
        {
            AddToQueueFront(track);
           SkipToNext();
        }

        private void PlayMusic(MusicTrack track, float fade = -1f)
        {
             if (track == null) return;
            if (fade < 0f) fade = m_defaultCrossfadeSecs;

            // prepare idle source with new track
            m_idleMusic.outputAudioMixerGroup = track.output ? track.output : m_musicGroup;
            m_idleMusic.volume = track.volume; //scaled by mixer

            m_idleMusic.clip = track.track;
            m_idleMusic.loop = false; // Don't loop since we want to queue to next track
            m_idleMusic.Play();
            StartCrossfade(fade);
        }


        public void Pause(bool paused)
        {

            foreach (var audiosource in FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
            {
                if (paused) audiosource.Pause();
                else audiosource.UnPause();
            }
            m_isPaused = paused;

        }

        public void SetMusicPitch(float pitch)
        {
            m_activeMusic.pitch = pitch;
            m_idleMusic.pitch = pitch;
        }

        // ---- Crossfade helpers ----
        private void StartCrossfade(float seconds)
        {
            if (m_xfadeCo != null) StopCoroutine(m_xfadeCo);
            m_xfadeCo = StartCoroutine(CrossfadeCo(seconds));
        }

        private IEnumerator CrossfadeCo(float sec)
        {
            var from = m_activeMusic;
            var to = m_idleMusic;

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
            m_activeMusic = to;
            m_idleMusic = from;
            m_xfadeCo = null;
        }

        // Music queue management methods
        public void AddToQueue(MusicTrack track)
        {
            if (track == null) return;
            m_musicQueue.Enqueue(track);

            // If no music is currently playing, start playing
            if (m_currentTrack == null && !m_activeMusic.isPlaying)
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
            while (m_musicQueue.Count > 0)
            {
                tempQueue.Enqueue(m_musicQueue.Dequeue());
            }

            m_musicQueue = tempQueue;

            // If no music is currently playing, start playing
            if (m_currentTrack == null && !m_activeMusic.isPlaying)
            {
                PlayNextInQueue();
            }
        }

        public void ClearQueue()
        {
            m_musicQueue.Clear();
        }

        public void SkipToNext()
        {
            if (m_musicQueue.Count > 0)
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
            return m_musicQueue.Count;
        }

        public MusicTrack GetCurrentTrack()
        {
            return m_currentTrack;
        }

        public bool IsQueueEmpty()
        {
            return m_musicQueue.Count == 0;
        }

        private void PlayNextInQueue()
        {
            if (m_musicQueue.Count == 0)
            {
                m_currentTrack = null;
                return;
            }

            m_currentTrack = m_musicQueue.Dequeue();
            PlayMusic(m_currentTrack);

            // Start checking if the current track has finished
            StartQueueCheck();
        }

        private void StartQueueCheck()
        {
            if (m_queueCheckCo != null)
            {
                StopCoroutine(m_queueCheckCo);
            }
            m_queueCheckCo = StartCoroutine(CheckMusicEnd());
        }

        private void RestartStartingMusic()
        {
            // Add all starting tracks back to the queue
            foreach (var track in m_startingMusicTracks)
            {
                m_musicQueue.Enqueue(track);
            }

            // Start playing the first track
            if (m_musicQueue.Count > 0)
            {
                PlayNextInQueue();
            }
        }

        private IEnumerator CheckMusicEnd()
        {
            if (m_currentTrack == null || m_activeMusic == null) yield break;

            // Allow crossfade to swap active/idle first
            yield return null;

            // Wait until one of the music sources is actually playing the current track
            AudioSource trackSource = null;
            while (m_currentTrack != null && trackSource == null)
            {
                if (m_isPaused) yield return null;
                else if (m_musicA != null && m_musicA.clip == m_currentTrack.track && m_musicA.isPlaying) trackSource = m_musicA;
                else if (m_musicB != null && m_musicB.clip == m_currentTrack.track && m_musicB.isPlaying) trackSource = m_musicB;
                else yield return null;
            }

            if (m_currentTrack == null || trackSource == null) yield break;

            // Wait for that source to finish playback (music sources do not loop)
            while (m_isPaused || (trackSource.isPlaying && trackSource.clip == m_currentTrack.track))
            {
                yield return null;
            }

            // Advance to the next track or restart starting music
            if (m_musicQueue.Count > 0)
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
            m_musicA = CreateMusicSource("Music_A"); // empty music source
            m_musicB = CreateMusicSource("Music_B");
            m_activeMusic = m_musicA; m_idleMusic = m_musicB;
        }

        private AudioSource CreateMusicSource(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = false; // We don't allow looping here because of the queue system
            src.spatialBlend = 0f;           // music is 2D
            src.outputAudioMixerGroup = m_musicGroup;
            return src;
        }

        
    }
}
