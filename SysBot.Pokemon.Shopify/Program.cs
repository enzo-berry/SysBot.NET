using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Fleck;
using PKHeX.Core;
using ShopifySharp;

namespace SysBot.Pokemon.Shopify
{
    internal class Program
    {
        // For debugging purposes.
        private static async Task Main(string[] args)
        {
            FleckLog.Level = LogLevel.Debug;
            var server = new WebSocketServer("ws://0.0.0.0");

            server.Start(socket =>
            {
                socket.OnOpen = async () =>
                {

                };
                socket.OnClose = () => Console.WriteLine("Close!");
                socket.OnMessage = message =>
                {
                    Console.WriteLine("Received: " + message);
                };
            });

            Console.WriteLine("WebSocket server started at ws://0.0.0.0");
            Console.ReadLine();

            //Fetching data from Shopify.
        }

    }
}
