#r "Microsoft.WindowsAzure.Storage"
#r "Newtonsoft.Json"

using Newtonsoft.Json;
using System.Text;
using System.Net;
using Microsoft.WindowsAzure.Storage.Table;
using System.Threading.Tasks;

public class ResultEntity : TableEntity
{
    public string Name {get; set;}
    public double Temperature {get; set;}
    public double Humidity {get; set;}
    public string isActivated {get; set;}
    public string isAcknowledged {get; set;}
}

public class StatusResult
{
    public string Name {get; set;}
    public double Temperature {get; set;}
    public double Humidity {get; set;}
    public string isActivated {get; set;}
    public string isAcknowledged {get; set;}
    public string Timestamp {get; set;}
}

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, CloudTable inTable, TraceWriter log)
{
    var maxDate = DateTime.UtcNow.AddSeconds(-30);
    log.Info($"checking on datetime: {maxDate}");

    var query = new TableQuery<ResultEntity>();
;

        var outResult = new List<StatusResult>();
        var dictionaryResult = new Dictionary<string, StatusResult>();

        var results = await inTable.ExecuteQuerySegmentedAsync(query, null);
        var filtered = from device in results select new StatusResult 
            {
                Name = device.Name,
                Temperature = device.Temperature,
                Humidity = device.Humidity,
                isActivated = device.isActivated,
                isAcknowledged = device.isAcknowledged,
                Timestamp = device.Timestamp.ToString()
            };

    //return req.CreateResponse(HttpStatusCode.OK, filtered.ToList());
    var jsonToReturn = JsonConvert.SerializeObject(filtered.ToList());

    return new HttpResponseMessage(HttpStatusCode.OK)
    {
        Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
    };
}
