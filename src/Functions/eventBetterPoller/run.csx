
#r "Newtonsoft.Json"
#r "Microsoft.WindowsAzure.Storage"
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

static string access_token = "754050f3ce5f6723bc1cd03291f2ff04fbbd2fc7 ";

public class KnownDevices : TableEntity
{
    public string DeviceId {get; set;}
    public string Name {get; set;}
    public string Location {get; set;}
}

public class Result : TableEntity
{
    public string Name {get; set;}
    public double Temperature {get; set;}
    public double Humidity {get; set;}
    public string isActivated {get; set;}
    public string isAcknowledged {get; set;}
}

public static async Task Run(string myQueueItem, ICollector<string> outputQueueItem, 
    ICollector<string> outputCurrentQueueItem,
    TraceWriter log)
{
    log.Info($"C# Queue trigger function processed: {myQueueItem}");

    var myEntity = JsonConvert.DeserializeObject<KnownDevices>(myQueueItem);

    var httpQuery = $"https://api.particle.io/v1/devices/{myEntity.DeviceId}/status?access_token={access_token}";

    using(var client = new HttpClient())
    {
        client.BaseAddress = new Uri(httpQuery);
        var result = await client.GetAsync("");
        string resultContent = await result.Content.ReadAsStringAsync();
        log.Info($"results for {myEntity.Name}: {resultContent}");

        outputQueueItem.Add(resultContent);
        
        outputCurrentQueueItem.Add(resultContent);
    }

}
