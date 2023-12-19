using System.Net.Http;
using Newtonsoft.Json;

namespace ItemTracker.Universalis;

public class UniversalisClient
{
    private readonly HttpClient httpClient = new();
    private const string BaseUrl = "https://universalis.app/api/v2/";

    private string dataCenter = null!;
    private uint currentServer; 

    private bool created;
    
    public async Task CreateAsync()
    {
        currentServer = Plugin.ClientState.LocalPlayer!.CurrentWorld.Id;
        var dcs = await GetDatacenters();

        foreach (var dc in dcs!.Where(dc => dc.Worlds.Contains(currentServer)))
            dataCenter = dc.Name;
        
        created = true;
    }

    private async Task<List<Datacenter>?> GetDatacenters()
    {
        var dcJson = await httpClient.GetStringAsync(BaseUrl + "data-centers");
        var dcs = JsonConvert.DeserializeObject<List<Datacenter>>(dcJson);
        return dcs;
    }

    public async Task<MarketData?> GetDatacenterListings(uint itemId)
    {
        if (!created) await CreateAsync();
        var marketDataJson = await httpClient.GetStringAsync($"{BaseUrl}/{dataCenter}/{itemId}");
        var marketData = JsonConvert.DeserializeObject<MarketData>(marketDataJson);
        return marketData;
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
    [JsonProperty("listings")] public Listing[] Listings { get; set; } = null!;
}

public class Listing
{
    [JsonProperty("worldName")] public string WorldName { get; set; } = null!;
    [JsonProperty("quantity")] public int Quantity { get; set; }
    [JsonProperty("pricePerUnit")] public int Price { get; set; }
    [JsonProperty("hq")] public bool HQ { get; set; }
}
