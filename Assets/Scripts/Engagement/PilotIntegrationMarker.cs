using UnityEngine;

public sealed class PilotIntegrationMarker : MonoBehaviour
{
    public string integratedTarget = "A";
    public string disabledLegacyObjectName;
    public bool originalTransformPreserved = true;
    public bool originalObjectDeleted;
}
