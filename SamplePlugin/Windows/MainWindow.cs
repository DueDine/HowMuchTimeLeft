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
    private IDtrBarEntry entry;

    public MainWindow(Plugin plugin)
        : base("My Amazing Window", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        Plugin = plugin;
        entry = Plugin.DtrBar.Get("TimeLeft");
    }

    public void Dispose() { }

    public unsafe override void Draw()
    {
        ImGui.Text("仅在选择角色页面按下按钮!!!");

        if (ImGui.Button("点击我"))
        {
            var time = AgentLobby.Instance()->LobbyData.LobbyUIClient.SubscriptionInfo;
            var raw = getBytes(*time);
            Plugin.PluginLog.Debug($"Time left: {raw}");
            Plugin.Configuration.timeLeft = raw;
            Plugin.Configuration.timeTill = DateTimeOffset.Now.AddSeconds(int.Parse(raw)).ToString();
            Plugin.Configuration.Save();
        }

        if (!string.IsNullOrEmpty(Plugin.Configuration.timeLeft))
        {
            ImGui.Text($"还剩下: {Plugin.Configuration.timeLeft} 秒点卡");
            entry.Text = $"点卡至: {Plugin.Configuration.timeTill}";
        }

        if (ImGui.Button("清除"))
        {
            Plugin.Configuration.timeLeft = null;
            Plugin.Configuration.Save();
        }
    }

    private static string getBytes(LobbySubscriptionInfo str)
    {
        var size = Marshal.SizeOf(str);
        var arr = new byte[size];

        var ptr = IntPtr.Zero;
        try
        {
            ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(str, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }

        var ret = "";
        ret = string.Join("", arr.Skip(24).Take(3).Reverse().Select(x => x.ToString("X2")));
        ret = Convert.ToInt32(ret, 16).ToString();
        return ret;
    }
}
