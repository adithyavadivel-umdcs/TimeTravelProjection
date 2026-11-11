using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum TimelineId
{
    Past,
    Future
}

[DisallowMultipleComponent]
public sealed class TimeTravelManager : MonoBehaviour
{
    [Serializable]
    private sealed class TimelineWorld
    {
        public TimelineId timeline;
        public GameObject root;

        public TimelineWorld(TimelineId timeline)
        {
            this.timeline = timeline;
        }
    }

    [SerializeField] private TimelineWorld[] timelines =
    {
        new TimelineWorld(TimelineId.Past),
        new TimelineWorld(TimelineId.Future)
    };

    public TimelineId CurrentTimeline { get; private set; } = TimelineId.Past;
    public bool IsInitialized { get; private set; }
    public event Action<TimelineId> TimelineChanged;

    private void Start()
    {
        if (!SetTimeline(TimelineId.Past))
        {
            Debug.LogError("TimeTravelManager: Cannot start. Check the timeline assignments.", this);
            enabled = false;
        }
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
        {
            int currentIndex = Array.FindIndex(timelines, entry => entry.timeline == CurrentTimeline);
            SetTimeline(timelines[(currentIndex + 1) % timelines.Length].timeline);
        }
    }

    public bool SetTimeline(TimelineId timeline)
    {
        if (!ValidateConfiguration())
            return false;

        int targetIndex = Array.FindIndex(timelines, entry => entry.timeline == timeline);
        if (targetIndex < 0)
        {
            Debug.LogError($"TimeTravelManager: No world assigned for {timeline}.", this);
            return false;
        }

        if (IsInitialized && CurrentTimeline == timeline)
            return true;

        // Disable other worlds first so their renderers, colliders, and scripts stop together.
        for (int i = 0; i < timelines.Length; i++)
        {
            if (i != targetIndex)
                timelines[i].root.SetActive(false);
        }

        CurrentTimeline = timeline;
        timelines[targetIndex].root.SetActive(true);
        IsInitialized = true;
        Debug.Log($"TimeTravelManager: Active timeline = {CurrentTimeline}.", this);
        TimelineChanged?.Invoke(CurrentTimeline);
        return true;
    }

    private bool ValidateConfiguration()
    {
        if (timelines == null || timelines.Length < 2)
        {
            Debug.LogError("TimeTravelManager: Assign at least two timelines.", this);
            return false;
        }

        for (int i = 0; i < timelines.Length; i++)
        {
            if (timelines[i] == null || timelines[i].root == null)
            {
                Debug.LogError($"TimeTravelManager: Timeline entry {i} needs a world root.", this);
                return false;
            }

            Transform root = timelines[i].root.transform;
            if (transform.IsChildOf(root) ||
                (Camera.main != null && Camera.main.transform.IsChildOf(root)))
            {
                Debug.LogError("TimeTravelManager: Keep the manager and Main Camera outside all world roots.", this);
                return false;
            }

            for (int j = 0; j < i; j++)
            {
                Transform otherRoot = timelines[j].root.transform;
                if (timelines[i].timeline == timelines[j].timeline ||
                    root.IsChildOf(otherRoot) || otherRoot.IsChildOf(root))
                {
                    Debug.LogError("TimeTravelManager: Use unique timelines and separate, unnested world roots.", this);
                    return false;
                }
            }
        }

        return true;
    }
}
