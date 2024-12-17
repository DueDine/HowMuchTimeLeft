using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace SamplePlugin;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool checkTime { get; set; } = false;

    public string? lastSuccessRecord { get; set; } = null;

    public bool showOnDTR { get; set; } = false;

    public string? timeLeft { get; set; } = null;

    public string? timeTill { get; set; } = null;

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
