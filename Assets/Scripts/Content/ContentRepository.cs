using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class ContentRepository
{
    private readonly string resourcePath;

    public ContentRepository(string resourcePath = "Content/alphabet-pt-br")
    {
        this.resourcePath = resourcePath;
    }

    public LearningContentPack Load()
    {
        var asset = Resources.Load<TextAsset>(resourcePath);
        if (asset == null)
        {
            throw new InvalidOperationException("Pacote de conteúdo não encontrado em Resources/" + resourcePath + ".json");
        }

        return LoadFromJson(asset.text);
    }

    public static LearningContentPack LoadFromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new InvalidOperationException("JSON de conteúdo vazio.");
        }

        var pack = JsonUtility.FromJson<LearningContentPack>(json);
        Validate(pack);
        return pack;
    }

    public static void Validate(LearningContentPack pack)
    {
        if (pack == null)
        {
            throw new InvalidOperationException("Pacote de conteúdo inválido.");
        }

        if (string.IsNullOrWhiteSpace(pack.id))
        {
            throw new InvalidOperationException("O pacote de conteúdo precisa de um id.");
        }

        if (pack.items == null || pack.items.Length == 0)
        {
            throw new InvalidOperationException("O pacote de conteúdo não possui itens.");
        }

        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var targets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in pack.items)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.id) || string.IsNullOrWhiteSpace(item.target))
            {
                throw new InvalidOperationException("Todo item precisa de id e target.");
            }

            if (!ids.Add(item.id))
            {
                throw new InvalidOperationException("Id de conteúdo duplicado: " + item.id);
            }

            if (!targets.Add(item.target))
            {
                throw new InvalidOperationException("Target de conteúdo duplicado: " + item.target);
            }

            ValidateOptionalResources(item);
        }

        if (string.Equals(pack.version, "2.0.0", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(pack.id, "alphabet-pt-br", StringComparison.OrdinalIgnoreCase))
        {
            ValidateAlphabetV2(pack);
        }
    }

    public static LearningContentItem FindByTarget(LearningContentPack pack, string target)
    {
        Validate(pack);
        if (string.IsNullOrWhiteSpace(target))
        {
            return null;
        }

        foreach (var item in pack.items)
        {
            if (item != null && string.Equals(item.target, target, StringComparison.OrdinalIgnoreCase))
            {
                return item;
            }
        }

        return null;
    }

    private static void ValidateAlphabetV2(LearningContentPack pack)
    {
        if (pack.items.Length != 26)
        {
            throw new InvalidOperationException("O pacote 2.0 do alfabeto precisa preservar 26 itens.");
        }

        LearningContentItem letterA = null;
        for (var index = 0; index < pack.items.Length; index++)
        {
            var item = pack.items[index];
            var expectedTarget = ((char)('A' + index)).ToString();
            var expectedId = "letter-" + char.ToLowerInvariant(expectedTarget[0]);
            if (!string.Equals(item.target, expectedTarget, StringComparison.Ordinal) ||
                !string.Equals(item.id, expectedId, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("IDs e targets A-Z precisam manter ordem e identidade no schema 2.0.");
            }

            if (expectedTarget == "A")
            {
                letterA = item;
            }
            else if (string.IsNullOrWhiteSpace(item.title) || string.IsNullOrWhiteSpace(item.prefabResource))
            {
                throw new InvalidOperationException("A letra " + expectedTarget + " não possui fallback básico válido.");
            }
        }

        if (letterA == null ||
            !string.Equals(letterA.word, "Arara", StringComparison.OrdinalIgnoreCase) ||
            letterA.syllables == null || letterA.syllables.Length == 0 ||
            string.IsNullOrWhiteSpace(letterA.spokenPhrase) ||
            string.IsNullOrWhiteSpace(letterA.companionId) ||
            string.IsNullOrWhiteSpace(letterA.contentPrefabResource) ||
            !string.Equals(letterA.interactionType, "tap", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(letterA.interactionPrompt) ||
            string.IsNullOrWhiteSpace(letterA.interactionTargetName) ||
            string.IsNullOrWhiteSpace(letterA.completionRule) ||
            letterA.targetHoldSeconds < 0f)
        {
            throw new InvalidOperationException("A letra A não possui a configuração completa da vertical slice.");
        }
    }

    private static void ValidateOptionalResources(LearningContentItem item)
    {
        // Optional resources may be blank. When present they must be project-relative Resources paths.
        var paths = new[]
        {
            item.audioResource,
            item.prefabResource,
            item.companionPrefabResource,
            item.contentPrefabResource,
            item.narrationLetterResource,
            item.narrationWordResource,
            item.narrationPromptResource,
            item.narrationSuccessResource,
            item.contextualSoundResource
        };

        foreach (var path in paths)
        {
            if (!string.IsNullOrEmpty(path) && (path.StartsWith("/") || path.Contains("..")))
            {
                throw new InvalidOperationException("Caminho de recurso opcional inválido em " + item.id + ".");
            }
        }
    }
}
