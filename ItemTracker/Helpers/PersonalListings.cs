using Dalamud.DrunkenToad.Extensions;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets2;
using OtterGui;

namespace ItemTracker.Helpers;

public static class PersonalListings
{
    private static List<List<GameItem>> lists = new ();
    private static int ListIndex = -1;
    
    private static string Name = "";
    private static bool ContextOpen;
    private static readonly Vector2 NameWindowSize = new (300, 90);

    public static void Draw()
    {
        DrawListButtons();
        DrawList();
    }
    
    
    private static void DrawListButtons()
    {
        ImGui.TableSetupColumn("Column 1", ImGuiTableColumnFlags.WidthFixed, 30.0f); 
        ImGui.TableNextColumn();
        if (ImGui.Button("Add List"))
        {
            ContextOpen = true;
            //lists.Add(new List<GameItem>());
            
        }

        if (ContextOpen)
        {
            var position = ImGui.GetWindowPos() + (ImGui.GetWindowSize() / 2) - (NameWindowSize / 2);
            ImGui.Begin("Naming Window", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoMove);
            ImGui.SetWindowFocus();
            ImGui.SetWindowPos(position);
            ImGui.SetWindowSize(NameWindowSize);
            ImGuiUtil.Center("Enter List Name");
            ImGui.SetNextItemWidth(-1);
            ImGui.InputTextWithHint("##Naming", "Enter Name...", ref Name, 100);
            ImGui.BeginTable("List Name", 4);
            ImGui.TableSetupColumn("NCol 1", ImGuiTableColumnFlags.None, 94);
            ImGui.TableSetupColumn("NCol 2", ImGuiTableColumnFlags.None, 56);
            ImGui.TableSetupColumn("NCol 3", ImGuiTableColumnFlags.None, 56);
            ImGui.TableSetupColumn("NCol 4", ImGuiTableColumnFlags.None, 94);
            ImGui.TableNextColumn();
            ImGui.TableNextColumn();
            ContextOpen = !ImGui.Button(" Close ");
            ImGui.TableNextColumn();
            ImGui.Button("Confirm");
            ImGui.TableNextColumn();
            ImGui.TableNextColumn();
            ImGui.EndTable();
            ImGui.End();
        }
        for (var i = 0; i < lists.Count; i++)
        {
            if (ImGui.Button(i < 9 ? $"  List  {i + 1}  " :  $"  List {i + 1} "))
            {
                ListIndex = i;
            }
        }
    }
    
    private static void DrawList()
    {
        ImGui.TableNextColumn();
        ImGui.Text(ListIndex == -1 ? "No list selected" : $"List {ListIndex + 1}");
        ImGui.Separator();

        var x = Plugin.GetRecipes(5057);
        foreach (var i in x)
        {
            ImGui.Text($"{i.ItemName} - {i.Job}");
            foreach (var j in i.Ingredients)
            {
                ImGui.Text($"       {j.Value.Name}");
            }
        }

    }
    
    public class PersonalList
    {
        public string Name { get; private set; }
        public List<PersonalItem> Items { get; private set; }
        
        public PersonalList(string name)
        {
            Name = name;
            Items = new List<PersonalItem>();
        }
    }

    public class PersonalItem
    {
        public Item Item { get; private set; }
        public List<Recipe> Recipes { get; private set; }

        public PersonalItem()
        {
            
        }
        
    }

    public class RecipeJob
    {
        public string Job;
        public string ItemName;
        public List<LazyRow<Item>> Ingredients { get; private set; }
        
        public RecipeJob(string job, Recipe recipe)
        {
            Job = job;
            ItemName = recipe.ItemResult.Value!.Name;
            Ingredients = new List<LazyRow<Item>>();
            foreach (var i in recipe.Ingredient)
            {
                if (i.Value != null && !i.Value.Name.ToString().IsNullOrWhitespace())
                {
                    Ingredients.Add(i);
                }
            }
        }
    }
}
