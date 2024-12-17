using System;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ImGuiNET;

namespace SamplePlugin.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin Plugin;
    

    public MainWindow(Plugin plugin)
        : base("菜单", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        Plugin = plugin;
        
    }

    public void Dispose() { }

    public unsafe override void Draw()
    {

        var checkTime = Plugin.Configuration.checkTime;
        if (ImGui.Checkbox("启动时检查", ref checkTime))
        {
            Plugin.Configuration.checkTime = checkTime;
            Plugin.Configuration.Save();
        }

        if (!string.IsNullOrEmpty(Plugin.Configuration.timeLeft))
        {
            ImGui.Text($"在上次记录时还剩下: {Plugin.Configuration.timeLeft} 秒点卡");
        }

        var showOnDTR = Plugin.Configuration.showOnDTR;
        if (ImGui.Checkbox("在服务器信息栏上显示", ref showOnDTR))
        {
            Plugin.Configuration.showOnDTR = showOnDTR;
            Plugin.Configuration.Save();
        }

        if (ImGui.Button("重置"))
        {
            Plugin.Configuration.timeLeft = null;
            Plugin.Configuration.timeTill = null;
            Plugin.Configuration.lastSuccessRecord = null;
            Plugin.Configuration.Save();
        }

    }

    private void ToggleConfig(IFramework framework)
    {
        var config = Plugin.GameConfig.UiControl.GetBool("ObjectBorderingType");
        if (Plugin.ClientState.IsGPosing && config == true)
        {
            Plugin.PluginLog.Debug("GPose is active -> False");
            Plugin.GameConfig.UiControl.Set("ObjectBorderingType", false);
        }
        else if (!Plugin.ClientState.IsGPosing && config == false)
        {
            Plugin.PluginLog.Debug("Not active -> Revert");
            Plugin.GameConfig.UiControl.Set("ObjectBorderingType", true);
        }
    }
}
