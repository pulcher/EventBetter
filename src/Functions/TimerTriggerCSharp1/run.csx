#r "Microsoft.WindowsAzure.Storage"
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Threading.Tasks;

    public class KnownDevices : TableEntity
    {
        public string DeviceId {get; set;}
        public string Name {get; set;}
        public string Location {get; set;}
    }

    public static async Task Run(TimerInfo myTimer, CloudTable currentDevices,
        ICollector<string> outputQueueItem, TraceWriter log)
    {
        log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

        TableQuery<KnownDevices> query = new TableQuery<KnownDevices>();

        // Execute the query and loop through the results
        foreach (KnownDevices device in  
            await currentDevices.ExecuteQuerySegmentedAsync(query, null))
        {
            var message = $"{{ Name: \"{device.Name}\", DeviceId: \"{device.DeviceId}\" }}";
            log.Info(
                $"Sending: {message}");
            outputQueueItem.Add(message);
        }
    }
