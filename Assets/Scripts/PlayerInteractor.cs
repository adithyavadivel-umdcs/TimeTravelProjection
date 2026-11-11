using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private Camera viewCamera;
    [SerializeField, Min(0.1f)] private float interactionDistance = 3f;
    [SerializeField] private LayerMask interactionLayers = ~0;

    private void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.eKey.wasPressedThisFrame)
        {
            return;
        }

        if (viewCamera == null)
        {
            Debug.LogError("PlayerInteractor: Assign View Camera.", this);
            return;
        }

        // A ray is an invisible line. This one starts at the center of the camera view.
        Ray ray = viewCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        bool hitSomething = Physics.Raycast(ray, out hit, interactionDistance,
            interactionLayers, QueryTriggerInteraction.Ignore);

        if (!hitSomething)
        {
            Debug.Log("PlayerInteractor: No object in reach at the center of the camera view.", this);
            return;
        }

        // A collider may be on a child, so also search its parents for this component.
        TimelineInteraction interaction = hit.collider.GetComponentInParent<TimelineInteraction>();
        if (interaction == null || !interaction.isActiveAndEnabled)
        {
            Debug.Log($"PlayerInteractor: Hit '{hit.collider.name}', but it has no enabled TimelineInteraction.", this);
            return;
        }

        interaction.Interact();
    }
}
