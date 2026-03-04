using GulfInfoTracker.Plugins.Abstractions;

namespace GulfInfoTracker.Plugins.News;

public class AlJazeeraPlugin(SourceConfig config, HttpClient http) : RssSourcePlugin(config, http);
