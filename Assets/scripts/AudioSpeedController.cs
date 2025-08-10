using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioSpeedController : MonoBehaviour
{
    [Range(0.1f, 3f)]
    public float playbackSpeed = 1f;

    public AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.pitch = playbackSpeed;
    }

    void Update()
    {
        if (audioSource.pitch != playbackSpeed)
            audioSource.pitch = playbackSpeed;
    }
}
