using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepScript : MonoBehaviour
{
    [Header("Footstep Audio")]
    public AudioSource footstep;

    [Header("Breathing Audio")]
    public AudioSource walkingBreaths; // Now acts as "normal/idle" breathing
    public AudioSource runningBreaths;
    public AudioSource heavingBreaths;

    [Header("Breathing Settings")]
    public float fadeSpeed = 2f;
    public float heavingDuration = 3f;
    public float movementThreshold = 0.1f;
    public float minRunTimeForHeaving = 10f;

    private bool wasRunning = false;
    private bool isHeaving = false;
    private float runningTime = 0f;
    private Coroutine heavingCoroutine;
    private Coroutine fadeCoroutine;

    void Start()
    {
        // Initialize all breathing audio sources
        if (walkingBreaths != null)
        {
            walkingBreaths.loop = true;
            walkingBreaths.volume = 1f; // Start with normal breathing at full volume
            walkingBreaths.Play(); // Start playing immediately
            Debug.Log("Normal breathing started");
        }

        if (runningBreaths != null)
        {
            runningBreaths.loop = true;
            runningBreaths.volume = 0f;
        }

        if (heavingBreaths != null)
        {
            heavingBreaths.loop = true;
            heavingBreaths.volume = 0f;
        }
    }

    void Update()
    {
        // Stop all sounds if game is paused/ended
        if (Time.timeScale == 0f)
        {
            StopAllBreathingSounds();
            if (footstep != null)
                footstep.enabled = false;
            return;
        }

        // Movement detection
        bool moving = Mathf.Abs(Input.GetAxis("Horizontal")) > movementThreshold ||
                     Mathf.Abs(Input.GetAxis("Vertical")) > movementThreshold;
        bool sprinting = moving && Input.GetKey(KeyCode.LeftShift);

        // Track running time
        if (sprinting)
        {
            runningTime += Time.deltaTime;
        }
        else if (wasRunning)
        {
            // Player just stopped running
            if (runningTime >= minRunTimeForHeaving && !isHeaving)
            {
                StartHeaving();
            }
            runningTime = 0f; // Reset running time
        }

        // Handle footstep audio
        HandleFootsteps(moving, sprinting);

        // Handle breathing audio
        HandleBreathing(moving, sprinting);
    }

    void StopAllBreathingSounds()
    {
        if (walkingBreaths != null && walkingBreaths.isPlaying)
            walkingBreaths.Stop();
        if (runningBreaths != null && runningBreaths.isPlaying)
            runningBreaths.Stop();
        if (heavingBreaths != null && heavingBreaths.isPlaying)
            heavingBreaths.Stop();

        // Stop all coroutines
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
        if (heavingCoroutine != null)
        {
            StopCoroutine(heavingCoroutine);
            heavingCoroutine = null;
        }
    }

    void HandleFootsteps(bool moving, bool sprinting)
    {
        if (moving && footstep != null)
        {
            footstep.enabled = true;
            footstep.pitch = sprinting ? 1.5f : 1f;
        }
        else if (footstep != null)
        {
            footstep.enabled = false;
        }
    }

    void HandleBreathing(bool moving, bool sprinting)
    {
        // If player is currently heaving, don't change breathing state
        if (isHeaving)
        {
            wasRunning = sprinting;
            return;
        }

        // Determine target breathing state
        BreathingState targetState = BreathingState.Normal; // Default to normal breathing

        if (sprinting)
        {
            targetState = BreathingState.Running;
        }

        // Only fade if we need to change state
        if (ShouldChangeBreathingState(targetState))
        {
            FadeToBreathingState(targetState);
        }

        wasRunning = sprinting;
    }

    bool ShouldChangeBreathingState(BreathingState targetState)
    {
        // Check current active state
        if (targetState == BreathingState.Normal && walkingBreaths != null && walkingBreaths.volume > 0.5f)
            return false;
        if (targetState == BreathingState.Running && runningBreaths != null && runningBreaths.volume > 0.5f)
            return false;
        if (targetState == BreathingState.Heaving && heavingBreaths != null && heavingBreaths.volume > 0.5f)
            return false;

        return true;
    }

    void FadeToBreathingState(BreathingState targetState)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeBreathingCoroutine(targetState));
    }

    void StartHeaving()
    {
        isHeaving = true;

        // Stop any existing fade
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        // Start heaving sound and fade to it
        fadeCoroutine = StartCoroutine(FadeBreathingCoroutine(BreathingState.Heaving));

        // Stop heaving after duration
        if (heavingCoroutine != null)
        {
            StopCoroutine(heavingCoroutine);
        }
        heavingCoroutine = StartCoroutine(HeavingDurationCoroutine());
    }

    IEnumerator HeavingDurationCoroutine()
    {
        yield return new WaitForSeconds(heavingDuration);
        isHeaving = false;

        // Fade back to normal breathing (always goes back to normal after heaving)
        FadeToBreathingState(BreathingState.Normal);
    }

    IEnumerator FadeBreathingCoroutine(BreathingState targetState)
    {
        // Start the target audio source if it's not already playing
        AudioSource targetSource = GetBreathingAudioSource(targetState);
        if (targetSource != null && !targetSource.isPlaying)
        {
            targetSource.Play();
        }

        // Fade out all non-target sources and fade in target
        while (true)
        {
            bool fadeComplete = true;

            // Fade normal breathing (walking/idle)
            if (walkingBreaths != null)
            {
                float target = (targetState == BreathingState.Normal) ? 1f : 0f;
                walkingBreaths.volume = Mathf.MoveTowards(walkingBreaths.volume, target, fadeSpeed * Time.deltaTime);
                if (Mathf.Abs(walkingBreaths.volume - target) > 0.01f)
                    fadeComplete = false;

                // Start playing if we're fading in
                if (target > 0f && !walkingBreaths.isPlaying)
                    walkingBreaths.Play();
            }

            // Fade running breathing
            if (runningBreaths != null)
            {
                float target = (targetState == BreathingState.Running) ? 1f : 0f;
                runningBreaths.volume = Mathf.MoveTowards(runningBreaths.volume, target, fadeSpeed * Time.deltaTime);
                if (Mathf.Abs(runningBreaths.volume - target) > 0.01f)
                    fadeComplete = false;

                // Start playing if we're fading in
                if (target > 0f && !runningBreaths.isPlaying)
                    runningBreaths.Play();
            }

            // Fade heaving breathing
            if (heavingBreaths != null)
            {
                float target = (targetState == BreathingState.Heaving) ? 1f : 0f;
                heavingBreaths.volume = Mathf.MoveTowards(heavingBreaths.volume, target, fadeSpeed * Time.deltaTime);
                if (Mathf.Abs(heavingBreaths.volume - target) > 0.01f)
                    fadeComplete = false;

                // Start playing if we're fading in
                if (target > 0f && !heavingBreaths.isPlaying)
                    heavingBreaths.Play();
            }

            if (fadeComplete)
                break;

            yield return null;
        }

        // Don't stop the normal breathing - keep it running
        if (runningBreaths != null && runningBreaths.volume <= 0f && runningBreaths.isPlaying)
            runningBreaths.Stop();
        if (heavingBreaths != null && heavingBreaths.volume <= 0f && !isHeaving && heavingBreaths.isPlaying)
            heavingBreaths.Stop();
    }

    AudioSource GetBreathingAudioSource(BreathingState state)
    {
        switch (state)
        {
            case BreathingState.Normal:
                return walkingBreaths;
            case BreathingState.Running:
                return runningBreaths;
            case BreathingState.Heaving:
                return heavingBreaths;
            default:
                return null;
        }
    }

    enum BreathingState
    {
        Normal,
        Running,
        Heaving
    }
}
