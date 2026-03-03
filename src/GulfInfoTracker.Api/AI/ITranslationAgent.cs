namespace GulfInfoTracker.Api.AI;

public interface ITranslationAgent
{
    Task<string> TranslateAsync(string text, string fromLang, string toLang, CancellationToken ct = default);
}
