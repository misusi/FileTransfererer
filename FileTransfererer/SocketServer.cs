using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FileTransfererer
{
    class SocketServer
    {
        private readonly Socket listener;
        public SocketServer(int port)
        {
            IPEndPoint listenerEndpoint = new IPEndPoint(IPAddress.Loopback, port);
            listener = new Socket(SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(listenerEndpoint);
            listener.Listen(1);
            listener.BeginAccept(Accept, null);
        }
        public void Stop()
        {
            listener.Close();
        }
        private async void Accept(IAsyncResult result)
        {
            try
            {
                using (Socket client = listener.EndAccept(result))
                using (NetworkStream stream = new NetworkStream(client))
                using (StreamReader reader = new StreamReader(stream))
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    Console.WriteLine("SERVER: accepted new client");
                    string text;
                    while ((text = await reader.ReadLineAsync()) != null)
                    {
                        Console.WriteLine("SERVER: End of stream");
                    }
                    listener.BeginAccept(Accept, null);
                }
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("SERVER: server was closed");
            }
            catch (SocketException e)
            {
                Console.WriteLine("SERVER: Exception: " + e);
            }
        }
    }
}
