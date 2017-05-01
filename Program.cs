using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;

namespace ns_traffic_light_signalr_client
{
    public enum TrafficLightState
    {
        /// <summary>
        /// Default value: all the lights are off
        /// </summary>
        Off,
        /// <summary>
        /// The green light is on
        /// </summary>
        Green,
        /// <summary>
        /// The orange (amber) light is on
        /// </summary>
        Orange,
        /// <summary>
        /// The red light is on
        /// </summary>
        Red
    }

    class Program
    {
        static void Main(string[] args)
        {
            //Set connection
            var connection = new HubConnection("http://localhost:5000/");
            //Make proxy to hub based on hub name on server
            var myHub = connection.CreateHubProxy("TrafficLightHub");

            connection.EnsureReconnecting();

            //Start connection
            connection.Start().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Console.WriteLine("There was an error opening the connection:{0}", task.Exception.GetBaseException());
                }
                else
                {
                    Console.WriteLine("Connected");
                }
            }).Wait();

            connection.Closed += () =>
            {
                Console.WriteLine("Connection closed");
                while (connection.State == ConnectionState.Disconnected)
                {
                    Console.WriteLine("Trying to reconnect...");
                    if (!StartConnection(connection).GetAwaiter().GetResult())
                    {
                        Task.Delay(3000).Wait();
                    }
                }
            };
            connection.Reconnected += () => Console.WriteLine("Reconnected");
            connection.Reconnecting += () => Console.WriteLine("Reconnecting...");

            myHub.On<TrafficLightState>("UpdateLight", (s) => Console.WriteLine($"Youhou {s}"));

            Console.ReadLine();

            connection.Stop();
        }

        private static Task<bool> StartConnection(HubConnection connection)
        {
            return connection.Start().ContinueWith<bool>(task =>
            {
                if (task.IsFaulted)
                {
                    Console.WriteLine("There was an error opening the connection:{0}", task.Exception.GetBaseException());
                    return false;
                }

                Console.WriteLine("Connected");
                return true;
            });
        }
    }
}
