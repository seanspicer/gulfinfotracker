using GulfInfoTracker.Plugins.Abstractions;

namespace GulfInfoTracker.Plugins.News;

public class MuscatDailyPlugin(SourceConfig config, HttpClient http) : RssSourcePlugin(config, http);
