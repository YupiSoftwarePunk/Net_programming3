using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client.Model
{
    public class UdpClientService
    {
        private readonly UdpClient udpClient;
        private readonly IPEndPoint serverEndPoint;
        private readonly string userName;

        public event Action<string>? MessageReceived;

        public UdpClientService(string serverIp, int port, string userName)
        {
            udpClient = new UdpClient();
            serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), port);
            this.userName = userName;
        }


        public async Task ConnectAsync()
        {
            await SendMessage($"HELLO:{userName}");
            _ = Task.Run(ReceiveMessage);
        }

        public async Task DisconnectAsync()
        {
            await SendMessage($"BYE:{userName}");
            udpClient.Close();
        }


        public async Task SendMessage(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            await udpClient.SendAsync(data, data.Length, serverEndPoint);
        }


        private async Task ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    var result = await udpClient.ReceiveAsync();
                    string msg = Encoding.UTF8.GetString(result.Buffer);
                    //MessageReceived?.Invoke(msg);
                }
                catch
                {
                    break;
                }
            }
        }
    }
}
