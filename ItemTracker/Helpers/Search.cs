using ImGuiNET;
using ItemTracker.Universalis;
using Lumina.Text;

namespace ItemTracker.Helpers;

public static class Search
{
    public static float SelectableHeight;

    public static uint CurrentItem;
    public static string SearchString = "";
    public static List<SeString> SearchResults = new ();
    public static MarketData? MarketData;
    public static bool IsFetching;
    
    private static Vector2 contextMenuPos;
    private static bool dataRetrieved;
    private static int selectedIndex = -1;
    
    
    public static void Draw()
    {
        // Draw the search input
        ImGui.BeginTable("Search", 2);
        ImGui.TableSetupColumn("", ImGuiTableColumnFlags.NoHeaderLabel, ImGui.GetWindowWidth() - 90);
        ImGui.TableSetupColumn("", ImGuiTableColumnFlags.NoHeaderLabel, 90);
        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(-1); // Use available width
        if (ImGui.InputTextWithHint("##Search", "Search...", ref SearchString, 100))
            UpdateSearchResults();
        
        ImGui.TableNextColumn();
        if(ImGui.Button(MarketListings.MarketChildOpen ? "Close Market" : "Open Market")) MarketListings.OpenMarketWindow();
        ImGui.TableNextColumn();
        ImGui.EndTable();

        // Get the position and size of the search bar
        var searchBarPos = ImGui.GetItemRectMin();
        var searchBarSize = ImGui.GetItemRectSize();
        contextMenuPos = new Vector2(searchBarPos.X, searchBarPos.Y + searchBarSize.Y);

        // Draw a child window as a simulated context menu
        if (SearchString != "" && SearchResults.Count > 0)
        {
            var listOffset = (ImGui.GetStyle().FramePadding.Y * 2) + 3f;
            var height = Math.Clamp((SelectableHeight * SearchResults.Count) + listOffset, SelectableHeight, (SelectableHeight * 8) + listOffset);
            ImGui.SetNextWindowPos(contextMenuPos);
            ImGui.SetNextWindowSize(new Vector2(searchBarSize.X, height)); // Adjust height as needed
            
            if (ImGui.BeginChild("SearchResults", new Vector2(searchBarSize.X, height), true, SearchResults.Count <= 8 ? ImGuiWindowFlags.NoScrollbar : ImGuiWindowFlags.HorizontalScrollbar))
            {
                foreach (var result in SearchResults)
                {
                    if (ImGui.Selectable(result.ToString(), CurrentItem == Plugin.ItemIds[result]))
                    {
                        CurrentItem = Plugin.ItemIds[result];
                        ImGui.OpenPopup("ItemContextMenu");
                    }
                }
                // Context menu for the selected item
                if (ImGui.BeginPopupContextItem("ItemContextMenu"))
                {
                    if (ImGui.MenuItem("Add Item To List"))
                    {
                        // Handle adding item to list
                    }
                    if (ImGui.MenuItem("Check Item Prices"))
                    {
                        FetchMarketDataAsync(CurrentItem); // Asynchronously fetch data
                        if(!MarketListings.MarketChildOpen) MarketListings.OpenMarketWindow();
                    }
                    ImGui.EndPopup();
                }
                ImGui.EndChild();
            }
        }
    }
    private static async void FetchMarketDataAsync(uint itemId)
    {
        try
        {
            IsFetching = true;
            MarketData = await Plugin.UClient.GetMarketBoardDataAsync(Plugin.Items.GetRow(itemId));
            if (MarketData != null && MarketData.Listings.Any())
            {
                Plugin.ChatGui.Print($"Fetched {MarketData.Listings.Length} listings.");
            }
            else
            {
                Plugin.ChatGui.Print("No listings fetched.");
            }
        }
        catch (Exception ex)
        {
            Plugin.ChatGui.PrintError($"Error in FetchMarketDataAsync - {ex}");
        }

        IsFetching = false;
    }
    private static void UpdateSearchResults()
    {
        
        if (string.IsNullOrWhiteSpace(SearchString))
        {
            SearchResults.Clear();
            return;
        }

        SearchResults.Clear(); // Clear existing results
        foreach (var itemName in Plugin.ItemIds.Keys)
        {
            if (itemName.ToString().Contains(SearchString, StringComparison.OrdinalIgnoreCase))
            {
                SearchResults.Add(itemName);
            }
        }
    }
}
