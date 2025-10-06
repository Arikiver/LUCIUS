using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;

public class VideoSceneTransition : MonoBehaviour
{
    [Header("Video Components")]
    public VideoPlayer videoPlayer;          // Assign VideoPlayer component
    public RawImage videoSurface;            // Fullscreen RawImage to display video
    public RenderTexture videoRenderTexture; // RenderTexture for video output
    public AudioSource audioSource;         // AudioSource for video audio (optional)

    [Header("UI Fade")]
    public Image fadeOverlay;                // Black Image for fade effect
    public Canvas fadeCanvas;                // Canvas containing the fade overlay
    public float fadeInDuration = 0.8f;      // Duration of fade IN (to black before video)
    public float fadeOutDuration = 1.0f;     // Duration of fade OUT (to black after video)
    public float pauseBetweenFades = 0.3f;   // Brief pause in black before video starts

    [Header("Scene Management")]
    public int targetSceneIndex = 1;         // Scene to load after video

    [Header("Audio Settings")]
    public bool useAudioSource = false;      // Use AudioSource or Direct audio output

    private bool isVideoPlaying = false;
    private bool hasVideoEnded = false;
    private bool isTransitioning = false;

    void Awake()
    {
        SetupVideoPlayer();
        SetupFade();
        SetupVideoSurface();

        // Subscribe to video end event
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoEnded;
            videoPlayer.prepareCompleted += OnVideoPrepared;
        }
    }

    void SetupVideoPlayer()
    {
        if (videoPlayer == null) return;

        // Basic video setup - CRITICAL: Don't prepare automatically!
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = videoRenderTexture;

        // Setup audio output
        if (useAudioSource && audioSource != null)
        {
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            videoPlayer.controlledAudioTrackCount = 1;
            videoPlayer.EnableAudioTrack(0, true);
            videoPlayer.SetTargetAudioSource(0, audioSource);
        }
        else
        {
            // Use direct audio output (simpler)
            videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        }
    }

    void SetupVideoSurface()
    {
        if (videoSurface != null && videoRenderTexture != null)
        {
            // Assign render texture to RawImage
            videoSurface.texture = videoRenderTexture;

            // Make sure RawImage is fullscreen by setting anchors
            RectTransform rt = videoSurface.rectTransform;
            rt.anchorMin = Vector2.zero;        // Bottom-left
            rt.anchorMax = Vector2.one;         // Top-right  
            rt.offsetMin = Vector2.zero;        // No offset from anchors
            rt.offsetMax = Vector2.zero;        // No offset from anchors

            // Hide initially
            videoSurface.gameObject.SetActive(false);
        }
    }

    void SetupFade()
    {
        if (fadeOverlay != null)
        {
            // Make fade overlay fullscreen
            RectTransform rt = fadeOverlay.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Set to black and transparent initially
            fadeOverlay.color = new Color(0, 0, 0, 0);
            fadeOverlay.raycastTarget = false; // Don't block input initially
        }

        // Setup fade canvas to be on top of everything
        if (fadeCanvas != null)
        {
            fadeCanvas.overrideSorting = true;
            fadeCanvas.sortingOrder = 1000; // Very high value to be on top
            fadeCanvas.enabled = false; // Disabled initially
        }
    }

    // Call this method from Button OnClick event
    public void PlayFullscreenVideo()
    {
        if (videoPlayer == null || videoSurface == null)
        {
            Debug.LogError("VideoPlayer or VideoSurface not assigned!");
            return;
        }

        if (isTransitioning)
        {
            Debug.Log("Already transitioning, ignoring button press");
            return;
        }

        Debug.Log("Starting video transition...");

        // Reset states
        isVideoPlaying = false;
        hasVideoEnded = false;
        isTransitioning = true;

        // Start the full transition sequence
        StartCoroutine(HandleVideoTransition());
    }

    private IEnumerator HandleVideoTransition()
    {
        // Step 1: Enable fade canvas
        if (fadeCanvas != null)
        {
            fadeCanvas.enabled = true;
        }

        // Step 2: Fade to black (fade IN)
        Debug.Log("Fading to black...");
        yield return StartCoroutine(FadeIn());

        // Step 3: Show video surface and prepare video while screen is black
        if (videoSurface != null)
        {
            videoSurface.gameObject.SetActive(true);
        }

        // Prepare video
        videoPlayer.Prepare();

        // Wait for video to be prepared (or timeout after 5 seconds)
        float timeout = 5f;
        float elapsed = 0f;
        while (!videoPlayer.isPrepared && elapsed < timeout)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!videoPlayer.isPrepared)
        {
            Debug.LogError("Video failed to prepare within timeout!");
            isTransitioning = false;
            yield break;
        }

        // Step 4: Brief pause in black
        Debug.Log("Brief pause in black...");
        yield return new WaitForSecondsRealtime(pauseBetweenFades);

        // Step 5: Start video and fade from black (fade OUT from black to show video)
        Debug.Log("Starting video and fading from black...");
        videoPlayer.Play();
        isVideoPlaying = true;

        yield return StartCoroutine(FadeFromBlack());

        // Step 6: Hide fade canvas during video playback
        if (fadeCanvas != null)
        {
            fadeCanvas.enabled = false;
        }

        isTransitioning = false;
        Debug.Log("Video transition complete, video now playing");
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        Debug.Log("Video prepared and ready to play");
        // Video preparation is handled in the coroutine now
    }

    private void OnVideoEnded(VideoPlayer vp)
    {
        Debug.Log("Video ended, starting final fade...");
        if (hasVideoEnded) return; // Prevent multiple calls

        hasVideoEnded = true;
        isVideoPlaying = false;

        StartCoroutine(HandleVideoEnd());
    }

    private IEnumerator HandleVideoEnd()
    {
        // Wait a brief moment to ensure video has fully stopped
        yield return new WaitForSeconds(0.1f);

        // Enable fade canvas to be on top
        if (fadeCanvas != null)
        {
            fadeCanvas.enabled = true;
        }

        // Start final fade to black
        yield return StartCoroutine(FadeToBlack());

        // Hide video surface
        if (videoSurface != null)
        {
            videoSurface.gameObject.SetActive(false);
        }

        // Load target scene
        LoadTargetScene();
    }

    // Fade IN to black (before video starts)
    private IEnumerator FadeIn()
    {
        if (fadeOverlay == null)
        {
            Debug.LogWarning("Fade overlay not assigned, skipping fade in effect");
            yield break;
        }

        // Enable raycast blocking during fade
        fadeOverlay.raycastTarget = true;

        float elapsedTime = 0f;
        Color startColor = new Color(0, 0, 0, 0); // Transparent
        Color endColor = new Color(0, 0, 0, 1);   // Solid black

        fadeOverlay.color = startColor;

        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float normalizedTime = elapsedTime / fadeInDuration;

            // Smooth fade curve
            float smoothStep = normalizedTime * normalizedTime * (3f - 2f * normalizedTime);
            fadeOverlay.color = Color.Lerp(startColor, endColor, smoothStep);

            yield return null;
        }

        // Ensure final color is exactly black
        fadeOverlay.color = endColor;
    }

    // Fade OUT from black (reveal the video)
    private IEnumerator FadeFromBlack()
    {
        if (fadeOverlay == null)
        {
            Debug.LogWarning("Fade overlay not assigned, skipping fade from black effect");
            yield break;
        }

        float elapsedTime = 0f;
        Color startColor = new Color(0, 0, 0, 1);   // Solid black
        Color endColor = new Color(0, 0, 0, 0);     // Transparent

        fadeOverlay.color = startColor;

        while (elapsedTime < fadeInDuration) // Use same duration as fade in
        {
            elapsedTime += Time.unscaledDeltaTime;
            float normalizedTime = elapsedTime / fadeInDuration;

            // Smooth fade curve
            float smoothStep = normalizedTime * normalizedTime * (3f - 2f * normalizedTime);
            fadeOverlay.color = Color.Lerp(startColor, endColor, smoothStep);

            yield return null;
        }

        // Ensure final color is transparent
        fadeOverlay.color = endColor;
        fadeOverlay.raycastTarget = false; // Re-enable input during video
    }

    // Fade TO black (after video ends)
    private IEnumerator FadeToBlack()
    {
        if (fadeOverlay == null)
        {
            Debug.LogWarning("Fade overlay not assigned, skipping fade out effect");
            yield break;
        }

        Debug.Log("Starting final fade to black...");

        // Enable raycast blocking during fade
        fadeOverlay.raycastTarget = true;

        float elapsedTime = 0f;
        Color startColor = new Color(0, 0, 0, 0); // Transparent
        Color endColor = new Color(0, 0, 0, 1);   // Solid black

        fadeOverlay.color = startColor;

        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float normalizedTime = elapsedTime / fadeOutDuration;

            // Smooth fade curve
            float smoothStep = normalizedTime * normalizedTime * (3f - 2f * normalizedTime);
            fadeOverlay.color = Color.Lerp(startColor, endColor, smoothStep);

            yield return null;
        }

        // Ensure final color is exactly black
        fadeOverlay.color = endColor;
        Debug.Log("Final fade complete!");
    }

    private void LoadTargetScene()
    {
        Debug.Log($"Loading scene {targetSceneIndex}...");
        // Validate scene index
        if (targetSceneIndex >= 0 && targetSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(targetSceneIndex);
        }
        else
        {
            Debug.LogError($"Invalid scene index: {targetSceneIndex}. Check Build Settings.");
        }
    }

    // Optional: Method to stop video manually
    public void StopVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            isVideoPlaying = false;
            hasVideoEnded = true;
        }

        if (videoSurface != null)
        {
            videoSurface.gameObject.SetActive(false);
        }

        isTransitioning = false;
    }

    // Cleanup
    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoEnded;
            videoPlayer.prepareCompleted -= OnVideoPrepared;
        }
    }

    // Optional: Skip video on ESC key (for testing)
    void Update()
    {
        if (isVideoPlaying && Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Video skipped by user");
            OnVideoEnded(videoPlayer);
        }
    }
}
