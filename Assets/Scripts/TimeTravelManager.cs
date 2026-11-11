using System;
using UnityEngine;
using UnityEngine.InputSystem;

// An enum gives each time period a name instead of using unexplained numbers.
public enum TimelineId
{
    Past,
    Future
}

[DisallowMultipleComponent]
public class TimeTravelManager : MonoBehaviour
{
    // One Inspector entry connects a timeline name to its parent GameObject.
    [Serializable]
    private class TimelineWorld
    {
        public TimelineId timeline;
        public GameObject root;

        public TimelineWorld(TimelineId timeline)
        {
            this.timeline = timeline;
        }
    }

    // SerializeField makes a private variable editable in the Unity Inspector.
    [SerializeField] private TimelineWorld[] timelines =
    {
        new TimelineWorld(TimelineId.Past),
        new TimelineWorld(TimelineId.Future)
    };

    // Other scripts can read these properties, but only this manager can change them.
    public TimelineId CurrentTimeline { get; private set; } = TimelineId.Past;
    public bool IsInitialized { get; private set; }
    // Other components subscribe to this notification when they need timeline updates.
    // Action<TimelineId> means each listener receives the new timeline name.
    public event Action<TimelineId> TimelineChanged;

    // Unity calls Start once when this component first begins running.
    private void Start()
    {
        if (!SetTimeline(TimelineId.Past))
        {
            Debug.LogError("TimeTravelManager: Cannot start. Check the timeline assignments.", this);
            enabled = false;
        }
    }

    // Unity calls Update once every frame. Check whether T was pressed this frame.
    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            SwitchToNextTimeline();
        }
    }

    private void SwitchToNextTimeline()
    {
        if (!ValidateConfiguration())
        {
            return;
        }

        int currentIndex = FindTimelineIndex(CurrentTimeline);
        int nextIndex = currentIndex + 1;

        // After the final entry, start again at the first entry.
        if (nextIndex >= timelines.Length)
        {
            nextIndex = 0;
        }

        SetTimeline(timelines[nextIndex].timeline);
    }

    private int FindTimelineIndex(TimelineId timeline)
    {
        for (int i = 0; i < timelines.Length; i++)
        {
            if (timelines[i].timeline == timeline)
            {
                return i;
            }
        }

        // -1 means no matching entry was found.
        return -1;
    }

    public bool SetTimeline(TimelineId timeline)
    {
        if (!ValidateConfiguration())
        {
            return false;
        }

        int targetIndex = FindTimelineIndex(timeline);
        if (targetIndex < 0)
        {
            Debug.LogError($"TimeTravelManager: No world assigned for {timeline}.", this);
            return false;
        }

        if (IsInitialized && CurrentTimeline == timeline)
        {
            return true;
        }

        // Disable other worlds first so their renderers, colliders, and scripts stop together.
        for (int i = 0; i < timelines.Length; i++)
        {
            if (i != targetIndex)
            {
                timelines[i].root.SetActive(false);
            }
        }

        CurrentTimeline = timeline;
        timelines[targetIndex].root.SetActive(true);
        IsInitialized = true;
        Debug.Log($"TimeTravelManager: Active timeline = {CurrentTimeline}.", this);
        // Notify any components that are currently listening.
        if (TimelineChanged != null)
        {
            TimelineChanged(CurrentTimeline);
        }
        return true;
    }

    // Check Inspector assignments before changing which world is active.
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
