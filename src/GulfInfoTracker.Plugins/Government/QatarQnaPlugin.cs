using GulfInfoTracker.Plugins.Abstractions;

namespace GulfInfoTracker.Plugins.Government;

public class QatarQnaPlugin(SourceConfig config, HttpClient http) : GovRssPlugin(config, http);
