using System;

public interface ITargetTrackingService
{
    event Action<string> TargetFound;
    event Action<string> TargetLost;
}

public sealed class TargetTrackingService : ITargetTrackingService
{
    public event Action<string> TargetFound;
    public event Action<string> TargetLost;

    public void NotifyFound(string targetName)
    {
        if (!string.IsNullOrWhiteSpace(targetName))
        {
            var handler = TargetFound;
            if (handler != null)
            {
                handler(targetName);
            }
        }
    }

    public void NotifyLost(string targetName)
    {
        if (!string.IsNullOrWhiteSpace(targetName))
        {
            var handler = TargetLost;
            if (handler != null)
            {
                handler(targetName);
            }
        }
    }
}
