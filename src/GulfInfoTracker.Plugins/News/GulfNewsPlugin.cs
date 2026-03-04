using GulfInfoTracker.Plugins.Abstractions;

namespace GulfInfoTracker.Plugins.News;

public class GulfNewsPlugin(SourceConfig config, HttpClient http) : RssSourcePlugin(config, http);
