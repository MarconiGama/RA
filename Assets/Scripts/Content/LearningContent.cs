using System;

[Serializable]
public sealed class LearningContentPack
{
    public string id;
    public string title;
    public string language;
    public string version;
    public LearningContentItem[] items;
}

[Serializable]
public sealed class LearningContentItem
{
    public string id;
    public string target;
    public string title;
    public string description;
    public string audioResource;
    public string prefabResource;
    public string category;

    // Engagement schema 2.0 fields. Kept flat for Unity 2019 JsonUtility.
    public string letterName;
    public string word;
    public string[] syllables;
    public string spokenPhrase;
    public string initialSoundExample;
    public string educatorNote;
    public string companionId;
    public string companionPrefabResource;
    public string contentPrefabResource;
    public string narrationLetterResource;
    public string narrationWordResource;
    public string narrationPromptResource;
    public string narrationSuccessResource;
    public string contextualSoundResource;
    public string interactionType;
    public string interactionPrompt;
    public string interactionTargetName;
    public string entranceAnimation;
    public string idleAnimation;
    public string interactionAnimation;
    public string successAnimation;
    public string completionRule;
    public float targetHoldSeconds;
    public string sensoryIntensity;
    public string contentTier;
    public bool pedagogicalReviewRequired;

    public bool HasExtendedExperience
    {
        get { return !string.IsNullOrWhiteSpace(interactionType); }
    }

    public string EffectiveLetterName
    {
        get { return string.IsNullOrWhiteSpace(letterName) ? target : letterName; }
    }

    public string EffectiveWord
    {
        get { return string.IsNullOrWhiteSpace(word) ? title : word; }
    }
}
