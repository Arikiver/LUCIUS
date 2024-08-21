using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepScript : MonoBehaviour
{
    public AudioSource footstep;

    void Update()
    {
        bool moving = Input.GetKey("w") || Input.GetKey("s") || Input.GetKey("a") || Input.GetKey("d");
        bool sprinting = moving && Input.GetKey(KeyCode.LeftShift);

        if (moving)
        {
            footstep.enabled = true;
            footstep.pitch = sprinting ? 1.5f : 1f;
        }
        else
        {
            footstep.enabled = false;
        }
    }
}
