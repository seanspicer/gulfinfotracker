using GulfInfoTracker.Plugins.Abstractions;

namespace GulfInfoTracker.Plugins.News;

public class BloombergPlugin(SourceConfig config, HttpClient http) : RssSourcePlugin(config, http);
