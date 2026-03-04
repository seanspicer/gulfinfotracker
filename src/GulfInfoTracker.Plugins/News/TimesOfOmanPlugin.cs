using GulfInfoTracker.Plugins.Abstractions;

namespace GulfInfoTracker.Plugins.News;

public class TimesOfOmanPlugin(SourceConfig config, HttpClient http) : RssSourcePlugin(config, http);
