namespace GulfInfoTracker.Plugins.Abstractions;

public interface ISourcePlugin
{
    string PluginId { get; }
    string DisplayName { get; }
    string Country { get; }
    string Type { get; }  // "government" or "news"
    Task<IReadOnlyList<RawArticle>> FetchAsync(CancellationToken ct = default);
    ArticleCandidate? ParseArticle(RawArticle raw);
}
