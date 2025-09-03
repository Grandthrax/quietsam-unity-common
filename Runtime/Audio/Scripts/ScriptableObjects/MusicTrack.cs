using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "New Music Track", menuName = "Audio/Music Track")]
public class MusicTrack : ScriptableObject
{
    [Header("Clips")]

    public AudioClip track;    
    public AudioMixerGroup output;
    [Range(0f,1f)] public float volume = 1f;

    [Header("Optional musical info")]
    public float bpm = 120f;
    public int beatsPerBar = 4;
}