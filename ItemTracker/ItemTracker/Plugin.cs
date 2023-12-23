using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ItemTracker.Managers;
using ItemTracker.Universalis;
using ItemTracker.Windows;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets2;
using Lumina.Text;
using static ItemTracker.Helpers.PersonalListings;

namespace ItemTracker
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Item Tracker";
        private readonly string[] CommandName = {"/it", "/itemtracker"};

        private DalamudPluginInterface PluginInterface { get; init; }
        private ICommandManager CommandManager { get; init; }
        public Configuration Configuration { get; init; }
        public WindowSystem WindowSystem = new("ItemTracker");

        private ConfigWindow ConfigWindow { get; init; }
        private MainWindow MainWindow { get; init; }

        [PluginService]
        [RequiredVersion("1.0")]
        public static IGameGui GameGui { get; private set; } = null!;

        [PluginService]
        [RequiredVersion("1.0")]
        public static IChatGui ChatGui { get; private set; } = null!;
        
        [PluginService]
        [RequiredVersion("1.0")]
        public static IDataManager DataManager { get; private set; } = null!;
        
        [PluginService]
        [RequiredVersion("1.0")]
        public static IClientState ClientState { get; private set; } = null!;
        public static HoverManager HoverManager { get; private set; } = null!;
        public static UniversalisClient UClient { get; private set; }
        public static ExcelSheet<Item> Items { get; private set; }
        public static ExcelSheet<ClassJob> Jobs { get; private set; }
        public static Dictionary<SeString, uint> ItemIds { get; private set; }
        public static ExcelSheet<RecipeLookup> Recipes { get; private set; }

        private static bool isLoaded;
        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] ICommandManager commandManager)
        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);

            // you might normally want to embed resources and load them from the manifest stream
            var imagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");
            var goatImage = this.PluginInterface.UiBuilder.LoadImage(imagePath);

            ConfigWindow = new ConfigWindow(this);
            MainWindow = new MainWindow(this, goatImage);

            WindowSystem.AddWindow(ConfigWindow);
            WindowSystem.AddWindow(MainWindow);
            

            HoverManager = new HoverManager(this);
            foreach(var command in CommandName)
                this.CommandManager.AddHandler(command, new CommandInfo(OnCommand)
                {
                    HelpMessage = "A useful message to display in /xlhelp"
                });

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        }

        public void Dispose()
        {
            this.WindowSystem.RemoveAllWindows();
            
            ConfigWindow.Dispose();
            MainWindow.Dispose();

            
            foreach(var command in CommandName)
                this.CommandManager.RemoveHandler(command);
        }

        private void OnCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            MainWindow.IsOpen = true;
            if(!isLoaded) LoadData();
        }

        private void DrawUI()
        {
            this.WindowSystem.Draw();
        }

        public void DrawConfigUI()
        {
            ConfigWindow.IsOpen = true;
        }

        private static void LoadData()
        {
            UClient = new UniversalisClient(ClientState.LocalPlayer.CurrentWorld.GameData.DataCenter.Value.Name);
            Items = DataManager.GameData.Excel.GetSheet<Item>()!;
            Jobs = DataManager.GameData.Excel.GetSheet<ClassJob>()!;
            Recipes = DataManager.GameData.GetExcelSheet<RecipeLookup>()!;
            CreateItemDictionary();
            isLoaded = true;
        }
        private static void CreateItemDictionary()
        {
            try
            {
                var itemSheet = DataManager.GameData.Excel.GetSheet<Item>();

                var itemDictionary = itemSheet!
                                     .Where(i => !string.IsNullOrEmpty(i.Name))
                                     .GroupBy(i => i.Name)
                                     .ToDictionary(g => g.Key, g => g.First().RowId);

                ItemIds = itemDictionary;
            } catch(Exception ex) {ChatGui.PrintError($"{ex}");}
        }

        public static List<RecipeJob> GetRecipes(uint itemId)
        {
            try
            {
                var recipeSheet = Recipes.GetRow(itemId);
                var recipes = new List<RecipeJob>();
                for (var i = 8u; i < 16; i++) //Carpenter ID starts at 8, Cul ID stops at 15
                {
                    var recipe = i switch
                    {
                        8 => recipeSheet!.CRP.Value!,
                        9 => recipeSheet!.BSM.Value!,
                        10 => recipeSheet!.ARM.Value!,
                        11 => recipeSheet!.GSM.Value!,
                        12 => recipeSheet!.LTW.Value!,
                        13 => recipeSheet!.WVR.Value!,
                        14 => recipeSheet!.ALC.Value!,
                        15 => recipeSheet!.CUL.Value!,
                        _ => null
                    };
                    if (recipe == null || recipe.ItemResult.Value.Name.ToString().IsNullOrWhitespace()) continue; 
                    recipes.Add(new RecipeJob(Jobs.GetRow(i)!.Name, recipe));
                }

                return recipes;
            }
            catch (Exception e)
            {
                ChatGui.PrintError($"{e}");
            }

            return new List<RecipeJob>();
        }
    }
}
