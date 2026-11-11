using System;
using UnityEngine;
using UnityEngine.Events;

// Keep this controller active; assign a separate child as the target it hides or moves.
[DisallowMultipleComponent]
public sealed class TimelineDependentObject : MonoBehaviour
{
    [Serializable]
    private sealed class TimelineState
    {
        public TimelineId timeline;
        public bool exists = true;
        public bool applyLocalTransform;
        public Vector3 localPosition;
        public Vector3 localEulerAngles;
        public UnityEvent onApplied = new UnityEvent();

        public TimelineState(TimelineId timeline)
        {
            this.timeline = timeline;
        }
    }

    [SerializeField] private TimeTravelManager manager;
    [SerializeField] private GameObject target;
    [SerializeField] private TimelineState[] states =
    {
        new TimelineState(TimelineId.Past),
        new TimelineState(TimelineId.Future)
    };

    private void OnEnable()
    {
        if (!ValidateConfiguration())
        {
            enabled = false;
            return;
        }

        manager.TimelineChanged += ApplyTimeline;
        // Handles components enabled after the manager's initial timeline event.
        if (manager.IsInitialized)
            ApplyTimeline(manager.CurrentTimeline);
    }

    private void OnDisable()
    {
        if (manager != null)
            manager.TimelineChanged -= ApplyTimeline;
    }

    private void ApplyTimeline(TimelineId timeline)
    {
        if (target == null)
        {
            Debug.LogWarning("TimelineDependentObject: Target was removed; skipping update.", this);
            return;
        }

        TimelineState state = Array.Find(states, entry => entry.timeline == timeline);
        // A missing entry also means this object does not exist in that timeline.
        if (state == null || !state.exists)
        {
            target.SetActive(false);
            return;
        }

        if (state.applyLocalTransform)
        {
            target.transform.localPosition = state.localPosition;
            target.transform.localRotation = Quaternion.Euler(state.localEulerAngles);
        }

        // SetActive controls child renderers, colliders, and behaviours together.
        // Individual collider.enabled settings are preserved.
        target.SetActive(true);
        state.onApplied?.Invoke();
    }

    private bool ValidateConfiguration()
    {
        if (manager == null || target == null || states == null)
        {
            Debug.LogError("TimelineDependentObject: Assign Manager, Target, and States.", this);
            return false;
        }

        if (transform.IsChildOf(target.transform) || manager.transform.IsChildOf(target.transform))
        {
            Debug.LogError("TimelineDependentObject: Target must not contain its controller or the manager.", this);
            return false;
        }

        for (int i = 0; i < states.Length; i++)
        {
            if (states[i] == null)
            {
                Debug.LogError($"TimelineDependentObject: State {i} is empty.", this);
                return false;
            }

            for (int j = 0; j < i; j++)
            {
                if (states[i].timeline == states[j].timeline)
                {
                    Debug.LogError("TimelineDependentObject: Use one state per timeline.", this);
                    return false;
                }
            }
        }

        return true;
    }
}
