using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using Dalamud.DrunkenToad.Extensions;
using Dalamud.Game.Text;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.System.Configuration;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using ImGuiNET;
using ImGuiScene;
using ItemTracker.Helpers;
using ItemTracker.Universalis;
using Lumina;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using Lumina.Text;
using OtterGui;
using OtterGui.Table;
using Item = Lumina.Excel.GeneratedSheets2.Item;
using Recipe = Lumina.Excel.GeneratedSheets2.Recipe;
using SystemConfig = FFXIVClientStructs.FFXIV.Common.Configuration.SystemConfig;
using Timer = System.Timers.Timer;

namespace ItemTracker.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin plugin;

    public MainWindow(Plugin plugin, IDalamudTextureWrap goatImage) : base(
        "Item Tracker")
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        
        this.plugin = plugin;

        var lineHeight = ImGui.GetTextLineHeight();
        var verticalPadding = ImGui.GetStyle().FramePadding.Y;
        
        Search.SelectableHeight = lineHeight + (2 * verticalPadding) - 1.5f;
    }

    public void Dispose()
    {
        
    }
    public override void Draw()
    {
        if(Plugin.ClientState.LocalPlayer == null) return;
        MarketListings.UpdateWindowSize();
        MarketListings.Draw();
        Search.Draw();
        if (ImGui.BeginTable("MainTable", 2, ImGuiTableFlags.Resizable))
        {
            PersonalListings.Draw();
            ImGui.TableNextColumn();
            ImGui.EndTable();
        }
        
    }
    
}
