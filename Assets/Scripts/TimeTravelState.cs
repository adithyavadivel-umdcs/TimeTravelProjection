using System;
using System.Collections.Generic;
using UnityEngine;

// Place on an always-active object outside the timeline roots.
// State lasts for this Play session; disk saving can be added separately.
[DisallowMultipleComponent]
public class TimeTravelState : MonoBehaviour
{
    // Each string is the key of an interaction that has already happened.
    private List<string> flags = new List<string>();
    // Listeners receive the key whenever a new interaction is recorded.
    public event Action<string> FlagRecorded;

    public bool HasFlag(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        return flags.Contains(key);
    }

    public void RecordFlag(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            Debug.LogError("TimeTravelState: Interaction key cannot be empty.", this);
            return;
        }

        // Do not record the same interaction twice.
        if (flags.Contains(key))
        {
            return;
        }

        flags.Add(key);
        Debug.Log($"TimeTravelState: Recorded '{key}'.", this);

        if (FlagRecorded != null)
        {
            FlagRecorded(key);
        }
    }
}
