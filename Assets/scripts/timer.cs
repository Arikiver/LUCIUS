using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    public float time = 0f;
    private ObjectMovement obj;
    public TextMeshProUGUI timerText;

    private void Start()
    {
        obj = FindObjectOfType<ObjectMovement>();

        if (obj == null)
        {
            Debug.LogError("ObjectMovement component not found in the scene!");
        }

        if (timerText == null)
        {
            Debug.LogError("Timer Text UI not assigned!");
        }
    }

    void Update()
    {
        // Enhanced null checking
        if (obj != null && obj.gameOverText != null && !obj.gameOverText.activeSelf)
        {
            time += Time.deltaTime;
            UpdateTimerText();
        }
    }

    void UpdateTimerText()
    {
        if (timerText != null)
        {
            // Convert time to minutes and seconds
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);
            string timerString = string.Format("{0:00}:{1:00}", minutes, seconds);
            timerText.text = "Time: " + timerString;
        }
    }
}
