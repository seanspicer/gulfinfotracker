using GulfInfoTracker.Api.Data.Entities;
using GulfInfoTracker.Api.Data.Repositories;
using GulfInfoTracker.Api.Controllers;
using GulfInfoTracker.Api.Models;
using Moq;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace GulfInfoTracker.Api.Tests;

[TestFixture]
public class ArticlesControllerTests
{
    private Mock<IArticleRepository> _repoMock = null!;
    private ArticlesController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _repoMock = new Mock<IArticleRepository>();
        _controller = new ArticlesController(_repoMock.Object);
    }

    private static Article CreateArticle(string topicId = "T1") => new()
    {
        Id          = Guid.NewGuid(),
        PluginId    = "uae-gov",
        HeadlineEn  = "Test Headline",
        SourceUrl   = "https://example.com/test",
        PublishedAt = DateTime.UtcNow,
        Country     = "UAE",
        ArticleTopics = [new ArticleTopic { TopicId = topicId, ArticleId = Guid.NewGuid() }]
    };

    [Test]
    public async Task GetArticles_ReturnsOkWithCorrectEnvelope()
    {
        var articles = new List<Article> { CreateArticle() };
        _repoMock.Setup(r => r.QueryAsync(It.IsAny<ArticleQuery>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new ArticleQueryResult(articles, 1));

        var result = await _controller.GetArticles(null, null, null, "newest", 1, 20, null, CancellationToken.None);

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var paged = ok!.Value as PagedResult<ArticleListItem>;
        Assert.That(paged, Is.Not.Null);
        Assert.That(paged!.Total, Is.EqualTo(1));
        Assert.That(paged.Data, Has.Count.EqualTo(1));
        Assert.That(paged.Page, Is.EqualTo(1));
    }

    [Test]
    public async Task GetArticles_TopicFilter_PassedToRepository()
    {
        _repoMock.Setup(r => r.QueryAsync(It.Is<ArticleQuery>(q => q.Topic == "T3"), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new ArticleQueryResult([], 0));

        await _controller.GetArticles("T3", null, null, "newest", 1, 20, null, CancellationToken.None);

        _repoMock.Verify(r => r.QueryAsync(
            It.Is<ArticleQuery>(q => q.Topic == "T3"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetArticles_CountryFilter_PassedToRepository()
    {
        _repoMock.Setup(r => r.QueryAsync(It.Is<ArticleQuery>(q => q.Country == "UAE"), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new ArticleQueryResult([], 0));

        await _controller.GetArticles(null, "UAE", null, "newest", 1, 20, null, CancellationToken.None);

        _repoMock.Verify(r => r.QueryAsync(
            It.Is<ArticleQuery>(q => q.Country == "UAE"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetArticle_UnknownId_Returns404()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Article?)null);

        var result = await _controller.GetArticle(Guid.NewGuid(), CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task GetArticle_KnownId_ReturnsDetail()
    {
        var article = CreateArticle();
        _repoMock.Setup(r => r.GetByIdAsync(article.Id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(article);

        var result = await _controller.GetArticle(article.Id, CancellationToken.None);

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var detail = ok!.Value as ArticleDetail;
        Assert.That(detail!.Id, Is.EqualTo(article.Id));
    }
}
