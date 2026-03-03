using GulfInfoTracker.Plugins.Abstractions;

namespace GulfInfoTracker.Plugins.News;

public class FtPlugin(SourceConfig config, HttpClient http) : RssSourcePlugin(config, http);
