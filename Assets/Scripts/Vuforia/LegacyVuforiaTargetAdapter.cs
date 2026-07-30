using UnityEngine;
using Vuforia;

[RequireComponent(typeof(TrackableBehaviour))]
public sealed class LegacyVuforiaTargetAdapter : MonoBehaviour, ITrackableEventHandler
{
    [SerializeField] private string targetName = "A";

    private TrackableBehaviour trackable;
    private bool reportedFound;

    public string TargetName { get { return targetName; } }

    private void Awake()
    {
        trackable = GetComponent<TrackableBehaviour>();
        if (trackable != null && !string.IsNullOrWhiteSpace(trackable.TrackableName))
        {
            targetName = trackable.TrackableName;
        }
    }

    private void OnEnable()
    {
        if (trackable == null) trackable = GetComponent<TrackableBehaviour>();
        if (trackable != null) trackable.RegisterTrackableEventHandler(this);
    }

    public void OnTrackableStateChanged(
        TrackableBehaviour.Status previousStatus,
        TrackableBehaviour.Status newStatus)
    {
        var found = newStatus == TrackableBehaviour.Status.DETECTED ||
                    newStatus == TrackableBehaviour.Status.TRACKED ||
                    newStatus == TrackableBehaviour.Status.EXTENDED_TRACKED;
        if (found && !reportedFound)
        {
            reportedFound = true;
            EngagementRuntimeServices.Tracking.NotifyFound(targetName);
        }
        else if (!found && reportedFound)
        {
            reportedFound = false;
            EngagementRuntimeServices.Tracking.NotifyLost(targetName);
        }
    }

    private void OnDisable()
    {
        if (trackable != null) trackable.UnregisterTrackableEventHandler(this);
        if (reportedFound)
        {
            reportedFound = false;
            EngagementRuntimeServices.Tracking.NotifyLost(targetName);
        }
    }
}
