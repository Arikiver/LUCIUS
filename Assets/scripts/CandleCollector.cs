using UnityEngine;

public class CandleCollector : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource source;
    public AudioClip clip1;

    [Header("Cached References")]
    private RandomSpawn spawner;
    private ObjectMovement objectMovement;

    void Start()
    {
        // Cache references once
        objectMovement = FindObjectOfType<ObjectMovement>();
        spawner = FindObjectOfType<RandomSpawn>();

        // Validate references
        if (objectMovement == null)
        {
            Debug.LogError("ObjectMovement component not found in the scene!");
        }

        if (spawner == null)
        {
            Debug.LogError("RandomSpawn component not found in the scene!");
        }

        if (source == null)
        {
            source = GetComponent<AudioSource>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger entered: " + other.gameObject.name);

        if (other.CompareTag("Candle"))
        {
            Debug.Log("Candle collected!");

            // Play collection sound
            if (source != null && clip1 != null)
            {
                source.PlayOneShot(clip1);
            }

            // Deactivate the candle GameObject
            other.gameObject.SetActive(false);

            // Update spawner count
            if (spawner != null)
            {
                spawner.spawnCount++;
            }

            // Reset candle mechanics
            if (objectMovement != null)
            {
                float y = objectMovement.initialPositionY;
                objectMovement.ChangeY(y);
                objectMovement.timer = 0f;
                objectMovement.candleLife = 20f;
                objectMovement.ResetMovement(); // New method we'll add
            }
        }
    }
}
