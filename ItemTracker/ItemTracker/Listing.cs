using ImGuiNET;
using Newtonsoft.Json;
using OtterGui;
using System.Collections.Generic;

namespace ItemTracker;

public class Listing
{
    [JsonProperty("worldName")] public string WorldName { get; set; } = null!;
    [JsonProperty("quantity")] public int Quantity { get; set; }
    [JsonProperty("pricePerUnit")] public int Price { get; set; }
    [JsonProperty("hq")] public bool HQ { get; set; }
}
