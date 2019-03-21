#r "Newtonsoft.Json"
#r "Microsoft.WindowsAzure.Storage"
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

public class ErrorMessage
{
    public string error {get; set;}
    public string info {get; set;}
}

public class CoreInfo
{
    public string last_app {get; set;}
    public string last_heard {get; set;}
    public string connected {get; set;}
    public string last_hadshake_at {get; set;}
    public string deviceID {get; set;}
    public int    product_id {get; set;}
}

public class Result : TableEntity
{
    public string Name {get; set;}
    public double Temperature {get; set;}
    public double Humidity {get; set;}
    public string isActivated {get; set;}
    public string isAcknowledged {get; set;}
}

public class ValidMessage
{
    public string cmd {get; set;}
    public string name {get; set;}
    public string result {get; set;}
    //public CoreInfo coreInfo {get; set;}
}

public static async Task Run(string myQueueItem, CloudTable outTable, TraceWriter log)
{
    log.Info($"received: {myQueueItem}");

    var error = JsonConvert.DeserializeObject<ErrorMessage>(myQueueItem);

    if (!string.IsNullOrEmpty(error.error)) 
    {
        log.Warning($"Error found: {error.error}");
    }
    else 
    {
        log.Info($"myQueueItem = {myQueueItem}");
        var messageRecv = JsonConvert.DeserializeObject<ValidMessage>(myQueueItem);

        var statusInfo = JsonConvert.DeserializeObject<Result>(messageRecv.result);
        log.Info($"uh huh: name: {statusInfo.Name}, isActivated: {statusInfo.isActivated}");
        
        statusInfo.PartitionKey = string.Empty;
        statusInfo.RowKey = statusInfo.Name;

        var operation = TableOperation.InsertOrMerge(statusInfo);
        outTable.Execute(operation);

        log.Info($"result: {messageRecv.result}");
    }
}
