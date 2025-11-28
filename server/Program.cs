using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace server
{
    internal class Program
    {
        private static readonly ConcurrentDictionary<IPEndPoint, string> clients = new ConcurrentDictionary<IPEndPoint, string>();
        private static readonly object locker = new object();
        private static UdpClient udpServer;

        static async Task Main(string[] args)
        {
            udpServer = new UdpClient();
            Console.WriteLine("UDP сервер запущен и ожидает подключения на порту 8080...");

            Console.WriteLine("Сервер запущен и ожидает подключения...");

            while (true)
            {
                try
                {
                    UdpReceiveResult result = await udpServer.ReceiveAsync();
                    IPEndPoint clientEndPoint = result.RemoteEndPoint;
                    string message = Encoding.UTF8.GetString(result.Buffer);

                    //_ = ReceiveMessages(clientEndPoint, message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при получении данных: {ex.Message}");
                }
                
            }
        }
    }
}
