using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ItemTracker.Managers;
using ItemTracker.Universalis;
using ItemTracker.Windows;

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
        }

        private void DrawUI()
        {
            this.WindowSystem.Draw();
        }

        public void DrawConfigUI()
        {
            ConfigWindow.IsOpen = true;
        }
    }
}
