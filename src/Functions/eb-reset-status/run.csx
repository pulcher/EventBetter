#r "Newtonsoft.Json"
#r "Microsoft.WindowsAzure.Storage"
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

static string access_token = "754050f3ce5f6723bc1cd03291f2ff04fbbd2fc7 ";

public class KnownDevices : TableEntity
{
    public string DeviceId {get; set;}
    public string Name {get; set;}
    public string Location {get; set;}
}
    
public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, CloudTable currentDevices, TraceWriter log)
{
    var resetSent = false;

    log.Info("C# HTTP trigger function processed a request.");

    // parse query parameter
    string name = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "name", true) == 0)
        .Value;

    if (name == null)
    {
        // Get request body
        dynamic data = await req.Content.ReadAsAsync<object>();
        name = data?.name;
    }

    TableQuery<KnownDevices> query = new TableQuery<KnownDevices>();

    foreach (KnownDevices device in  
        await currentDevices.ExecuteQuerySegmentedAsync(query, null))
    {
        if (device.Name == name)
        {
            var httpQuery = $"https://api.particle.io/v1/devices/{device.DeviceId}/resetStatus?access_token={access_token}";

            using(var client = new HttpClient())
            {
                client.BaseAddress = new Uri(httpQuery);
                var result = await client.PostAsync(httpQuery, new StringContent("{}"));
                string resultContent = await result.Content.ReadAsStringAsync();
                log.Info($"results for {device.Name}: {resultContent}");
                log.Info($"httpQuery for {device.Name}: {httpQuery}");
            }

            var message = $"{{ Name: \"{device.Name}\", DeviceId: \"{device.DeviceId}\" }}";

            resetSent = true;
            break;
        }
    }

    return !resetSent
        ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name on the query string or in the request body")
        : req.CreateResponse(HttpStatusCode.OK, "Resetting " + name);
}
