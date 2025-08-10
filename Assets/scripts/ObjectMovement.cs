using UnityEngine;
using System.Collections;

public class ObjectMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 1f;
    private bool moving = true;

    [Header("Timer Settings")]
    public float timer = 0f;
    public float candleLife = 20f;
    public float initialPositionY;

    [Header("Components")]
    public Sprite unlit;
    public Light lght;
    public AudioSource source;
    public AudioClip clip1;
    public AudioClip clip2; // Candle extinguish sound

    [Header("Death Audio")]
    public AudioSource deathAudioSource; // Audio source for death sound
    public AudioClip deathSoundClip; // Same death sound as enemy collision
    public float delayBeforeDeathSound = 0.5f; // Delay between extinguish and death sound

    [Header("Game Over")]
    public GameObject gameOverText;

    private SpriteRenderer spriteRenderer;
    private bool gameEnded = false; // Prevent multiple triggers

    void Start()
    {
        // Cache components
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Validate components
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component missing!");
        }

        if (gameOverText != null)
        {
            gameOverText.SetActive(false);
        }

        // Debug audio components
        if (source == null)
        {
            Debug.LogError("Main AudioSource is missing!");
        }
        if (clip2 == null)
        {
            Debug.LogError("Candle extinguish sound (clip2) is not assigned!");
        }

        initialPositionY = transform.position.y;
        if (source != null)
        {
            source.Play();
        }
    }

    public void ChangeY(float y)
    {
        Vector3 newPosition = transform.position;
        newPosition.y = y;
        transform.position = newPosition;
    }

    public void ResetMovement()
    {
        moving = true;
        gameEnded = false; // Reset game end flag
        timer = 0f; // Reset timer

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        if (lght != null)
        {
            lght.enabled = true;
        }

        if (source != null && !source.isPlaying)
        {
            source.Play();
        }
    }

    void Update()
    {
        // Stop everything if game is paused/ended
        if (Time.timeScale == 0f || gameEnded)
        {
            if (source != null && source.isPlaying)
            {
                source.Stop();
            }
            return;
        }

        if (moving)
        {
            timer += Time.deltaTime;
            if (timer <= candleLife)
            {
                // Move the object downward
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = true;
                }

                transform.Translate(Vector3.down * speed * Time.deltaTime);
            }
            else
            {
                // Stop movement when time is up
                StopCandle();
            }
        }
    }

    private void StopCandle()
    {
        if (gameEnded) return; // Prevent multiple calls

        gameEnded = true;
        moving = false;

        Debug.Log("Candle burning out - attempting to play extinguish sound");

        // Stop candle sound and play extinguish sound first
        if (source != null)
        {
            source.Stop();
            Debug.Log("Stopped main candle sound");

            if (clip2 != null)
            {
                Debug.Log("Playing extinguish sound (clip2)");
                source.volume = 1f; // Ensure volume is up
                source.PlayOneShot(clip2);

                // Additional check
                if (source.isPlaying)
                {
                    Debug.Log("Extinguish sound is now playing");
                }
                else
                {
                    Debug.LogError("Extinguish sound failed to play!");
                }
            }
            else
            {
                Debug.LogError("clip2 (extinguish sound) is null!");
            }
        }
        else
        {
            Debug.LogError("AudioSource is null!");
        }

        // Visual changes (immediate)
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
            if (unlit != null)
            {
                spriteRenderer.sprite = unlit;
            }
        }

        if (lght != null)
        {
            lght.enabled = false;
        }

        // Show game over UI (immediate)
        if (gameOverText != null)
        {
            gameOverText.SetActive(true);
        }

        // Start coroutine to play death sound after delay, then stop game
        StartCoroutine(PlayDeathSoundAndStopGame());
    }

    private IEnumerator PlayDeathSoundAndStopGame()
    {
        // Wait for the specified delay
        float waitTime = delayBeforeDeathSound;

        Debug.Log($"Waiting {waitTime} seconds before playing death sound");
        yield return new WaitForSeconds(waitTime);

        // Play death sound effect
        if (deathAudioSource != null && deathSoundClip != null)
        {
            Debug.Log("Playing death sound");
            deathAudioSource.PlayOneShot(deathSoundClip);
        }

        // Wait a brief moment for death sound to start playing
        yield return new WaitForSeconds(0.1f);

        // Stop the entire game
        StopGame();
    }

    private void StopGame()
    {
        Debug.Log("Stopping game completely");

        // Pause the game
        Time.timeScale = 0f;

        // Disable player movement
        FirstPersonController playerController = FindObjectOfType<FirstPersonController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // Stop enemy AI
        EnemyAI enemyAI = FindObjectOfType<EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.enabled = false;
        }

        // Stop footstep and breathing sounds
        FootstepScript footstepScript = FindObjectOfType<FootstepScript>();
        if (footstepScript != null)
        {
            footstepScript.enabled = false;
        }

        // Stop enemy sight music trigger
        EnemySightMusicTrigger musicTrigger = FindObjectOfType<EnemySightMusicTrigger>();
        if (musicTrigger != null)
        {
            musicTrigger.enabled = false;
        }

        // Unlock and show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Game ended - Candle burned out!");
    }
}
