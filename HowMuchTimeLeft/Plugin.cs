using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using System.Runtime.InteropServices;
using System;
using System.Linq;
using SamplePlugin.Windows;
using Dalamud.Game.Config;
using FFXIVClientStructs.FFXIV.Client.UI;
using Dalamud.Game.Gui.Dtr;

namespace SamplePlugin;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog PluginLog { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IDtrBar DtrBar { get; private set; } = null!;
    [PluginService] internal static IGameConfig GameConfig { get; private set; } = null!;
    [PluginService] internal static IGameGui GameGui { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;

    private const string CommandName = "/pmycommand";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("SamplePlugin");

    private MainWindow MainWindow { get; init; }

    private readonly IDtrBarEntry entry;

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        MainWindow = new MainWindow(this);

        entry = DtrBar.Get("TimeLeft");

        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "打开主菜单"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;

        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

        Framework.Update += OnUpdate;
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        MainWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);

        Framework.Update -= OnUpdate;
    }


    private void OnCommand(string command, string args)
    {
        ToggleMainUI();
    }

    private unsafe void OnUpdate(IFramework framework)
    {
        if (Configuration.checkTime)
        {
            if (ClientState.IsLoggedIn == false)
            {
                if (AgentLobby.Instance() != null)
                {
                    // If lastRecord within one hour, then return
                    if (Configuration.lastSuccessRecord != null)
                    {
                        if (DateTime.Now.Subtract(DateTime.Parse(Configuration.lastSuccessRecord)).TotalMinutes < 5)
                        {
                            return;
                        }
                    }
                    try
                    {
                        Configuration.timeLeft = getBytes(*(AgentLobby.Instance()->LobbyData.LobbyUIClient.SubscriptionInfo));
                        Configuration.timeTill = DateTime.Now.AddSeconds(int.Parse(Configuration.timeLeft)).ToString("MM-dd HH:mm");
                        Configuration.lastSuccessRecord = DateTime.Now.ToString();
                        PluginLog.Debug("记录成功");
                        Configuration.Save();
                    }
                    catch (Exception)
                    {
                        return;
                    }
                }
            }
        }

        if (Configuration.showOnDTR)
        {
            if (Configuration.timeLeft != null)
            {
                entry.Text = $"点卡至 {Configuration.timeTill}";
                entry.Shown = true;
            }
        }

        if (!Configuration.showOnDTR)
        {
            entry.Shown = false;
        }
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleMainUI() => MainWindow.Toggle();

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
        // 理论最多到 FFFFFF, 真没有人冲到这个数吧
        // 什么神奇程序员反过来存的
        ret = string.Join("", arr.Skip(24).Take(3).Reverse().Select(x => x.ToString("X2")));
        ret = Convert.ToInt32(ret, 16).ToString();
        return ret;
    }
}
