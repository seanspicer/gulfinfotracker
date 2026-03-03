using System.Text.Json;
using Anthropic.SDK;
using Anthropic.SDK.Messaging;
using GulfInfoTracker.Api.Data.Entities;

namespace GulfInfoTracker.Api.AI;

/// <summary>
/// Two-agent Claude pipeline:
/// 1. ExtractionAgent (claude-opus-4-6) - extracts named entities, factual claims, consistency rating
/// 2. ScoringAgent (claude-opus-4-6) - produces credibility score (0-100), reasoning, and topic tags
/// </summary>
public class ClaudeCredibilityPipeline(
    AnthropicClient claude,
    ILogger<ClaudeCredibilityPipeline> logger) : ICredibilityPipeline
{
    private const string ExtractionModel = "claude-opus-4-6";
    private const string ScoringModel    = "claude-opus-4-6";

    public async Task<ScoringResult> ScoreAsync(Article article, int corroborationCount, CancellationToken ct = default)
    {
        var extraction = await RunExtractionAsync(article, ct);
        var scoring    = await RunScoringAsync(article, extraction, corroborationCount, ct);
        return scoring;
    }

    private async Task<ExtractionOutput> RunExtractionAsync(Article article, CancellationToken ct)
    {
        var systemPrompt = """
            You are a factual extraction agent. Given a news article, extract:
            1. Named entities (people, organisations, locations, events) as a JSON array of strings
            2. Key factual claims (up to 5) as a JSON array of strings
            3. Internal consistency rating: "high", "medium", or "low"

            Respond ONLY with valid JSON in this exact format:
            {
              "namedEntities": ["entity1", "entity2"],
              "factualClaims": ["claim1", "claim2"],
              "consistencyRating": "high"
            }
            """;

        var userMessage = $"""
            Article headline: {article.HeadlineEn}
            Article content: {article.SummaryEn ?? "(no content)"}
            Source: {article.PluginId} ({article.Country})
            """;

        var response = await claude.Messages.GetClaudeMessageAsync(new MessageParameters
        {
            Model = ExtractionModel,
            MaxTokens = 1024,
            System = [new SystemMessage(systemPrompt)],
            Messages = [new Message(RoleType.User, userMessage)],
        }, ct);

        var text = response.Content.OfType<TextContent>().FirstOrDefault()?.Text ?? "{}";

        try
        {
            return JsonSerializer.Deserialize<ExtractionOutput>(text,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new ExtractionOutput();
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Failed to parse extraction JSON for article {Id}", article.Id);
            return new ExtractionOutput();
        }
    }

    private async Task<ScoringResult> RunScoringAsync(
        Article article,
        ExtractionOutput extraction,
        int corroborationCount,
        CancellationToken ct)
    {
        var systemPrompt = """
            You are a credibility scoring agent for a Gulf-region news aggregator.
            Score articles 0-100 based on:
            - Source authority (government sources score higher than editorial; tier-1 outlets score higher than unknown)
            - Presence of verifiable factual claims
            - Cross-source corroboration (higher count = higher score)
            - Internal consistency of the article
            - Recency and specificity

            Also assign 1-3 topic IDs from: T1 (Politics & Government), T2 (Economy & Finance),
            T3 (Energy & Oil), T4 (Business & Investment), T5 (Iran/Israel/US Conflict)

            Respond ONLY with valid JSON in this exact format:
            {
              "score": 75,
              "reasoning": "Two to four sentence explanation of the score.",
              "topicIds": ["T1"]
            }
            """;

        var userMessage = $"""
            Article headline: {article.HeadlineEn}
            Article content: {article.SummaryEn ?? "(no content)"}
            Source plugin: {article.PluginId}
            Country: {article.Country}
            Corroborating sources (same event, ±24h): {corroborationCount}

            Extraction results:
            Named entities: {JsonSerializer.Serialize(extraction.NamedEntities)}
            Factual claims: {JsonSerializer.Serialize(extraction.FactualClaims)}
            Internal consistency: {extraction.ConsistencyRating}
            """;

        var response = await claude.Messages.GetClaudeMessageAsync(new MessageParameters
        {
            Model = ScoringModel,
            MaxTokens = 512,
            System = [new SystemMessage(systemPrompt)],
            Messages = [new Message(RoleType.User, userMessage)],
        }, ct);

        var text = response.Content.OfType<TextContent>().FirstOrDefault()?.Text ?? "{}";

        try
        {
            var output = JsonSerializer.Deserialize<ScoringOutput>(text,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (output is null)
                throw new JsonException("Null scoring output");

            var namedEntitiesJson = JsonSerializer.Serialize(extraction.NamedEntities);

            return new ScoringResult(
                Score: Math.Clamp(output.Score, 0, 100),
                Reasoning: output.Reasoning ?? string.Empty,
                TopicIds: output.TopicIds ?? [],
                NamedEntitiesJson: namedEntitiesJson
            );
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Failed to parse scoring JSON for article {Id}", article.Id);
            throw; // Let ScoringBackgroundService handle retry
        }
    }

    private sealed class ExtractionOutput
    {
        public List<string> NamedEntities { get; set; } = [];
        public List<string> FactualClaims { get; set; } = [];
        public string ConsistencyRating { get; set; } = "medium";
    }

    private sealed class ScoringOutput
    {
        public int Score { get; set; }
        public string? Reasoning { get; set; }
        public List<string>? TopicIds { get; set; }
    }
}
