using ImGuiNET;
using ItemTracker.Universalis;

namespace ItemTracker.Helpers;

public static class MarketListings
{
    private static float x = 1;
    
    public static bool MarketChildOpen;
    public static Vector2 ChildWindowPosition;
    public static Vector2 ChildWindowSize;

    private static bool IsSliding;
    private static bool IsOpening;
    
    public static void Draw()
    {
        if (MarketChildOpen || IsSliding)
        {
            ImGui.SetNextWindowPos(ChildWindowPosition);
            ImGui.SetNextWindowSize(ChildWindowSize);
            
            ImGui.Begin("Child Window", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize);
            DrawHeader();
            if (Search.MarketData == null)
            {
                ImGui.Text(Search.IsFetching ? "Fetching..." : "No Item Selected");
            }
            else
            {
                try
                {
                    if (ImGui.BeginTable("##listingsTable", 4, ImGuiTableFlags.ScrollY))
                    {
                        ImGui.TableSetupColumn("World", ImGuiTableColumnFlags.None, 100f);
                        ImGui.TableSetupColumn("Quantity", ImGuiTableColumnFlags.None, 100f);
                        ImGui.TableSetupColumn("Price", ImGuiTableColumnFlags.None, 100f);
                        ImGui.TableSetupColumn("HQ", ImGuiTableColumnFlags.None, 50f);
                        ImGui.TableHeadersRow();
                        
                        foreach (var listing in Search.MarketData.Listings)
                        {
                            ImGui.TableNextRow();
                            ImGui.TableNextColumn();
                            if (ImGui.Selectable(listing.WorldName, false, ImGuiSelectableFlags.SpanAllColumns))
                            {
                                // Handle row selection here
                            }

                            ImGui.TableNextColumn();
                            ImGui.Text(listing.Quantity.ToString());
                            ImGui.TableNextColumn();
                            ImGui.Text(listing.Price.ToString());
                            ImGui.TableNextColumn();
                            ImGui.Text(listing.HQ ? "Yes" : "No");
                        }

                        ImGui.EndTable();
                    }
                }
                catch (Exception ex)
                {
                    Plugin.ChatGui.PrintError($"Error drawing listing selector: {ex}");
                }
            }
            ImGui.End();
        }
    }

    private static void DrawHeader()
    {
        ImGui.BeginTable("Title", 3);
        ImGui.TableSetupColumn("MC1");
        ImGui.TableSetupColumn("MC2", ImGuiTableColumnFlags.None, ChildWindowSize.X);
        ImGui.TableSetupColumn("MC3", ImGuiTableColumnFlags.None, 55);
        ImGui.TableNextColumn();
        ImGui.TableNextColumn();
        OtterGui.ImGuiUtil.Center($"{(Search.MarketData == null ? "Market Listings" : Search.MarketData.Item.Name)}");
        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(55);
        if (ImGui.Button("Clear"))
        {
            Search.CurrentItem = 0;
            Search.MarketData = null;
        }
        ImGui.NextColumn();
        ImGui.EndTable();
    }
    public static void OpenMarketWindow()
    {
        IsSliding = true;
        IsOpening = !IsOpening;
    }
    public static void UpdateWindowSize()
    {
        ChildWindowPosition = ImGui.GetWindowPos() + new Vector2(ImGui.GetWindowSize().X, 0);            
        ChildWindowSize = new Vector2(x * 30, ImGui.GetWindowSize().Y);
        if (IsSliding) AnimateWindow();
    }

    private static void AnimateWindow()
    {
        if (IsOpening)
        {
            x++;
            if (x > 10)
            {
                MarketChildOpen = true;
                IsSliding = false;
            }
        }
        else
        {
            x--;
            if (x <= 1)
            {
                MarketChildOpen = false;
                IsSliding = false;
            }
        }
    }
}
