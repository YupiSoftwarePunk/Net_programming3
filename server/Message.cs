using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace server
{
    public class Message
    {
        public string Content { get; set; }
        public string SenderName { get; set; }
        public IPEndPoint SenderEndPoint { get; set; }
        public MessageType Type { get; set; }
        public DateTime Timestamp { get; set; }

        public enum MessageType
        {
            Text,
            Connection,
            Disconnection
        }


        public Message() { }

        public Message(string content, string senderName, IPEndPoint senderEndPoint, 
               MessageType type = MessageType.Text)
        {
            Content = content;
            SenderName = senderName;
            SenderEndPoint = senderEndPoint;
            Type = type;
            Timestamp = DateTime.Now;
        }


        public byte[] ToByteArray()
        {
            return Encoding.UTF8.GetBytes(Content);
        }

        public static Message FromByteArray(byte[] data, IPEndPoint senderEndPoint, string senderName = "")
        {
            return new Message(
                Encoding.UTF8.GetString(data),
                senderName,
                senderEndPoint
            );
        }
    }
}
