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
    }

    void Update()
    {
        // Check if obj is not null and if gameOverText is not null before accessing its properties
        if (obj != null && obj.gameOverText != null && obj.gameOverText.activeSelf == false)
        {
            time += Time.deltaTime;

            // Update TextMesh text
            UpdateTimerText();
        }
    }

    void UpdateTimerText()
    {
        // Convert time to minutes and seconds
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        string timerString = string.Format("{0:00}:{1:00}", minutes, seconds);

        // Update TextMesh text
        timerText.text = "Time: " + timerString;
    }
}
