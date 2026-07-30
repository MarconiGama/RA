using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class TapInteraction : MonoBehaviour, IContentInteraction
{
    [SerializeField] private Camera inputCamera;
    [SerializeField] private string interactionType = "tap";
    [SerializeField] private string targetName = "PLACEHOLDER_ARARA";

    private bool active;
    private bool fired;
    private float activatedAt;

    public event Action<InteractionResult> Completed;
    public event Action DemonstrationRequested;

    public bool IsActive { get { return active; } }
    public bool HasFired { get { return fired; } }

    public void Activate()
    {
        active = true;
        fired = false;
        activatedAt = Time.realtimeSinceStartup;
    }

    public void Deactivate()
    {
        active = false;
    }

    public void Demonstrate()
    {
        if (!active || fired)
        {
            return;
        }
        var handler = DemonstrationRequested;
        if (handler != null)
        {
            handler();
        }
    }

    public bool TryTrigger()
    {
        if (!active || fired)
        {
            return false;
        }
        fired = true;
        active = false;
        var duration = (long)Mathf.Max(0f, (Time.realtimeSinceStartup - activatedAt) * 1000f);
        var handler = Completed;
        if (handler != null)
        {
            handler(InteractionResult.Success(interactionType, targetName, duration));
        }
        return true;
    }

    private void OnMouseDown()
    {
        TryTrigger();
    }

    private void Update()
    {
        if (!active || fired || Input.touchCount == 0)
        {
            return;
        }
        var touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began)
        {
            return;
        }
        var cameraToUse = inputCamera == null ? Camera.main : inputCamera;
        if (cameraToUse == null)
        {
            return;
        }
        RaycastHit hit;
        if (Physics.Raycast(cameraToUse.ScreenPointToRay(touch.position), out hit) && hit.collider == GetComponent<Collider>())
        {
            TryTrigger();
        }
    }
}
