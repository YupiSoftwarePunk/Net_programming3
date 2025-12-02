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
        private readonly int port;
        private readonly UdpClient udp;
        private readonly List<ClientInfo> clients = new();
        private readonly object locker = new();

        public UdpServer(int port)
        {
            this.port = port;
            udp = new UdpClient(port);
        }

        public async Task StartAsync()
        {
            Console.WriteLine($"UDP сервер слушает порт {port}.");
            while (true)
            {
                UdpReceiveResult result;
                try
                {
                    result = await udp.ReceiveAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при получении пакета: {ex.Message}");
                    continue;
                }

                var remote = result.RemoteEndPoint;
                var message = Encoding.UTF8.GetString(result.Buffer);

                _ = ReceivedMessage(remote, message);
            }
        }

        private async Task ReceivedMessage(IPEndPoint sender, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            { 
                return; 
            }

            if (message.StartsWith("HELLO:", StringComparison.Ordinal))
            {
                var name = message.Substring(6).Trim();
                bool added = false;

                lock (locker)
                {
                    if (!clients.Any(c => c.EndPoint.Equals(sender)))
                    {
                        clients.Add(new ClientInfo(name, sender));
                        added = true;
                    }
                }

                if (added)
                {
                    Console.WriteLine($"{name} подключился ({sender}).");
                    await SendMessage($"{name} ONLINE");
                }
                else
                {
                    return;
                }
                
            }

            if (message.StartsWith("BYE:", StringComparison.Ordinal))
            {
                var name = message.Substring(4).Trim();
                bool removed = false;

                lock (locker)
                {
                    var client = clients.FirstOrDefault(c => c.EndPoint.Equals(sender));
                    if (client != null)
                    {
                        clients.Remove(client);
                        removed = true;
                    }
                }

                if (removed)
                {
                    Console.WriteLine($"{name} отключился ({sender}).");
                    await SendMessage($"{name} OFFLINE");
                }
                else
                {
                    return;
                }
            }

            if (message.StartsWith("MSG:", StringComparison.Ordinal))
            {
                var parts = message.Split(':', 3);
                if (parts.Length == 3)
                {
                    var name = parts[1];
                    var text = parts[2];

                    Console.WriteLine($"[{name}] : {text}");
                    await SendMessage($"MSG:{name}:{text}");
                    return;
                }
                else
                {
                    Console.WriteLine($"Неверный формат MSG от {sender}: {message}");
                    return;
                }
            }
            else
            {
                Console.WriteLine($"Неизвестный пакет от {sender}: {message}");
            }
        }

        private async Task SendMessage(string message)
        {
            byte[] payload = Encoding.UTF8.GetBytes(message);
            List<IPEndPoint> targets;

            lock (locker)
            {
                targets = clients.Select(c => c.EndPoint).ToList();
            }

            foreach (var endpoint in targets)
            {
                try
                {
                    await udp.SendAsync(payload, payload.Length, endpoint);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка рассылки {endpoint}: {ex.Message}");
                }
            }
        }
    }
}
