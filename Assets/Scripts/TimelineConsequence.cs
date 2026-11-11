using UnityEngine;

// Keep this component active when its target is hidden. It can live under FutureWorld.
[DisallowMultipleComponent]
public sealed class TimelineConsequence : MonoBehaviour
{
    [SerializeField] private TimeTravelState state;
    [SerializeField] private string interactionKey = "test-interaction";
    [SerializeField] private GameObject target;
    [SerializeField] private bool activeWhenRecorded = true;

    private void OnEnable()
    {
        if (state == null || target == null || string.IsNullOrWhiteSpace(interactionKey))
        {
            Debug.LogError("TimelineConsequence: Assign State, Target, and a nonempty Interaction Key.", this);
            enabled = false;
            return;
        }

        if (transform.IsChildOf(target.transform) || state.transform.IsChildOf(target.transform))
        {
            Debug.LogError("TimelineConsequence: Target must not contain its controller or State.", this);
            enabled = false;
            return;
        }

        state.FlagRecorded += OnFlagRecorded;
        // Refresh even if the flag changed while this timeline was inactive.
        Refresh();
    }

    private void OnDisable()
    {
        if (state != null)
            state.FlagRecorded -= OnFlagRecorded;
    }

    private void OnFlagRecorded(string key)
    {
        if (key == interactionKey)
            Refresh();
    }

    private void Refresh()
    {
        if (target != null && state != null)
            target.SetActive(state.HasFlag(interactionKey) == activeWhenRecorded);
    }
}
