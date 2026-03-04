using GulfInfoTracker.Plugins.Abstractions;

namespace GulfInfoTracker.Plugins.News;

public class SaudiGazettePlugin(SourceConfig config, HttpClient http) : RssSourcePlugin(config, http);
