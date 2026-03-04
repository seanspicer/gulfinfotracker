using GulfInfoTracker.Api.AI;
using GulfInfoTracker.Api.Data.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using OpenAI;

namespace GulfInfoTracker.Api.Tests;

/// <summary>
/// Integration tests that call the real OpenAI API.
/// Requires OPENAI_API_KEY environment variable — skipped automatically if absent.
/// Run with: dotnet test --filter Category=Integration
/// </summary>
[TestFixture]
[Category("Integration")]
public class OpenAiScoringIntegrationTests
{
    private OpenAiCredibilityPipeline _pipeline = null!;

    [SetUp]
    public void SetUp()
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets("gulf-info-tracker-api")
            .AddEnvironmentVariables()
            .Build();

        var apiKey = config["OpenAi:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey)) apiKey = config["OPENAI_API_KEY"];
        if (string.IsNullOrWhiteSpace(apiKey))
            Assert.Ignore("No OpenAI API key found in user secrets or environment — skipping integration test.");

        _pipeline = new OpenAiCredibilityPipeline(
            new OpenAIClient(apiKey),
            config,
            NullLogger<OpenAiCredibilityPipeline>.Instance);
    }

    [Test]
    public async Task ScoreAsync_GovernmentEnergyArticle_ReturnsValidResult()
    {
        var article = new Article
        {
            Id         = Guid.NewGuid(),
            PluginId   = "uae-gov",
            HeadlineEn = "UAE announces $10 billion investment in renewable energy infrastructure",
            SummaryEn  = """
                The United Arab Emirates has announced a major $10 billion investment plan
                to expand its renewable energy infrastructure over the next five years.
                The initiative, announced by the Ministry of Energy and Infrastructure,
                will fund 15 new solar and wind projects across Abu Dhabi and Dubai.
                Officials said the programme supports the UAE's Net Zero 2050 strategic
                initiative and will create an estimated 50,000 jobs. The projects are
                expected to add 20 gigawatts of clean energy capacity to the national grid.
                International partners including European and Asian firms have already
                signed letters of intent to participate in the scheme.
                """,
            Country    = "UAE",
            PublishedAt = DateTime.UtcNow,
            SourceUrl  = "https://wam.ae/test-article",
        };

        var result = await _pipeline.ScoreAsync(article, corroborationCount: 2);

        Assert.Multiple(() =>
        {
            Assert.That(result.Score, Is.InRange(0, 100),         "Score must be 0-100");
            Assert.That(result.Reasoning, Is.Not.Empty,            "Reasoning must be present");
            Assert.That(result.TopicIds, Is.Not.Empty,             "At least one topic must be assigned");
            Assert.That(result.TopicIds, Has.All.Matches<string>(id =>
                new[] { "T1", "T2", "T3", "T4", "T5" }.Contains(id)),
                "All topic IDs must be valid");
            Assert.That(result.NamedEntitiesJson, Is.Not.Null,     "Named entities JSON must be present");
        });
    }

    [Test]
    public async Task ScoreAsync_GovernmentSource_ScoresHigherThanMinimum()
    {
        var article = new Article
        {
            Id         = Guid.NewGuid(),
            PluginId   = "saudi-spa",
            HeadlineEn = "Saudi Arabia's Vision 2030 passes midpoint with key economic targets met",
            SummaryEn  = """
                Saudi Arabia's Vision 2030 programme has passed its midpoint with officials
                confirming that non-oil revenue targets for 2025 have been met ahead of schedule.
                The Public Investment Fund reported assets under management exceeding $700 billion.
                Tourism numbers reached 100 million visitors annually, surpassing the original target.
                Crown Prince Mohammed bin Salman chaired the programme review committee in Riyadh.
                The Kingdom's GDP grew by 6% in the non-oil sector, driven by entertainment,
                technology, and financial services. International rating agencies upgraded Saudi
                Arabia's sovereign credit rating in response to the fiscal improvements.
                """,
            Country    = "SA",
            PublishedAt = DateTime.UtcNow,
            SourceUrl  = "https://spa.gov.sa/test-article",
        };

        var result = await _pipeline.ScoreAsync(article, corroborationCount: 3);

        // Government source with specific claims and corroboration should score reasonably well
        Assert.That(result.Score, Is.GreaterThan(40),
            "A government source with verifiable claims should score above 40");
    }

    [Test]
    public async Task ScoreAsync_EmptyContent_DoesNotThrow()
    {
        var article = new Article
        {
            Id         = Guid.NewGuid(),
            PluginId   = "ft",
            HeadlineEn = "Brief headline with no content",
            SummaryEn  = null,
            Country    = "INTL",
            PublishedAt = DateTime.UtcNow,
            SourceUrl  = "https://ft.com/test-article",
        };

        ScoringResult? result = null;
        Assert.DoesNotThrowAsync(async () =>
            result = await _pipeline.ScoreAsync(article, corroborationCount: 0));

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Score, Is.InRange(0, 100));
    }
}
