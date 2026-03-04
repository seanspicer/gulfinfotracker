using GulfInfoTracker.Plugins.Abstractions;

namespace GulfInfoTracker.Plugins.News;

public class ArabianBusinessPlugin(SourceConfig config, HttpClient http) : RssSourcePlugin(config, http);
