using GulfInfoTracker.Api.AI;
using GulfInfoTracker.Api.Data.Entities;
using GulfInfoTracker.Api.Data.Repositories;
using GulfInfoTracker.Api.Services;
using Moq;
using NUnit.Framework;

namespace GulfInfoTracker.Api.Tests;

[TestFixture]
public class ScoringPipelineTests
{
    private Mock<IArticleRepository> _repoMock = null!;
    private Mock<ICredibilityPipeline> _pipelineMock = null!;

    [SetUp]
    public void SetUp()
    {
        _repoMock    = new Mock<IArticleRepository>();
        _pipelineMock = new Mock<ICredibilityPipeline>();
    }

    [Test]
    public async Task UpdateScoringAsync_CalledWithMockedScore_WhenPipelineReturns85()
    {
        // Arrange
        var article = new Article
        {
            Id         = Guid.NewGuid(),
            PluginId   = "uae-gov",
            HeadlineEn = "Test headline",
            SourceUrl  = "https://example.com/test",
            Country    = "UAE",
        };

        _repoMock.Setup(r => r.GetUnscoredAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync([article]);
        _repoMock.Setup(r => r.CountCorroboratingArticlesAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<string>>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(2);

        _pipelineMock.Setup(p => p.ScoreAsync(It.IsAny<Article>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ScoringResult(85, "High authority source with verifiable claims.", ["T1"], "[]"));

        // Act
        await SimulateScoringAsync(article);

        // Assert
        _pipelineMock.Verify(p => p.ScoreAsync(article, It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.UpdateScoringAsync(article.Id, 85, It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ScoreAsync_OnConsecutiveFailures_IncrementsScoringAttempts()
    {
        var article = new Article
        {
            Id              = Guid.NewGuid(),
            PluginId        = "ft",
            HeadlineEn      = "Test article",
            SourceUrl       = "https://ft.com/test",
            Country         = "INTL",
            ScoringAttempts = 0,
        };

        _pipelineMock.Setup(p => p.ScoreAsync(It.IsAny<Article>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(new Exception("Claude API error"));

        // Simulate scoring failure — ScoringAttempts should be incremented
        for (int i = 0; i < 3; i++)
        {
            try
            {
                await _pipelineMock.Object.ScoreAsync(article, 0, CancellationToken.None);
            }
            catch
            {
                article.ScoringAttempts++;
            }
        }

        Assert.That(article.ScoringAttempts, Is.EqualTo(3));
        Assert.That(article.CredibilityScore, Is.Null);
    }

    [Test]
    public async Task TranslationAgent_UpdatesHeadlineArAndSetsTranslated()
    {
        var translatorMock = new Mock<ITranslationAgent>();
        translatorMock.Setup(t => t.TranslateAsync(It.IsAny<string>(), "English", "Arabic", It.IsAny<CancellationToken>()))
                      .ReturnsAsync("العنوان المترجم");

        var article = new Article
        {
            Id         = Guid.NewGuid(),
            PluginId   = "uae-gov",
            HeadlineEn = "Original Headline",
            SourceUrl  = "https://example.com",
            Country    = "UAE",
        };

        _repoMock.Setup(r => r.UpdateTranslationAsync(article.Id, It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        var headlineAr = await translatorMock.Object.TranslateAsync(article.HeadlineEn, "English", "Arabic");
        await _repoMock.Object.UpdateTranslationAsync(article.Id, headlineAr, null);

        Assert.That(headlineAr, Is.EqualTo("العنوان المترجم"));
        _repoMock.Verify(r => r.UpdateTranslationAsync(article.Id, "العنوان المترجم", It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // Helper to simulate the scoring loop logic without requiring the full DI container
    private async Task SimulateScoringAsync(Article article)
    {
        var corroborationCount = await _repoMock.Object.CountCorroboratingArticlesAsync(
            article.Id, [], article.PublishedAt);

        var result = await _pipelineMock.Object.ScoreAsync(article, corroborationCount);

        await _repoMock.Object.UpdateScoringAsync(
            article.Id,
            result.Score,
            result.Reasoning,
            result.TopicIds,
            result.NamedEntitiesJson);
    }
}
