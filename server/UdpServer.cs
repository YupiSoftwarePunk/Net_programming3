using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace server
{
    public class UdpServer
    {
        private readonly UdpClient udpClient;
        private readonly List<ClientInfo> clients = new List<ClientInfo>();
        private readonly object locker = new object();
        private readonly int port;

        public UdpServer(int port)
        {
            this.port = port;
            udpClient = new UdpClient(port);
        }


        public async Task StartAsync()
        {
            Console.WriteLine($"UDP сервер запущен и ожидает подключения на порту {port}...");
            while (true)
            {
                try
                {
                    UdpReceiveResult result = await udpClient.ReceiveAsync();
                    var clientEndPoint = result.RemoteEndPoint;
                    string message = Encoding.UTF8.GetString(result.Buffer);
                    _ = HandleClientMessageAsync(clientEndPoint, message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при получении данных: {ex.Message}");
                }
            }
        }


        private async Task HandleClientMessageAsync(IPEndPoint sender, string message)
        {
            if (message.StartsWith("HELLO:"))
            {
                string name = message.Substring(6).Trim();
                lock (locker)
                {
                    if (!clients.Any(c => c.EndPoint.Equals(sender)))
                    {
                        clients.Add(new ClientInfo(name, sender));
                        Console.WriteLine($"{name} подключился.");
                        Broadcast($"{name} ONLINE", sender);
                    }
                }
            }
            else if (message.StartsWith("BYE:")) 
            {
                string name = message.Substring(4).Trim();
                lock (locker)
                {
                    var client = clients.FirstOrDefault(c => c.EndPoint.Equals(sender));
                    if (client != null)
                    {
                        clients.Remove(client);
                        Console.WriteLine($"{name} отключился.");
                        Broadcast($"{name} OFFLINE", sender);
                    }
                }
            }
            else 
            {
                Console.WriteLine($"Сообщение от {sender}: {message}");
                Broadcast(message, sender);
            }
        }


        private void Broadcast(string message, IPEndPoint sender)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            lock (locker)
            {
                foreach (var client in clients)
                {
                    if (!client.EndPoint.Equals(sender))
                    {
                        udpClient.SendAsync(data, data.Length, client.EndPoint);
                    }
                }
            }
        }
    }
}
