using GulfInfoTracker.Plugins.Abstractions;

namespace GulfInfoTracker.Plugins.Government;

public class UaeGovPlugin(SourceConfig config, HttpClient http) : GovRssPlugin(config, http);
