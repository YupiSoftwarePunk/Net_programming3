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

                _ = Task.Run(() => ReceivedMessage(remote, message));
            }
        }


        private async Task ReceivedMessage(IPEndPoint sender, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return; 
            }

            if (message.StartsWith("HELLO:"))
            {
                string name = message.Substring(6).Trim();

                bool nameIsTaken;
                lock (locker)
                {
                    nameIsTaken = clients.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                    var existing = clients.FirstOrDefault(c => c.Name == name);
                    if (existing != null)
                    {
                        clients.Remove(existing);
                        Console.WriteLine($"Заменяем старый endpoint для {name}");
                    }

                    clients.Add(new ClientInfo(name, sender));
                }

                Console.WriteLine($"{name} подключился");
                await SendUsers();
                await SendMessage($"{name} ONLINE");
            }
            else if (message.StartsWith("BYE:"))
            {
                string name = message.Substring(4).Trim();

                lock (locker)
                {
                    clients.RemoveAll(c => c.Name == name);
                }

                Console.WriteLine($"{name} отключился");
                await SendUsers();
                await SendMessage($"{name} OFFLINE");
            }
            else if (message.StartsWith("MSG:"))
            {
                Console.WriteLine($"Сообщение: {message}");
                await SendMessage(message);
            }
            else
            {
                Console.WriteLine($"Неизвестный пакет: {message}");
            }
        }



        private async Task SendUsers()
        {
            List<ClientInfo> clientsCopy;
            lock (locker)
            {
                clientsCopy = clients.ToList();
            }

            string users = string.Join(",", clientsCopy.Select(c => c.Name));
            await SendMessage($"USERS:{users}");
        }


        private async Task SendMessage(string message, string? excludeName = null)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);

            List<ClientInfo> clientsCopy;
            lock (locker)
            {
                clientsCopy = clients.ToList();
            }

            foreach (var client in clientsCopy)
            {
                if (excludeName != null && client.Name == excludeName)
                {
                    continue; 
                }

                try
                {
                    await udp.SendAsync(data, data.Length, client.EndPoint);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка отправки {client.Name}: {ex.Message}");
                }
            }
        }
    }
}
