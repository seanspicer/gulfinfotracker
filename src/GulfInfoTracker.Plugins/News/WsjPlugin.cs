using GulfInfoTracker.Plugins.Abstractions;

namespace GulfInfoTracker.Plugins.News;

public class WsjPlugin(SourceConfig config, HttpClient http) : RssSourcePlugin(config, http);
