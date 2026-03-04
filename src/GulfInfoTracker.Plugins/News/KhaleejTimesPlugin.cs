using GulfInfoTracker.Plugins.Abstractions;

namespace GulfInfoTracker.Plugins.News;

public class KhaleejTimesPlugin(SourceConfig config, HttpClient http) : RssSourcePlugin(config, http);
