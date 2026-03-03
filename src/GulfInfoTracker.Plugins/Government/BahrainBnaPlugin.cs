using GulfInfoTracker.Plugins.Abstractions;

namespace GulfInfoTracker.Plugins.Government;

public class BahrainBnaPlugin(SourceConfig config, HttpClient http) : GovRssPlugin(config, http);
