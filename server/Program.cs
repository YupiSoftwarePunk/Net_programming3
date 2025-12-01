using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace server
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            UdpServer server = new UdpServer(8080);
            Console.WriteLine("UDP сервер запущен...");
            await server.StartAsync();
        }
    }
}
