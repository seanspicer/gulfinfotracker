using OpenAI;
using OpenAI.Chat;

namespace GulfInfoTracker.Api.AI;

/// <summary>
/// Translation agent using gpt-4o-mini for cost efficiency (mirrors ClaudeTranslationAgent).
/// </summary>
public class OpenAiTranslationAgent(OpenAIClient openAi) : ITranslationAgent
{
    private const string TranslationModel = "gpt-4.1-nano";

    public async Task<string> TranslateAsync(string text, string fromLang, string toLang, CancellationToken ct = default)
    {
        var systemPrompt = $"You are a professional translator. Translate the following text from {fromLang} to {toLang}. Output ONLY the translated text with no additional commentary, preamble, or explanation.";

        var chatClient = openAi.GetChatClient(TranslationModel);
        var response = await chatClient.CompleteChatAsync(
            [
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(text),
            ],
            new ChatCompletionOptions { MaxOutputTokenCount = 1024 },
            ct);

        return response.Value.Content[0].Text ?? text;
    }
}
