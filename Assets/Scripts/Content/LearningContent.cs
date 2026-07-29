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
}
