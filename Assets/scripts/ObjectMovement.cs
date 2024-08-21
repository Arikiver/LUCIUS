using UnityEngine;

public class ObjectMovement : MonoBehaviour
{
    public float speed = 1f; // Adjust the speed as needed
    private bool moving = true;
    public float timer = 0f;
    public float candleLife = 20f;
    public float initialPositionY; // Declare initialPositionY here
    public Sprite unlit;
    public Light lght;

    public AudioSource source;
    public AudioClip clip1;
    public AudioClip clip2;

    public GameObject gameOverText; // Reference to the Game Over text object

    void Start()
    {
        gameOverText.SetActive(false);
        initialPositionY = transform.position.y; // Assign initial Y position here
        source.Play();
    }

    public void ChangeY(float y)
    {
        Vector3 newPosition = transform.position;
        newPosition.y = y;
        transform.position = newPosition;
    }

    void Update()
    {
        if (moving)
        {
            timer += Time.deltaTime;
            if (timer <= candleLife) // Move for 20 seconds
            {
                // Move the object downward
                GetComponent<Animator>().enabled = true;
                transform.Translate(Vector3.down * speed * Time.deltaTime);
            }
            else
            {
                // Object should stop moving after 20 seconds
                source.Stop();
                moving = false;
                source.PlayOneShot(clip2);
                Debug.Log("Unlit sprite: " + unlit); // Check if the sprite is assigned correctly
                Debug.Log("SpriteRenderer component: " + GetComponent<SpriteRenderer>().sprite); // Check if SpriteRenderer component is present
                GetComponent<Animator>().enabled = false;
                lght.enabled = false;
                GetComponent<SpriteRenderer>().sprite = unlit;

                // Check if candle life is over
                if (timer >= candleLife)
                {
                    // Display game over text
                    if (gameOverText != null)
                        gameOverText.SetActive(true);
                }
            }
        }
    }
}
