using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class Server
    {
        private TcpListener listener;
        private readonly List<TcpClient> clients = new List<TcpClient>();

        public Server(int port)
        {
            listener = new TcpListener(IPAddress.Any, port);
        }

        public void Start()
        {
            listener.Start();
            Console.WriteLine("Server started...");
            while (true)
            {
                var client = listener.AcceptTcpClient();
                clients.Add(client);
                Console.WriteLine("Client connected");
                Thread clientThread = new Thread(() => HandleClient(client));
                clientThread.Start();
            }
        }

        private void HandleClient(TcpClient client)
        {
            var stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead;

            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Received: {message}");
                BroadcastMessage(message);
            }

            client.Close();
        }

        private void BroadcastMessage(string message)
        {
            byte[] msg = Encoding.UTF8.GetBytes(message);
            foreach (var client in clients)
            {
                var stream = client.GetStream();
                stream.Write(msg, 0, msg.Length);
            }
        }
    }
}