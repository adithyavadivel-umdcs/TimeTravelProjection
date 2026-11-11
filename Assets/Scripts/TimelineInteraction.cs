using UnityEngine;

[DisallowMultipleComponent]
public class TimelineInteraction : MonoBehaviour
{
    [SerializeField] private TimeTravelManager manager;
    [SerializeField] private TimeTravelState state;
    [SerializeField] private TimelineId requiredTimeline = TimelineId.Past;
    // Use the same key on the future TimelineConsequence component.
    [SerializeField] private string interactionKey = "test-interaction";

    // PlayerInteractor calls this after the camera ray hits this object.
    public void Interact()
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        if (manager == null || state == null)
        {
            Debug.LogError("TimelineInteraction: Assign Manager and State.", this);
            return;
        }

        if (!manager.IsInitialized || manager.CurrentTimeline != requiredTimeline)
        {
            Debug.Log($"TimelineInteraction: Requires {requiredTimeline}.", this);
            return;
        }

        if (state.HasFlag(interactionKey))
        {
            Debug.Log($"TimelineInteraction: '{interactionKey}' was already recorded.", this);
        }
        else
        {
            state.RecordFlag(interactionKey);
        }
    }
}
