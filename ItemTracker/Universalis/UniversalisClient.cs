using System.Net.Http;
using Lumina.Excel.GeneratedSheets2;
using Newtonsoft.Json;

namespace ItemTracker.Universalis;

public class UniversalisClient
{
    private readonly HttpClient httpClient;
    private readonly string dataCenter;
    public UniversalisClient(string dataCenter)
    {
        httpClient = new HttpClient();
        this.dataCenter = dataCenter;
    }
    
    public async Task<MarketData?> GetMarketBoardDataAsync(Item item)
    {
        try
        {
            var response = await httpClient.GetAsync($"https://universalis.app/api/{dataCenter}/{item.RowId}");
            if (!response.IsSuccessStatusCode)
            {
                // Handle non-success status code
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var marketData = JsonConvert.DeserializeObject<MarketData>(content);
            marketData!.Item = item;
            return marketData;
        }
        catch (Exception ex)
        {
            Plugin.ChatGui.PrintError($"{ex}");
            return null;
        }
    }
}


public class World
{
    [JsonProperty("id")]
    public uint Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }
}

public class Datacenter
{
    [JsonProperty("name")] public string Name { get; set; } = "";
    [JsonProperty("worlds")] public uint[] Worlds { get; set; } = null!;
}

public class MarketData
{
    public Item Item;
    [JsonProperty("listings")] public Listing[] Listings { get; set; } = null!;
}

