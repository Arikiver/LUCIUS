using UnityEngine;

public class EnemyCollision : MonoBehaviour
{
    [Header("Game Over")]
    public GameObject gameOverUI; // Assign Canvas UI element in Inspector

    [Header("Death Audio")]
    public AudioSource deathAudioSource; // Audio source for death sound
    public AudioClip deathSoundClip; // Death sound effect

    private bool gameEnded = false; // Prevent multiple triggers

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !gameEnded && Time.timeScale > 0f)
        {
            gameEnded = true;
            Debug.Log("Player died! Game Over!");

            // Play death sound effect
            if (deathAudioSource != null && deathSoundClip != null)
            {
                deathAudioSource.PlayOneShot(deathSoundClip);
            }

            // Show game over UI
            if (gameOverUI != null)
            {
                gameOverUI.SetActive(true);
            }

            // Stop the game
            StopGame();
        }
    }

    private void StopGame()
    {
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

        // Stop candle movement if it's still running
        ObjectMovement candleMovement = FindObjectOfType<ObjectMovement>();
        if (candleMovement != null)
        {
            candleMovement.enabled = false;
        }

        // Unlock and show cursor for potential restart button
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Game stopped - Player death");
    }
}
