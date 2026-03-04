using Anthropic.SDK;
using GulfInfoTracker.Api.AI;
using OpenAI;

namespace GulfInfoTracker.Api.Extensions;

public static class AiServiceExtensions
{
    public static IServiceCollection AddAiServices(this IServiceCollection services, IConfiguration config)
    {
        var provider = config["AiProvider"] ?? "OpenAi";

        switch (provider)
        {
            case "OpenAi":
                var openAiKey = config["OpenAi:ApiKey"];
                if (string.IsNullOrWhiteSpace(openAiKey)) openAiKey = config["OPENAI_API_KEY"];
                if (string.IsNullOrWhiteSpace(openAiKey)) break;
                services.AddSingleton(new OpenAIClient(openAiKey));
                services.AddScoped<ICredibilityPipeline, OpenAiCredibilityPipeline>();
                services.AddScoped<ITranslationAgent, OpenAiTranslationAgent>();
                break;

            case "Claude":
                var claudeKey = config["Claude:ApiKey"];
                if (string.IsNullOrWhiteSpace(claudeKey)) claudeKey = config["ANTHROPIC_API_KEY"];
                if (string.IsNullOrWhiteSpace(claudeKey)) break;
                services.AddSingleton(new AnthropicClient(claudeKey));
                services.AddScoped<ICredibilityPipeline, ClaudeCredibilityPipeline>();
                services.AddScoped<ITranslationAgent, ClaudeTranslationAgent>();
                break;
        }

        return services;
    }
}
