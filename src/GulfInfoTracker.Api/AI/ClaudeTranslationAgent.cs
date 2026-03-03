using Anthropic.SDK;
using Anthropic.SDK.Messaging;

namespace GulfInfoTracker.Api.AI;

/// <summary>
/// Translation-only agent using claude-haiku-4-5 for cost efficiency.
/// </summary>
public class ClaudeTranslationAgent(AnthropicClient claude) : ITranslationAgent
{
    private const string TranslationModel = "claude-haiku-4-5-20251001";

    public async Task<string> TranslateAsync(string text, string fromLang, string toLang, CancellationToken ct = default)
    {
        var systemPrompt = $"You are a professional translator. Translate the following text from {fromLang} to {toLang}. Output ONLY the translated text with no additional commentary, preamble, or explanation.";

        var response = await claude.Messages.GetClaudeMessageAsync(new MessageParameters
        {
            Model = TranslationModel,
            MaxTokens = 1024,
            System = [new SystemMessage(systemPrompt)],
            Messages = [new Message(RoleType.User, text)],
        }, ct);

        return response.Content.OfType<TextContent>().FirstOrDefault()?.Text ?? text;
    }
}
