using System.Text.Json;
using GulfInfoTracker.Api.Data.Entities;
using OpenAI;
using OpenAI.Chat;

namespace GulfInfoTracker.Api.AI;

/// <summary>
/// Two-step OpenAI credibility pipeline using gpt-4.1-nano with JSON schema structured outputs.
/// 1. ExtractionAgent — named entities, factual claims, consistency rating
/// 2. ScoringAgent — credibility score (0-100), reasoning, topic tags
/// </summary>
public class OpenAiCredibilityPipeline(
    OpenAIClient openAi,
    IConfiguration config,
    ILogger<OpenAiCredibilityPipeline> logger) : ICredibilityPipeline
{
    private string CredibilityModel => config["OpenAi:CredibilityModel"] ?? "gpt-4.1-nano";

    private static readonly ChatResponseFormat ExtractionFormat =
        ChatResponseFormat.CreateJsonSchemaFormat(
            "extraction_output",
            BinaryData.FromString("""
                {
                  "type": "object",
                  "properties": {
                    "namedEntities":    { "type": "array", "items": { "type": "string" } },
                    "factualClaims":    { "type": "array", "items": { "type": "string" } },
                    "consistencyRating":{ "type": "string" }
                  },
                  "required": ["namedEntities", "factualClaims", "consistencyRating"],
                  "additionalProperties": false
                }
                """),
            null, true);

    private static readonly ChatResponseFormat ScoringFormat =
        ChatResponseFormat.CreateJsonSchemaFormat(
            "scoring_output",
            BinaryData.FromString("""
                {
                  "type": "object",
                  "properties": {
                    "score":    { "type": "integer" },
                    "reasoning":{ "type": "string"  },
                    "topicIds": { "type": "array", "items": { "type": "string", "enum": ["T1","T2","T3","T4","T5"] } }
                  },
                  "required": ["score", "reasoning", "topicIds"],
                  "additionalProperties": false
                }
                """),
            null, true);

    public async Task<ScoringResult> ScoreAsync(Article article, int corroborationCount, CancellationToken ct = default)
    {
        var extraction = await RunExtractionAsync(article, ct);
        return await RunScoringAsync(article, extraction, corroborationCount, ct);
    }

    private async Task<ExtractionOutput> RunExtractionAsync(Article article, CancellationToken ct)
    {
        var systemPrompt = """
            You are a factual extraction agent. Given a news article, extract:
            1. Named entities (people, organisations, locations, events)
            2. Key factual claims (up to 5)
            3. Internal consistency rating: "high", "medium", or "low"
            """;

        var userMessage = $"""
            Article headline: {article.HeadlineEn}
            Article content: {article.SummaryEn ?? "(no content)"}
            Source: {article.PluginId} ({article.Country})
            """;

        var chatClient = openAi.GetChatClient(CredibilityModel);
        var response = await chatClient.CompleteChatAsync(
            [
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userMessage),
            ],
            new ChatCompletionOptions { MaxOutputTokenCount = 1024, ResponseFormat = ExtractionFormat },
            ct);

        try
        {
            return JsonSerializer.Deserialize<ExtractionOutput>(
                response.Value.Content[0].Text,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new ExtractionOutput();
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Failed to deserialize extraction output for article {Id}", article.Id);
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

            Assign 1-3 topic IDs from: T1 (Politics & Government), T2 (Economy & Finance),
            T3 (Energy & Oil), T4 (Business & Investment), T5 (Iran/Israel/US Conflict)
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

        var chatClient = openAi.GetChatClient(CredibilityModel);
        var response = await chatClient.CompleteChatAsync(
            [
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userMessage),
            ],
            new ChatCompletionOptions { MaxOutputTokenCount = 512, ResponseFormat = ScoringFormat },
            ct);

        var output = JsonSerializer.Deserialize<ScoringOutput>(
            response.Value.Content[0].Text,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("Null scoring output from model");

        return new ScoringResult(
            Score: Math.Clamp(output.Score, 0, 100),
            Reasoning: output.Reasoning,
            TopicIds: output.TopicIds,
            NamedEntitiesJson: JsonSerializer.Serialize(extraction.NamedEntities)
        );
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
        public string Reasoning { get; set; } = string.Empty;
        public List<string> TopicIds { get; set; } = [];
    }
}
