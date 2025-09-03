using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "New SFX Event", menuName = "Audio/SFX Event")]
public class SfxEvent : ScriptableObject
{
    [Header("Clips & Randomization")]
    public AudioClip[] clips;
    [Range(0f, 1f)] public float volume = 1f;

    // add some jitter so that it doesn't always sound the same
    public Vector2 volumeJitter = new Vector2(0f, 0.1f);
    public Vector2 pitchJitter = new Vector2(0.95f, 1.05f);

    [Header("Routing & Spatial")]
    public AudioMixerGroup outputGroup;

    [Tooltip("0=2D, 1=3D")]
    [Range(0f, 1f)] public float spatialBlend = 0f;   // 0=2D, 1=3D

    [Tooltip("Logarithmic is more realistic. USe custom to create your own dropoff (why would you do that?). Linear when we dont care about realism.")]
    public AudioRolloffMode rolloff = AudioRolloffMode.Logarithmic;

    [Tooltip("The distance at which the sound is at full volume.")]
    public float minDistance = 1f;

    [Tooltip("The distance at which the sound is at 0 volume.")]
    public float maxDistance = 30f;

    [Range(0, 256)]
    public int priority = 128;

    [Header("Behavior")]
    public bool loop = false;
    public bool allowMultiple = true;    // can multiple sounds play at once?
    public float minRepeatDelay = 0.05f; // how long to wait before playing the same sound again
    public bool respectTime = true; // do we pause the sound when the game is paused?

    public AudioClip GetRandomClip() =>
        (clips == null || clips.Length == 0) ? null : clips[Random.Range(0, clips.Length)];
}