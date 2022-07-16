using FileTransfererer;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class SocketListener
{
    private static List<string> pathList = new List<string>();
    private static string listPath = string.Empty;
    private static readonly int TRANSFER_PORT = 666;
    private static string partnerAddress = "000.000.000.000";
    private static IPAddress localIp = IPAddress.Any;
    private static IPAddress targetIp;
    private static TcpClient socket;

    private static int _kport = 666;

    public static event Action<string> MessageReceieved;

    public static int Main(String[] args)
    {
        //ReadArgv();
        //if (listPath != string.Empty)
        //{
        //    ReadListFromFile(listPath);
        //    if (pathList.Count > 0)
        //    {
        //        OpenLocalSocket();
        //        SendFiles();
        //    }
        //    //CloseLocalSocket();
        //}

        SocketServer server = new SocketServer(_kport);
        Socket remote = new Socket(SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Loopback, _kport);

        remote.Connect(remoteEndPoint);

        using (NetworkStream stream = new NetworkStream(remote))
        using (StreamReader reader = new StreamReader(stream))
        using (StreamWriter writer = new StreamWriter(stream))
        {
            Task receiveTask = _Receive(reader);
            string text;

            Console.WriteLine("CLIENT: connected. Enter text to send...");

            while ((text = Console.ReadLine()) != "")
            {
                writer.WriteLine(text);
                writer.Flush();
            }

            remote.Shutdown(SocketShutdown.Send);
            receiveTask.Wait();
        }

        server.Stop();

        return 0;
    }

    private static async Task _Receive(StreamReader reader)
    {
        string receiveText;

        while ((receiveText = await reader.ReadLineAsync()) != null)
        {
            Console.WriteLine("CLIENT: received \"" + receiveText + "\"");
        }

        Console.WriteLine("CLIENT: end-of-stream");
    }

    private static void ReadThread()
    {
        NetworkStream netStream = socket.GetStream();
        while(socket.Connected)
        {
            byte[] bytes = new byte[socket.ReceiveBufferSize];
            netStream.Read(bytes, 0, (int)socket.ReceiveBufferSize);
            MessageReceieved(Encoding.UTF8.GetString(bytes));
        }
    }

    private static void ReadArgv()
    {
        string[] argv = Environment.GetCommandLineArgs();
        for (int i = 0; i < argv.Length; i++)
        {
            if (argv[i] == "-l" || argv[i] == "--list")
            {
                if(i < argv.Length-1)
                {
                    listPath = argv[++i];
                }
            }
            if (argv[i] == "-i" || argv[i] == "--ip")
            {
                if (i < argv.Length - 1)
                {
                    partnerAddress = argv[++i];
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
            while(!sr.EndOfStream)
            {
                string? line = sr.ReadLine();
                if (line != null)
                {
                    pathList.Add(line.Trim('"'));
                }
            }
        }
        catch(IOException e)
        {
            Console.WriteLine("The file could not be read: " + e.Message);
        }

        return pathList;
    }

    private static void SendFiles()
    {
        NetworkStream netStream = socket.GetStream();
        while(socket.Connected)
        {
            byte[] bytes = new byte[socket.ReceiveBufferSize];
            netStream.Read(bytes, 0, (int)socket.ReceiveBufferSize);
            MessageReceieved(Encoding.UTF8.GetString(bytes));
        }
    }

    private static void OpenLocalSocket()
    {
        socket = new TcpClient(partnerAddress.ToString(), TRANSFER_PORT);
        Thread listenThread = new Thread(ReadThread);
        listenThread.Start();
    }
    private static void EstablishRemoteConnection()
    {
    }

    private static void CloseLocalSocket()
    {
    }
}
