using GulfInfoTracker.Plugins.Abstractions;

namespace GulfInfoTracker.Plugins.News;

public class KuwaitTimesPlugin(SourceConfig config, HttpClient http) : RssSourcePlugin(config, http);
