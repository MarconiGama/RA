using System;

[Serializable]
public sealed class InteractionResult
{
    public bool succeeded;
    public string interactionType;
    public string targetName;
    public long durationMs;

    public static InteractionResult Success(string interactionType, string targetName, long durationMs)
    {
        return new InteractionResult
        {
            succeeded = true,
            interactionType = interactionType ?? string.Empty,
            targetName = targetName ?? string.Empty,
            durationMs = durationMs < 0 ? 0 : durationMs
        };
    }
}
