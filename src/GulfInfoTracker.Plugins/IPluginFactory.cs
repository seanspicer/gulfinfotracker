using GulfInfoTracker.Plugins.Abstractions;

namespace GulfInfoTracker.Plugins;

public interface IPluginFactory
{
    IReadOnlyList<ISourcePlugin> GetEnabledPlugins();
}
