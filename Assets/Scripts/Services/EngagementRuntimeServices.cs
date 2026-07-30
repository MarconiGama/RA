public static class EngagementRuntimeServices
{
    private static TargetTrackingService tracking;
    private static SensorySettingsService sensory;
    private static ProgressService progress;
    private static LocalTelemetryService telemetry;

    public static TargetTrackingService Tracking
    {
        get { return tracking ?? (tracking = new TargetTrackingService()); }
    }

    public static SensorySettingsService Sensory
    {
        get { return sensory ?? (sensory = new SensorySettingsService()); }
    }

    public static ProgressService Progress
    {
        get { return progress ?? (progress = new ProgressService()); }
    }

    public static LocalTelemetryService Telemetry
    {
        get { return telemetry ?? (telemetry = new LocalTelemetryService()); }
    }

    public static void ResetForTests()
    {
        tracking = new TargetTrackingService();
        sensory = new SensorySettingsService(false);
        progress = new ProgressService();
        telemetry = new LocalTelemetryService();
    }
}
