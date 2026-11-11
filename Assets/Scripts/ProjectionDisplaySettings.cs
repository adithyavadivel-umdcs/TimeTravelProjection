using UnityEngine;

[DisallowMultipleComponent]
public sealed class ProjectionDisplaySettings : MonoBehaviour
{
    [SerializeField] private Camera viewCamera;
    [SerializeField, Min(320)] private int width = 1920;
    [SerializeField, Min(240)] private int height = 1080;
    [SerializeField] private bool fullscreen = true;
    [SerializeField, Range(20f, 120f)] private float fieldOfView = 60f;
    [SerializeField] private bool useSecondDisplay;
    [SerializeField, Min(1)] private int secondaryRefreshRate = 60;
    [SerializeField] private bool useVSync = true;
    [SerializeField, Min(1)] private int frameRateLimit = 60;

    private void Start()
    {
        ApplySettings();
    }

    [ContextMenu("Apply Projection Settings")]
    public void ApplySettings()
    {
        if (viewCamera == null)
        {
            Debug.LogError("ProjectionDisplaySettings: Assign View Camera.", this);
            return;
        }

        viewCamera.fieldOfView = Mathf.Clamp(fieldOfView, 20f, 120f);
        if (!Application.isPlaying)
            return;

        // With VSync enabled, the monitor refresh determines the frame rate.
        QualitySettings.vSyncCount = useVSync ? 1 : 0;
        Application.targetFrameRate = useVSync ? -1 : Mathf.Max(1, frameRateLimit);

#if UNITY_EDITOR
        Debug.Log("ProjectionDisplaySettings: FOV and frame settings applied. " +
            "Fullscreen and physical display selection require a standalone build.", this);
#else
        int displayIndex = 0;
        if (useSecondDisplay)
        {
            if (Display.displays.Length > 1)
            {
                displayIndex = 1;
                if (!Display.displays[1].active)
                {
#if UNITY_STANDALONE_WIN
                    Display.displays[1].Activate(Mathf.Max(320, width), Mathf.Max(240, height),
                        new RefreshRate { numerator = (uint)Mathf.Max(1, secondaryRefreshRate), denominator = 1 });
#else
                    Display.displays[1].Activate();
#endif
                }
            }
            else
            {
                Debug.LogWarning("ProjectionDisplaySettings: Second display unavailable; using primary display.", this);
            }
        }

        viewCamera.targetDisplay = displayIndex;
        if (displayIndex == 0)
        {
            Screen.SetResolution(Mathf.Max(320, width), Mathf.Max(240, height),
                fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
        }

        Debug.Log($"ProjectionDisplaySettings: Camera output uses display {displayIndex + 1}. " +
            $"Requested resolution {width}x{height}, FOV {fieldOfView}. " +
            "Secondary display activation settings take effect at launch; restart after changing them.", this);
#endif
    }
}
