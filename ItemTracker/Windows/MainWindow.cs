using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using Dalamud.Game.Text;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ImGuiNET;
using ItemTracker.Universalis;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using Lumina.Text;

namespace ItemTracker.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin plugin;

    private readonly Dictionary<SeString, uint> itemsId;
    private List<SeString> searchResults = new ();
    private ExcelSheet<Item> items;

    private bool textChanged;

    //private UniversalisClient UClient;

    public MainWindow(Plugin plugin, IDalamudTextureWrap goatImage) : base(
        "Item Tracker", ImGuiWindowFlags.AlwaysVerticalScrollbar)
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        
        this.plugin = plugin;

        itemsId = CreateItemDictionary();
        items = Plugin.DataManager.GameData.Excel.GetSheet<Item>()!;

    }

    public void Dispose()
    {
        
    }

    private byte[] currentSearch = new byte[100];
    private uint currentItem;
    private string text = "Search";
    private bool searchMode = true;

    private string searchString = "";
    private int index;
    public override void Draw()
    {
        // Search input field
        ImGui.Text(searchString);
        if (ImGui.InputText(text, currentSearch, 100))
        {
            UpdateSearchString();
            textChanged = true;
            searchMode = true;
            UpdateSearchResults();
        }
        
        if(searchMode) SearchMode();
        else DisplayMode();
        
    }
    
    private bool dataRetrieved;
    private MarketData? marketData;
    private void DisplayMode()
    {

        ImGui.Text($"{items.GetRow(currentItem)!.Name} -- {currentItem}");
        if(marketData == null) ImGui.Text("Waiting...");
        else
        {
            ImGui.Selectable(GetPaddedString(new (){("World", 15), ("Quantity", 15), ("Price", 10), ("HQ", 5)}));

            foreach (var listing in marketData.Listings)
                ImGui.Selectable(GetPaddedString(new(0)
                {
                    (listing.WorldName, 15), ($"{listing.Quantity}", 15),
                    ($"{listing.Price}", 10), ($"{listing.HQ}", 5)
                }));

        }
    }

    private string GetPaddedString(List<(string, int)> strings)
    {
        var sb = new StringBuilder();
        foreach (var s in strings)
            sb.Append(s.Item1.PadRight(s.Item2));

        return sb.ToString();
    }
    private async void SearchMode()
    {
        // Display search results
        foreach (var result in searchResults)
        {
            if (ImGui.Selectable(result.ToString(), currentItem == itemsId[result]))
            {
                currentItem = itemsId[result];
                searchMode = false;
            }
        }
    }
    
    private void UpdateSearchResults()
    {
        
        if (string.IsNullOrWhiteSpace(searchString))
        {
            searchResults.Clear();
            return;
        }

        searchResults.Clear(); // Clear existing results
        foreach (var itemName in itemsId.Keys)
        {
            if (itemName.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase))
            {
                searchResults.Add(itemName);
            }
        }
    }
    private void UpdateSearchString()
    {
        int nullIndex = Array.IndexOf(currentSearch, (byte)0);
        if (nullIndex != -1)
        {
            // Zero out the rest of the array from the first null byte
            Array.Clear(currentSearch, nullIndex + 1, currentSearch.Length - nullIndex - 1);
        }

        searchString = Encoding.UTF8.GetString(currentSearch).TrimEnd('\0');
    }

    public Dictionary<SeString, uint> CreateItemDictionary()
    {
        var itemSheet = Plugin.DataManager.GameData.Excel.GetSheet<Item>();
        if (itemSheet == null) return new Dictionary<SeString, uint>();

        var itemDictionary = itemSheet
                             .Where(i => !string.IsNullOrEmpty(i.Name))
                             .GroupBy(i => i.Name)
                             .ToDictionary(g => g.Key, g => g.First().RowId);

        return itemDictionary;
    }

}
