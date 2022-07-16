using System;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace FileTransferererClient
{
    class Program
    {
        private static readonly Socket ClientSocket = new Socket
            (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static List<string> pathList = new List<string>();
        private static string listPath = string.Empty;
        private static IPAddress serverIP = IPAddress.None;

        private const int PORT = 666;

        static void Main()
        {
            Console.Title = "Client";
            ReadArgv();
            if (listPath != string.Empty)
            {
                ReadListFromFile(listPath);
                if (serverIP == IPAddress.None)
                {
                    serverIP = PromptForIP();
                }
                if (serverIP != IPAddress.None)
                {
                    ConnectToServer(serverIP);
                    RequestLoop();
                }
            }
            Exit();
        }

        private static IPAddress PromptForIP()
        {
            Console.WriteLine("Enter server ip address: ");
            IPAddress serverIP;
            try
            {
                serverIP = IPAddress.Parse(Console.ReadLine());
            }
            catch (Exception e)
            {
                serverIP = IPAddress.None;
            }
            return serverIP;
        }

        private static void ConnectToServer(IPAddress serverIP)
        {
            int attempts = 0;

            while (!ClientSocket.Connected)
            {
                try
                {
                    attempts++;
                    Console.WriteLine("Connecting to " + serverIP.ToString() + ":" + PORT + ": attempt " + attempts);
                    // Change IPAddress.Loopback to a remote IP to connect to a remote host.
                    ClientSocket.Connect(serverIP, PORT);
                }
                catch (SocketException)
                {
                    Console.Clear();
                }
            }

            Console.Clear();
            Console.WriteLine("Connected");
        }

        private static void RequestLoop()
        {
            Console.WriteLine(@"<Type ""exit"" to properly disconnect client>");

            while (true)
            {
                SendRequest();
                ReceiveResponse();
            }
        }

        /// <summary>
        /// Close socket and exit program.
        /// </summary>
        private static void Exit()
        {
            SendString("exit"); // Tell the server we are exiting
            ClientSocket.Shutdown(SocketShutdown.Both);
            ClientSocket.Close();
            Environment.Exit(0);
        }

        private static void SendRequest()
        {
            Console.Write("Send a request: ");
            string request = Console.ReadLine();
            SendString(request);

            if (request.ToLower() == "exit")
            {
                Exit();
            }
        }

        /// <summary>
        /// Sends a string to the server with ASCII encoding.
        /// </summary>
        private static void SendString(string text)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(text);
            ClientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }

        private static void ReceiveResponse()
        {
            var buffer = new byte[2048];
            int received = ClientSocket.Receive(buffer, SocketFlags.None);
            if (received == 0) return;
            var data = new byte[received];
            Array.Copy(buffer, data, received);
            string text = Encoding.ASCII.GetString(data);
            Console.WriteLine(text);
        }

        private static void ReadArgv()
        {
            string[] argv = Environment.GetCommandLineArgs();
            for (int i = 0; i < argv.Length; i++)
            {
                if (argv[i] == "-l" || argv[i] == "--list")
                {
                    if (i < argv.Length - 1)
                    {
                        listPath = argv[++i];
                    }
                }
                if (argv[i] == "-i" || argv[i] == "--ip")
                {
                    if (i < argv.Length - 1)
                    {
                        serverIP = IPAddress.Parse(argv[++i]);
                    }
                }
                //if (argv[i] == "-w" || argv[i] == "--window")
                //{
                //    OpenFileDialog op;
                //}
            }
        }

        private static List<string> ReadListFromFile(string filePath)
        {

            try
            {
                var sr = new StreamReader(filePath);
                while (!sr.EndOfStream)
                {
                    string? line = sr.ReadLine();
                    if (line != null)
                    {
                        pathList.Add(line.Trim('"'));
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("The file could not be read: " + e.Message);
            }

            return pathList;
        }
    }
}
