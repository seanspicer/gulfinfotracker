using GulfInfoTracker.Plugins.Abstractions;

namespace GulfInfoTracker.Plugins.Government;

public class SaudiSpaPlugin(SourceConfig config, HttpClient http) : GovRssPlugin(config, http);
