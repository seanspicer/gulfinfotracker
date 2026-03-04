using GulfInfoTracker.Plugins.Abstractions;

namespace GulfInfoTracker.Plugins.Government;

public class OmanOnaPlugin(SourceConfig config, HttpClient http) : GovRssPlugin(config, http);
