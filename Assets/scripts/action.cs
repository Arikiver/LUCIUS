using UnityEngine;

public class Action : MonoBehaviour
{
    public AudioSource source;
    private RandomSpawn spawner; // Assign a reference to this field
    public AudioClip clip1;
    private ObjectMovement objectMovement; // This will hold the reference to ObjectMovement component

    void Start()
    {
        // Find the ObjectMovement component in the scene and assign it to objectMovement variable
        objectMovement = FindObjectOfType<ObjectMovement>();

        // Find the RandomSpawn component in the scene and assign it to spawner variable
        spawner = FindObjectOfType<RandomSpawn>();

        // Check if objectMovement is found
        if (objectMovement == null)
        {
            Debug.LogError("ObjectMovement component not found in the scene!");
        }

        // Check if spawner is found
        if (spawner == null)
        {
            Debug.LogError("RandomSpawn component not found in the scene!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger entered: " + other.gameObject.name);

        if (other.CompareTag("Candle"))
        {
            Debug.Log("Candle collider detected");

            source.PlayOneShot(clip1);

            // Deactivate the candle GameObject
            other.gameObject.SetActive(false);

            // Increment spawnCount in the spawner
            if (spawner != null)
            {
                spawner.spawnCount++;
            }
            else
            {
                Debug.LogError("RandomSpawn component is not assigned!");
            }

            // Check if objectMovement is assigned before accessing its properties
            if (objectMovement != null)
            {
                // Set the Y position to initialPositionY and reset candleLife
                float y = objectMovement.initialPositionY;
                objectMovement.ChangeY(y);
                objectMovement.timer = 0f;
                objectMovement.candleLife = 20f;
            }
            else
            {
                Debug.LogError("ObjectMovement component is not assigned!");
            }
        }
    }
}
