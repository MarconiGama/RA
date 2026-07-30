public sealed class LetterExperienceContext
{
    public LearningContentItem Content { get; private set; }
    public TargetTrackingService Tracking { get; private set; }
    public SensorySettingsService Sensory { get; private set; }
    public AudioCueService Audio { get; private set; }
    public InteractionCoordinator Interaction { get; private set; }
    public LetterExperienceHud Hud { get; private set; }
    public SubtitlePresenter Subtitles { get; private set; }
    public ExperienceStepIndicator StepIndicator { get; private set; }
    public PlaceholderMotionController LumiMotion { get; private set; }
    public PlaceholderMotionController ContentMotion { get; private set; }
    public ProgressService Progress { get; private set; }
    public LocalTelemetryService Telemetry { get; private set; }
    public string ProfileId { get; private set; }

    public LetterExperienceContext(
        LearningContentItem content,
        TargetTrackingService tracking,
        SensorySettingsService sensory,
        AudioCueService audio,
        InteractionCoordinator interaction,
        LetterExperienceHud hud,
        SubtitlePresenter subtitles,
        ExperienceStepIndicator stepIndicator,
        PlaceholderMotionController lumiMotion,
        PlaceholderMotionController contentMotion,
        ProgressService progress,
        LocalTelemetryService telemetry,
        string profileId = "local")
    {
        Content = content;
        Tracking = tracking ?? EngagementRuntimeServices.Tracking;
        Sensory = sensory ?? EngagementRuntimeServices.Sensory;
        Audio = audio;
        Interaction = interaction;
        Hud = hud;
        Subtitles = subtitles;
        StepIndicator = stepIndicator;
        LumiMotion = lumiMotion;
        ContentMotion = contentMotion;
        Progress = progress ?? EngagementRuntimeServices.Progress;
        Telemetry = telemetry ?? EngagementRuntimeServices.Telemetry;
        ProfileId = string.IsNullOrWhiteSpace(profileId) ? "local" : profileId;
    }
}
