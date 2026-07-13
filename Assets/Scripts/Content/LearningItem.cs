using System;

namespace RealidadeA.Content
{
    [Serializable]
    public sealed class LearningItem
    {
        public string id;
        public string targetName;
        public string title;
        public string description;
        public string audioResource;
        public string modelResource;
        public string category;
    }

    [Serializable]
    public sealed class LearningPack
    {
        public string id;
        public string version;
        public string language;
        public string title;
        public LearningItem[] items;
    }
}
