using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace WpfWcfNamedPipeBinding
{
    public class NamedPipeServer
    {
        public const uint DUPLEX = (0x00000003);
        public const uint FILE_FLAG_OVERLAPPED = (0x40000000);

        public const int BUFFER_SIZE = 100;
        public int ClientType;
        private Client client;
        private SafeFileHandle clientHandle;
        public Client clientse = null;

        private Thread listenThread;
        private Thread loopThread;
        public string pipeName;

        public NamedPipeServer(string PName, int Mode)
        {
            pipeName = PName;
            ClientType = Mode; //0 Reading Pipe, 1 Writing Pipe
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern SafeFileHandle CreateNamedPipe(
            String pipeName,
            uint dwOpenMode,
            uint dwPipeMode,
            uint nMaxInstances,
            uint nOutBufferSize,
            uint nInBufferSize,
            uint nDefaultTimeOut,
            IntPtr lpSecurityAttributes);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int ConnectNamedPipe(
            SafeFileHandle hNamedPipe,
            IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int DisconnectNamedPipe(
            SafeFileHandle hNamedPipe);

        public void Start()
        {
            listenThread = new Thread(ListenForClients);
            listenThread.Start();
        }

        private void ListenForClients()
        {
            while (true)
            {
                clientHandle = CreateNamedPipe(pipeName, DUPLEX | FILE_FLAG_OVERLAPPED, 0, 255, BUFFER_SIZE, BUFFER_SIZE,
                    0, IntPtr.Zero);

                //could not create named pipe
                if (clientHandle.IsInvalid)
                    return;

                int success = ConnectNamedPipe(clientHandle, IntPtr.Zero);

                //could not connect client
                if (success == 0)
                    return;

                clientse = new Client();
                clientse.handle = clientHandle;
                clientse.stream = new FileStream(clientse.handle, FileAccess.ReadWrite, BUFFER_SIZE, true);

                if (ClientType == 0)
                {
                    var readThread = new Thread(Read);
                    readThread.Start();
                }
            }
        }

        private void Read()
        {
            //Client client = (Client)clientObj;
            //clientse.stream = new FileStream(clientse.handle, FileAccess.ReadWrite, BUFFER_SIZE, true);
            byte[] buffer = null;
            var encoder = new ASCIIEncoding();

            while (true)
            {
                int bytesRead = 0;

                try
                {
                    buffer = new byte[BUFFER_SIZE];
                    bytesRead = clientse.stream.Read(buffer, 0, BUFFER_SIZE);
                }
                catch
                {
                    //read error has occurred
                    break;
                }

                //client has disconnected
                if (bytesRead == 0)
                    break;

                //fire message received event
                //if (this.MessageReceived != null)
                //    this.MessageReceived(clientse, encoder.GetString(buffer, 0, bytesRead));

                int ReadLength = 0;
                for (int i = 0; i < BUFFER_SIZE; i++)
                {
                    if (buffer[i].ToString("x2") != "cc")
                    {
                        ReadLength++;
                    }
                    else
                        break;
                }
                if (ReadLength > 0)
                {
                    var Rc = new byte[ReadLength];
                    Buffer.BlockCopy(buffer, 0, Rc, 0, ReadLength);

                    Console.WriteLine("C# App: Received " + ReadLength + " Bytes: " +
                                      encoder.GetString(Rc, 0, ReadLength));
                    buffer.Initialize();
                }
            }

            //clean up resources
            clientse.stream.Close();
            clientse.handle.Close();
        }

        public void SendMessage(string message, Client client)
        {
            var encoder = new ASCIIEncoding();
            byte[] messageBuffer = encoder.GetBytes(message);

            if (client.stream.CanWrite)
            {
                client.stream.Flush();
                client.stream.Write(messageBuffer, 0, messageBuffer.Length);
                client.stream.Flush();
            }
        }


        public void SendMessageLoop()
        {
            loopThread = new Thread(SendMessageLoopProcess);
            loopThread.Start();
        }


        private void SendMessageLoopProcess()
        {
            while (true)
            {
                Thread.Sleep(10000);
                if (clientse != null)
                {
                    SendMessage("Test loop", clientse);
                    break;
                }

                Thread.Sleep(100);
            }
        }

        public void StopServer()
        {
            //clean up resources

            DisconnectNamedPipe(clientHandle);

            loopThread.Abort();
            listenThread.Abort();
        }

        public class Client
        {
            public SafeFileHandle handle;
            public FileStream stream;
        }
    }
}
