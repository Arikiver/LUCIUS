using UnityEngine;

public class EnemySightMusicTrigger : MonoBehaviour
{
    [Header("References")]
    public EnemyAI enemyAI; // Reference to the enhanced enemy AI
    public AudioSource audioSource; // Main audio source (usually on player)
    public AudioSource enemyAudioSource; // Audio source on the enemy for scream

    [Header("Audio Clips")]
    public AudioClip jumpscareClip;
    public AudioClip enemyScreamClip; // Enemy scream when detecting player
    public AudioClip chaseMusicClip;
    public AudioClip ambientMusicClip; // Optional: background music for wandering

    [Header("Settings")]
    public float musicFadeTime = 1f; // Time to fade between music tracks
    public float jumpscareVolume = 1f;
    public float enemyScreamVolume = 0.8f;
    public float chaseMusicVolume = 0.7f;
    public float ambientMusicVolume = 0.3f;

    // Internal state tracking
    private enum MusicState { Ambient, Jumpscare, Chase, Silent }
    private MusicState currentMusicState = MusicState.Ambient;
    private MusicState previousMusicState = MusicState.Silent;

    private bool hasPlayedJumpscareThisChase = false;
    private Coroutine fadeCoroutine;

    void Start()
    {
        if (jumpscareClip == null) Debug.LogWarning("Jumpscare clip not assigned!");
        if (enemyScreamClip == null) Debug.LogWarning("Enemy scream clip not assigned!");
        if (chaseMusicClip == null) Debug.LogWarning("Chase music clip not assigned!");

        // Start with ambient music if available
        if (ambientMusicClip != null)
        {
            PlayAmbientMusic();
        }
    }

    void Update()
    {
        if (enemyAI == null) return;

        // Check enemy state and update music accordingly
        EnemyAI.EnemyState currentEnemyState = GetEnemyState();

        switch (currentEnemyState)
        {
            case EnemyAI.EnemyState.Wandering:
                HandleWanderingMusic();
                break;

            case EnemyAI.EnemyState.Chasing:
                HandleChaseMusic();
                break;

            case EnemyAI.EnemyState.Searching:
                HandleSearchingMusic();
                break;
        }
    }

    void HandleWanderingMusic()
    {
        if (currentMusicState != MusicState.Ambient)
        {
            // Reset jumpscare flag when returning to wandering
            hasPlayedJumpscareThisChase = false;

            // Stop chase music with fade out
            if (currentMusicState == MusicState.Chase)
            {
                Debug.Log("Fading out chase music");
                if (fadeCoroutine != null)
                {
                    StopCoroutine(fadeCoroutine);
                }
                fadeCoroutine = StartCoroutine(FadeOutAndStop(musicFadeTime));
                currentMusicState = MusicState.Silent;
            }

            // After fade out, play ambient music if available
            Invoke(nameof(DelayedAmbientMusic), musicFadeTime + 0.1f);
        }
    }

    void DelayedAmbientMusic()
    {
        if (GetEnemyState() == EnemyAI.EnemyState.Wandering && ambientMusicClip != null)
        {
            PlayAmbientMusic();
        }
    }

    void HandleChaseMusic()
    {
        // State validation
        if (currentMusicState == MusicState.Ambient)
        {
            hasPlayedJumpscareThisChase = false; // Reset flag when entering chase from ambient
        }

        // Play enemy scream first when chase begins
        if (!hasPlayedJumpscareThisChase && currentMusicState != MusicState.Jumpscare && currentMusicState != MusicState.Chase)
        {
            PlayEnemyScream();
        }
        // If scream has finished, play jumpscare and chase music
        else if (hasPlayedJumpscareThisChase && currentMusicState != MusicState.Chase)
        {
            PlayJumpscareAndChaseMusic();
        }
    }

    void HandleSearchingMusic()
    {
        // During searching, keep chase music but at lower volume
        if (currentMusicState == MusicState.Chase)
        {
            // Optionally lower the chase music volume during search
            if (fadeCoroutine == null)
            {
                fadeCoroutine = StartCoroutine(FadeToVolume(chaseMusicVolume * 0.5f, musicFadeTime));
            }
        }
    }

    void PlayEnemyScream()
    {
        Debug.Log("Playing enemy scream first!");
        hasPlayedJumpscareThisChase = true;

        // Play enemy scream sound from the enemy's position
        if (enemyAudioSource != null && enemyScreamClip != null)
        {
            // Stop any current audio on enemy
            enemyAudioSource.Stop();

            // Configure for better audibility
            enemyAudioSource.volume = enemyScreamVolume;
            enemyAudioSource.pitch = 1f; // Normal pitch
            enemyAudioSource.spatialBlend = 0.7f; // Mix of 3D and 2D for better audibility
            enemyAudioSource.rolloffMode = AudioRolloffMode.Linear;
            enemyAudioSource.maxDistance = 50f; // Increase max hearing distance
            enemyAudioSource.minDistance = 5f;

            enemyAudioSource.PlayOneShot(enemyScreamClip);

            // Schedule jumpscare and chase music after scream
            float delay = enemyScreamClip != null ? enemyScreamClip.length : 0.5f;
            Invoke(nameof(PlayJumpscareAndChaseMusic), delay);
        }
        else
        {
            Debug.LogWarning("Enemy Audio Source or Scream Clip not assigned! Playing on main audio source instead.");
            // Fallback to main audio source if enemy audio source isn't assigned
            audioSource.volume = enemyScreamVolume;
            audioSource.PlayOneShot(enemyScreamClip);
            float delay = enemyScreamClip != null ? enemyScreamClip.length : 0.5f;
            Invoke(nameof(PlayJumpscareAndChaseMusic), delay);

        }
    }

    void PlayJumpscareAndChaseMusic()
    {
        // Only play if still in chase state
        if (GetEnemyState() == EnemyAI.EnemyState.Chasing || GetEnemyState() == EnemyAI.EnemyState.Searching)
        {
            Debug.Log("Playing jumpscare and chase music!");
            currentMusicState = MusicState.Jumpscare;

            // Stop current music and play jumpscare
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }

            audioSource.Stop();
            audioSource.volume = jumpscareVolume;
            audioSource.PlayOneShot(jumpscareClip);

            // Schedule chase music to play after jumpscare
            Invoke(nameof(PlayChaseMusic), jumpscareClip.length);
        }
    }

    void PlayChaseMusic()
    {
        if (GetEnemyState() == EnemyAI.EnemyState.Chasing || GetEnemyState() == EnemyAI.EnemyState.Searching)
        {
            Debug.Log("Playing chase music!");
            currentMusicState = MusicState.Chase;

            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }

            // Make sure we stop any current audio first
            audioSource.Stop();

            // Set up chase music
            audioSource.clip = chaseMusicClip;
            audioSource.loop = true;
            audioSource.volume = chaseMusicVolume;
            audioSource.Play();
        }
    }

    void PlayAmbientMusic()
    {
        if (currentMusicState != MusicState.Ambient && ambientMusicClip != null)
        {
            Debug.Log("Playing ambient music");
            currentMusicState = MusicState.Ambient;

            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            fadeCoroutine = StartCoroutine(FadeToNewTrack(ambientMusicClip, ambientMusicVolume, true));
        }
    }

    void FadeToSilence()
    {
        if (currentMusicState != MusicState.Silent)
        {
            Debug.Log("Fading to silence");
            currentMusicState = MusicState.Silent;

            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            fadeCoroutine = StartCoroutine(FadeToVolume(0f, musicFadeTime, true));
        }
    }

    // Helper method to get enemy state
    EnemyAI.EnemyState GetEnemyState()
    {
        if (enemyAI == null) return EnemyAI.EnemyState.Wandering;
        return enemyAI.CurrentState;
    }

    // Coroutine to fade audio volume
    System.Collections.IEnumerator FadeToVolume(float targetVolume, float fadeTime, bool stopWhenDone = false)
    {
        float startVolume = audioSource.volume;
        float elapsedTime = 0f;

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsedTime / fadeTime);
            yield return null;
        }

        audioSource.volume = targetVolume;

        if (stopWhenDone && targetVolume == 0f)
        {
            audioSource.Stop();
            audioSource.clip = null; // Clear clip to prevent issues
        }

        fadeCoroutine = null;
    }

    // Coroutine to fade out and stop audio
    System.Collections.IEnumerator FadeOutAndStop(float fadeTime)
    {
        float startVolume = audioSource.volume;
        float elapsedTime = 0f;

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / fadeTime);
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();
        audioSource.clip = null; // Clear the clip to prevent looping issues
        fadeCoroutine = null;
    }

    // Coroutine to fade to a new track
    System.Collections.IEnumerator FadeToNewTrack(AudioClip newClip, float targetVolume, bool loop)
    {
        // Fade out current track
        yield return StartCoroutine(FadeToVolume(0f, musicFadeTime / 2f));

        // Switch to new track
        audioSource.clip = newClip;
        audioSource.loop = loop;
        audioSource.Play();

        // Fade in new track
        yield return StartCoroutine(FadeToVolume(targetVolume, musicFadeTime / 2f));
    }

    // Clean up coroutines when disabled
    void OnDisable()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
    }
    void OnDestroy()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
        CancelInvoke(); // Cancel all pending Invoke calls
    }
}