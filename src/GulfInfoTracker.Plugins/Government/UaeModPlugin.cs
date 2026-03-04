using GulfInfoTracker.Plugins.Abstractions;

namespace GulfInfoTracker.Plugins.Government;

public class UaeModPlugin(SourceConfig config, HttpClient http) : GovRssPlugin(config, http);
