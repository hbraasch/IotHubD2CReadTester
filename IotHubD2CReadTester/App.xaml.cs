using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Azure.Messaging.EventHubs.Consumer;

namespace IotHubD2CReadTester
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();

            Task.Factory.StartNew(async () => {
                var cts = new CancellationTokenSource();
                await ReceiveMessagesFromDeviceAsync(cts.Token);
            });
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        // Event Hub-compatible endpoint.  Copy this information from "Event Hub-compatible endpoint" from the "Built-in endpoints" section of your IoT Hub instance in the Azure portal
        private const string connectionString = "Endpoint=sb... Add your own please";

        // Event Hub-compatible name
        private const string eventHubName = "Add your own please";

        // Asynchronously create a PartitionReceiver for a partition and then start
        // reading any messages sent from the simulated client.
        public static async Task ReceiveMessagesFromDeviceAsync(CancellationToken cancellationToken)
        {
            // Create the consumer using the default consumer group using a direct connection to the service.
            // Information on using the client with a proxy can be found in the README for this quick start, here:
            //   https://github.com/Azure-Samples/azure-iot-samples-csharp/tree/master/iot-hub/Quickstarts/read-d2c-messages/README.md#websocket-and-proxy-support
            //
            EventHubConsumerClient consumer = new EventHubConsumerClient(EventHubConsumerClient.DefaultConsumerGroupName, connectionString, eventHubName);

            Debug.WriteLine("Listening for messages on all partitions");

            try
            {
                // Begin reading events for all partitions, starting with the first event in each partition and waiting indefinitely for
                // events to become available.

                var iterator = consumer.ReadEventsAsync(new ReadEventOptions { MaximumWaitTime = TimeSpan.FromMilliseconds(60000) }).GetAsyncEnumerator();

                while (await iterator.MoveNextAsync())
                {
                    var partitionEvent = iterator.Current;

                    Debug.WriteLine("Message received on partition {0}:", partitionEvent.Partition.PartitionId);

                    string data = Encoding.UTF8.GetString(partitionEvent.Data.Body.ToArray());
                    Debug.WriteLine("\t{0}:", data);

                    Debug.WriteLine("Application properties (set by device):");
                    foreach (var prop in partitionEvent.Data.Properties)
                    {
                        Debug.WriteLine("\t{0}: {1}", prop.Key, prop.Value);
                    }

                    Debug.WriteLine("System properties (set by IoT Hub):");
                    foreach (var prop in partitionEvent.Data.SystemProperties)
                    {
                        Debug.WriteLine("\t{0}: {1}", prop.Key, prop.Value);
                    }
                }
            }

            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

    }
}
