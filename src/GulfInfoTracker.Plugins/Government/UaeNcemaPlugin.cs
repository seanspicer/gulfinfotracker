using GulfInfoTracker.Plugins.Abstractions;

namespace GulfInfoTracker.Plugins.Government;

public class UaeNcemaPlugin(SourceConfig config, HttpClient http) : GovRssPlugin(config, http);
