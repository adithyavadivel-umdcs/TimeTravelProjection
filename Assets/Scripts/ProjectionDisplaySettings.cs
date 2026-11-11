using UnityEngine;

[DisallowMultipleComponent]
public class ProjectionDisplaySettings : MonoBehaviour
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
        {
            return;
        }

        ApplyFrameRateSettings();

        // UNITY_EDITOR is defined only when this code is compiled for the Editor.
        // The standalone build compiles the other branch instead.
#if UNITY_EDITOR
        Debug.Log("ProjectionDisplaySettings: FOV and frame settings applied. " +
            "Fullscreen and physical display selection require a standalone build.", this);
#else
        ApplyDisplaySettings();
#endif
    }

    private void ApplyFrameRateSettings()
    {
        // VSync follows the monitor refresh rate. Otherwise use our own frame limit.
        if (useVSync)
        {
            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = -1;
        }
        else
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = Mathf.Max(1, frameRateLimit);
        }
    }

    private void ApplyDisplaySettings()
    {
        int displayIndex = 0;
        int safeWidth = Mathf.Max(320, width);
        int safeHeight = Mathf.Max(240, height);

        if (useSecondDisplay)
        {
            if (Display.displays.Length > 1)
            {
                displayIndex = 1;
                ActivateSecondDisplay(safeWidth, safeHeight);
            }
            else
            {
                Debug.LogWarning("ProjectionDisplaySettings: Second display unavailable; using primary display.", this);
            }
        }

        // Unity counts displays starting at 0: primary = 0, second = 1.
        viewCamera.targetDisplay = displayIndex;
        if (displayIndex == 0)
        {
            FullScreenMode mode = FullScreenMode.Windowed;
            if (fullscreen)
            {
                mode = FullScreenMode.FullScreenWindow;
            }

            Screen.SetResolution(safeWidth, safeHeight, mode);
        }

        Debug.Log($"ProjectionDisplaySettings: Camera output uses display {displayIndex + 1}. " +
            $"Requested resolution {width}x{height}, FOV {fieldOfView}. " +
            "Restart after changing secondary display activation settings.", this);
    }

    private void ActivateSecondDisplay(int requestedWidth, int requestedHeight)
    {
        // An already-active display cannot be activated again with new settings.
        if (Display.displays[1].active)
        {
            return;
        }

#if UNITY_STANDALONE_WIN
        RefreshRate refreshRate = new RefreshRate();
        refreshRate.numerator = (uint)Mathf.Max(1, secondaryRefreshRate);
        refreshRate.denominator = 1;
        Display.displays[1].Activate(requestedWidth, requestedHeight, refreshRate);
#else
        // On macOS and Linux, use the external display's native settings.
        Display.displays[1].Activate();
#endif
    }
}
