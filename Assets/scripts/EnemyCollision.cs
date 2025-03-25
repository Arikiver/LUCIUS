using UnityEngine;

public class EnemyCollision : MonoBehaviour
{
    public GameObject gameOverUI; // Assign Canvas UI element in Inspector

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Collision detected with Player!"); // Check if this prints
            gameOverUI.SetActive(true);
            Debug.Log("Game Over UI should be active: " + gameOverUI.activeSelf);
        }
    }
}
