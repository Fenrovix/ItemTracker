using System;
using Dalamud.Game.Text;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.GeneratedSheets;

namespace ItemTracker.Managers;

public class HoverManager
{
    public uint ItemId;
    public bool ItemQuality;

    private readonly Plugin plugin;

    public HoverManager(Plugin plugin)
    {
        this.plugin = plugin;
        Plugin.GameGui.HoveredItemChanged += HoveredItemChanged;
    }


    public void HoveredItemChanged(object? sender, ulong itemId)
    {
        if(itemId == 0) return;

        var realItemId = itemId >= 1000000 ? Convert.ToUInt32(itemId - 1000000) : Convert.ToUInt32(itemId);
        
        var item = Plugin.DataManager.GameData.Excel.GetSheet<Item>()?.GetRow(realItemId);

        if (item == null)
        {
            Plugin.ChatGui.PrintError($"Failed to retrieve game data for itemId {realItemId}.");
            return;
        }
        
        Plugin.ChatGui.Print(new XivChatEntry()
        {
            Message = $"{item.Name} : {realItemId}",
            Type = XivChatType.SystemMessage
        });
    }
}
