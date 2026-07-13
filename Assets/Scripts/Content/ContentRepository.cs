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

        var pack = JsonUtility.FromJson<LearningContentPack>(asset.text);
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
        }
    }
}
