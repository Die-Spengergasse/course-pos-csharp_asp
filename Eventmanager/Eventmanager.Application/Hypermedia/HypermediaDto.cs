using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Hypermedia;

public abstract record HypermediaDto
{
    private Dictionary<string, HypermediaLink> _links = new();

    [JsonInclude]
    public IReadOnlyDictionary<string, HypermediaLink> Links => _links.AsReadOnly();

    // Interne oder geschützte Methode, damit die Extensions Links hinzufügen können
    internal void AddLinkInternal(string rel, HypermediaLink link)
    {
        _links[rel] = link; // Nutzt den Indexer, um Abstürze bei doppelten Keys zu vermeiden
    }
}
