using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public sealed class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private Camera viewCamera;
    [SerializeField, Min(0.1f)] private float interactionDistance = 3f;
    [SerializeField] private LayerMask interactionLayers = ~0;

    private void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.eKey.wasPressedThisFrame)
            return;

        if (viewCamera == null)
        {
            Debug.LogError("PlayerInteractor: Assign View Camera.", this);
            return;
        }

        Ray ray = viewCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance,
            interactionLayers, QueryTriggerInteraction.Ignore))
        {
            TimelineInteraction interaction = hit.collider.GetComponentInParent<TimelineInteraction>();
            if (interaction != null && interaction.isActiveAndEnabled)
            {
                interaction.Interact();
                return;
            }
        }

        Debug.Log("PlayerInteractor: No interactable object in reach at the center of the camera view.", this);
    }
}
