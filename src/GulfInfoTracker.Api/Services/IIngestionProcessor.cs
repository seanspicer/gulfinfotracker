using GulfInfoTracker.Plugins.Abstractions;

namespace GulfInfoTracker.Api.Services;

public interface IIngestionProcessor
{
    Task IngestAsync(ArticleCandidate candidate, CancellationToken ct = default);
    Task IngestPluginAsync(ISourcePlugin plugin, CancellationToken ct = default);
}
