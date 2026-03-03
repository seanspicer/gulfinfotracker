using GulfInfoTracker.Plugins.Abstractions;

namespace GulfInfoTracker.Plugins.News;

public class NytPlugin(SourceConfig config, HttpClient http) : RssSourcePlugin(config, http);
