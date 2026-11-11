using System;
using System.Collections.Generic;
using UnityEngine;

// Place on an always-active object outside the timeline roots.
// State lasts for this Play session; disk saving can be added separately.
[DisallowMultipleComponent]
public sealed class TimeTravelState : MonoBehaviour
{
    private readonly HashSet<string> flags = new HashSet<string>(StringComparer.Ordinal);
    public event Action<string> FlagRecorded;

    public bool HasFlag(string key) => !string.IsNullOrWhiteSpace(key) && flags.Contains(key);

    public void RecordFlag(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            Debug.LogError("TimeTravelState: Interaction key cannot be empty.", this);
            return;
        }

        if (flags.Add(key))
        {
            Debug.Log($"TimeTravelState: Recorded '{key}'.", this);
            FlagRecorded?.Invoke(key);
        }
    }
}
