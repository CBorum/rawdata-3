using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace sharp
{
    class RDJTPServer
    {
        int port;
        TcpListener server = null;
        public Func<string, Response> onMessage;

        public RDJTPServer(int port, Func<string, Response> onMessage)
        {
            this.onMessage = onMessage;
            this.port = port;
            try
            {
                server = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
            }
        }

        public void Run()
        {
            Console.WriteLine("Listening on port {0}", port);
            server.Start();

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                new Thread(() => handleConnection(client)).Start();
            }
        }

        void handleConnection(TcpClient client)
        {
            try
            {

                Byte[] bytes = new Byte[256];
                String data = null;
                NetworkStream stream = client.GetStream();
                int i = stream.Read(bytes, 0, bytes.Length);
                if (i != 0)
                {
                    data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                    var response = onMessage(data);
                    var json = response.AsJson();
                    Console.WriteLine("response: " + json);
                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(json);
                    stream.Write(msg, 0, msg.Length);
                }
            }
            catch (IOException)
            {
                Console.WriteLine("IO exception");
            }
            finally
            {
                client.Close();
            }
        }
    }
}